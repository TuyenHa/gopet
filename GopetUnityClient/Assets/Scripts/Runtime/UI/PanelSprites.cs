using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>Sprite bo góc vẽ bằng code cho khung menu.
    ///
    /// <para>Vẽ thay vì sinh ảnh AI vì khung cần <b>viền dày đều tuyệt đối</b> ở cả bốn cạnh
    /// mới 9-slice được. Ảnh sinh ra luôn lệch vài pixel và hay kèm quầng sáng, phóng to là
    /// lộ. Hàm này cho viền sắc nét ở mọi kích thước.</para></summary>
    public static class PanelSprites
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Hình chữ nhật bo góc, viền trong <paramref name="borderPx"/> pixel.
        /// Tô trắng — đổi màu bằng <c>Image.color</c>; viền lấy màu qua kênh alpha riêng nên
        /// gọi hai lần (một nền, một viền) nếu cần hai màu khác nhau.</summary>
        public static Sprite Rounded(int radius = 10, int borderPx = 0)
        {
            var key = $"r{radius}b{borderPx}";
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var size = radius * 2 + 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var px = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    px[y * size + x] = Pixel(x, y, size, radius, borderPx);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f,
                100f, 0, SpriteMeshType.FullRect,
                new Vector4(radius + 1, radius + 1, radius + 1, radius + 1));
            Cache[key] = sprite;
            return sprite;
        }

        private static Color32 Pixel(int x, int y, int size, int radius, int borderPx)
        {
            // Khoảng cách tới mép theo từng trục, chỉ khác 0 khi nằm trong vùng bo góc.
            var cx = Mathf.Max(0, Mathf.Max(radius - x, x - (size - 1 - radius)));
            var cy = Mathf.Max(0, Mathf.Max(radius - y, y - (size - 1 - radius)));
            var dist = Mathf.Sqrt(cx * cx + cy * cy);
            var inside = Mathf.Clamp01(radius + 0.5f - dist);
            if (inside <= 0f) return new Color32(255, 255, 255, 0);

            if (borderPx <= 0) return new Color32(255, 255, 255, (byte)(inside * 255));

            // Viền = vành ngoài borderPx pixel. Phần lõi để alpha thấp cho Image.color tô nền,
            // phần viền alpha đầy.
            var edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
            var onBorder = edge < borderPx || dist > radius - borderPx;
            return new Color32(255, 255, 255, (byte)((onBorder ? 1f : 0f) * inside * 255));
        }
    }
}
