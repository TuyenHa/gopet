using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Lưới Rương đồ 10×10. Chạm một ô sẽ mở thông tin vật phẩm.</summary>
    public sealed class InventoryGridView : MonoBehaviour
    {
        private const int DefaultColumns = 14;
        private const int DefaultSlotCount = 100;
        private const float Gap = 3f;
        private const float BarHeight = 3f;
        private static readonly Color EmptySlot = new Color(0.97f, 0.98f, 1f, 1f);
        private static readonly Color FilledSlot = new Color(0.9f, 0.95f, 1f, 1f);
        private static readonly Color SlotBorder = new Color(0.68f, 0.73f, 0.82f, 1f);
        private MenuItemInfo[] _items = new MenuItemInfo[0];
        private Image[] _fills;
        private int _columns = DefaultColumns;
        private int _slotCount = DefaultSlotCount;
        private RemoteAssetCache _assets;
        private Transform _popupParent;
        private GuiderHandler _guider;
        private MenuScreen _screen;
        private float _slotSize;

        /// <summary>
        /// Chạm một ô có đồ. Có người nghe thì KHÔNG mở popup chi tiết mặc định — màn hình
        /// dùng lại lưới (Thợ Rèn) tự dựng popup riêng.
        /// </summary>
        public event Action<MenuItemInfo, int> ItemClicked;

        /// <summary>Tổng chiều cao các ô, cho màn hình đặt lưới trong khung cuộn.</summary>
        public float ContentHeight =>
            Mathf.CeilToInt(_slotCount / (float)_columns) * (_slotSize + Gap) - Gap;

        /// <summary>Bề ngang thật của lưới (ô bị kẹp cỡ tối đa nên có thể hẹp hơn chỗ được cho).</summary>
        public float ContentWidth => _columns * (_slotSize + Gap) - Gap;

        /// <param name="width">Bề ngang lưới; 0 = đọc từ rect cha (Rương đồ).</param>
        public static InventoryGridView Create(Transform parent, Transform popupParent,
            RemoteAssetCache assets, GuiderHandler guider, float width = 0f,
            int columns = DefaultColumns, int slotCount = DefaultSlotCount)
        {
            var go = new GameObject("Inventory Grid", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var view = go.AddComponent<InventoryGridView>();
            view._assets = assets;
            view._popupParent = popupParent;
            view._guider = guider;
            view._columns = Mathf.Max(1, columns);
            view._slotCount = Mathf.Max(1, slotCount);
            view.BuildSlots(width);
            return view;
        }

        public void Bind(MenuScreen screen)
        {
            _screen = screen;
            _items = screen?.Items ?? new MenuItemInfo[0];
            for (var i = 0; i < _slotCount; i++)
            {
                var slot = transform.GetChild(i).gameObject;
                var item = i < _items.Length ? _items[i] : null;
                BindSlot(slot, item, i);
            }
        }

        private void BuildSlots(float width)
        {
            // 14 cột giúp lưới phủ hết pane Rương đồ ở màn hình ngang. Kích thước
            // ô co theo pane để không còn khoảng trống lớn bên phải.
            var available = width > 0f
                ? width
                : Mathf.Max(420f, ((RectTransform)transform).rect.width - 4f);
            _slotSize = Mathf.Clamp((available - (_columns - 1) * Gap) / _columns, 28f, 38f);
            _fills = new Image[_slotCount];
            for (var i = 0; i < _slotCount; i++)
            {
                var go = new GameObject("Slot" + i, typeof(RectTransform), typeof(Image),
                    typeof(Button));
                go.transform.SetParent(transform, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2((i % _columns) * (_slotSize + Gap),
                    -(i / _columns) * (_slotSize + Gap));
                rect.sizeDelta = new Vector2(_slotSize, _slotSize);
                // Viền hai lớp đều nét, không dùng Outline (nhân mesh chéo → góc nhoè/trắng).
                _fills[i] = RoundedBorder.Apply(go, 6f, EmptySlot, SlotBorder);

                var icon = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
                icon.transform.SetParent(go.transform, false);
                var iconRect = (RectTransform)icon.transform;
                UiBuilder.Stretch(iconRect);
                iconRect.offsetMin = new Vector2(2f, 2f);
                iconRect.offsetMax = new Vector2(-2f, -2f);
                icon.GetComponent<RawImage>().raycastTarget = false;

                var index = i;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (index >= _items.Length || _items[index] == null) return;
                    if (ItemClicked != null) ItemClicked(_items[index], index);
                    else ShowDetails(_items[index], index);
                });
            }
        }

        private void BindSlot(GameObject slot, MenuItemInfo item, int index)
        {
            var icon = slot.transform.Find("Icon").GetComponent<RawImage>();
            icon.texture = null;
            _fills[index].color = item == null ? EmptySlot : FilledSlot;
            DecorateSlot(slot, item);
            InventorySlotCountBadge.Bind(slot, item?.Title);
            if (item == null || _assets == null || string.IsNullOrEmpty(item.ImagePath)) return;

            var expected = item.ImagePath;
            _assets.Get(expected, ImagePackets.TypeIcon, icon, texture =>
            {
                if (this == null || icon == null || index >= _items.Length) return;
                if (_items[index] == null || _items[index].ImagePath != expected) return;
                icon.texture = texture;
            });
        }

        /// <summary>
        /// Vạch độ bền ở mép dưới ô cho trang bị pet (mô tả có "Độ bền: X/Max"): xanh → cam
        /// → đỏ theo phần trăm còn lại. Dùng cho cả Rương đồ lẫn lưới sửa ở Thợ Rèn.
        /// </summary>
        private static void DecorateSlot(GameObject slot, MenuItemInfo item)
        {
            var bar = slot.transform.Find("Durability");
            var hasValue = BlacksmithRepairPopupView.TryParseDurability(item?.Description,
                out var current, out var max, out _);
            if (!hasValue)
            {
                if (bar != null) bar.gameObject.SetActive(false);
                return;
            }

            if (bar == null) bar = CreateSlotBar(slot.transform);
            bar.gameObject.SetActive(true);
            var fill = (RectTransform)bar.GetChild(0);
            var ratio = Mathf.Clamp01(current / (float)max);
            fill.anchorMax = new Vector2(ratio, 1f);
            fill.GetComponent<Image>().color = BlacksmithRepairPopupView.DurabilityColor(ratio);
        }

        private static Transform CreateSlotBar(Transform slot)
        {
            var track = new GameObject("Durability", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(slot, false);
            var rect = (RectTransform)track.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(4f, 3f);
            rect.offsetMax = new Vector2(-4f, 3f + BarHeight);
            var trackImage = track.GetComponent<Image>();
            trackImage.color = new Color(0.16f, 0.2f, 0.28f, 0.35f);
            trackImage.raycastTarget = false;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().raycastTarget = false;
            return track.transform;
        }

        private void ShowDetails(MenuItemInfo item, int index)
        {
            if (_popupParent == null) return;
            InventoryItemPopupView.Create(_popupParent, item, _assets, item.CanSelect
                ? () => _guider?.Select(_screen, index)
                : null);
        }
    }
}
