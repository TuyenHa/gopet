using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hàng lọc 7 chip ("Tất cả".."Vật phẩm") + nút sắp xếp xoay vòng ở góc phải, dùng cho
    /// tab Chợ của <see cref="MarketPopupView"/>.
    ///
    /// <para>Thấp hơn <see cref="PopupTabRail"/> (26 so với 38) và có thêm nút sort không
    /// phải tab, nên không tái dùng nguyên khối được — vẫn dùng chung bảng màu viên thuốc
    /// (<see cref="PopupPalette.TabActive"/>/<see cref="PopupPalette.TabInactive"/>).</para>
    /// </summary>
    public sealed class MarketFilterBar : MonoBehaviour
    {
        public const float Height = 26f;

        private const float SortWidth = 72f;
        private const float ChipGap = 2f;
        private const float ChipHeight = 20f;

        /// <summary>Nhãn hiển thị ↔ filter gửi server. Khớp GopetManager.KIOSK_* phía server
        /// (hat=0, weapon=1, armour=2, gem=3, pet=4, other=5).</summary>
        private static readonly (sbyte Filter, string Label)[] Filters =
        {
            (-1, "Tất cả"),
            (1, "Vũ khí"),
            (2, "Giáp"),
            (0, "Mũ"),
            (3, "Ngọc"),
            (4, "Pet"),
            (5, "Vật phẩm"),
        };

        private static readonly string[] SortLabels = { "Mới nhất", "Giá ↑", "Giá ↓" };

        private Image[] _chipBgs;
        private Text[] _chipLabels;
        private Text _sortLabel;
        private int _sortIndex;
        private int _activeFilterIndex;

        /// <summary>Người chơi chọn chip khác — không bắn khi chọn lại đúng chip đang chọn.</summary>
        public event Action<sbyte> FilterChanged;

        /// <summary>Bấm nút sort — xoay vòng 0→1→2→0.</summary>
        public event Action<sbyte> SortChanged;

        public static MarketFilterBar Create(RectTransform parent, Font font, float width)
        {
            var go = new GameObject("FilterBar", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(0f, -Height);
            rect.offsetMax = Vector2.zero;

            var bar = go.AddComponent<MarketFilterBar>();
            bar.BuildChips(font, Mathf.Max(1f, width - SortWidth - ChipGap));
            bar.BuildSortButton(font);
            return bar;
        }

        private void SelectFilterIndex(int index, bool notify)
        {
            if (index == _activeFilterIndex && notify) return;

            _activeFilterIndex = index;
            for (var i = 0; i < _chipBgs.Length; i++) Paint(i, i == index);
            if (notify) FilterChanged?.Invoke(Filters[index].Filter);
        }

        private void Paint(int index, bool active)
        {
            _chipBgs[index].color = active ? PopupPalette.TabActive : PopupPalette.TabInactive;
            _chipLabels[index].color = active ? Color.white : PopupPalette.TextDark;
        }

        private void BuildChips(Font font, float chipsWidth)
        {
            _chipBgs = new Image[Filters.Length];
            _chipLabels = new Text[Filters.Length];

            // Floor để tránh cạnh chip lệch nửa pixel (mờ viền do UI scaling không nguyên).
            var chipWidth = Mathf.Floor((chipsWidth - ChipGap * (Filters.Length - 1)) / Filters.Length);
            for (var i = 0; i < Filters.Length; i++)
            {
                var go = new GameObject($"Chip_{Filters[i].Label}", typeof(RectTransform),
                    typeof(Image), typeof(Button));
                go.transform.SetParent(transform, false);

                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(chipWidth, ChipHeight);
                rect.anchoredPosition = new Vector2(i * (chipWidth + ChipGap), 0f);

                _chipBgs[i] = go.GetComponent<Image>();
                RoundedUiSprite.Apply(_chipBgs[i], 5f);

                var label = UiBuilder.MakeText(go.transform, font, "Label", 9, true);
                label.alignment = TextAnchor.MiddleCenter;
                label.text = Filters[i].Label;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 6;
                label.resizeTextMaxSize = 9;
                _chipLabels[i] = label;
                Paint(i, i == 0);

                var captured = i;
                go.GetComponent<Button>().onClick.AddListener(() => SelectFilterIndex(captured, notify: true));
            }
        }

        private void BuildSortButton(Font font)
        {
            var go = new GameObject("SortButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(SortWidth, ChipHeight);
            rect.anchoredPosition = Vector2.zero;

            var bg = go.GetComponent<Image>();
            RoundedUiSprite.Apply(bg, 5f);
            bg.color = PopupPalette.HeaderBlue;

            _sortLabel = UiBuilder.MakeText(go.transform, font, "Label", 9, true);
            _sortLabel.alignment = TextAnchor.MiddleCenter;
            _sortLabel.text = SortLabels[0];
            _sortLabel.color = Color.white;
            UiBuilder.SetFontStyle(_sortLabel, FontStyle.Bold);

            go.GetComponent<Button>().onClick.AddListener(CycleSort);
        }

        private void CycleSort()
        {
            _sortIndex = (_sortIndex + 1) % SortLabels.Length;
            _sortLabel.text = SortLabels[_sortIndex];
            SortChanged?.Invoke((sbyte)_sortIndex);
        }
    }
}
