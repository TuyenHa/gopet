using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        /// <summary>Khe giữa mép trên Content (ngay dưới badge tiêu đề) và hàng tab.</summary>
        private const float TabTop = 4f;
        private const float TabGap = 6f;

        private void BuildTabs(Transform content, float contentWidth)
        {
            _tabBackgrounds = new Image[_tabNames.Length];
            var width = (contentWidth - TabGap * 3f) / 4f;
            for (var i = 0; i < _tabNames.Length; i++)
            {
                var go = new GameObject("Tab_" + _tabNames[i], typeof(RectTransform),
                    typeof(Image), typeof(Button));
                go.transform.SetParent(content, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(width, TabHeight);
                rect.anchoredPosition = new Vector2(i * (width + TabGap), -TabTop);

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

        private void BuildBody(Transform content)
        {
            var go = new GameObject("Body", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -TabTop - TabHeight - 8f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.985f, 0.99f, 1f, 1f);
            _body = go.transform;
        }
    }
}
