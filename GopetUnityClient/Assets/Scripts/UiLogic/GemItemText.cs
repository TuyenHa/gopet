using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Tách tên ngọc thô của server thành tên và dòng hiệu ứng cho thẻ trong popup Kho ngọc.
    ///
    /// <para>Server ghép tất cả vào một chuỗi, hiệu ứng dùng tag J2ME thay icon:</para>
    /// <code>"ngọc lửa  up: 0 Tăng 5% (hp) Tăng 5% (mp) Tăng 5% (atk)"</code>
    /// <para>→ tên "Ngọc lửa", hiệu ứng "Up 0 · Tăng 5% HP · Tăng 5% MP · Tăng 5% ATK".
    /// Chuỗi không có "up:" thì trả nguyên tên (đã bỏ tag), không có hiệu ứng.</para>
    /// </summary>
    public sealed class GemItemText
    {
        private const string UpMarker = "up:";
        private const string Separator = "  ·  ";
        private static readonly Regex Effect = new Regex(@"([^()]*?)\((\w+)\)", RegexOptions.Compiled);
        private static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.Compiled);

        public string Name = string.Empty;
        public string Effects = string.Empty;

        public static GemItemText Parse(string raw)
        {
            var result = new GemItemText();
            raw = raw ?? string.Empty;
            var up = raw.IndexOf(UpMarker, System.StringComparison.OrdinalIgnoreCase);
            if (up < 0)
            {
                result.Name = Capitalize(Clean(JarIconTokens.Strip(raw)));
                return result;
            }

            result.Name = Capitalize(Clean(JarIconTokens.Strip(raw.Substring(0, up))));
            var rest = raw.Substring(up + UpMarker.Length).TrimStart();
            var digits = 0;
            while (digits < rest.Length && char.IsDigit(rest[digits])) digits++;

            var parts = new List<string>();
            if (digits > 0) parts.Add("Up " + rest.Substring(0, digits));
            rest = rest.Substring(digits);

            var consumed = 0;
            foreach (Match match in Effect.Matches(rest))
            {
                var stat = StatLabel(match.Groups[2].Value);
                var label = Clean(match.Groups[1].Value);
                var piece = stat == null ? label : Clean(label + " " + stat);
                if (piece.Length > 0) parts.Add(piece);
                consumed = match.Index + match.Length;
            }
            var tail = Clean(rest.Substring(consumed));
            if (tail.Length > 0) parts.Add(tail);

            result.Effects = string.Join(Separator, parts);
            return result;
        }

        /// <summary>Tag chỉ số → nhãn chữ. Tag khác (icon trang trí) trả null để bỏ đi.</summary>
        private static string StatLabel(string tag)
        {
            switch (tag)
            {
                case "hp": return "HP";
                case "mp": return "MP";
                case "atk": return "ATK";
                case "def": return "DEF";
                case "str": return "STR";
                case "agi": return "AGI";
                case "int": return "INT";
                default: return null;
            }
        }

        private static string Clean(string text) => Spaces.Replace(text ?? string.Empty, " ").Trim();

        private static string Capitalize(string text) =>
            string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0]) + text.Substring(1);
    }
}
