using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một thẻ item trong popup cửa hàng: icon, tên + yêu cầu chỉ số, mô tả, chip chỉ
    /// số và nút giá màu xanh lá bên phải.
    ///
    /// <para>Khác <see cref="MenuItemRow"/> (dòng menu generic dùng chung cho 162 màn
    /// hình) ở chỗ nó đọc được khuôn chuỗi RIÊNG của shop qua <see cref="ShopItemText"/>
    /// và có nút mua kèm giá. Tách ra thay vì nhồi thêm một chế độ nữa vào
    /// <see cref="MenuItemRow"/> — dòng menu đã gánh sẵn hai chế độ card rồi.</para>
    ///
    /// <para>Phần dựng hình nằm ở <c>ShopItemRow.Build.cs</c>.</para>
    /// </summary>
    public sealed partial class ShopItemRow : MonoBehaviour
    {
        /// <summary>155px trên ảnh mẫu, quy về ref-unit của canvas.</summary>
        public const float Height = 68f;

        private RawImage _icon;
        private Text _title;
        private Text _description;
        private RectTransform _chips;
        private Text _price;
        private Image _priceFill;
        private Image _priceEdge;
        private Button _priceButton;
        private GameObject _separator;
        private Font _font;

        /// <summary>Đường dẫn icon đang chờ tải. Callback về trễ mà không khớp thì bỏ.</summary>
        private string _iconPath;

        private string _actionLabel = "Mua";

        /// <summary>Người chơi bấm mua dòng này.</summary>
        public event Action Buy;

        /// <param name="actionLabel">Chữ trên nút khi dòng không kèm giá ("Mua", "Đổi"…).</param>
        public void Bind(MenuItemInfo item, RemoteAssetCache assets, string actionLabel = "Mua")
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _actionLabel = actionLabel;

            var text = ShopItemText.Parse(item.Title, item.Description);
            _title.text = string.IsNullOrEmpty(text.Requirement)
                ? text.Name
                : text.Name + " (" + text.Requirement + ")";
            _description.text = text.Description;

            BuildChips(text.Stats);
            PlaceDescription(text.Stats.Length > 0);
            BindPrice(item);
            LoadIcon(item.ImagePath, assets);
        }

        /// <summary>Kẻ vạch ngăn dưới chân dòng. Dòng cuối không kẻ, nếu không thừa một vạch lửng.</summary>
        public void SetSeparatorVisible(bool visible)
        {
            if (_separator != null) _separator.SetActive(visible);
        }

        private void BindPrice(MenuItemInfo item)
        {
            var options = item.PaymentOptions;
            var hasPrice = options != null && options.Length > 0;
            _price.text = hasPrice ? ShopItemText.ShortMoney(options[0].MoneyText) : _actionLabel;

            // Nút giữ nguyên xanh tươi kể cả khi KHÔNG đủ tiền — người chơi cần thấy
            // giá của món mình đang nhắm tới, không phải một nút xỉn khó đọc. Bấm vào
            // vẫn có phản hồi: xem ShopPopupView.TryBuy.
            //
            // Chỉ xám khi server khoá hẳn dòng (CanSelect = false), vì lúc đó gửi lên
            // server cũng im lặng bỏ qua.
            _priceFill.color = item.CanSelect ? PopupPalette.PriceGreen : PopupPalette.PriceLocked;
            _priceEdge.color = item.CanSelect ? PopupPalette.PriceGreenEdge : PopupPalette.PriceLockedEdge;
            _priceButton.interactable = item.CanSelect;
            _icon.color = new Color(1f, 1f, 1f, item.CanSelect ? 1f : 0.45f);
        }

        private void LoadIcon(string path, RemoteAssetCache assets)
        {
            // Xoá ngay ảnh cũ: dòng này vừa được tái dùng cho item khác, giữ texture cũ
            // là hiện nhầm icon cho tới khi ảnh mới về.
            _icon.texture = null;
            _iconPath = path;
            if (assets == null || string.IsNullOrEmpty(path)) return;

            var expected = path;
            assets.Get(path, ImagePackets.TypeIcon, texture =>
            {
                if (this == null || _icon == null) return;
                if (_iconPath != expected) return;   // đã đổi tab / đổi item trong lúc chờ
                _icon.texture = texture;
            });
        }

        private void BuildChips(ShopItemText.StatChip[] stats)
        {
            for (var i = _chips.childCount - 1; i >= 0; i--)
                Destroy(_chips.GetChild(i).gameObject);

            foreach (var stat in stats)
            {
                var value = string.IsNullOrEmpty(stat.Value) ? "0" : stat.Value;
                ShopChip.Create(_chips, _font, IconFor(stat.Label), stat.Label + " " + value);
            }
        }

        private static string IconFor(string label)
        {
            if (label == "Tấn công") return HudSkin.StatAttack;
            if (label == "Phòng thủ") return HudSkin.StatDefense;
            return null;
        }
    }
}
