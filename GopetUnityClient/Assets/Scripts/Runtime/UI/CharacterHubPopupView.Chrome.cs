using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private void BuildTabs()
        {
            _tabBackgrounds = new Image[_tabNames.Length];
            const float padding = 12f;
            const float gap = 6f;
            var width = (PopupWidth - padding * 2f - gap * 3f) / 4f;
            for (var i = 0; i < _tabNames.Length; i++)
            {
                var go = new GameObject("Tab_" + _tabNames[i], typeof(RectTransform),
                    typeof(Image), typeof(Button));
                go.transform.SetParent(transform, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(width, TabHeight);
                rect.anchoredPosition = new Vector2(padding + i * (width + gap), -HeaderHeight - 6f);

                var image = go.GetComponent<Image>();
                RoundedUiSprite.Apply(image);
                image.color = TabInactive;
                _tabBackgrounds[i] = image;

                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(go.transform, false);
                var iconRect = (RectTransform)icon.transform;
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(21f, 21f);
                var labelWidth = TabLabelWidth((int)_tabOrder[i]);
                var groupLeft = (width - (21f + 6f + labelWidth)) * 0.5f;
                iconRect.anchoredPosition = new Vector2(groupLeft + 10.5f, 0f);
                var iconImage = icon.GetComponent<Image>();
                iconImage.sprite = HubIcon((int)_tabOrder[i] + 1);
                iconImage.preserveAspect = true;

                var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 14, true);
                label.text = _tabNames[i];
                label.alignment = TextAnchor.MiddleLeft;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
                label.color = TabText;
                var labelRect = label.rectTransform;
                labelRect.anchorMin = labelRect.anchorMax = new Vector2(0f, 0.5f);
                labelRect.pivot = new Vector2(0f, 0.5f);
                labelRect.sizeDelta = new Vector2(labelWidth, TabHeight);
                labelRect.anchoredPosition = new Vector2(groupLeft + 27f, 0f);
                var captured = _tabOrder[i];
                go.GetComponent<Button>().onClick.AddListener(() => SelectTab(captured));
            }
        }

        private void BuildBody()
        {
            var go = new GameObject("Body", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 12f);
            rect.offsetMax = new Vector2(-12f, -HeaderHeight - TabHeight - 14f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.985f, 0.99f, 1f, 1f);
            _body = go.transform;
        }

        private void BuildHeader()
        {
            var icon = new GameObject("Inventory icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0f, 1f);
            iconRect.sizeDelta = new Vector2(38f, 38f);
            iconRect.anchoredPosition = new Vector2(16f, -10f);
            var iconImage = icon.GetComponent<Image>();
            RoundedUiSprite.Apply(iconImage);
            iconImage.color = new Color(0.2f, 0.58f, 0.94f, 1f);

            iconImage.sprite = HubIcon(0);
            iconImage.preserveAspect = true;

            var title = UiBuilder.MakeText(transform, UiBuilder.DefaultFont(), "Header title", 18, false);
            title.text = "Hành trang";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.color = new Color(0.14f, 0.2f, 0.3f, 1f);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.sizeDelta = new Vector2(340f, 24f);
            titleRect.anchoredPosition = new Vector2(64f, -11f);

            var subtitle = UiBuilder.MakeText(transform, UiBuilder.DefaultFont(), "Header subtitle", 11, false);
            subtitle.text = "Quản lý vật phẩm của bạn";
            subtitle.color = new Color(0.32f, 0.39f, 0.5f, 1f);
            var subtitleRect = subtitle.rectTransform;
            subtitleRect.anchorMin = subtitleRect.anchorMax = new Vector2(0f, 1f);
            subtitleRect.pivot = new Vector2(0f, 1f);
            subtitleRect.sizeDelta = new Vector2(340f, 18f);
            subtitleRect.anchoredPosition = new Vector2(64f, -33f);

            var divider = new GameObject("Header divider", typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(transform, false);
            var dividerRect = (RectTransform)divider.transform;
            dividerRect.anchorMin = new Vector2(0f, 1f);
            dividerRect.anchorMax = new Vector2(1f, 1f);
            dividerRect.pivot = new Vector2(0.5f, 1f);
            dividerRect.offsetMin = new Vector2(12f, -HeaderHeight);
            dividerRect.offsetMax = new Vector2(-12f, -HeaderHeight + 1f);
            divider.GetComponent<Image>().color = new Color(0.83f, 0.88f, 0.95f, 1f);
        }

        private void BuildClose()
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(36f, 36f);
            rect.anchoredPosition = new Vector2(-18f, -18f);
            var image = go.GetComponent<Image>();
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null) image.sprite = sprite;
            else image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
            var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "X", 18, true);
            label.text = "×";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        private Transform MakePane(string name, float minX, float maxX)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_body, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(-5f, -5f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.94f, 0.97f, 1f, 1f);
            return go.transform;
        }

        private static Transform MakeListHost(Transform parent, float top)
        {
            var go = new GameObject("List", typeof(RectTransform), typeof(Image), typeof(Mask));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -top);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.45f);
            go.GetComponent<Mask>().showMaskGraphic = true;
            return go.transform;
        }

        private static Text MakeTitle(Transform parent, string value, Sprite icon = null)
        {
            var text = UiBuilder.MakeText(parent, UiBuilder.DefaultFont(), "Title", 15, false);
            text.text = value;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.alignment = icon == null ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            text.color = new Color(0.14f, 0.24f, 0.44f, 1f);
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(icon == null ? 6f : 42f, -34f);
            rect.offsetMax = new Vector2(-6f, -6f);
            if (icon != null)
            {
                var iconGo = new GameObject("Title icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(parent, false);
                var iconRect = (RectTransform)iconGo.transform;
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 1f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(20f, 20f);
                iconRect.anchoredPosition = new Vector2(22f, -20f);
                var iconImage = iconGo.GetComponent<Image>();
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
            }
            return text;
        }

        private static Button MakeAction(Transform parent, string label, float top, Sprite icon = null)
        {
            var go = new GameObject("Action_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(8f, -(top + 34f));
            rect.offsetMax = new Vector2(-8f, -top);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 13, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UiBuilder.TextMain;
            if (icon != null)
            {
                var buttonWidth = Mathf.Max(140f, ((RectTransform)parent).rect.width - 16f);
                var labelWidth = Mathf.Min(buttonWidth - 36f, label.Length * 7.2f);
                var groupLeft = (buttonWidth - (19f + 7f + labelWidth)) * 0.5f;
                var iconGo = new GameObject("Action icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(go.transform, false);
                var iconRect = (RectTransform)iconGo.transform;
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(19f, 19f);
                iconRect.anchoredPosition = new Vector2(groupLeft + 9.5f, 0f);
                var iconImage = iconGo.GetComponent<Image>();
                iconImage.sprite = icon;
                iconImage.preserveAspect = true;
                var textRect = text.rectTransform;
                textRect.anchorMin = textRect.anchorMax = new Vector2(0f, 0.5f);
                textRect.pivot = new Vector2(0f, 0.5f);
                textRect.sizeDelta = new Vector2(labelWidth, 34f);
                textRect.anchoredPosition = new Vector2(groupLeft + 26f, 0f);
                text.alignment = TextAnchor.MiddleLeft;
            }
            return go.GetComponent<Button>();
        }

        private static float TabLabelWidth(int index)
        {
            switch (index)
            {
                case 0: return 74f;
                case 1: return 24f;
                case 2: return 54f;
                case 3: return 46f;
                default: return 64f;
            }
        }

        private static Sprite HubIcon(int index)
        {
            if (index < 0 || index > 4) return null;
            if (_hubIcons[index] != null) return _hubIcons[index];

            var texture = Resources.Load<Texture2D>("Ui/Generated/character_hub_icons");
            if (texture == null) return null;

            // Sprite sheet tạo gồm 5 ô cùng bề rộng. Cắt phần trung tâm để loại
            // khoảng trong suốt dư thừa, nhờ vậy icon tab hiển thị đậm và rõ.
            var cellWidth = texture.width / 5f;
            var rect = new Rect(index * cellWidth, texture.height * 0.2f,
                cellWidth, texture.height * 0.6f);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Character hub icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _hubIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _hubIcons = new Sprite[5];

        private static Sprite HubActionIcon(int index)
        {
            if (index < 0 || index > 2) return null;
            if (_hubActionIcons[index] != null) return _hubActionIcons[index];
            var texture = Resources.Load<Texture2D>("Ui/Generated/character_hub_actions");
            if (texture == null) return null;
            var cellWidth = texture.width / 3f;
            var rect = new Rect(index * cellWidth, texture.height * 0.2f,
                cellWidth, texture.height * 0.6f);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Character hub action icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _hubActionIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _hubActionIcons = new Sprite[3];

        private static Sprite SettingsIcon(int index)
        {
            if (index < 0 || index > 7) return null;
            if (_settingsIcons[index] != null) return _settingsIcons[index];
            var texture = Resources.Load<Texture2D>("Ui/Generated/settings_icons");
            if (texture == null) return null;

            // Sprite sheet 4x2: speaker, music, effects, language / swords, lock, logout, exit.
            var cellWidth = texture.width / 4f;
            var cellHeight = texture.height / 2f;
            var column = index % 4;
            var rowFromBottom = index < 4 ? 1 : 0;
            var rect = new Rect(column * cellWidth, rowFromBottom * cellHeight,
                cellWidth, cellHeight);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Settings icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _settingsIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _settingsIcons = new Sprite[8];

        private static Sprite FriendsIcon(int index)
        {
            if (index < 0 || index > 2) return null;
            if (_friendsIcons[index] != null) return _friendsIcons[index];
            var texture = Resources.Load<Texture2D>("Ui/Generated/friends_icons");
            if (texture == null) return null;
            var cellWidth = texture.width / 3f;
            var rect = new Rect(index * cellWidth, 0f, cellWidth, texture.height);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Friends icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _friendsIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _friendsIcons = new Sprite[3];

    }
}
