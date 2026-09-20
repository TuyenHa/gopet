using System;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Tra ảnh NPC/quái đã unpack sẵn từ jar dưới <c>Resources/Jar/Art/Raw/</c>, theo
    /// ĐÚNG đường dẫn server gửi (vd <c>npcs/mgo.png</c>, cột <c>imgPath</c> bảng
    /// <c>npc</c>). Dùng cho <see cref="WorldActorView"/> để hiện ảnh ngay tại chỗ thay
    /// vì luôn chờ round-trip <c>RemoteAssetCache</c> — vốn là đường duy nhất trước đây
    /// và phụ thuộc server phục vụ đúng file trong thư mục <c>assets/</c> của nó.
    /// </summary>
    public static class JarActorSprites
    {
        private const string LocalRoot = "Jar/Art/Raw/";

        /// <summary>
        /// Trả texture cục bộ nếu đã unpack, hoặc <c>null</c> nếu chưa — KHÔNG ném lỗi
        /// như <c>JarSkin</c>, vì thiếu bản cục bộ là tình huống hợp lệ (rơi về mạng).
        /// Dữ liệu cũ có dòng dùng backslash, thậm chí gạch đôi (<c>npcs\\ten.png</c>) —
        /// chuẩn hoá qua <see cref="JarAssetPath"/> trước khi tra.
        /// </summary>
        public static Texture2D LoadLocalTexture(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;
            var normalized = JarAssetPath.Normalize(imagePath);
            if (normalized.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(0, normalized.Length - 4);
            var sprite = Resources.Load<Sprite>(LocalRoot + normalized);
            return sprite != null ? sprite.texture : null;
        }
    }
}
