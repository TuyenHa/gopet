using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public enum PetGridMode
    {
        Receive,
        Shop,
        Top
    }

    /// <summary>Danh sách pet dạng thẻ ngang dùng cho tab Nhận pet và Shop pet.</summary>
    public sealed class PetGridView : MonoBehaviour
    {
        private const float Gap = 4f;
        private const float CardHeight = 46f;
        private const float IconSize = 31f;
        private const float PortraitWidth = 50f;
        private const float InfoWidth = 90f;
        private const float TopInfoWidth = 200f;
        private const float StatWidth = 68f;
        private const float ActionWidth = 60f;

        private static readonly Color Blue = new Color(0.035f, 0.48f, 0.96f, 1f);
        private static readonly Color MainText = new Color(0.13f, 0.16f, 0.21f, 1f);
        private static readonly Color MutedText = new Color(0.32f, 0.35f, 0.4f, 1f);

        private readonly List<GameObject> _cards = new List<GameObject>();
        private Font _font;
        private RemoteAssetCache _assets;
        private GuiderHandler _guider;
        private MenuScreen _screen;
        private RectTransform _content;
        private ScrollRect _scrollRect;
        private string _buttonLabel = "Nhận";
        private PetGridMode _mode;

        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public static PetGridView Create(Transform parent, Font font)
        {
            var go = new GameObject("PetGrid", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(go.transform, false);
            var content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;

            var view = go.AddComponent<PetGridView>();
            view._font = font;
            view._content = content;
            view._scrollRect = go.GetComponent<ScrollRect>();
            view._scrollRect.viewport = (RectTransform)go.transform;
            view._scrollRect.content = content;
            view._scrollRect.horizontal = false;
            view._scrollRect.vertical = true;
            view._scrollRect.movementType = ScrollRect.MovementType.Clamped;
            view._scrollRect.scrollSensitivity = CardHeight * 0.75f;
            return view;
        }

        public void Bind(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider,
            string buttonLabel = "Nhận", PetGridMode mode = PetGridMode.Receive)
        {
            _screen = screen;
            _assets = assets;
            _guider = guider;
            _buttonLabel = string.IsNullOrEmpty(buttonLabel) ? "Nhận" : buttonLabel;
            _mode = mode;
            foreach (var card in _cards) if (card != null) Destroy(card);
            _cards.Clear();
            if (screen == null || screen.Items == null) return;

            for (var i = 0; i < screen.Items.Length; i++) BuildCard(screen.Items[i], i);
            _content.sizeDelta = new Vector2(0f,
                Mathf.Max(screen.Items.Length * (CardHeight + Gap), ((RectTransform)transform).rect.height));
            _content.anchoredPosition = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (_content == null || _cards.Count == 0) return;
            for (var i = 0; i < _cards.Count; i++)
            {
                var rect = (RectTransform)_cards[i].transform;
                rect.sizeDelta = new Vector2(0f, CardHeight);
                rect.anchoredPosition = new Vector2(0f, -i * (CardHeight + Gap));
            }
        }

        private void BuildCard(MenuItemInfo item, int index)
        {
            var go = new GameObject($"PetCard_{index}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_content, false);
            _cards.Add(go);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            // Anchor đã kéo kín theo chiều ngang; sizeDelta.x phải bằng 0.
            // Nếu cộng thêm chiều rộng content, nút Nhận/Mua sẽ bị đẩy ra ngoài viewport.
            rect.sizeDelta = new Vector2(0f, CardHeight);
            rect.anchoredPosition = new Vector2(0f, -index * (CardHeight + Gap));
            // Dùng hai lớp Image 9-slice thay cho Outline: Outline có thể bị hở nét
            // ở cạnh thẳng khi Canvas làm tròn theo pixel hoặc bị scale.
            var border = go.GetComponent<Image>();
            RoundedUiSprite.Apply(border);
            border.color = new Color(0.72f, 0.82f, 0.97f, 1f);
            var surface = new GameObject("Surface", typeof(RectTransform), typeof(Image));
            surface.transform.SetParent(go.transform, false);
            var surfaceRect = (RectTransform)surface.transform;
            UiBuilder.Stretch(surfaceRect);
            surfaceRect.offsetMin = new Vector2(1f, 1f);
            surfaceRect.offsetMax = new Vector2(-1f, -1f);
            var surfaceImage = surface.GetComponent<Image>();
            RoundedUiSprite.Apply(surfaceImage);
            surfaceImage.color = new Color(0.995f, 0.998f, 1f, 1f);
            surfaceImage.raycastTarget = false;

            var strength = FindStat(item.Description, "str", "sức mạnh");
            var agility = FindStat(item.Description, "agi", "độ nhanh");
            BuildPetIcon(go.transform, item);
            BuildInfo(go.transform, item);
            if (_mode == PetGridMode.Top)
            {
                BuildStat(go.transform, "hp-v2", "Máu tối đa", MaxHp(item.Description, strength), 0);
                BuildStat(go.transform, "mp", "Kỹ năng", MaxMp(item.Description, agility), 1);
            }
            else
            {
                BuildStat(go.transform, "strength-v2", "Sức mạnh", strength, 0);
                BuildStat(go.transform, "agility-v2", "Độ nhanh", agility, 1);
                BuildStat(go.transform, "hp-v2", "Máu tối đa", MaxHp(item.Description, strength), 2);
                BuildStat(go.transform, "mp", "Kỹ năng", MaxMp(item.Description, agility), 3);
                BuildAction(go.transform, item, index);
            }
        }

        private void BuildPetIcon(Transform parent, MenuItemInfo item)
        {
            var go = new GameObject("PetIcon", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            rect.anchoredPosition = new Vector2(10f, 0f);
            var icon = go.GetComponent<RawImage>();
            icon.texture = _assets == null ? null : _assets.Placeholder;
            if (_assets != null && !string.IsNullOrEmpty(item.ImagePath))
            {
                var expected = item.ImagePath;
                _assets.Get(expected, ImagePackets.TypeIcon, texture =>
                {
                    if (this != null && icon != null && item.ImagePath == expected) icon.texture = texture;
                });
            }

        }

        private void BuildInfo(Transform parent, MenuItemInfo item)
        {
            var infoWidth = _mode == PetGridMode.Top ? TopInfoWidth : InfoWidth;
            var title = UiBuilder.MakeText(parent, _font, "Title", 8, false);
            title.text = Shorten(CapitalizeFirst(item.Title));
            title.color = Blue;
            title.fontStyle = FontStyle.Bold;
            Place(title.rectTransform, PortraitWidth, 16f, -6f, infoWidth);

            var description = UiBuilder.MakeText(parent, _font, "Description", 6, false);
            description.text = _mode == PetGridMode.Shop
                ? ShopElementLabel(item.Description)
                : _mode == PetGridMode.Top
                    ? TopRankLabel(item.Description)
                    : CleanDescription(item.Description);
            description.color = MutedText;
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.verticalOverflow = VerticalWrapMode.Truncate;
            Place(description.rectTransform, PortraitWidth, 27f, -21f, infoWidth);
        }

        private void BuildStat(Transform parent, string iconName, string label, string value, int column)
        {
            var infoWidth = _mode == PetGridMode.Top ? TopInfoWidth : InfoWidth;
            var start = PortraitWidth + infoWidth + column * StatWidth;
            var separator = new GameObject($"Separator_{iconName}", typeof(RectTransform), typeof(Image));
            separator.transform.SetParent(parent, false);
            var separatorRect = (RectTransform)separator.transform;
            separatorRect.anchorMin = new Vector2(0f, 0f);
            separatorRect.anchorMax = new Vector2(0f, 1f);
            separatorRect.pivot = new Vector2(0f, 0.5f);
            separatorRect.sizeDelta = new Vector2(1f, -12f);
            separatorRect.anchoredPosition = new Vector2(start, 0f);
            separator.GetComponent<Image>().color = new Color(0.87f, 0.91f, 0.96f, 1f);

            var icon = new GameObject($"{iconName}Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(parent, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.sizeDelta = new Vector2(15f, 15f);
            iconRect.anchoredPosition = new Vector2(start + 5f, 0f);
            var iconImage = icon.GetComponent<Image>();
            var iconPath = iconName == "mp"
                ? "Jar/Art/Raw/pet/battle/potion"
                : $"Ui/PetStats/{iconName}";
            iconImage.sprite = Resources.Load<Sprite>(iconPath);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            var text = UiBuilder.MakeText(parent, _font, $"{iconName}Text", 7, false);
            text.text = label + "\n<size=9><color=#087EF4>" + (value ?? "—") + "</color></size>";
            text.supportRichText = true;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = MainText;
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            text.rectTransform.pivot = new Vector2(0f, 0.5f);
            text.rectTransform.sizeDelta = new Vector2(StatWidth - 23f, 31f);
            text.rectTransform.anchoredPosition = new Vector2(start + 23f, -1f);
        }

        private void BuildAction(Transform parent, MenuItemInfo item, int index)
        {
            var go = new GameObject("Action", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(ActionWidth, 24f);
            rect.anchoredPosition = new Vector2(-5f, 0f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = item.CanSelect ? Blue : new Color(0.65f, 0.69f, 0.75f, 1f);
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 7, true);
            text.text = ActionLabel(item);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            var captured = index;
            go.GetComponent<Button>().interactable = item.CanSelect;
            go.GetComponent<Button>().onClick.AddListener(() => SelectItem(item, captured));
        }

        private void SelectItem(MenuItemInfo item, int index)
        {
            if (_screen == null || _guider == null || !item.CanSelect) return;
            Action send = () => _guider.Select(_screen, index,
                item.PaymentOptions != null && item.PaymentOptions.Length > 0 ? 0 : -1);
            if (item.ShowDialog && ConfirmRequested != null)
                ConfirmRequested(MenuSelection.PromptFor(_screen, index), send);
            else
                send();
        }

        private string ActionLabel(MenuItemInfo item)
        {
            return _buttonLabel;
        }

        private static void Place(RectTransform rect, float left, float height, float top, float width)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left, top);
        }

        private static string CapitalizeFirst(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            value = value.Trim();
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string Shorten(string value)
        {
            const int maxLength = 20;
            return value.Length > maxLength ? value.Substring(0, maxLength - 3) + "..." : value;
        }

        private static string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Thông tin pet";
            var cleaned = Regex.Replace(description,
                @"(?:[+-]?\d+(?:\.\d+)?\s*\(\s*(?:str|int|agi|hp|mp)\s*\)|\(\s*(?:str|int|agi|hp|mp)\s*\)\s*[+-]?\d+(?:\.\d+)?)",
                string.Empty, RegexOptions.IgnoreCase).Trim(' ', ',', ';', '-', '|');
            return string.IsNullOrWhiteSpace(cleaned) ? "Pet đồng hành" : cleaned;
        }

        private static string ShopElementLabel(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Hệ: —";
            var match = Regex.Match(description, @"Hệ\s*:\s*([^.。,;\r\n]+)", RegexOptions.IgnoreCase);
            return match.Success ? "Hệ: " + match.Groups[1].Value.Trim() : "Hệ: —";
        }

        private static string TopRankLabel(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Hạng: —";
            var match = Regex.Match(description,
                @"Hạng\s*(\d+)\s*:\s*Cấp\s*(\d+)\s*hiện\s*có\s*([\d.,]+)\s*kinh\s*nghiệm",
                RegexOptions.IgnoreCase);
            return match.Success
                ? $"Hạng {match.Groups[1].Value}: Cấp {match.Groups[2].Value}, Kinh nghiệm {match.Groups[3].Value}"
                : "Hạng: —";
        }

        private static string FindStat(string value, string shortName, string displayName)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var pattern = $@"(?:([+-]?\d+(?:\.\d+)?)\s*\(\s*{Regex.Escape(shortName)}\s*\)|\(\s*{Regex.Escape(shortName)}\s*\)\s*([+-]?\d+(?:\.\d+)?)|{Regex.Escape(displayName)}\s*[:=]?\s*([+-]?\d+(?:\.\d+)?))";
            var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            for (var i = 1; i < match.Groups.Count; i++)
                if (match.Groups[i].Success) return match.Groups[i].Value;
            return null;
        }

        private static string MaxHp(string description, string strength)
        {
            var explicitHp = FindStat(description, "hp", "máu tối đa");
            if (!string.IsNullOrEmpty(explicitHp)) return explicitHp;
            return int.TryParse(strength, out var baseStrength)
                ? (baseStrength * 4 + 23).ToString()
                : null;
        }

        private static string MaxMp(string description, string agility)
        {
            var explicitMp = FindStat(description, "mp", "MP");
            if (!string.IsNullOrEmpty(explicitMp)) return explicitMp;
            return int.TryParse(agility, out var baseAgility)
                ? (baseAgility * 5 + 22).ToString()
                : null;
        }
    }
}
