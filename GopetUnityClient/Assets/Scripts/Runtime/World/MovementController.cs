using System.Diagnostics;
using Gopet.Net.Map;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Input liên tục (WASD / mũi tên / D-pad) → di chuyển avatar mỗi frame → lấy mẫu
    /// bằng <see cref="PathSampler"/> → flush qua <see cref="MapHandler.SendMove"/> mỗi
    /// <see cref="PathSampler.FlushIntervalMs"/>. Cùng pattern <c>ew.java:436-497</c>.
    ///
    /// <para><b>Không click-to-move</b>. Jar không có; server chỉ nhìn 2 số cuối nên chấp
    /// nhận, nhưng hành vi client khác hẳn — người chơi jar cũ sẽ thấy lạ.</para>
    /// </summary>
    public sealed class MovementController : MonoBehaviour
    {
        /// <summary>Tốc độ đi mặc định — 4 ô/giây (96 px/s). Chỉnh sau khi có <c>playerData.speed</c> từ server.</summary>
        public const float DefaultWalkSpeedPxPerSec = 96f;

        private static readonly Stopwatch Clock = Stopwatch.StartNew();

        private MapScene _scene;
        private MapHandler _handler;
        private CameraFollower _camera;
        private GameHud _hud;
        private int _mapId;
        private int _userId;
        private float _jarX;
        private float _jarY;
        private readonly PathSampler _sampler = new PathSampler();
        private InputAction _wasdAction;
        private InputAction _arrowAction;

        public float WalkSpeed { get; set; } = DefaultWalkSpeedPxPerSec;
        public int CurrentMapId => _mapId;
        public Vector2 JarPosition => new Vector2(_jarX, _jarY);
        public bool InputEnabled { get; set; } = true;

        /// <summary>Hướng cuối cùng (0=đông,1=tây,2=nam,3=bắc). Server ghi log — client dùng animation sau.</summary>
        public int LastDirection { get; private set; } = 0;

        private void Awake()
        {
            _wasdAction = new InputAction("Move WASD", InputActionType.Value);
            var composite = _wasdAction.AddCompositeBinding("2DVector");
            composite.With("Up", "<Keyboard>/w");
            composite.With("Down", "<Keyboard>/s");
            composite.With("Left", "<Keyboard>/a");
            composite.With("Right", "<Keyboard>/d");
            _arrowAction = new InputAction("Move Arrows", InputActionType.Value);
            var arrows = _arrowAction.AddCompositeBinding("2DVector");
            arrows.With("Up", "<Keyboard>/upArrow");
            arrows.With("Down", "<Keyboard>/downArrow");
            arrows.With("Left", "<Keyboard>/leftArrow");
            arrows.With("Right", "<Keyboard>/rightArrow");
        }

        private void OnEnable()
        {
            _wasdAction?.Enable();
            _arrowAction?.Enable();
        }

        private void OnDisable()
        {
            _wasdAction?.Disable();
            _arrowAction?.Disable();
        }

        private void OnDestroy()
        {
            _wasdAction?.Dispose();
            _arrowAction?.Dispose();
            _wasdAction = null;
            _arrowAction = null;
        }

        public static MovementController Attach(MapScene scene, MapHandler handler, CameraFollower camera, GameHud hud,
            int mapId, int userId, int initialJarX, int initialJarY)
        {
            var go = new GameObject("MovementController");
            go.transform.SetParent(scene.transform, false);
            var mc = go.AddComponent<MovementController>();
            mc._scene = scene;
            mc._handler = handler;
            mc._camera = camera;
            mc._hud = hud;
            mc._mapId = mapId;
            mc._userId = userId;
            mc._jarX = initialJarX;
            mc._jarY = MapCollision.NearestVisiblePlayerY(
                scene.Map?.Map, initialJarX, initialJarY);
            if (mc._jarY != initialJarY)
                scene.Self?.SnapTo(initialJarX, (int)mc._jarY);
            return mc;
        }

        private void Update()
        {
            if (_scene?.Self == null) return;

            if (!InputEnabled)
            {
                _scene.Self.SetLocomotion(LastDirection, false);
                return;
            }

            var (dx, dy) = ReadDirection();
            var nowMs = Clock.ElapsedMilliseconds;

            if (dx != 0f || dy != 0f)
            {
                Walk(dx, dy);
                if (!_sampler.Recording) _sampler.Start(nowMs);
                _sampler.Sample((int)_jarX, (int)_jarY);

                if (_sampler.ShouldFlush(nowMs))
                {
                    FlushToServer();
                    _sampler.Start(nowMs);
                }
            }
            else if (_sampler.Recording && _sampler.ShouldFlush(nowMs))
            {
                FlushToServer(); // đứng yên đủ lâu → chốt path
            }
            if (dx == 0f && dy == 0f)
            {
                _scene.Self.SetLocomotion(LastDirection, false);
            }
        }

        private void Walk(float dx, float dy)
        {
            var len = Mathf.Sqrt(dx * dx + dy * dy);
            dx /= len; dy /= len;
            var delta = WalkSpeed * Time.deltaTime;
            var nextX = _jarX + dx * delta;
            var nextY = _jarY + dy * delta;
            var map = _scene.Map?.Map;
            if (MapCollision.CanStand(map, Mathf.RoundToInt(nextX), Mathf.RoundToInt(_jarY))) _jarX = nextX;
            // Enforce the visual top margin only while travelling upward. A server spawn/warp
            // inside that margin must still be able to walk downward instead of becoming stuck.
            var canMoveY = dy < 0f
                ? MapCollision.CanPlayerStand(map, Mathf.RoundToInt(_jarX), Mathf.RoundToInt(nextY))
                : MapCollision.CanStand(map, Mathf.RoundToInt(_jarX), Mathf.RoundToInt(nextY));
            if (canMoveY) _jarY = nextY;

            LastDirection = MapPlacement.Direction4(Mathf.RoundToInt(dx * 100), Mathf.RoundToInt(dy * 100));
            // Walking vertically must keep the last horizontal look direction. Resetting it to
            // left on every up/down frame makes the camera jump sideways and also disturbs the
            // apparent pet trail at corners.
            if (_camera != null && (LastDirection == 0 || LastDirection == 1))
                _camera.FaceRight = LastDirection == 0;

            _scene.Self.SnapTo((int)_jarX, (int)_jarY);
            _scene.Self.SetLocomotion(LastDirection, true);
        }

        private void FlushToServer()
        {
            var points = _sampler.Flush((int)_jarX, (int)_jarY);
            _handler.SendMove(_userId, LastDirection, _mapId, points);
        }

        /// <summary>Đọc WASD / mũi tên. Y jar hướng xuống — S/mũi-tên-xuống = dy dương.</summary>
        public void ResetForMap(int mapId, int jarX, int jarY)
        {
            if (_sampler.Recording) _sampler.Flush((int)_jarX, (int)_jarY);
            _mapId = mapId;
            _jarX = jarX;
            _jarY = MapCollision.NearestVisiblePlayerY(_scene.Map?.Map, jarX, jarY);
            if (_jarY != jarY)
                _scene.Self?.SnapTo(jarX, (int)_jarY);
        }

        private (float dx, float dy) ReadDirection()
        {
            var mobile = _hud?.Joystick?.Direction ?? Vector2.zero;
            if (mobile.sqrMagnitude > 0.001f) return (mobile.x, -mobile.y);
            if (_hud != null && _hud.IsTyping) return (0f, 0f);
            var keyboard = (_wasdAction?.ReadValue<Vector2>() ?? Vector2.zero) +
                (_arrowAction?.ReadValue<Vector2>() ?? Vector2.zero);
            keyboard = Vector2.ClampMagnitude(keyboard, 1f);
            // Input System uses screen/gamepad convention (up = +Y), while the
            // JAR map coordinates increase downward (up = -Y).
            return (keyboard.x, -keyboard.y);
        }
    }
}
