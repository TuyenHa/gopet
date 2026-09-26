using System.Text;
using System.Text.RegularExpressions;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Tô màu tiến độ nhiệm vụ: server gửi mỗi yêu cầu một hàng dạng
    /// "Tiêu diệt khủng long 3/10 tại Đấu trường" (<c>TaskCalculator.getTaskLines</c>);
    /// hàng nào đã đạt (đã làm ≥ cần) bọc thẻ màu rich text.
    ///
    /// <para>Thuần C# để test ngoài Unity. Chữ đến từ dữ liệu server (tên quái/map) nên
    /// '&lt;' '&gt;' bị thay trước khi ghép thẻ — Text cũ của Unity không có cách escape.</para>
    /// </summary>
    public static class TaskProgressText
    {
        /// <summary>Xanh lá, đủ sáng trên nền tối của HUD lẫn nền trắng của popup.</summary>
        public const string DoneColorHex = "#3FCB57";

        private static readonly Regex Progress = new Regex(@"(\d+)\s*/\s*(\d+)");

        /// <summary>Hàng có tiến độ "x/y" với x ≥ y (và y &gt; 0).</summary>
        public static bool IsLineDone(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;
            var match = Progress.Match(line);
            return match.Success
                   && long.TryParse(match.Groups[1].Value, out var done)
                   && long.TryParse(match.Groups[2].Value, out var need)
                   && need > 0 && done >= need;
        }

        /// <summary>Trả chuỗi rich text: hàng đạt bọc &lt;color&gt;, hàng khác giữ nguyên màu.</summary>
        public static string Colorize(string text, string doneColorHex = DoneColorHex)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var lines = text.Split('\n');
            var sb = new StringBuilder(text.Length + 32);
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0) sb.Append('\n');
                var safe = lines[i].Replace('<', '‹').Replace('>', '›');
                if (IsLineDone(lines[i]))
                    sb.Append("<color=").Append(doneColorHex).Append('>').Append(safe).Append("</color>");
                else
                    sb.Append(safe);
            }
            return sb.ToString();
        }
    }
}
