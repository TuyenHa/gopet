using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>Làm đẹp tên NPC trước khi hiện lên map.
    ///
    /// <para>Dữ liệu gốc lưu tên NPC ở dạng <b>HOA TOÀN BỘ, không dấu</b> — <c>'BANH SINH NHAT'</c>,
    /// <c>'GIAN THUONG'</c>. Đó là do bitmap font của jar chỉ có <c>Đ Ă Á Â</c> trong bảng chữ
    /// hoa, nên tên nào cũng phải bỏ dấu và viết hoa mới hiện đủ chữ. Client giờ dùng font TTF
    /// nên ràng buộc đó không còn, chỉ còn lại kiểu chữ trông như đang hét.</para>
    ///
    /// <para>Thuần C#, không phụ thuộc UnityEngine, để test được ở <c>Gopet.Net.Tests</c>.</para></summary>
    public static class NpcDisplayNames
    {
        /// <summary>Đổi tên HOA TOÀN BỘ thành kiểu chữ hoa đầu mỗi từ.
        ///
        /// <para>Tên đã có sẵn chữ thường thì GIỮ NGUYÊN: đó là tên người đặt (người chơi, pet)
        /// hoặc dữ liệu đã được sửa tay, đụng vào là làm hỏng.</para>
        ///
        /// <para>Không khôi phục dấu tiếng Việt — muốn có dấu thì phải sửa thẳng dữ liệu, đoán
        /// dấu từ chữ không dấu là trò may rủi (<c>'GIAN THUONG'</c> ra "gian thương" hay
        /// "giàn thướng"?).</para></summary>
        public static string Prettify(string name)
        {
            if (string.IsNullOrEmpty(name) || HasLowercase(name)) return name;

            var sb = new StringBuilder(name.Length);
            var startOfWord = true;
            foreach (var c in name)
            {
                sb.Append(startOfWord ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                // Chữ và số nối liền là cùng một từ; mọi thứ khác (cách, gạch, dấu chấm)
                // mở từ mới — nhờ vậy "LV.3" không thành "Lv.3" sai chỗ.
                startOfWord = !char.IsLetterOrDigit(c);
            }
            return sb.ToString();
        }

        private static bool HasLowercase(string name)
        {
            foreach (var c in name) if (char.IsLower(c)) return true;
            return false;
        }
    }
}
