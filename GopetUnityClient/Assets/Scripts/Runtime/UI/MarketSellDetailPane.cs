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
    /// Cột phải của <see cref="MarketSellPopupView"/>: icon + tên + mô tả cuộn được, ô
    /// "Giá bán" (luôn hiện) và ô "Số lượng" (chỉ khi món có count &gt; 1), dòng "Thực
    /// nhận" tính thuế 5%, nút "Đăng bán" và dòng lỗi đỏ.
    ///
    /// <para>Khung dựng qua <see cref="RoundedBorder"/> như <see cref="LetterDetailPane"/> —
    /// viền liền một nét, không dùng <see cref="Outline"/> (góc nhoè, xem
    /// <see cref="BlacksmithRepairPopupView"/>).</para>
    /// </summary>
    public sealed partial class MarketSellDetailPane : MonoBehaviour
    {
        private const long MinPrice = 1L;
        private const long MaxPrice = 2_000_000_000L;
        private const int TaxPercent = 5;

        private RemoteAssetCache _assets;
        private Text _placeholder;
        private GameObject _content;
        private RawImage _icon;
        private string _iconPath;
        private StarNameLabel _name;
        private Text _descText;
        private ScrollRect _descScroll;
        private InputField _priceInput;
        private GameObject _qtyRow;
        private InputField _qtyInput;
        private Text _netText;
        private Text _errorText;
        private Button _sellButton;
        private MarketSellableItem _item;
        private bool _busy;

        /// <summary>Bấm "Đăng bán": (món, số lượng, tổng giá).</summary>
        public event Action<MarketSellableItem, int, int> SellRequested;

        public static MarketSellDetailPane Create(RectTransform parent, Font font, RemoteAssetCache assets)
        {
            var go = new GameObject("SellDetail", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);

            var pane = go.AddComponent<MarketSellDetailPane>();
            pane._assets = assets;
            pane.Build(font);
            pane.Show(null);
            return pane;
        }

        /// <summary><c>null</c> = chưa chọn món, chỉ hiện lời mời chọn bên trái.</summary>
        public void Show(MarketSellableItem item)
        {
            _item = item;
            _busy = false;
            _errorText.text = string.Empty;

            var has = item != null;
            _content.SetActive(has);
            _placeholder.gameObject.SetActive(!has);
            if (!has) return;

            LoadIcon(item.IconPath);
            _name.SetName(item.Name);
            _descText.text = item.Desc ?? string.Empty;
            _descScroll.verticalNormalizedPosition = 1f;

            _qtyRow.SetActive(item.Count > 1);
            _priceInput.text = string.Empty;
            _priceInput.interactable = true;
            _qtyInput.text = Math.Max(1, item.Count).ToString(CultureInfo.InvariantCulture);
            _qtyInput.interactable = true;

            UpdateComputed();
        }

        /// <summary>Server từ chối: dòng đỏ trong pane. Toast đã do UiRoot lo.</summary>
        public void ShowError(string message) => _errorText.text = message ?? string.Empty;

        /// <summary>Khoá ô nhập + nút trong lúc chờ server trả lời TYPE_MARKET_RESULT.</summary>
        public void SetBusy(bool busy)
        {
            _busy = busy;
            _priceInput.interactable = !busy;
            _qtyInput.interactable = !busy;
            UpdateComputed();
        }

        private void Submit()
        {
            if (_item == null || !TryReadPrice(out var price) || !TryReadCount(out var count)) return;
            SellRequested?.Invoke(_item, count, price);
        }

        private void UpdateComputed()
        {
            var priceOk = TryReadPrice(out var price);
            // price tới MaxPrice=2B: price*95 tràn int32 (int.MaxValue ~2.15B) — ép long trước
            // khi nhân, không phải sau.
            _netText.text = priceOk
                ? $"Thực nhận: {FormatCoin((long)price * (100 - TaxPercent) / 100)} ngọc (thuế {TaxPercent}%)"
                : $"Thực nhận: -- ngọc (thuế {TaxPercent}%)";

            var countOk = TryReadCount(out _);
            _sellButton.interactable = !_busy && _item != null && _item.Tradable && priceOk && countOk;
        }

        private bool TryReadPrice(out int price)
        {
            price = 0;
            if (!long.TryParse(_priceInput.text, out var value)) return false;
            if (value < MinPrice || value > MaxPrice) return false;
            price = (int)value;
            return true;
        }

        private bool TryReadCount(out int count)
        {
            // Server chuẩn hoá Count về tối thiểu 1 cho đồ không xếp chồng, nhưng vẫn kẹp ở đây
            // phòng dữ liệu cũ/server chưa vá gửi Count=0 — tránh nút Đăng bán bị mờ vô cớ.
            count = Math.Max(1, _item?.Count ?? 0);
            if (!_qtyRow.activeSelf) return _item != null;
            if (!int.TryParse(_qtyInput.text, out var value)) return false;
            if (value < 1 || value > Math.Max(1, _item?.Count ?? 0)) return false;
            count = value;
            return true;
        }

        /// <summary>Gõ xong thì kẹp về khoảng hợp lệ thay vì chỉ khoá nút — người chơi gõ
        /// "9999999999" thấy số bị kẹp ngay còn hơn bấm Đăng bán mà không hiểu vì sao mờ.</summary>
        private void ClampPriceOnEndEdit()
        {
            if (!long.TryParse(_priceInput.text, out var value)) { UpdateComputed(); return; }
            value = Math.Clamp(value, MinPrice, MaxPrice);
            _priceInput.text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void ClampCountOnEndEdit()
        {
            var max = Math.Max(1, _item?.Count ?? 1);
            if (!int.TryParse(_qtyInput.text, out var value)) { UpdateComputed(); return; }
            value = Math.Clamp(value, 1, max);
            _qtyInput.text = value.ToString(CultureInfo.InvariantCulture);
        }

        private void LoadIcon(string path)
        {
            _icon.texture = null;
            _iconPath = path;
            if (_assets == null || string.IsNullOrEmpty(path)) return;

            var expected = path;
            _assets.Get(path, ImagePackets.TypeIcon, _icon, texture =>
            {
                if (this == null || _icon == null || _iconPath != expected) return;
                _icon.texture = texture;
            });
        }

        /// <summary>12000 → "12.000" (chấm ngăn nghìn kiểu Việt), khớp MarketListingRowView.</summary>
        private static string FormatCoin(long value) =>
            value.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '.');
    }
}
