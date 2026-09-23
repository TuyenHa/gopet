using System;
using System.Collections.Generic;
using Gopet.UiLogic;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>Avatar dùng chung cho self và người chơi khác, có tên và hoạt ảnh đi.</summary>
    public sealed class PlayerAvatar : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>Bấm avatar (self hoặc người khác). Caller quyết định mở menu gì.</summary>
        public event Action<PlayerAvatar> Tapped;

        private readonly Queue<Vector2Int> _path = new Queue<Vector2Int>();
        private readonly PositionInterpolator _interp = new PositionInterpolator();
        private const float NameScale = 0.75f; // nhỏ hơn cho khớp jar; đồng bộ WorldActorView
        private const float NameHeight = JarNameLabel.Height * NameScale;

        /// <summary>Khoảng cách từ đỉnh nhãn tên tới đáy danh hiệu (px game).</summary>
        public const float TitleGap = 10f;
        private AvatarAppearance _appearance;
        private CharacterSkinView _skin;
        private CharacterWingView _wing;
        private bool _facingLeft;
        private bool _moving;
        private JarNameLabel _nameLabel;
        private int _mapHeightPixels;
        private int _targetJarY;
        // Self dời bằng SnapTo mỗi frame (dx=dy=0) nên không suy ra "đang đi" từ nội suy được.
        // MovementController gọi SetLocomotion(true) mỗi frame khi đi → giữ mốc THỜI GIAN này
        // (không dùng frameCount vì thứ tự Update giữa các component không xác định).
        private const float CommandHoldSeconds = 0.12f;
        private float _commandedMovingUntil;

        /// <summary>
        /// Đỉnh đầu THẬT tính từ chân, đo theo hình đang hiện (skin hoặc avatar mặc định) —
        /// skin cao/thấp khác nhau nên tên và danh hiệu phải bám theo, không đặt cứng.
        /// </summary>
        public float HeadTopY { get; private set; }

        /// <summary>Mốc đáy danh hiệu: đầu → tên → <see cref="TitleGap"/> → danh hiệu.</summary>
        public Transform TitleAnchor { get; private set; }

        public int UserId { get; private set; }
        public string PlayerName { get; private set; }
        /// <summary>Current foot coordinate in the map's top-down JAR coordinate system.</summary>
        public int JarY => _targetJarY;
        public (int jarX, int jarY) JarPosition =>
            MapPlacement.WorldToJar(_interp.X, _interp.Y, _mapHeightPixels);

        public static PlayerAvatar Spawn(Transform parent, int userId, string name, int gender,
            int jarX, int jarY, int mapHeightPixels)
        {
            var go = new GameObject($"Avatar {userId} {name}", typeof(BoxCollider2D));
            if (parent != null) go.transform.SetParent(parent, false);
            // Bounding box tap: avatar jar cao ~64 px, rộng ~48; đặt tâm ngay body.
            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(48f, 64f);
            collider.offset = new Vector2(0f, 32f);
            var avatar = go.AddComponent<PlayerAvatar>();
            avatar.UserId = userId;
            // Server có thể nhúng token icon dạng (saoden), (vang), (ngoc)… vào tên
            // (bảng dj.java:54). Client Unity chưa có atlas icon, strip hết để tên sạch.
            avatar.PlayerName = Gopet.UiLogic.JarIconTokens.Strip(name ?? string.Empty);
            avatar._mapHeightPixels = mapHeightPixels;
            avatar._appearance = AvatarAppearance.Create(go.transform, gender);
            avatar.CreateNameLabel();
            avatar.TitleAnchor = new GameObject("Title Anchor").transform;
            avatar.TitleAnchor.SetParent(go.transform, false);
            avatar.RefreshHeadTop();
            avatar.SnapTo(jarX, jarY);
            return avatar;
        }

        private void CreateNameLabel()
        {
            // World-space name label (JarNameLabel): đáy tên sát đỉnh đầu (RefreshHeadTop), mọc lên,
            // canh giữa, màu xanh 0x3B5998 như cp.d() — giống y hệt jar, không viền/nhân đôi.
            _nameLabel = JarNameLabel.Create(transform, Vector3.zero, NameScale, PlayerName);
        }

        /// <summary>Đo lại đỉnh đầu rồi xếp tên và mốc danh hiệu lên trên nó.</summary>
        private void RefreshHeadTop()
        {
            HeadTopY = _skin != null && _skin.HasSprite ? _skin.TopY : _appearance.TopY;
            _nameLabel.transform.localPosition = new Vector3(0f, HeadTopY, 0f);
            TitleAnchor.localPosition = new Vector3(0f, HeadTopY + NameHeight + TitleGap, 0f);
        }

        public void SnapTo(int jarX, int jarY)
        {
            var (wx, wy) = MapPlacement.JarToWorld(jarX, jarY, _mapHeightPixels);
            _interp.Snap(wx, wy);
            _targetJarY = jarY;
            _path.Clear();
            ApplyTransform();
        }

        public void MoveTo(int jarX, int jarY)
        {
            _path.Clear();
            SetTarget(jarX, jarY);
        }

        public void MoveAlong(int[] points)
        {
            _path.Clear();
            if (points == null) return;
            for (var i = 0; i + 1 < points.Length; i += 2)
                _path.Enqueue(new Vector2Int(points[i], points[i + 1]));
            AdvancePath();
        }

        public void SetLocomotion(int direction, bool moving)
        {
            _appearance.SetFacing(direction);
            _facingLeft = direction == 1;
            _moving = moving;
            _skin?.SetFacing(_facingLeft);
            _skin?.SetMoving(moving);
            _commandedMovingUntil = moving ? Time.time + CommandHoldSeconds : 0f;
        }

        public void ApplySkin(string path, RemoteAssetCache assets)
        {
            if (_skin != null) Destroy(_skin.gameObject);
            _skin = null;
            var equipped = !string.IsNullOrEmpty(path);
            if (_appearance != null) _appearance.gameObject.SetActive(!equipped);
            if (equipped)
            {
                _skin = CharacterSkinView.Create(transform, path, assets);
                _skin.SetFacing(_facingLeft);
                _skin.SetMoving(_moving);
                // Ảnh skin tải bất đồng bộ: có ảnh rồi mới biết skin cao bao nhiêu.
                _skin.Loaded += RefreshHeadTop;
            }
            RefreshHeadTop();
        }

        public void ApplyWing(string path, int verticalOffset, RemoteAssetCache assets)
        {
            if (_wing != null) Destroy(_wing.gameObject);
            _wing = null;
            if (string.IsNullOrEmpty(path)) return;
            _wing = CharacterWingView.Create(transform, path, verticalOffset, assets);
        }

        private void SetTarget(int jarX, int jarY)
        {
            var (wx, wy) = MapPlacement.JarToWorld(jarX, jarY, _mapHeightPixels);
            _interp.SetTarget(wx, wy);
            _targetJarY = jarY;
        }

        private void AdvancePath()
        {
            if (_path.Count == 0) return;
            var next = _path.Dequeue();
            SetTarget(next.x, next.y);
        }

        private void Update()
        {
            _interp.Tick(Time.deltaTime);
            var dx = _interp.TargetX - _interp.X;
            var dy = _interp.TargetY - _interp.Y;
            if (dx * dx + dy * dy < 1f && _path.Count > 0) AdvancePath();
            _moving = dx * dx + dy * dy >= 0.25f || _path.Count > 0 || Time.time < _commandedMovingUntil;
            _appearance.SetMoving(_moving);
            _skin?.SetMoving(_moving);
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            transform.localPosition = new Vector3(_interp.X, _interp.Y, 0f);
            var order = MapPlacement.ActorSortingOrder(_targetJarY);
            _appearance.SetSortingOrder(order);
            _skin?.SetSortingOrder(order + 10);
            // JAR vẽ cánh trước avatar; part thấp nhất bắt đầu tại order + 10.
            _wing?.SetSortingOrder(order + 9);
            _nameLabel?.SetSortingOrder(order + 20);
        }

        public void OnPointerClick(PointerEventData eventData) => Tapped?.Invoke(this);
    }
}
