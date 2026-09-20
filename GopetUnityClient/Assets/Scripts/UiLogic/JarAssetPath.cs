using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Chuẩn hoá đường dẫn ảnh do server gửi (cột <c>npc.imgPath</c>, <c>gopet_pet.frameImg</c>…).
    ///
    /// <para>Dump dữ liệu gốc có vài dòng viết kiểu Windows: <c>npcs\ten.png</c>, thậm chí
    /// <c>npcs\\ten.png</c> (hai dấu, do escape lúc nhập). Chỉ đổi <c>\</c> thành <c>/</c> là
    /// chưa đủ — <c>npcs\\giaNoel.png</c> thành <c>npcs//giaNoel.png</c>, và
    /// <c>Resources.Load</c> không tra được đường dẫn có dấu gạch đôi nên NPC mất sprite,
    /// chỉ còn nhãn tên (ÔNG GIÀ NOEL và SỨ GIẢ THIÊN THẦN ở Thành Phố Linh Thú).</para>
    ///
    /// <para>Gộp luôn dấu gạch lặp và bỏ dấu gạch đầu chuỗi để một đường dẫn chỉ còn đúng
    /// một dạng — dùng chung cho cả tra ảnh cục bộ lẫn khoá cache/gói xin ảnh.</para>
    /// </summary>
    public static class JarAssetPath
    {
        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            var sb = new StringBuilder(path.Length);
            foreach (var raw in path)
            {
                var c = raw == '\\' ? '/' : raw;
                // Bỏ gạch lặp và gạch mở đầu: cả hai đều làm hỏng tra cứu Resources.
                if (c == '/' && (sb.Length == 0 || sb[sb.Length - 1] == '/')) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
