using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Tab "Sửa trang bị" của <see cref="BlacksmithNpcTabsView"/>: lưới 100 ô dùng lại
    /// <see cref="InventoryGridView"/> của Rương đồ (50 ô, 10 cột × 5 hàng), đặt trong khung
    /// cuộn. Vạch độ bền ở mép dưới ô do chính lưới vẽ.
    /// </summary>
    public sealed partial class BlacksmithNpcTabsView
    {
        private const float GridPadding = 7f;
        private const int SlotCount = 50;
        private const int Columns = 10;

        private InventoryGridView _grid;
        private Text _emptyHint;

        private GameObject BuildGridPanel(Transform parent)
        {
            var panel = new GameObject("RepairGrid", typeof(RectTransform), typeof(Image),
                typeof(RectMask2D), typeof(ScrollRect));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(1f, 1f);
            rect.offsetMax = new Vector2(-1f, -1f);
            // ScrollRect chỉ nhận kéo khi có Graphic bắt raycast phủ hết vùng cuộn.
            panel.GetComponent<Image>().color = Color.clear;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(panel.transform, false);
            var content = (RectTransform)contentGo.transform;
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 1f);
            content.pivot = new Vector2(0.5f, 1f);

            var gridWidth = _frame.ContentWidth - 2f - GridPadding * 2f;
            _grid = InventoryGridView.Create(content, transform, _assets, _guider, gridWidth,
                Columns, SlotCount);
            _grid.ItemClicked += OpenRepairPopup;
            // Ô kẹp cỡ tối đa nên lưới hẹp hơn chỗ được cho — lấy bề ngang thật để căn giữa.
            content.sizeDelta = new Vector2(_grid.ContentWidth, _grid.ContentHeight + GridPadding * 2f);
            var gridRect = (RectTransform)_grid.transform;
            gridRect.offsetMin = new Vector2(0f, GridPadding);
            gridRect.offsetMax = new Vector2(0f, -GridPadding);

            var scroll = panel.GetComponent<ScrollRect>();
            scroll.viewport = rect;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            _emptyHint = UiBuilder.MakeText(panel.transform, _font, "EmptyHint", 13, true);
            _emptyHint.alignment = TextAnchor.MiddleCenter;
            _emptyHint.color = PopupPalette.TextMuted;
            _emptyHint.raycastTarget = false;
            _emptyHint.text = "Pet của bạn chưa có trang bị nào.";
            _emptyHint.gameObject.SetActive(false);
            return panel;
        }

        private void BindGrid(MenuScreen screen)
        {
            _grid.Bind(screen);
            _emptyHint.gameObject.SetActive(screen.Items == null || screen.Items.Length == 0);
        }

        private void OpenRepairPopup(MenuItemInfo item, int index)
        {
            var screen = _screen;
            BlacksmithRepairPopupView.Create(transform, _font, item, _assets, _stoneCount, () =>
            {
                // Lưới có thể đã được server gửi lại trong lúc xem — chỉ gửi khi còn đúng màn đó.
                if (_guider != null && screen != null && ReferenceEquals(screen, _screen)
                    && index < screen.Items.Length && screen.Items[index].CanSelect)
                    _guider.Select(screen, index);
            });
        }
    }
}
