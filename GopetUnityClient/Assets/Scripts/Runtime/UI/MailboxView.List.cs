using System.Collections.Generic;
using System.Linq;
using Gopet.Net.Social;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>Phần danh sách thư của <see cref="MailboxView"/>.</summary>
    public sealed partial class MailboxView
    {
        /// <summary>Nền nhãn loại. Chữ trắng trên cả hai màu này đều còn đọc rõ.</summary>
        private static readonly Color AdminBadge = new Color(0.30f, 0.55f, 0.88f, 1f);
        private static readonly Color EventBadge = new Color(0.78f, 0.42f, 0.10f, 1f);

        private readonly List<PopupTextRow> _rows = new List<PopupTextRow>();

        /// <summary>
        /// Khung danh sách dùng chung với popup cửa hàng — cùng nền trắng bo góc, cùng
        /// viền mảnh, cùng cách cuộn. Chỉ khác ở kiểu dòng bên trong.
        /// </summary>
        private void BuildList()
        {
            _list = PopupItemList.Create(_frame.Content, _font);
            var rect = (RectTransform)_list.transform;
            // Chỉ chiếm nửa trái; nửa phải là ô đọc thư (xem MailboxView.BuildDetail).
            rect.offsetMax = new Vector2(
                -(_frame.ContentWidth * (1f - ListWidthFraction)),
                -(PopupTabRail.Height + PopupTabRail.Gap));
        }

        private void RebuildList()
        {
            foreach (var row in _rows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _rows.Clear();

            var letters = Filtered();
            for (var i = 0; i < letters.Count; i++)
            {
                var letter = letters[i];
                var row = PopupTextRow.Create(_list.Rows, _font);
                row.Bind(letter.Title, letter.ShortContent, !letter.IsMark);
                ApplyBadge(row, letter.Type);
                // Dòng cuối tắt vạch: để lại là thừa một nét sát mép trong của khung.
                row.SetSeparatorVisible(i < letters.Count - 1);
                ((RectTransform)row.transform).anchoredPosition =
                    new Vector2(0f, -i * PopupTextRow.Height);
                row.Clicked += () => _detail.Show(letter);
                _rows.Add(row);
            }

            _list.SetRowsHeight(letters.Count * PopupTextRow.Height);
            _list.ShowPlaceholder(letters.Count == 0 ? "Chưa có thư nào." : null);
        }

        /// <summary>Thư bạn bè không gắn nhãn — tiêu đề của nó đã là tên người gửi.</summary>
        private static void ApplyBadge(PopupTextRow row, sbyte type)
        {
            switch (type)
            {
                case TypeAdmin: row.SetBadge("BQT", AdminBadge, Color.white); break;
                case TypeEvent: row.SetBadge("SK", EventBadge, Color.white); break;
                default: row.SetBadge(null, Color.clear, Color.clear); break;
            }
        }

        /// <summary>
        /// Lọc theo tab rồi xếp: chưa đọc lên trước, trong cùng nhóm thì thư hệ thống trên
        /// thư bạn bè. <c>OrderBy</c> của LINQ ổn định nên thứ tự gốc của server được giữ
        /// trong từng nhóm.
        /// </summary>
        private List<Letter> Filtered()
        {
            var filter = TabFilters[Mathf.Clamp(_rail.ActiveIndex, 0, TabFilters.Length - 1)];
            if (_mailbox?.Letters == null) return new List<Letter>();

            return _mailbox.Letters
                .Where(letter => filter == TypeAll || letter.Type == filter)
                .OrderBy(letter => letter.IsMark ? 1 : 0)
                .ThenBy(letter => TypeRank(letter.Type))
                .ToList();
        }

        private static int TypeRank(sbyte type) => type switch
        {
            TypeAdmin => 0,
            TypeEvent => 1,
            _ => 2,
        };
    }
}
