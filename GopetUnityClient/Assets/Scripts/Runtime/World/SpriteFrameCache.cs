using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Frames are shared by texture identity and count. The texture owner
    /// releases them when the texture is no longer used, never while replacing a path.</summary>
    public static class SpriteFrameCache
    {
        private static readonly Dictionary<Texture2D, Dictionary<int, Sprite[]>> Cache =
            new Dictionary<Texture2D, Dictionary<int, Sprite[]>>();

        public static Sprite[] Slice(string path, Texture2D texture, int count)
        {
            if (texture == null) return Array.Empty<Sprite>();
            if (count <= 0 || texture.width < count) count = 1;
            if (!Cache.TryGetValue(texture, out var slices))
            {
                slices = new Dictionary<int, Sprite[]>();
                Cache.Add(texture, slices);
            }
            if (slices.TryGetValue(count, out var cached))
            {
                var usable = true;
                foreach (var frame in cached)
                    if (frame == null) { usable = false; break; }
                if (usable) return cached;
                DestroyFrames(cached);
            }
            var width = texture.width / count;
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
                frames[i] = Sprite.Create(texture, new Rect(i * width, 0f, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            return slices[count] = frames;
        }

        public static void Release(Texture2D texture)
        {
            if (ReferenceEquals(texture, null) || !Cache.TryGetValue(texture, out var slices)) return;
            foreach (var frames in slices.Values) DestroyFrames(frames);
            Cache.Remove(texture);
        }

        /// <summary>Called when the owning game scene is torn down.</summary>
        public static void Clear()
        {
            foreach (var slices in Cache.Values)
                foreach (var frames in slices.Values) DestroyFrames(frames);
            Cache.Clear();
        }

        private static void DestroyFrames(Sprite[] frames)
        {
            foreach (var frame in frames)
                if (frame != null) UnityEngine.Object.Destroy(frame);
        }
    }
}
