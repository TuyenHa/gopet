using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nạp sprite lấy từ client J2ME cũ, theo tên, có cache. Một chỗ duy nhất gọi
    /// <c>Resources.Load</c> cho asset của <c>tools/unpack-jar-dat</c> — cùng lý do
    /// với <c>GopetCmd.cs</c> sinh tự động: một chỗ sai còn hơn mười chỗ sai.
    ///
    /// <para><b>Vì sao dưới <c>Resources/</c>:</b> đây là asset nạp theo TÊN CHUỖI lúc
    /// runtime, không phải tham chiếu kéo-thả trong Inspector. Cách duy nhất để tên
    /// chuỗi tra ra đúng sprite trong build thật (không chỉ Editor) là đặt nó dưới một
    /// thư mục tên <c>Resources</c>. Xem <c>tools/unpack-jar-dat/README.md</c>.</para>
    ///
    /// <para>Tên sai phải NÉM, không trả <c>null</c> lặng lẽ: sprite null là một ô
    /// trống suốt màn hình, không có lỗi nào báo — cùng bài học với
    /// <c>UiBuilder.BuiltinFont</c> ở P5.</para>
    /// </summary>
    public static class JarSkin
    {
        private const string ArtRoot = "Jar/Art/";

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>Nội dung file đã sao ra mảng byte nên không chết theo asset bị dọn — khác
        /// <see cref="Cache"/>, ở đây không cần kiểm null lại.</summary>
        private static readonly Dictionary<string, byte[]> BytesCache = new Dictionary<string, byte[]>();

        /// <summary>Ảnh giải từ một trong 5 kho <c>.dat</c>. Ví dụ: <c>Bank("lg", 0)</c> — banner màn đăng nhập.</summary>
        public static Sprite Bank(string bankName, int index)
        {
            return Load($"{ArtRoot}{bankName}/{index}", $"{bankName}.dat[{index}]");
        }

        /// <summary>
        /// Ảnh PNG rời có sẵn trong jar (không qua kho <c>.dat</c>). Đường dẫn tương
        /// đối so với gốc jar, không kèm đuôi <c>.png</c>. Ví dụ: <c>Raw("meLogo")</c>,
        /// <c>Raw("pet/button/heal")</c>.
        /// </summary>
        public static Sprite Raw(string relativePathNoExtension)
        {
            return Load($"{ArtRoot}Raw/{relativePathNoExtension}", relativePathNoExtension);
        }

        /// <summary>
        /// File nhị phân rời trong jar (mô tả khung hiệu ứng, cạnh file <c>.png</c> cùng tên).
        /// Trong jar file này KHÔNG có đuôi; muốn Unity nhận là <c>TextAsset</c> thì bản chép
        /// vào <c>Resources</c> phải mang đuôi <c>.bytes</c> — đuôi đó không nằm trong khoá.
        /// </summary>
        public static byte[] RawBytes(string relativePathNoExtension)
        {
            var resourcePath = $"{ArtRoot}Raw/{relativePathNoExtension}";
            if (BytesCache.TryGetValue(resourcePath, out var cached)) return cached;

            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy dữ liệu \"{relativePathNoExtension}\" tại Resources/{resourcePath}.bytes.");
            }
            return BytesCache[resourcePath] = asset.bytes;
        }

        private static Sprite Load(string resourcePath, string label)
        {
            // Kiểm "!= null" vì asset có thể đã bị dọn (thoát Play Mode,
            // Resources.UnloadUnusedAssets) trong khi Dictionary TĨNH vẫn giữ tham chiếu.
            if (Cache.TryGetValue(resourcePath, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy sprite \"{label}\" tại Resources/{resourcePath}. " +
                    "Đã chạy `node tools/unpack-jar-dat/index.js` chưa?");
            }

            Cache[resourcePath] = sprite;
            return sprite;
        }
    }
}
