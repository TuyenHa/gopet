using System;
using System.Globalization;
using Gopet.Net.Images;
using Gopet.Net.Market;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một hàng ki-ốt dùng chung cho cả 2 tab của <see cref="MarketPopupView"/>: icon,
    /// tên (kèm "x{count}" nếu &gt; 1), giá (icon ngọc + số chấm ngăn nghìn), dòng thứ 3
    /// do caller truyền (người bán ở tab Chợ, thời gian còn lại ở tab Gian hàng), và một
    /// khoảng trống bên phải (<see cref="ActionSlot"/>) để caller tự gắn 1-2 nút hành động
    /// — tab Chợ gắn "Mua", tab Gian hàng gắn "Chỉ định" + "Gỡ" xếp chồng.
    ///
    /// <para>Cùng khuôn <see cref="ShopItemRow"/> (neo trải ngang, các vùng chữ đo từ mép
    /// trái/phải) nhưng row thấp hơn (46 so với 68) vì chỉ có 3 dòng chữ, không có chip
    /// chỉ số.</para>
    /// </summary>
    public sealed partial class MarketListingRowView : MonoBehaviour
    {
        public const float Height = 46f;

        internal const float IconSize = 36f;
        internal const float IconLeft = 5f;
        internal const float TextLeft = 47f;
        internal const float ActionWidth = 78f;
        internal const float ActionRight = 4f;

        private RawImage _icon;
        private StarNameLabel _titleName;
        private Text _price;
        private Text _sellerOrTime;
        private RectTransform _actionSlot;
        private GameObject _separator;
        private Font _font;
        private string _iconPath;

        /// <summary>Vùng trống bên phải hàng để caller gắn nút hành động — xem <see cref="AddButton"/>.</summary>
        public RectTransform ActionSlot => _actionSlot;

        public static MarketListingRowView Create(Transform parent, Font font)
        {
            var go = new GameObject("MarketListingRow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, Height);

            var row = go.AddComponent<MarketListingRowView>();
            row._font = font;
            row.Build(go.transform);
            return row;
        }

        public void Bind(MarketListingRow row, RemoteAssetCache assets, string sellerOrTimeText)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            _titleName.SetName(row.Name, row.Count > 1 ? $" x{row.Count}" : null);
            _price.text = FormatPrice(row.Price);
            _sellerOrTime.text = sellerOrTimeText ?? string.Empty;
            LoadIcon(row.IconPath, assets);
        }

        /// <summary>Kẻ vạch ngăn dưới chân dòng. Hàng cuối trang thì tắt.</summary>
        public void SetSeparatorVisible(bool visible)
        {
            if (_separator != null) _separator.SetActive(visible);
        }

        /// <summary>
        /// Gắn một nút xanh vào <see cref="ActionSlot"/>. <paramref name="top"/>/
        /// <paramref name="height"/> tính từ mép trên slot — cho phép 1 nút cao giữa hàng
        /// (Mua). <paramref name="xMin"/>/<paramref name="xMax"/> là phần bề ngang slot (0..1) —
        /// chia đôi để đặt 2 nút cạnh nhau (Chỉ định | Gỡ).
        /// </summary>
        public Button AddButton(string label, float top, float height, Action onClick,
            float xMin = 0f, float xMax = 1f)
        {
            var go = new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_actionSlot, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(xMin, 1f);
            rect.anchorMax = new Vector2(xMax, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(2f, -(top + height));
            rect.offsetMax = new Vector2(-2f, -top);

            var image = go.GetComponent<Image>();
            var button = go.GetComponent<Button>();
            RoundedUiSprite.Apply(image);
            image.color = PopupPalette.ButtonBlue;
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 10, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            button.onClick.AddListener(() => onClick?.Invoke());
            return button;
        }

        private void LoadIcon(string path, RemoteAssetCache assets)
        {
            _icon.texture = null;
            _iconPath = path;
            if (assets == null || string.IsNullOrEmpty(path)) return;

            var expected = path;
            assets.Get(path, ImagePackets.TypeIcon, _icon, texture =>
            {
                if (this == null || _icon == null) return;
                if (_iconPath != expected) return; // đã đổi trang/tab trong lúc chờ
                _icon.texture = texture;
            });
        }

        /// <summary>12000 → "12.000" (dấu chấm ngăn nghìn kiểu Việt, không phụ thuộc culture máy).</summary>
        private static string FormatPrice(long value) =>
            value.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '.');
    }
}
