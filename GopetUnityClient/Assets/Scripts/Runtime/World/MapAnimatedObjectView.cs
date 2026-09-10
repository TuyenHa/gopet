using System.Collections.Generic;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Vẽ object map từ atlas `_a.png` + metadata `_b`, tương ứng <c>y/dy/dz.java</c>.</summary>
    public sealed class MapAnimatedObjectView : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite> RegionCache = new Dictionary<string, Sprite>();
        private readonly List<SpriteRenderer> _parts = new List<SpriteRenderer>();
        private JarMapAnimation _animation;
        private Texture2D _texture;
        private JarMapAnimation.Clip _clip;
        private int _sortingOrder;
        private int _sequenceIndex;
        private float _nextFrameAt;

        public static MapAnimatedObjectView Create(Transform parent, int resourceId, JarMapObject item,
            int mapHeightPixels, int sortingOrder)
        {
            var metadata = Resources.Load<TextAsset>($"Jar/MapAnimations/{resourceId}");
            if (metadata == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu metadata hoạt ảnh map {resourceId}_b.");
                return null;
            }

            var go = new GameObject($"Animated Object {resourceId}");
            go.transform.SetParent(parent, false);
            var footY = item.Y - item.YOffset;
            var (x, y) = MapPlacement.JarToWorld(item.X, footY, mapHeightPixels);
            go.transform.localPosition = new Vector3(x, y, 0f);

            var view = go.AddComponent<MapAnimatedObjectView>();
            view._animation = JarMapAnimation.Parse(metadata.bytes);
            view._texture = JarSkin.Raw($"newMapData/{resourceId}_a").texture;
            view._sortingOrder = sortingOrder;
            var clipIndex = item.AnimationIndex;
            if (clipIndex < 0 || clipIndex >= view._animation.Clips.Length) clipIndex = 0;
            if (view._animation.Clips.Length == 0) return view;
            view._clip = view._animation.Clips[clipIndex];
            view.ShowCurrentFrame();
            return view;
        }

        private void Update()
        {
            if (_clip == null || _clip.FrameIndices.Length <= 1 || Time.time < _nextFrameAt) return;
            _sequenceIndex = (_sequenceIndex + 1) % _clip.FrameIndices.Length;
            ShowCurrentFrame();
        }

        private void ShowCurrentFrame()
        {
            if (_clip.FrameIndices.Length == 0) return;
            var frameIndex = _clip.FrameIndices[_sequenceIndex];
            if (frameIndex < 0 || frameIndex >= _animation.Frames.Length) return;
            var frame = _animation.Frames[frameIndex];
            EnsurePartCount(frame.Parts.Length);

            for (var i = 0; i < _parts.Count; i++)
            {
                var renderer = _parts[i];
                renderer.gameObject.SetActive(i < frame.Parts.Length);
                if (i >= frame.Parts.Length) continue;
                var part = frame.Parts[i];
                if (part.RegionIndex < 0 || part.RegionIndex >= _animation.Regions.Length)
                {
                    renderer.gameObject.SetActive(false);
                    continue;
                }

                var region = _animation.Regions[part.RegionIndex];
                renderer.sprite = RegionSprite(region, part.RegionIndex);
                renderer.sortingOrder = _sortingOrder + i;
                ApplyTransform(renderer.transform, part, region);
            }

            var duration = _clip.DurationsMs[_sequenceIndex];
            _nextFrameAt = Time.time + Mathf.Max(16, duration) / 1000f;
        }

        private void EnsurePartCount(int count)
        {
            while (_parts.Count < count)
            {
                var child = new GameObject($"Part {_parts.Count}", typeof(SpriteRenderer));
                child.transform.SetParent(transform, false);
                _parts.Add(child.GetComponent<SpriteRenderer>());
            }
        }

        private Sprite RegionSprite(JarMapAnimation.Region region, int index)
        {
            var key = $"{_texture.GetHashCode()}|{index}";
            if (RegionCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var rect = new Rect(region.X, _texture.height - region.Y - region.Height,
                region.Width, region.Height);
            return RegionCache[key] = Sprite.Create(_texture, rect, new Vector2(0.5f, 0.5f), 1f);
        }

        private static void ApplyTransform(Transform target, JarMapAnimation.Part part,
            JarMapAnimation.Region region)
        {
            var swapsAxes = part.Transform >= 4;
            var width = swapsAxes ? region.Height : region.Width;
            var height = swapsAxes ? region.Width : region.Height;
            target.localPosition = new Vector3(part.X + width * 0.5f,
                -part.Y - height * 0.5f, 0f);

            var mirror = part.Transform == 1 || part.Transform == 2 ||
                         part.Transform == 4 || part.Transform == 7;
            var angle = part.Transform switch
            {
                1 => 180f,
                3 => 180f,
                4 => 90f,
                5 => -90f,
                6 => 90f,
                7 => -90f,
                _ => 0f
            };
            target.localScale = mirror ? new Vector3(-1f, 1f, 1f) : Vector3.one;
            target.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
