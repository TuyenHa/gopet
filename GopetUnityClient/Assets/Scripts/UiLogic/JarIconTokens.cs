using System;
using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Server nhúng icon glyph bằng token dạng <c>(saoden)</c>, <c>(vang)</c>, <c>(ngoc)</c>…
    /// vào text (bảng 30 loại trong jar <c>dj.java:54</c>). Jar client tự replace token thành
    /// glyph icon từ <c>pet/icons.png</c>. Client Unity chưa có icon atlas — nếu không strip,
    /// user thấy raw như <c>"Rua Test (saoden)(saoden)(saoden)"</c>.
    ///
    /// <para>Hai lựa chọn:
    /// <list type="number">
    ///   <item>Render icon thật — cần atlas + inline sprite trong text (đòi hỏi Text Mesh Pro).</item>
    ///   <item>Strip hết — nhanh, hợp cho parity phase này.</item>
    /// </list>
    /// Class này làm option 2. Apply ở mọi chỗ hiển thị text server có thể chứa token.</para>
    /// </summary>
    public static class JarIconTokens
    {
        // 30 token từ dj.java:54 — order giữ nguyên để tra nhanh nếu cần render icon sau này.
        private static readonly string[] KnownTokens =
        {
            "(ngoc)", "(dau)", "(thoc)", "(vang)", "(str)", "(agi)", "(int)", "(atk)",
            "(def)", "(hp)", "(mp)", "(water)", "(thunder)", "(rock)", "(fire)", "(dark)",
            "(tree)", "(light)", "(sao)", "(chien)", "(bthu)", "(codo)", "(coxanh)", "(nha)",
            "(nguoi)", "(saoden)", "(chienluc)", "(nluong)", "(diem)", "(lua)",
        };

        /// <summary>Strip mọi token icon trong <paramref name="text"/>. Không thay bằng gì —
        /// chuỗi rút ngắn. Trim khoảng trắng thừa ở cuối.</summary>
        public static string Strip(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.IndexOf('(') < 0) return text; // fast-path: không có token nào

            var sb = new StringBuilder(text.Length);
            var i = 0;
            while (i < text.Length)
            {
                if (text[i] == '(' && TryMatchToken(text, i, out var length))
                {
                    i += length;
                    continue;
                }
                sb.Append(text[i]);
                i++;
            }
            return sb.ToString().TrimEnd();
        }

        private static bool TryMatchToken(string text, int start, out int length)
        {
            foreach (var tok in KnownTokens)
            {
                if (start + tok.Length <= text.Length &&
                    string.CompareOrdinal(text, start, tok, 0, tok.Length) == 0)
                {
                    length = tok.Length;
                    return true;
                }
            }
            length = 0;
            return false;
        }
    }
}
