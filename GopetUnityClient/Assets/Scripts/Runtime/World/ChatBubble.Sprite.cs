using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class ChatBubble
    {
        private static Sprite BubbleSprite(int width, int boxHeight)
        {
            var key = ((long)width << 32) | (uint)boxHeight;
            if (SpriteCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var logicalHeight = boxHeight + TailHeight;
            var texture = new Texture2D(width * Supersampling, logicalHeight * Supersampling,
                TextureFormat.RGBA32, false)
            {
                name = $"Chat Bubble {width}x{logicalHeight}",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[texture.width * texture.height];
            for (var py = 0; py < texture.height; py++)
            {
                for (var px = 0; px < texture.width; px++)
                {
                    var x = (px + 0.5f) / Supersampling;
                    var y = (py + 0.5f) / Supersampling;
                    var outer = InRoundedRect(x, y, 0.5f, TailHeight, width - 0.5f,
                                    logicalHeight - 0.5f, 6f) ||
                                InTail(x, y, width * 0.5f, 4.5f, 0.3f, TailHeight + 1.5f);
                    var inner = InRoundedRect(x, y, 1.5f, TailHeight + 1f, width - 1.5f,
                                    logicalHeight - 1.5f, 5f) ||
                                InTail(x, y, width * 0.5f, 3.2f, 1f, TailHeight + 1f);
                    pixels[py * texture.width + px] = inner
                        ? FillColor
                        : outer ? OutlineColor : new Color32(0, 0, 0, 0);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), Supersampling, 0, SpriteMeshType.FullRect);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            SpriteCache[key] = sprite;
            return sprite;
        }

        private static bool InRoundedRect(float x, float y, float left, float bottom,
            float right, float top, float radius)
        {
            if (x < left || x > right || y < bottom || y > top) return false;
            var centerX = Mathf.Clamp(x, left + radius, right - radius);
            var centerY = Mathf.Clamp(y, bottom + radius, top - radius);
            var dx = x - centerX;
            var dy = y - centerY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool InTail(float x, float y, float centerX, float halfBase,
            float tipY, float topY)
        {
            if (y < tipY || y > topY) return false;
            var progress = (y - tipY) / (topY - tipY);
            return Mathf.Abs(x - centerX) <= Mathf.Lerp(0.6f, halfBase, progress);
        }
    }
}
