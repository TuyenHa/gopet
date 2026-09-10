using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private int _frame;
        private float _nextFrame;

        public static CharacterAnimationView Create(Transform avatar, CharacterAnimation animation,
            RemoteAssetCache assets, int index)
        {
            var go = new GameObject($"Character animation {animation.Type}:{index}", typeof(SpriteRenderer));
            go.transform.SetParent(avatar, false);
            go.transform.localPosition = new Vector3(animation.OffsetX, -animation.OffsetY, 0f);
            var view = go.AddComponent<CharacterAnimationView>();
            view._renderer = go.GetComponent<SpriteRenderer>();
            view._renderer.sortingOrder = animation.DrawAtEnd ? 32010 : 31990;
            assets?.Get(animation.FrameImagePath, ImagePackets.TypeIcon, texture =>
            {
                if (view == null || texture == null) return;
                view._frames = Slice(animation.FrameImagePath, texture, animation.FrameCount);
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

        private static Sprite[] Slice(string path, Texture2D texture, int count)
        {
            var key = $"{path}|{texture.GetHashCode()}|{count}";
            if (Cache.TryGetValue(key, out var cached)) return cached;
            if (count <= 0 || texture.width < count) count = 1;
            var width = texture.width / count;
            var sprites = new Sprite[count];
            for (var i = 0; i < count; i++)
                sprites[i] = Sprite.Create(texture, new Rect(i * width, 0f, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            return Cache[key] = sprites;
        }
    }
}
