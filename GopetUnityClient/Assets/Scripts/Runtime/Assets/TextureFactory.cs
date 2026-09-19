using UnityEngine;

namespace Gopet.Runtime.Assets
{
    /// <summary>
    /// Biến byte PNG thành <see cref="Texture2D"/>, với đúng thiết lập cho pixel art.
    ///
    /// <para>Tách khỏi <see cref="RemoteAssetCache"/> vì đây là phần dễ sai một cách
    /// im lặng nhất: đặt nhầm bộ lọc không gây lỗi nào, chỉ làm ảnh mờ — và mờ thì
    /// dễ bị bỏ qua cho tới khi ai đó so với client cũ.</para>
    /// </summary>
    internal static class TextureFactory
    {
        /// <summary>Trả <c>null</c> nếu byte không phải PNG hợp lệ.</summary>
        public static Texture2D Decode(byte[] png)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(png))
            {
                Object.Destroy(texture);
                return null;
            }

            Configure(texture);
            return texture;
        }

        /// <summary>Ảnh xám mờ 1×1 dùng tạm trong lúc chờ tải.</summary>
        public static Texture2D Placeholder()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Configure(texture);

            texture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.35f));
            texture.Apply();
            return texture;
        }

        /// <summary>Ảnh 24×32 màu magenta viền vàng — dùng khi retry hết vẫn không có ảnh
        /// (hoặc PNG hỏng). Khác Placeholder để user PHÂN BIỆT được "đang tải" (1×1 gray
        /// vô hình) vs "load fail" (rectangle visible). Trước đây cả 2 trạng thái đều
        /// hiện Placeholder 1×1 → NPC vô hình, chỉ thấy tên.</summary>
        public static Texture2D Failed()
        {
            const int w = 24, h = 32;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Configure(texture);
            var body = new Color(0.9f, 0.15f, 0.55f, 0.9f);   // magenta báo hiệu
            var border = new Color(1f, 0.85f, 0.1f, 1f);      // viền vàng
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var edge = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    texture.SetPixel(x, y, edge ? border : body);
                }
            texture.Apply();
            return texture;
        }

        private static void Configure(Texture2D texture)
        {
            // Asset gốc là pixel art độ phân giải thấp; lọc bilinear làm mờ nhoè.
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
