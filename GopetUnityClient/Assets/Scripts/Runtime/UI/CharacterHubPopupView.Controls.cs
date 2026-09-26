using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        /// <param name="top">Khoảng chừa từ đỉnh thân popup (vd chỗ cho khay tab con).</param>
        private Transform MakePane(string name, float minX, float maxX, float top = 5f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_body, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(-5f, -top);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.94f, 0.97f, 1f, 1f);
            return go.transform;
        }

        /// <summary>Lề trong pane quanh khay tab con — khớp lề 6 của <see cref="MakeListHost"/>.</summary>
        private const float SubRailInset = 6f;

        /// <summary>
        /// Khay tab con (<see cref="PopupTabRail"/>, cùng style popup Chợ trời) ghim đỉnh
        /// <paramref name="pane"/>. <paramref name="columnWidth"/> là bề ngang CỘT (phần
        /// anchor), trừ lề pane 5×2 và lề khay <see cref="SubRailInset"/>×2 ra bề rộng khay.
        /// </summary>
        private static PopupTabRail MakeSubRail(Transform pane, float columnWidth, string[] labels)
        {
            var holder = new GameObject("SubTabs", typeof(RectTransform));
            holder.transform.SetParent(pane, false);
            var rect = (RectTransform)holder.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(SubRailInset, SubRailInset);
            rect.offsetMax = new Vector2(-SubRailInset, -SubRailInset);
            var railWidth = columnWidth - 10f - SubRailInset * 2f;
            return PopupTabRail.Create(rect, UiBuilder.DefaultFont(), railWidth, labels);
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
