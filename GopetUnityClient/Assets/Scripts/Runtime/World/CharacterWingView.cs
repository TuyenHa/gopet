using System;
using System.Collections.Generic;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed class CharacterWingView : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private int _frame;
        private float _nextFrame;

        public static CharacterWingView Create(Transform parent, string path, int frameCount,
            RemoteAssetCache assets)
        {
            var go = new GameObject("Equipped wing", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<CharacterWingView>();
            view._renderer = go.GetComponent<SpriteRenderer>();
            assets?.Get(path, ImagePackets.TypeIcon, texture =>
            {
                if (view == null || texture == null) return;
                view._frames = Slice(path, texture, frameCount);
                view._renderer.sprite = view._frames[0];
            });
            return view;
        }

        public void SetFacing(bool left)
        {
            if (_renderer != null) _renderer.flipX = left;
        }

        public void SetSortingOrder(int order)
        {
            if (_renderer != null) _renderer.sortingOrder = order;
        }

        private void Update()
        {
            if (_frames.Length < 2 || Time.time < _nextFrame) return;
            _nextFrame = Time.time + 0.2f;
            _frame = (_frame + 1) % _frames.Length;
            _renderer.sprite = _frames[_frame];
        }

        private static Sprite[] Slice(string path, Texture2D texture, int count)
        {
            count = Mathf.Clamp(count, 1, 64);
            if (texture.width < count) count = 1;
            var key = $"{path}|{texture.GetHashCode()}|{count}";
            if (Cache.TryGetValue(key, out var cached)) return cached;
            var width = texture.width / count;
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
                frames[i] = Sprite.Create(texture, new Rect(i * width, 0f, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            return Cache[key] = frames;
        }
    }
}
