using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private void BuildTabs()
        {
            _tabBackgrounds = new Image[_tabNames.Length];
            const float padding = 8f;
            const float gap = 4f;
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
                rect.anchoredPosition = new Vector2(padding + i * (width + gap), -padding);

                var image = go.GetComponent<Image>();
                RoundedUiSprite.Apply(image);
                image.color = TabInactive;
                _tabBackgrounds[i] = image;

                var label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 14, true);
                label.text = _tabNames[i];
                label.alignment = TextAnchor.MiddleCenter;
                label.fontStyle = FontStyle.Bold;
                label.color = TabText;
                var captured = (CharacterHubTab)i;
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
            rect.offsetMin = new Vector2(8f, 8f);
            rect.offsetMax = new Vector2(-8f, -TabHeight - 12f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = Color.white;
            _body = go.transform;
        }

        private void BuildClose()
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(36f, 36f);
            rect.anchoredPosition = new Vector2(5f, 5f);
            var image = go.GetComponent<Image>();
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null) image.sprite = sprite;
            else image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
            var label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "X", 18, true);
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
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -6f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.91f, 0.95f, 1f, 1f);
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

        private static Text MakeTitle(Transform parent, string value)
        {
            var text = UiBuilder.MakeText(parent, UiBuilder.BuiltinFont(), "Title", 15, false);
            text.text = value;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.14f, 0.24f, 0.44f, 1f);
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(6f, -34f);
            rect.offsetMax = new Vector2(-6f, -6f);
            return text;
        }

        private static Button MakeAction(Transform parent, string label, float top)
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
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 13, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UiBuilder.TextMain;
            return go.GetComponent<Button>();
        }
    }
}
