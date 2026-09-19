using System;
using System.Collections.Generic;
using Gopet.Net.Images;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Sprite pet đi cùng player. Đi lại đúng quỹ đạo cũ của owner ở một khoảng cách cố định,
    /// kể cả khi rẽ góc hoặc quay đầu; cycle qua frame ~200ms.
    ///
    /// <para>Frame layout khớp <see cref="WorldActorView.Frames"/>: sprite là 1 strip
    /// ngang gồm N frame kề nhau (server bơm <c>frameNum</c>). Xẻ width/frameNum,
    /// pivot (0.5, 0) đặt chân sprite tại y=0 rồi lift theo <c>VerticalOffset</c>
    /// của server (âm = lên trên).</para>
    /// </summary>
    public sealed class PetAvatar : MonoBehaviour
    {
        private const float FollowDistance = 40f;
        private const float InitialOffsetX = 28f;
        private const float TrailSampleSpacing = 1.5f;
        private const float TeleportDistance = 160f;
        private const float FrameInterval = 0.2f;

        /// <summary>Khoảng chết trước khi đổi hướng nhìn, pixel. Thiếu nó thì lúc pet đứng gần
        /// trùng trục owner, dấu của dx đảo liên tục và sprite lật qua lật lại.</summary>
        private const float FaceDeadZone = 5f;

        private static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();

        private Transform _owner;
        private SpriteRenderer _renderer;
        private JarNameLabel _label;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private float _nextFrameTime;
        private int _frame;
        private short _verticalOffset;
        private readonly PositionTrail _trail = new PositionTrail();
        private Vector3 _lastOwnerPosition;
        private bool _positionInitialized;

        public int OwnerUserId { get; private set; }

        public static PetAvatar Create(Transform parent, Transform owner, PetZoneEntry entry, RemoteAssetCache assets)
        {
            var go = new GameObject($"Pet #{entry.OwnerUserId} T{entry.PetIdTemplate}", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<PetAvatar>();
            view._owner = owner;
            view.OwnerUserId = entry.OwnerUserId;
            view._verticalOffset = entry.VerticalOffset;
            view.SnapBesideOwner();

            var spriteGo = new GameObject("Sprite", typeof(SpriteRenderer));
            spriteGo.transform.SetParent(go.transform, false);
            view._renderer = spriteGo.GetComponent<SpriteRenderer>();

            view._label = JarNameLabel.Create(go.transform, new Vector3(0f, 0f, 0f), 0.7f,
                JarIconTokens.Strip(entry.DisplayName ?? string.Empty));

            var frames = Mathf.Max((int)entry.FrameNum, 1);
            assets.Get(entry.FrameImagePath, ImagePackets.TypeNpc, texture =>
            {
                if (view == null || texture == null) return;
                view._frames = SliceFrames(entry.FrameImagePath, texture, frames);
                view._frame = 0;
                view._renderer.sprite = view._frames[0];
                view.PositionLabelAboveSprite();
            });

            return view;
        }

        private void PositionLabelAboveSprite()
        {
            if (_label == null || _renderer == null || _renderer.sprite == null) return;
            var h = _renderer.sprite.rect.height;
            _label.transform.localPosition = new Vector3(0f, h + 4f, 0f);
        }

        private void LateUpdate()
        {
            if (_owner == null)
            {
                Destroy(gameObject);
                return;
            }

            var ownerPos = _owner.position;
            if (!_positionInitialized) SnapBesideOwner();

            if (Vector2.Distance(ownerPos, _lastOwnerPosition) >= TeleportDistance)
            {
                SnapBesideOwner();
                ownerPos = _owner.position;
            }

            _trail.Add(ownerPos.x, ownerPos.y, TrailSampleSpacing);
            if (_trail.TotalLength >= FollowDistance)
            {
                var (trailX, trailY) = _trail.PointBehind(FollowDistance);
                transform.position = new Vector3(
                    trailX, trailY + _verticalOffset, ownerPos.z);
            }
            _lastOwnerPosition = ownerPos;

            // Use the owner's JAR foot row; converting from world Y here used to put pets in
            // the wrong sorting band on tall maps.
            var ownerAvatar = _owner.GetComponent<PlayerAvatar>();
            var jarY = ownerAvatar != null ? ownerAvatar.JarY : 0;
            var order = MapPlacement.ActorSortingOrder(jarY) - 1;
            _renderer.sortingOrder = order;
            _label?.SetSortingOrder(order + 20);

            FaceOwner();

            if (_frames.Length > 1 && Time.time >= _nextFrameTime)
            {
                _nextFrameTime = Time.time + FrameInterval;
                _frame = (_frame + 1) % _frames.Length;
                _renderer.sprite = _frames[_frame];
            }
        }

        /// <summary>Quay mặt về phía chủ. Pet đi sau lưng nên nếu không quay thì hay thấy nó
        /// quay lưng vào người chơi.</summary>
        private void FaceOwner()
        {
            if (_renderer == null || _owner == null) return;
            var dx = _owner.position.x - transform.position.x;
            if (Mathf.Abs(dx) < FaceDeadZone) return;
            // Sprite pet vẽ sẵn quay TRÁI — xem BattlePetCard: chỉ card BÊN TRÁI mới lật
            // (localScale.x = -1) để nhìn sang đối thủ bên phải. Nên chủ ở bên phải thì lật.
            _renderer.flipX = dx > 0f;
        }

        private void SnapBesideOwner()
        {
            if (_owner == null) return;
            var ownerPos = _owner.position;
            transform.position = new Vector3(
                ownerPos.x + InitialOffsetX,
                ownerPos.y + _verticalOffset,
                ownerPos.z);
            // Seed the trail from the pet's initial ground position to the owner. The pet can
            // therefore wait visibly beside a stationary owner, then start following without
            // a jump no matter which direction the owner moves first.
            _trail.Reset(ownerPos.x + InitialOffsetX, ownerPos.y);
            _trail.Add(ownerPos.x, ownerPos.y, 0.01f);
            _lastOwnerPosition = ownerPos;
            _positionInitialized = true;
        }

        private static Sprite[] SliceFrames(string path, Texture2D texture, int count)
        {
            var key = $"{path}|{texture.GetHashCode()}|{count}";
            if (FrameCache.TryGetValue(key, out var cached) && cached != null) return cached;

            if (texture.width < count) count = 1;
            var width = texture.width / count;
            var result = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = Sprite.Create(texture,
                    new Rect(i * width, 0f, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            }
            return FrameCache[key] = result;
        }
    }
}
