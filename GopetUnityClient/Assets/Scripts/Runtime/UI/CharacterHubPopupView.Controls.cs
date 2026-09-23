using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
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

    }
}
