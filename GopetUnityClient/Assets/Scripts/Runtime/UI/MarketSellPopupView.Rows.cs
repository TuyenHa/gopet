using System.Collections.Generic;
using System.Linq;
using Gopet.Net.Images;
using Gopet.Net.Market;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Cột trái của <see cref="MarketSellPopupView"/>: danh sách đồ bán được, sắp theo
    /// loại. Tách khỏi file chính để mỗi file dưới 200 dòng — cùng khuôn
    /// <c>MailboxView.List.cs</c>.
    /// </summary>
    public sealed partial class MarketSellPopupView
    {
        private const float RowIconSize = 30f;
        private const float RowIconLeft = 6f;
        private const float RowTextLeft = 42f;
        private const float RowBadgeWidth = 40f;
        private const float RowBadgeHeight = 14f;
        private const string DefaultLockMessage = "Vật phẩm đang khóa, bạn không thể bán được.";
        private static readonly Color LockBadge = new Color(0.86f, 0.27f, 0.27f, 1f);

        private void BuildList()
        {
            _list = PopupItemList.Create(_frame.Content, _font);
            var rect = (RectTransform)_list.transform;
            rect.offsetMax = new Vector2(-(_frame.ContentWidth * (1f - ListWidthFraction)), 0f);
        }

        /// <summary>Sắp theo loại (Trang bị / Ngọc / Vật phẩm / Pet) — thay cho tab lọc
        /// riêng (YAGNI, xem phase-05).</summary>
        private static int CategoryRank(sbyte source) => source switch
        {
            MarketSellableItem.SourceEquip => 0,
            MarketSellableItem.SourceGem => 1,
            MarketSellableItem.SourceNormal => 2,
            MarketSellableItem.SourcePet => 3,
            _ => 4,
        };

        private void RebuildRows()
        {
            foreach (var row in _rows)
            {
                if (row != null) Destroy(row);
            }
            _rows.Clear();

            var sorted = _items.OrderBy(i => CategoryRank(i.Source)).ThenBy(i => i.Name).ToList();
            for (var i = 0; i < sorted.Count; i++)
            {
                BuildRow(sorted[i], i, i < sorted.Count - 1);
            }

            _list.SetRowsHeight(sorted.Count * RowHeight);
            _list.ShowPlaceholder(sorted.Count == 0 ? "Bạn chưa có gì để bán." : null);
        }

        private void BuildRow(MarketSellableItem item, int index, bool separator)
        {
            var go = new GameObject("SellRow", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_list.Rows, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, RowHeight);
            rect.anchoredPosition = new Vector2(0f, -index * RowHeight);
            // Trong suốt: chỉ cần Image để bắt cú chạm trên cả dòng, không tô nền — tránh
            // đè lên góc bo của khung danh sách (xem PopupTextRow).
            go.GetComponent<Image>().color = Color.clear;

            BuildIcon(go.transform, item);
            BuildTitle(go.transform, item);
            if (!item.Tradable) BuildLockBadge(go.transform);
            if (separator) BuildSeparator(go.transform);
            if (!item.Tradable) go.AddComponent<CanvasGroup>().alpha = 0.5f;

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!item.Tradable)
                {
                    Message?.Invoke(string.IsNullOrEmpty(item.BlockReason) ? DefaultLockMessage : item.BlockReason);
                    return;
                }
                _detail.Show(item);
            });

            _rows.Add(go);
        }

        private void BuildIcon(Transform parent, MarketSellableItem item)
        {
            var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(parent, false);
            var rect = (RectTransform)frame.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(RowIconSize, RowIconSize);
            rect.anchoredPosition = new Vector2(RowIconLeft, 0f);
            RoundedBorder.Apply(frame, 4f, Color.white, PopupPalette.Hairline);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(frame.transform, false);
            UiBuilder.Stretch((RectTransform)iconGo.transform);
            var icon = iconGo.GetComponent<RawImage>();
            icon.raycastTarget = false;
            if (_assets != null && !string.IsNullOrEmpty(item.IconPath))
                _assets.Get(item.IconPath, ImagePackets.TypeIcon, icon,
                    texture => { if (icon != null) icon.texture = texture; });
        }

        private void BuildTitle(Transform parent, MarketSellableItem item)
        {
            var name = StarNameLabel.Create(parent, _font, 10, 9f);
            name.SetName(item.Name, item.Count > 1 ? $" x{item.Count}" : null);
            var title = name.Label;
            title.color = PopupPalette.TextDark;
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.verticalOverflow = VerticalWrapMode.Truncate;
            var rect = name.Rect;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(RowTextLeft, 2f);
            rect.offsetMax = new Vector2(item.Tradable ? -6f : -(RowBadgeWidth + 8f), -2f);
        }

        private void BuildLockBadge(Transform parent)
        {
            var badge = new GameObject("Lock", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(parent, false);
            var rect = (RectTransform)badge.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(RowBadgeWidth, RowBadgeHeight);
            rect.anchoredPosition = new Vector2(-6f, 0f);
            var image = badge.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 3f);
            image.color = LockBadge;
            image.raycastTarget = false;

            var label = UiBuilder.MakeText(badge.transform, _font, "Label", 8, true);
            label.text = "Khóa";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.raycastTarget = false;
        }

        private void BuildSeparator(Transform parent)
        {
            var go = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(RowTextLeft, 0f);
            rect.offsetMax = new Vector2(-6f, 1f);
            var image = go.GetComponent<Image>();
            image.color = PopupPalette.Hairline;
            image.raycastTarget = false;
        }
    }
}
