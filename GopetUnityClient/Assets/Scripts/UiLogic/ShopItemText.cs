using System;
using System.Collections.Generic;
using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Tách chuỗi thô của server thành các mẩu hiển thị được cho thẻ item trong
    /// popup cửa hàng.
    ///
    /// <para>Server ghép sẵn tên + yêu cầu chỉ số vào MỘT chuỗi, và mô tả + dải chỉ
    /// số vào một chuỗi khác (<c>ShopTemplateItem.getName/getDesc</c>):</para>
    /// <code>
    /// title = "búa gỗ(Yêu cầu   25 (str) ,  20 (agi) ,  20 (int))"
    /// desc  = "búa dành cho chiến binh( [80 (atk) -85 (atk) ] ,  [0 (def) -0 (def) ], ... )"
    /// </code>
    /// <para>Đổ nguyên si ra màn hình thì rối; tách ở đây để thẻ item có tiêu đề, mô
    /// tả và chip chỉ số riêng. <b>Không</b> bám vào từ "Yêu cầu" — chuỗi đó lấy từ
    /// <c>Language.Request</c> nên bản EN sẽ khác; chỉ bám vào các tag
    /// <c>(str)/(agi)/(int)/(atk)/(def)/(hp)/(mp)</c> mà server luôn chèn.</para>
    ///
    /// <para>Chuỗi không khớp khuôn (thức ăn, pet, item thường) trả về nguyên văn ở
    /// <see cref="Name"/>/<see cref="Description"/> và không có chip nào — hỏng khuôn
    /// thì mất trang trí chứ không mất thông tin.</para>
    /// </summary>
    public sealed partial class ShopItemText
    {
        /// <summary>Tên item, đã bỏ phần yêu cầu chỉ số.</summary>
        public string Name = string.Empty;

        /// <summary>Ví dụ "Yêu cầu 25 str, 20 agi". Rỗng khi item không đòi chỉ số nào.</summary>
        public string Requirement = string.Empty;

        /// <summary>Mô tả, đã bỏ khối dải chỉ số ở đuôi.</summary>
        public string Description = string.Empty;

        /// <summary>Chip chỉ số theo thứ tự hiển thị. Rỗng khi item không phải trang bị.</summary>
        public StatChip[] Stats = Array.Empty<StatChip>();

        /// <summary>Một chip: nhãn ("Tấn công") + giá trị ("80-85").</summary>
        public struct StatChip
        {
            public string Label;
            public string Value;
        }

        /// <summary>
        /// Nhãn giá gọn cho nút mua. Server nối đơn vị tiền vào sau con số
        /// (<c>MenuController.getMoneyText</c>): "20 (vang)", "5 (ngoc)", "3 thỏi bạc"…
        /// Hai đuôi <c>(vang)</c>/<c>(ngoc)</c> là tag kiểu J2ME — client cũ thay bằng
        /// icon, nên bỏ đi và để icon đồng tiền nói thay. Đuôi bằng chữ thường giữ
        /// nguyên vì không có icon tương ứng.
        /// </summary>
        public static string ShortMoney(string moneyText)
        {
            if (string.IsNullOrEmpty(moneyText)) return string.Empty;
            return moneyText.Replace("(vang)", string.Empty)
                            .Replace("(ngoc)", string.Empty)
                            .Trim();
        }

        public static ShopItemText Parse(string title, string description)
        {
            var result = new ShopItemText();
            result.ParseTitle(title ?? string.Empty);
            result.ParseDescription(description ?? string.Empty);
            return result;
        }

        private void ParseTitle(string title)
        {
            var open = GroupStart(title, "(str)");
            if (open < 0)
            {
                Name = Capitalize(title.Trim());
                return;
            }

            Name = Capitalize(title.Substring(0, open).Trim());

            var group = title.Substring(open);
            var sb = new StringBuilder();
            AppendRequirement(sb, group, "str");
            AppendRequirement(sb, group, "agi");
            AppendRequirement(sb, group, "int");
            if (sb.Length > 0) Requirement = "Yêu cầu " + sb;
        }

        private void ParseDescription(string description)
        {
            var open = GroupStart(description, "(atk)");
            if (open < 0)
            {
                Description = Capitalize(description.Trim());
                return;
            }

            Description = Capitalize(description.Substring(0, open).Trim());

            var group = description.Substring(open);
            Stats = PickChips(new[]
            {
                new StatChip { Label = "Tấn công", Value = Range(group, "atk") },
                new StatChip { Label = "Phòng thủ", Value = Range(group, "def") },
                new StatChip { Label = "HP", Value = Range(group, "hp") },
                new StatChip { Label = "MP", Value = Range(group, "mp") },
            });
        }
    }
}
