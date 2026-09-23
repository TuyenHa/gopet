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

    }
}
