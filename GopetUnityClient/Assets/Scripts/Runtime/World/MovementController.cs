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
    public sealed partial class MovementController : MonoBehaviour
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
        private bool _walking;
        private float _lockedUntil;

        /// <summary>Quay mặt nhân vật về một hướng (0=đông, 1=tây, 2=nam, 3=bắc) mà không
        /// cần người chơi bấm phím. Lúc tương tác pet, jar xoay người chơi về phía con pet.</summary>
        /// <summary>
        /// Cấm điều khiển trong <paramref name="seconds"/> giây rồi tự mở lại. Tách hẳn khỏi
        /// <see cref="InputEnabled"/> (cờ của trận đánh): nếu dùng chung một cờ thì hiệu ứng
        /// tương tác pet hết giờ giữa trận sẽ mở khoá ngay trong lúc đang đánh.
        /// </summary>
        public void LockInput(float seconds)
        {
            if (seconds <= 0f) return;
            _lockedUntil = Mathf.Max(_lockedUntil, Time.time + seconds);
        }

        public void FaceDirection(int direction)
        {
            LastDirection = direction;
            if (_camera != null && (direction == 0 || direction == 1)) _camera.FaceRight = direction == 0;
            _scene?.Self?.SetLocomotion(direction, false);
        }

        /// <summary>Đang có phím đi được giữ. Xem <see cref="WalkStarted"/> để biết vì sao
        /// người gọi cần phân biệt "đang đi" với "vừa bắt đầu đi".</summary>
        public bool IsWalking => _walking;

        /// <summary>
        /// Bắn ra ở khung hình người chơi CHUYỂN từ đứng yên sang đi. Chế độ hồi phục pet
        /// của jar tắt đúng lúc này (<c>ew.java:424</c>), nên cần mốc "bắt đầu đi" chứ không
        /// phải cờ "đang đi" — nếu không sẽ gửi gói tắt mỗi khung hình.
        /// </summary>
        public event System.Action WalkStarted;

        public float WalkSpeed { get; set; } = DefaultWalkSpeedPxPerSec;
        public int CurrentMapId => _mapId;
        public Vector2 JarPosition => new Vector2(_jarX, _jarY);
        public bool InputEnabled { get; set; } = true;

        /// <summary>Hướng cuối cùng (0=đông,1=tây,2=nam,3=bắc). Server ghi log — client dùng animation sau.</summary>
        public int LastDirection { get; private set; } = 0;

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

            if (!InputEnabled || Time.time < _lockedUntil)
            {
                _walking = false;
                _scene.Self.SetLocomotion(LastDirection, false);
                return;
            }

            var (dx, dy) = ReadDirection();
            var nowMs = Clock.ElapsedMilliseconds;

            if (dx != 0f || dy != 0f)
            {
                if (!_walking)
                {
                    _walking = true;
                    WalkStarted?.Invoke();
                }
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
                _walking = false;
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

    }
}
