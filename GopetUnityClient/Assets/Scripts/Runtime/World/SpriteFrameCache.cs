using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Xẻ một texture strip ngang thành các frame sprite, có cache dùng chung.
    ///
    /// <para>Mọi actor trong world đều xẻ ảnh theo cùng một quy ước của jar: các frame nằm
    /// cạnh nhau theo chiều ngang, rộng bằng nhau, gốc sprite đặt ở CHÂN (pivot 0.5, 0) và
    /// 1 pixel nguồn = 1 world unit. Trước đây NPC, quái, pet, nhân vật, trang phục và cánh
    /// mỗi bên chép một bản y hệt kèm từ điển cache riêng — sáu chỗ cùng một lỗi, sửa một
    /// chỗ là năm chỗ kia vẫn hỏng. Gom về đây.</para>
    /// </summary>
    public static class SpriteFrameCache
    {
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

        /// <param name="path">Đường dẫn ảnh, chỉ dùng làm khoá cache.</param>
        /// <param name="count">Số frame mong muốn; ép về 1 nếu không hợp lệ hoặc ảnh quá hẹp.</param>
        public static Sprite[] Slice(string path, Texture2D texture, int count)
        {
            if (texture == null) return Array.Empty<Sprite>();
            // Ép TRƯỚC khi dựng khoá: ép sau thì hai count khác nhau cùng ra một kết quả lại
            // nằm ở hai khoá, cache phình mà chẳng trúng lần nào.
            if (count <= 0 || texture.width < count) count = 1;

            var key = $"{path}|{count}";
            if (Cache.TryGetValue(key, out var cached) && IsUsable(cached, texture)) return cached;

            var width = texture.width / count;
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
                frames[i] = Sprite.Create(texture, new Rect(i * width, 0f, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            return Cache[key] = frames;
        }

        /// <summary>
        /// Entry cache còn dùng được hay không.
        ///
        /// <para><b>Phải kiểm từng PHẦN TỬ, không chỉ cái mảng.</b> Cache là static nên sống
        /// qua mọi lần đổi map trong cùng một domain. Sprite do <c>Sprite.Create</c> sinh ra
        /// không thuộc asset nào; rời map là actor bị huỷ, không còn ai tham chiếu, và
        /// <c>Resources.UnloadUnusedAssets</c> (Unity tự gọi khi nạp scene) huỷ luôn cả sprite
        /// lẫn texture gốc. Mảng vẫn khác null nhưng phần tử thành "destroyed" — gán vào
        /// <c>SpriteRenderer</c> cho ra sprite null, actor mất ảnh TRONG IM LẶNG: không
        /// exception, không log, không request mạng. Đúng triệu chứng "vào map lần đầu thì
        /// thấy NPC, đi map khác rồi quay lại thì mất".</para>
        ///
        /// <para>Kiểm thêm <c>frame.texture != texture</c>: sau khi unload,
        /// <c>Resources.Load</c> trả về INSTANCE texture mới, nên entry cũ dù còn sống cũng
        /// đang trỏ vào texture đã chết. So sánh ở đây cho phép khoá cache chỉ gồm path+count —
        /// entry cũ bị GHI ĐÈ thay vì đọng lại mãi dưới một khoá khác.</para>
        /// </summary>
        private static bool IsUsable(Sprite[] frames, Texture2D texture)
        {
            if (frames == null || frames.Length == 0) return false;
            foreach (var frame in frames)
                if (frame == null || frame.texture != texture) return false;
            return true;
        }
    }
}
