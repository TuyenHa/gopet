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
    public sealed class MenuItemRow : MonoBehaviour
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
        private bool _compactCard;

        public int Index { get; private set; }

        public MenuItemInfo Item { get; private set; }

        /// <summary>Dòng có bấm được không. Dòng <c>canSelect = 0</c> hiện mờ và không nhận click.</summary>
        public bool Interactable => _button != null && _button.interactable;

        public static MenuItemRow Create(Transform parent, Font font)
        {
            var go = new GameObject("MenuItemRow", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var row = go.AddComponent<MenuItemRow>();
            row._background = go.GetComponent<Image>();
            row._background.color = UiBuilder.Panel;
            row._button = go.GetComponent<Button>();

            // Neo trải ngang, ghim mép trên: chiều rộng bám theo vùng chứa, chiều
            // cao cố định.
            //
            // Để anchor mặc định (giữa) với sizeDelta.x = 0 thì dòng RỘNG 0 — vẫn
            // hiện chữ vì chữ tự vẽ, nhưng không nhận được click nào. Và test gọi
            // thẳng OnRowClicked thì không đời nào phát hiện ra.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, Height);

            row._icon = MakeChild<RawImage>(go.transform, "Icon", new Vector2(IconSize, IconSize));
            row._title = MakeText(go.transform, "Title", font, 20);
            row._description = MakeText(go.transform, "Description", font, 14);
            row._description.color = UiBuilder.TextMuted;
            row._attackBadge = MakeBadge(go.transform, "Attack badge", font, out row._attackText,
                new Color(0.29f, 0.52f, 0.16f, 1f));
            row._defenseBadge = MakeBadge(go.transform, "Defense badge", font, out row._defenseText,
                new Color(0.18f, 0.4f, 0.73f, 1f));

            var iconRect = row._icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(4f, 0f);

            PlaceText(row._title.rectTransform, 66f, 30f, -3f);
            PlaceText(row._description.rectTransform, 66f, 24f, -32f);
            row.PlaceBadges();
            row.SetCompactCard(false);

            return row;
        }

        public void Bind(MenuItemInfo item, int index, RemoteAssetCache assets, Action<int> onClick,
            bool compactCard = false)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Index = index;

            _title.text = item.Title;
            _description.text = item.Description;
            SetCompactCard(compactCard);
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
            if (_description != null) _description.gameObject.SetActive(!value);
            if (!value)
            {
                if (_attackBadge != null) _attackBadge.gameObject.SetActive(false);
                if (_defenseBadge != null) _defenseBadge.gameObject.SetActive(false);
            }
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

            assets.Get(path, ImagePackets.TypeIcon, texture =>
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

        private static T MakeChild<T>(Transform parent, string name, Vector2 size) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(T));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = size;
            return go.GetComponent<T>();
        }

        private static Text MakeText(Transform parent, string name, Font font, int size)
        {
            var text = MakeChild<Text>(parent, name, new Vector2(400f, 24f));
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleLeft;
            return text;
        }

        private static Image MakeBadge(Transform parent, string name, Font font, out Text text, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = color;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(76f, 17f);
            text = MakeText(go.transform, "Label", font, 9);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            UiBuilder.Stretch(text.rectTransform);
            return image;
        }

        private void PlaceBadges()
        {
            if (_attackBadge != null) _attackBadge.rectTransform.anchoredPosition = new Vector2(66f, 5f);
            if (_defenseBadge != null) _defenseBadge.rectTransform.anchoredPosition = new Vector2(145f, 5f);
        }

        private static void PlaceText(RectTransform rect, float left, float height, float top)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(left, -(height - top));
            rect.offsetMax = new Vector2(-8f, top);
        }
    }
}
