using System;
using System.Collections.Generic;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Kho gem có phân trang; chọn một dòng để mở thao tác.</summary>
    public sealed class GemInventoryView : MonoBehaviour
    {
        private const int PageSize = 6;
        private readonly List<GemItemInfo> _items = new List<GemItemInfo>();
        private Transform _rows;
        private Text _title;
        private Text _pageText;
        private int _page;

        public event Action<GemItemInfo> GemSelected;
        public event Action CloseRequested;
        public int ItemCount => _items.Count;

        public static GemInventoryView Create(Transform parent, Font font)
        {
            var backdrop = new GameObject("GemInventoryView", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(430f, 450f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());
            panel.GetComponent<Image>().color = UiBuilder.Panel;

            var view = backdrop.AddComponent<GemInventoryView>();
            view.Build(panel.transform, font ?? UiBuilder.DefaultFont());
            return view;
        }

        public void ApplyInventory(GemInventory inventory)
        {
            _items.Clear();
            if (inventory?.Items != null) _items.AddRange(inventory.Items);
            _page = 0;
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
            var maxPage = Math.Max(0, (_items.Count - 1) / PageSize);
            if (_page > maxPage) _page = maxPage;
            Refresh();
        }

        private void Build(Transform panel, Font font)
        {
            _title = UiBuilder.MakeText(panel, font, "Title", 18, false);
            UiBuilder.SetFontStyle(_title, FontStyle.Bold);
            _title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(_title.rectTransform, 10f, 34f, 52f);
            MakeButton(panel, font, "Đóng", 10f, 36f, 8f, 350f, () => CloseRequested?.Invoke());

            var rows = new GameObject("Rows", typeof(RectTransform));
            rows.transform.SetParent(panel, false);
            var rowsRect = (RectTransform)rows.transform;
            rowsRect.anchorMin = new Vector2(0f, 1f);
            rowsRect.anchorMax = new Vector2(1f, 1f);
            rowsRect.pivot = new Vector2(0f, 1f);
            rowsRect.offsetMin = new Vector2(12f, -390f);
            rowsRect.offsetMax = new Vector2(-12f, -52f);
            _rows = rows.transform;

            MakeButton(panel, font, "‹", 402f, 36f, 12f, 52f, () => ChangePage(-1));
            MakeButton(panel, font, "›", 402f, 36f, 366f, 52f, () => ChangePage(1));
            _pageText = UiBuilder.MakeText(panel, font, "Page", 13, false);
            _pageText.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(_pageText.rectTransform, 402f, 36f, 76f);
        }

        private void ChangePage(int delta)
        {
            var maxPage = Math.Max(0, (_items.Count - 1) / PageSize);
            _page = Math.Max(0, Math.Min(maxPage, _page + delta));
            Refresh();
        }

        private void Refresh()
        {
            if (_rows == null) return;
            for (var i = _rows.childCount - 1; i >= 0; i--)
                Destroy(_rows.GetChild(i).gameObject);

            _title.text = $"Kho ngọc ({_items.Count})";
            var pages = Math.Max(1, (_items.Count + PageSize - 1) / PageSize);
            _pageText.text = $"Trang {_page + 1}/{pages}";
            var first = _page * PageSize;
            var last = Math.Min(_items.Count, first + PageSize);
            for (var i = first; i < last; i++) MakeGemRow(_items[i], i - first);
            if (_items.Count == 0)
            {
                var empty = UiBuilder.MakeText(_rows, UiBuilder.DefaultFont(), "Empty", 14, false);
                empty.text = "Bạn chưa có ngọc.";
                empty.alignment = TextAnchor.MiddleCenter;
                UiBuilder.PlaceRow(empty.rectTransform, 100f, 40f, 8f);
            }
        }

        private void MakeGemRow(GemItemInfo item, int row)
        {
            var font = UiBuilder.DefaultFont();
            var go = new GameObject($"Gem:{item.ItemId}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_rows, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, row * 54f, 48f, 0f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, font, "Label", 14, true);
            text.text = $"{item.Name}\nLv {item.Level}   •   ID #{item.ItemId}";
            text.rectTransform.offsetMin = new Vector2(12f, 3f);
            text.rectTransform.offsetMax = new Vector2(-12f, -3f);
            go.GetComponent<Button>().onClick.AddListener(() => GemSelected?.Invoke(item));
        }

        private static Button MakeButton(Transform parent, Font font, string label, float top,
            float height, float left, float width, Action action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, font, "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => action());
            return button;
        }
    }
}
