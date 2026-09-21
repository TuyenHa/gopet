using System;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed class CharacterWingView : MonoBehaviour
    {
        private const int FrameCount = 2;
        private const float CharacterHeight = 74f;
        private const float TicksPerSecond = 30f;
        private const int TicksPerFrame = 8;
        private SpriteRenderer _left;
        private SpriteRenderer _right;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private int _frame = -1;
        private int _phase;

        public static CharacterWingView Create(Transform parent, string path, int verticalOffset,
            RemoteAssetCache assets)
        {
            var go = new GameObject("Equipped wing");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<CharacterWingView>();
            view._phase = UnityEngine.Random.Range(0, 1000);
            view._left = CreateHalf(go.transform, "Left wing", false);
            view._right = CreateHalf(go.transform, "Right wing", true);
            assets?.Get(path, ImagePackets.TypeIcon, texture =>
            {
                if (view == null || texture == null) return;
                view.ApplyTexture(path, texture, verticalOffset);
            });
            return view;
        }

        public void SetSortingOrder(int order)
        {
            if (_left != null) _left.sortingOrder = order;
            if (_right != null) _right.sortingOrder = order;
        }

        private static SpriteRenderer CreateHalf(Transform parent, string name, bool mirrored)
        {
            var child = new GameObject(name, typeof(SpriteRenderer));
            child.transform.SetParent(parent, false);
            var renderer = child.GetComponent<SpriteRenderer>();
            renderer.flipX = mirrored;
            return renderer;
        }

        private void ApplyTexture(string path, Texture2D texture, int verticalOffset)
        {
            _frames = Slice(path, texture);
            // ee.java: TOP_LEFT tại playerY - 74 + offset. Đổi sang trục Y của
            // Unity và sprite có pivot ở đáy => 74 - offset - chiều cao ảnh.
            var bottomY = CharacterHeight - verticalOffset - texture.height;
            var halfWidth = texture.width / (float)FrameCount;
            _left.transform.localPosition = new Vector3(-halfWidth * 0.5f, bottomY, 0f);
            _right.transform.localPosition = new Vector3(halfWidth * 0.5f, bottomY, 0f);
            SetFrame(0);
        }

        private void Update()
        {
            if (_frames.Length < FrameCount) return;
            // ee.java: (abs(BaseCanvas.ticks + q) >> 3) % 2.
            var ticks = Mathf.FloorToInt(Time.time * TicksPerSecond) + _phase;
            SetFrame(Mathf.Abs(ticks) / TicksPerFrame % FrameCount);
        }

        private void SetFrame(int frame)
        {
            if (_frames.Length == 0 || frame == _frame) return;
            _frame = frame;
            _left.sprite = _frames[frame];
            _right.sprite = _frames[frame];
        }

        private static Sprite[] Slice(string path, Texture2D texture)
        {
            // JAR dùng phép chia nguyên getWidth() >> 1, kể cả khi chiều rộng lẻ.
            var count = texture.width >= FrameCount ? FrameCount : 1;
            return SpriteFrameCache.Slice(path, texture, count);
        }
    }
}
