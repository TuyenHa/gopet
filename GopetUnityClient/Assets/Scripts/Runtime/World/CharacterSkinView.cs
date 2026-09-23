using System;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Full-body two-frame skin used by the legacy anim_characters assets.</summary>
    public sealed class CharacterSkinView : MonoBehaviour
    {
        /// <summary>
        /// Chiều cao chuẩn (px game) mọi skin được kéo về — trung vị của 125 ảnh
        /// anim_characters trên server. Ảnh gốc cao từ 50 tới 154px, đứng cạnh nhau
        /// người to người bé tràn màn hình.
        /// </summary>
        public const float StandardHeight = 70f;
        /// <summary>
        /// Giới hạn tỉ lệ để không vỡ pixel art: phóng to quá nhiều thì răng cưa không
        /// đều, skin "khủng" vẫn nhỏ lại rõ nhưng không bị bóp mất chi tiết.
        /// </summary>
        public const float MinScale = 0.7f;
        public const float MaxScale = 1.15f;

        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private bool _moving;
        private float _nextFrame;
        private int _frame;

        /// <summary>Bắn khi ảnh skin tải xong — lúc này mới đo được chiều cao thật.</summary>
        public event Action Loaded;
        public bool HasSprite => _frames.Length > 0;
        /// <summary>Tỉ lệ đang áp cho skin (1 khi chưa có ảnh).</summary>
        public float Scale => transform.localScale.y;
        /// <summary>Đỉnh skin tính từ chân (pivot đáy), px game, đã tính tỉ lệ chuẩn hoá.</summary>
        public float TopY => HasSprite ? _frames[0].bounds.max.y * Scale : 0f;

        /// <summary>Tỉ lệ kéo skin cao <paramref name="height"/> px về gần <see cref="StandardHeight"/>.</summary>
        public static float NormalizedScale(float height) =>
            height <= 0f ? 1f : Mathf.Clamp(StandardHeight / height, MinScale, MaxScale);

        public static CharacterSkinView Create(Transform parent, string path, RemoteAssetCache assets)
        {
            var go = new GameObject("Equipped character skin", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<CharacterSkinView>();
            view._renderer = go.GetComponent<SpriteRenderer>();
            assets?.Get(path, ImagePackets.TypeIcon, view, texture =>
            {
                // Cache gọi NGAY với ảnh chờ 1x1 khi chưa có ảnh thật: coi đó là skin đã tải thì
                // skin cao 1px → tên/danh hiệu rơi xuống chân cho tới khi ảnh thật về.
                if (view == null || texture == null || texture == assets.Placeholder) return;
                view._frames = Slice(path, texture);
                view._renderer.sprite = view._frames[0];
                // Pivot ở đáy nên scale quanh chân — nhân vật vẫn đứng đúng chỗ.
                view.transform.localScale = Vector3.one * NormalizedScale(view._frames[0].rect.height);
                view.Loaded?.Invoke();
            });
            return view;
        }

        public void SetFacing(bool left)
        {
            if (_renderer != null) _renderer.flipX = left;
        }

        public void SetMoving(bool moving)
        {
            _moving = moving;
            if (!moving && _frames.Length > 0)
            {
                _frame = 0;
                _renderer.sprite = _frames[0];
            }
        }

        public void SetSortingOrder(int order)
        {
            if (_renderer != null) _renderer.sortingOrder = order;
        }

        private void Update()
        {
            if (!_moving || _frames.Length < 2 || Time.time < _nextFrame) return;
            _nextFrame = Time.time + 0.16f;
            _frame = (_frame + 1) % _frames.Length;
            _renderer.sprite = _frames[_frame];
        }

        private static Sprite[] Slice(string path, Texture2D texture)
        {
            // Skin templates do not send frameCount; legacy anim_characters sheets are two
            // equal horizontal movement frames. Odd-width/small assets safely fall back to one.
            var count = texture.width >= 2 && texture.width % 2 == 0 ? 2 : 1;
            return SpriteFrameCache.Slice(path, texture, count);
        }
    }
}
