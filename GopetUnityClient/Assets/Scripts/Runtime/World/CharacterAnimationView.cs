using System;
using Gopet.Net.Images;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Animated achievement badge/decor attached to one player avatar.</summary>
    public sealed class CharacterAnimationView : MonoBehaviour
    {
        private const float FrameInterval = 0.2f;

        /// <summary>
        /// Ảnh danh hiệu gốc rộng ~140px — gấp đôi nhân vật, nhìn to và lơ lửng xa đầu.
        /// Thu nhỏ còn một nửa.
        /// </summary>
        public const float Scale = 0.5f;

        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private int _frame;
        private float _nextFrame;

        public static CharacterAnimationView Create(Transform avatar, CharacterAnimation animation,
            RemoteAssetCache assets, int index)
        {
            var go = new GameObject($"Character animation {animation.Type}:{index}", typeof(SpriteRenderer));
            go.transform.SetParent(avatar, false);
            // Parent là PlayerAvatar.TitleAnchor (trên tên, đã tính theo chiều cao nhân vật);
            // sprite pivot ở đáy nên chỉ còn cộng vX/vY server gửi (trục y jar hướng xuống).
            go.transform.localPosition = new Vector3(animation.OffsetX, -animation.OffsetY, 0f);
            go.transform.localScale = Vector3.one * Scale;
            var view = go.AddComponent<CharacterAnimationView>();
            view._renderer = go.GetComponent<SpriteRenderer>();
            // Trên mọi nhân vật (ActorBaseOrder 10000 + y) nhưng DƯỚI bong bóng chat (20000):
            // danh hiệu nằm ngay trên tên nên bong bóng chat đè lên, không được che chữ chat.
            view._renderer.sortingOrder = animation.DrawAtEnd ? 19_995 : 19_990;
            assets?.Get(animation.FrameImagePath, ImagePackets.TypeIcon, view, texture =>
            {
                if (view == null || texture == null) return;
                view._frames = SpriteFrameCache.Slice(animation.FrameImagePath, texture, animation.FrameCount);
                view._renderer.sprite = view._frames[0];
            });
            return view;
        }

        private void Update()
        {
            if (_frames.Length <= 1 || Time.time < _nextFrame) return;
            _nextFrame = Time.time + FrameInterval;
            _frame = (_frame + 1) % _frames.Length;
            _renderer.sprite = _frames[_frame];
        }

    }
}
