using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một dòng của menu: icon, tiêu đề, mô tả.
    ///
    /// <para>Dựng bằng code chứ không dùng prefab, để PlayMode test tạo được mà
    /// không cần asset nào — và để việc "dòng này trông thế nào" nằm trong tầm
    /// kiểm soát của test.</para>
    /// </summary>
    public sealed partial class MenuItemRow : MonoBehaviour
    {
        public const float Height = 64f;
        private const float IconSize = 56f;

        private Button _button;
        private RawImage _icon;
        private Text _title;
        private Text _description;
        private Image _attackBadge, _defenseBadge;
        private Text _attackText, _defenseText;
        private Image _background;
        private GameObject _divider;
        private bool _compactCard;
        private bool _lightCard;

        public int Index { get; private set; }

        public MenuItemInfo Item { get; private set; }

        /// <summary>Dòng có bấm được không. Dòng <c>canSelect = 0</c> hiện mờ và không nhận click.</summary>
        public bool Interactable => _button != null && _button.interactable;

        public void Bind(MenuItemInfo item, int index, RemoteAssetCache assets, Action<int> onClick,
            bool compactCard = false)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Index = index;

            _title.text = item.Title;
            _description.text = item.Description;
            SetCompactCard(compactCard);
            ApplyTitleStars();
            ApplyCompactStats(item.Description);

            // Dòng không cho chọn: mờ đi VÀ tắt nút. Chỉ làm mờ thôi thì vẫn bấm
            // được, và server sẽ im lặng bỏ qua — người chơi tưởng game treo.
            _button.interactable = item.CanSelect;
            SetFaded(!item.CanSelect);

            _button.onClick.RemoveAllListeners();
            if (item.CanSelect)
            {
                var captured = index;
                _button.onClick.AddListener(() => onClick(captured));
            }

            LoadIcon(item.ImagePath, assets);
        }

        public void SetCompactCard(bool value)
        {
            _compactCard = value;
            if (_background != null)
            {
                if (value)
                {
                    RoundedUiSprite.Apply(_background);
                    _background.color = new Color(0.14f, 0.15f, 0.18f, 0.96f);
                }
                else
                {
                    _background.sprite = null;
                    _background.type = Image.Type.Simple;
                    _background.color = UiBuilder.Panel;
                }
            }
            if (_title != null) _title.fontSize = value ? 17 : 20;
            LayoutTitleStars();
            if (_description != null) _description.gameObject.SetActive(!value);
            if (!value)
            {
                if (_attackBadge != null) _attackBadge.gameObject.SetActive(false);
                if (_defenseBadge != null) _defenseBadge.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Kiểu dòng cho danh sách nền sáng trong popup.
        ///
        /// <para>Dòng KHÔNG có nền riêng — khung chứa đã trắng sẵn. Tô nền đặc thì dòng
        /// đầu và dòng cuối phủ vuông lên bốn góc bo của khung, và mép dòng cắt vụn
        /// đường viền thành nét đứt. Phân tách nhau bằng vạch ngang ở chân dòng.</para>
        /// </summary>
        public void SetLightCard(bool value)
        {
            _lightCard = value;
            if (_background == null) return;

            if (value)
            {
                _background.sprite = null;
                _background.type = Image.Type.Simple;
                _background.color = Color.clear;
            }
            else
            {
                RoundedUiSprite.Apply(_background);
                _background.color = _compactCard
                    ? new Color(0.14f, 0.15f, 0.18f, 0.96f)
                    : UiBuilder.Panel;
            }

            if (_divider != null) _divider.SetActive(value);
            if (_title != null) _title.color = value ? PopupPalette.TextDark : UiBuilder.TextMain;
            if (_description != null)
                _description.color = value ? PopupPalette.TextMuted : UiBuilder.TextMuted;
        }

        /// <summary>Kẻ vạch ngăn dưới chân dòng. Dòng cuối không kẻ, nếu không thừa một nét lửng.</summary>
        public void SetSeparatorVisible(bool visible)
        {
            if (_divider != null) _divider.SetActive(_lightCard && visible);
        }

        private void ApplyCompactStats(string description)
        {
            if (!_compactCard) return;
            var parts = (description ?? string.Empty).Split(' ');
            var attack = FindStat(parts, "atk");
            var defense = FindStat(parts, "def");
            if (_attackBadge != null) _attackBadge.gameObject.SetActive(!string.IsNullOrEmpty(attack));
            if (_defenseBadge != null) _defenseBadge.gameObject.SetActive(!string.IsNullOrEmpty(defense));
            if (_attackText != null) _attackText.text = attack;
            if (_defenseText != null) _defenseText.text = defense;
        }

        private static string FindStat(string[] parts, string stat)
        {
            for (var i = 1; i < parts.Length; i++)
                if (parts[i].IndexOf(stat, StringComparison.OrdinalIgnoreCase) >= 0)
                    return parts[i - 1] + " " + parts[i];
            return string.Empty;
        }

        private void LoadIcon(string path, RemoteAssetCache assets)
        {
            // Xoá ảnh cũ ngay: dòng này vừa được tái dùng cho item khác, giữ lại
            // texture cũ là hiện nhầm icon cho tới khi ảnh mới về.
            if (_icon != null) _icon.texture = null;

            if (assets == null || string.IsNullOrEmpty(path)) return;

            // onReady có thể được gọi HAI lần (placeholder trước, ảnh thật sau) và
            // lần hai ở frame sau. Trong lúc đó người chơi có thể đã cuộn, dòng này
            // đã mang item khác — dán ảnh vào là dán nhầm. Chốt lại đường dẫn đang
            // hiển thị và bỏ qua mọi callback không còn khớp.
            var expected = path;

            assets.Get(path, ImagePackets.TypeIcon, _icon, texture =>
            {
                if (this == null || _icon == null) return;
                if (!ReferenceEquals(Item, null) && Item.ImagePath != expected) return;

                _icon.texture = texture;
            });
        }

        private void SetFaded(bool faded)
        {
            var alpha = faded ? 0.4f : 1f;
            _title.color = Tint(UiBuilder.TextMain, alpha);
            _description.color = Tint(UiBuilder.TextMuted, alpha);
            _icon.color = new Color(1f, 1f, 1f, alpha);
        }

        private static Color Tint(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
