using System;
using System.Collections.Generic;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Full-body two-frame skin used by the legacy anim_characters assets.</summary>
    public sealed class CharacterSkinView : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private bool _moving;
        private float _nextFrame;
        private int _frame;

        public static CharacterSkinView Create(Transform parent, string path, RemoteAssetCache assets)
        {
            var go = new GameObject("Equipped character skin", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<CharacterSkinView>();
            view._renderer = go.GetComponent<SpriteRenderer>();
            assets?.Get(path, ImagePackets.TypeIcon, texture =>
            {
                if (view == null || texture == null) return;
                view._frames = Slice(path, texture);
                view._renderer.sprite = view._frames[0];
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
            var key = $"{path}|{texture.GetHashCode()}";
            if (Cache.TryGetValue(key, out var cached)) return cached;
            // Skin templates do not send frameCount; legacy anim_characters sheets are two
            // equal horizontal movement frames. Odd-width/small assets safely fall back to one.
            var count = texture.width >= 2 && texture.width % 2 == 0 ? 2 : 1;
            var width = texture.width / count;
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
                frames[i] = Sprite.Create(texture, new Rect(i * width, 0f, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            return Cache[key] = frames;
        }
    }
}
