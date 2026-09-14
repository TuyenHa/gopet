using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>Áp dụng <see cref="ChoiceDialogLayout.Result"/> (thuần C#) vào RectTransform
    /// thật của Unity — tách khỏi phần dựng Image/Button để phần hình học dễ soát lỗi riêng.</summary>
    public sealed partial class ChoiceDialogView
    {
        /// <summary>Đặt panel cao động + vùng message theo layout. Ngang (≤3 nút): panel giữ
        /// đúng 190px như giao diện cũ, message dùng anchor cố định như trước. Dọc: panel cao
        /// theo số nút, vùng message co lại phía trên khối nút (offset theo pixel thay vì anchor
        /// cố định vì panel không còn chiều cao cố định).</summary>
        private void ApplyLayout(ChoiceDialogLayout.Result layout)
        {
            var panelRect = (RectTransform)_panel;
            panelRect.sizeDelta = new Vector2(ChoiceDialogLayout.PanelWidth, layout.PanelHeight);

            var messageRect = _message.rectTransform;
            if (layout.Vertical)
            {
                messageRect.anchorMin = Vector2.zero;
                messageRect.anchorMax = Vector2.one;
                messageRect.offsetMin = new Vector2(24f, layout.ButtonsTop);
                messageRect.offsetMax = new Vector2(-24f, -8f);
            }
            else
            {
                messageRect.anchorMin = new Vector2(0f, 0.42f);
                messageRect.anchorMax = new Vector2(1f, 1f);
                messageRect.offsetMin = new Vector2(24f, 12f);
                messageRect.offsetMax = new Vector2(-24f, -22f);
            }
        }

        /// <summary>Đặt kích thước + vị trí nút thứ <paramref name="index"/> trong tổng
        /// <paramref name="count"/> nút. Ngang: giữ nguyên công thức cũ (dàn đều quanh tâm,
        /// neo đáy y=16). Dọc: mỗi nút một hàng, index 0 TRÊN CÙNG — khớp thứ tự option server
        /// gửi (xem UiRoot.ShowNpcOptions).</summary>
        private static void PlaceButton(RectTransform rect, int index, int count, ChoiceDialogLayout.Result layout)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(layout.RowWidth, layout.RowHeight);

            if (!layout.Vertical)
            {
                var totalWidth = count * layout.RowWidth + (count - 1) * layout.Gap;
                rect.anchoredPosition = new Vector2(
                    -totalWidth / 2f + layout.RowWidth / 2f + index * (layout.RowWidth + layout.Gap),
                    ChoiceDialogLayout.BottomPadding);
                return;
            }

            var y = ChoiceDialogLayout.BottomPadding + (count - 1 - index) * (layout.RowHeight + layout.Gap);
            rect.anchoredPosition = new Vector2(0f, y);
        }
    }
}
