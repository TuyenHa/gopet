using System;
using System.Collections.Generic;
using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Phần quét chuỗi của <see cref="ShopItemText"/>: chọn chip nào để hiện, đọc dải
    /// chỉ số và con số yêu cầu ra khỏi khuôn chuỗi của server.
    /// </summary>
    public sealed partial class ShopItemText
    {
        /// <summary>Số chip tối đa một thẻ item chứa nổi theo bề ngang.</summary>
        private const int MaxChips = 3;

        /// <summary>
        /// Chọn chip để hiện, theo thứ tự atk → def → hp → mp.
        ///
        /// <para><b>Ưu tiên chỉ số khác 0.</b> Mũ cộng HP và MP mà vẫn cố nhét cả
        /// "Tấn công 0" lẫn "Phòng thủ 0" là thành 4 chip — quá bề ngang thẻ, chip nào
        /// cũng bị bóp lại và chữ tràn ra ngoài viền.</para>
        ///
        /// <para>Ít hơn hai chỉ số khác 0 thì bù bằng atk/def kể cả khi bằng 0: trang
        /// bị nào cũng có hai con số đó, để trống là người chơi tưởng dòng chưa tải
        /// xong. Vũ khí chỉ có atk vì thế vẫn ra "Tấn công 80-85" + "Phòng thủ 0",
        /// đúng như ảnh mẫu.</para>
        /// </summary>
        private static StatChip[] PickChips(StatChip[] all)
        {
            var picked = new List<StatChip>(MaxChips);
            foreach (var chip in all)
            {
                if (IsZero(chip.Value) || picked.Count == MaxChips) continue;
                picked.Add(chip);
            }

            for (var i = 0; i < 2 && picked.Count < 2; i++)
            {
                if (!IsZero(all[i].Value)) continue;
                picked.Insert(i, all[i]);
            }

            return picked.ToArray();
        }

        private static bool IsZero(string value) => value.Length == 0 || value == "0";

        /// <summary>
        /// Vị trí bắt đầu khối chỉ số chứa <paramref name="tag"/>, hay −1 nếu chuỗi
        /// không có tag đó.
        ///
        /// <para>Server bọc cả khối trong một cặp ngoặc, mà bản thân tag cũng nằm
        /// trong ngoặc — nên lùi từ tag về dấu '(' gần nhất TRƯỚC nó là ra dấu mở
        /// khối.</para>
        ///
        /// <para>Trừ item ngoại hình: mô tả của nó là <c>"+%s +%s +%s +%s"</c>
        /// (<c>ShopTemplateItem.getDesc</c>) — <b>không có ngoặc bao ngoài</b>, cả
        /// chuỗi chính là khối chỉ số. Không bắt trường hợp này thì nguyên đoạn
        /// <c>"+[0 (atk) -0 (atk) ] +[0 (def)…"</c> đổ thẳng ra chỗ dòng mô tả.</para>
        /// </summary>
        private static int GroupStart(string text, string tag)
        {
            var at = text.IndexOf(tag, StringComparison.Ordinal);
            if (at <= 0) return -1;

            var open = text.LastIndexOf('(', at - 1);
            return open < 0 ? 0 : open;
        }

        /// <summary>"[80 (atk) -85 (atk) ]" → "80-85"; "[0 (def) -0 (def) ]" → "0".</summary>
        private static string Range(string group, string tag)
        {
            var marker = "(" + tag + ")";
            var first = group.IndexOf(marker, StringComparison.Ordinal);
            if (first < 0) return string.Empty;

            var low = NumberBefore(group, first);
            var second = group.IndexOf(marker, first + marker.Length, StringComparison.Ordinal);
            if (second < 0) return low;

            var high = NumberBefore(group, second);
            return high.Length == 0 || high == low ? low : low + "-" + high;
        }

        private static void AppendRequirement(StringBuilder sb, string group, string tag)
        {
            var at = group.IndexOf("(" + tag + ")", StringComparison.Ordinal);
            if (at < 0) return;

            var value = NumberBefore(group, at);
            if (value.Length == 0 || value == "0") return;

            if (sb.Length > 0) sb.Append(", ");
            sb.Append(value).Append(' ').Append(tag);
        }

        /// <summary>
        /// Chuỗi chữ số đứng ngay trước vị trí <paramref name="at"/>, bỏ qua khoảng
        /// trắng chen giữa. Không xử lý dấu âm: dấu '-' duy nhất trong khối là dấu nối
        /// hai đầu dải ("80 (atk) -85 (atk)"), coi nó là dấu trừ thì ra "80--85".
        /// </summary>
        private static string NumberBefore(string text, int at)
        {
            var end = at - 1;
            while (end >= 0 && text[end] == ' ') end--;

            var start = end;
            while (start >= 0 && text[start] >= '0' && text[start] <= '9') start--;
            return start == end ? string.Empty : text.Substring(start + 1, end - start);
        }

        private static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var head = char.ToUpperInvariant(text[0]);
            return head == text[0] ? text : head + text.Substring(1);
        }
    }
}
