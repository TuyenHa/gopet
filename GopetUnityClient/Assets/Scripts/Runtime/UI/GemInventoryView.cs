using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "Kho ngọc": khung <see cref="GamePopupFrame"/> cùng danh sách
    /// <see cref="PopupItemList"/> của Cửa hàng — mỗi viên ngọc là một thẻ
    /// <see cref="ShopItemRow"/>, các dòng ngăn nhau bằng vạch kẻ ngang. Danh sách cuộn
    /// dọc nên không cần phân trang. Chạm nút "Chọn" để mở thao tác.
    /// </summary>
    public sealed class GemInventoryView : MonoBehaviour
    {
        private const string Title = "Kho ngọc";
        /// <summary>Nhỏ hơn khung mặc định 400×300 — danh sách ngọc ít cột, không cần rộng.</summary>
        private const float Width = 340f;
        private const float Height = 250f;
        private const string ActionLabel = "Chọn";

        private readonly List<GemItemInfo> _items = new List<GemItemInfo>();
        private GamePopupFrame _frame;
        private PopupItemList _list;
        private RemoteAssetCache _assets;

        public event Action<GemItemInfo> GemSelected;
        public event Action CloseRequested;
        public int ItemCount => _items.Count;

        public static GemInventoryView Create(Transform parent, Font font, RemoteAssetCache assets = null)
        {
            font = font ?? UiBuilder.DefaultFont();
            var frame = GamePopupFrame.Create(parent, font, Title, Width, Height, footer: string.Empty);
            frame.gameObject.name = "GemInventoryView";
            frame.UseCompactChrome();

            var view = frame.gameObject.AddComponent<GemInventoryView>();
            view._frame = frame;
            view._assets = assets;
            frame.Closed += () => view.CloseRequested?.Invoke();

            view.BuildList(frame.Content, font);
            return view;
        }

        /// <summary>Bản nhúng (tab Pet của Hành lý): chỉ danh sách, không khung/băng chân.</summary>
        public static GemInventoryView CreateEmbedded(Transform host, Font font, RemoteAssetCache assets = null)
        {
            var go = new GameObject("GemInventoryEmbedded", typeof(RectTransform));
            go.transform.SetParent(host, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var view = go.AddComponent<GemInventoryView>();
            view._assets = assets;
            view.BuildList(go.transform, font ?? UiBuilder.DefaultFont());
            return view;
        }

        private void BuildList(Transform parent, Font font)
        {
            _list = PopupItemList.Create(parent, font);
            _list.Activated += index =>
            {
                if (index >= 0 && index < _items.Count) GemSelected?.Invoke(_items[index]);
            };
            Refresh();
        }

        public void ApplyInventory(GemInventory inventory)
        {
            _items.Clear();
            if (inventory?.Items != null) _items.AddRange(inventory.Items);
            Refresh();
        }

        public void UpdateGem(GemItemInfo item)
        {
            if (item == null) return;
            var index = _items.FindIndex(x => x.ItemId == item.ItemId);
            if (index >= 0) _items[index] = item;
            else _items.Add(item);
            Refresh();
        }

        public void RemoveGem(int itemId)
        {
            _items.RemoveAll(x => x.ItemId == itemId);
            Refresh();
        }

        private void Refresh()
        {
            if (_list == null) return;
            var rows = new MenuItemInfo[_items.Count];
            for (var i = 0; i < rows.Length; i++) rows[i] = ToRow(_items[i]);
            _list.Bind(new MenuScreen { Title = Title, Items = rows }, _assets, ActionLabel);
            if (rows.Length == 0) _list.ShowPlaceholder("Bạn chưa có ngọc.");
            if (_frame != null) _frame.SetFooter(rows.Length == 0
                ? "Kho ngọc đang trống."
                : $"Có {rows.Length} viên ngọc · chạm \"{ActionLabel}\" để cường hoá.");
        }

        private static MenuItemInfo ToRow(GemItemInfo gem)
        {
            var text = GemItemText.Parse(gem.Name);
            var info = $"Lv {gem.Level}   •   ID #{gem.ItemId}";
            return new MenuItemInfo
            {
                ItemId = gem.ItemId,
                ImagePath = gem.IconPath,
                Title = text.Name,
                Description = string.IsNullOrEmpty(text.Effects) ? info : info + "\n" + text.Effects,
                CanSelect = true
            };
        }
    }
}
