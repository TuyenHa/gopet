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
        private const int Columns = 14;
        private const int SlotCount = 100;
        private const float Gap = 3f;
        private MenuItemInfo[] _items = new MenuItemInfo[0];
        private RemoteAssetCache _assets;
        private Transform _popupParent;
        private GuiderHandler _guider;
        private MenuScreen _screen;
        private float _slotSize;

        public static InventoryGridView Create(Transform parent, Transform popupParent,
            RemoteAssetCache assets, GuiderHandler guider)
        {
            var go = new GameObject("Inventory Grid", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var view = go.AddComponent<InventoryGridView>();
            view._assets = assets;
            view._popupParent = popupParent;
            view._guider = guider;
            view.BuildSlots();
            return view;
        }

        public void Bind(MenuScreen screen)
        {
            _screen = screen;
            _items = screen?.Items ?? new MenuItemInfo[0];
            for (var i = 0; i < SlotCount; i++)
            {
                var slot = transform.GetChild(i).gameObject;
                var item = i < _items.Length ? _items[i] : null;
                BindSlot(slot, item, i);
            }
        }

        private void BuildSlots()
        {
            // 14 cột giúp lưới phủ hết pane Rương đồ ở màn hình ngang. Kích thước
            // ô co theo pane để không còn khoảng trống lớn bên phải.
            var available = Mathf.Max(420f, ((RectTransform)transform).rect.width - 4f);
            _slotSize = Mathf.Clamp((available - (Columns - 1) * Gap) / Columns, 28f, 38f);
            for (var i = 0; i < SlotCount; i++)
            {
                var go = new GameObject("Slot" + i, typeof(RectTransform), typeof(Image),
                    typeof(Outline), typeof(Button));
                go.transform.SetParent(transform, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2((i % Columns) * (_slotSize + Gap),
                    -(i / Columns) * (_slotSize + Gap));
                rect.sizeDelta = new Vector2(_slotSize, _slotSize);
                RoundedUiSprite.Apply(go.GetComponent<Image>());
                go.GetComponent<Image>().color = new Color(0.97f, 0.98f, 1f, 1f);
                var border = go.GetComponent<Outline>();
                border.effectColor = new Color(0.68f, 0.73f, 0.82f, 0.95f);
                border.effectDistance = new Vector2(1f, -1f);

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
                    if (index < _items.Length && _items[index] != null) ShowDetails(_items[index], index);
                });
            }
        }

        private void BindSlot(GameObject slot, MenuItemInfo item, int index)
        {
            var icon = slot.transform.Find("Icon").GetComponent<RawImage>();
            icon.texture = null;
            slot.GetComponent<Image>().color = item == null
                ? new Color(0.97f, 0.98f, 1f, 1f)
                : new Color(0.9f, 0.95f, 1f, 1f);
            if (item == null || _assets == null || string.IsNullOrEmpty(item.ImagePath)) return;

            var expected = item.ImagePath;
            _assets.Get(expected, ImagePackets.TypeIcon, icon, texture =>
            {
                if (this == null || icon == null || index >= _items.Length) return;
                if (_items[index] == null || _items[index].ImagePath != expected) return;
                icon.texture = texture;
            });
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
