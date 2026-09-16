using System.Text;

namespace Gopet.Net.Guider
{
    /// <summary>
    /// Chuẩn hoá các tag văn bản server nhét vào tên pet / dialog (kế thừa J2ME):
    /// <c>(sao)</c> = sao vàng (đã đạt), <c>(saoden)</c> = sao trống. Client J2ME
    /// gốc thay bằng icon inline; Unity render UGUI hiển thị thô nếu không xử lý.
    ///
    /// <para>Thay bằng ký tự Unicode ★ / ☆ giữ được thông tin số sao mà không lộ
    /// mã tag ra người chơi. Áp ở lớp parse (MenuItemInfo, NpcOptions...) để mọi
    /// consumer xuống dưới đều nhận text sạch — DRY.</para>
    /// </summary>
    public static class GameTextTags
    {
        private const string TagFilledStar = "(sao)";
        private const string TagEmptyStar = "(saoden)";
        private const char GlyphFilledStar = '★';
        private const char GlyphEmptyStar = '☆';

        /// <summary>Trả về text đã thay tag. <c>null</c>/rỗng/không có tag → trả nguyên tham chiếu.</summary>
        public static string Substitute(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.IndexOf('(') < 0) return text;                       // fast path
            if (text.IndexOf(TagFilledStar) < 0 && text.IndexOf(TagEmptyStar) < 0) return text;

            var sb = new StringBuilder(text.Length);
            var i = 0;
            while (i < text.Length)
            {
                if (Matches(text, i, TagFilledStar)) { sb.Append(GlyphFilledStar); i += TagFilledStar.Length; }
                else if (Matches(text, i, TagEmptyStar)) { sb.Append(GlyphEmptyStar); i += TagEmptyStar.Length; }
                else { sb.Append(text[i]); i++; }
            }
            return sb.ToString();
        }

        private static bool Matches(string text, int at, string tag)
        {
            if (at + tag.Length > text.Length) return false;
            for (var k = 0; k < tag.Length; k++)
            {
                if (text[at + k] != tag[k]) return false;
            }
            return true;
        }
    }
}
