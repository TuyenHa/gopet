using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class PetGridView
    {
        private static void BuildRowDivider(Transform parent, bool visible)
        {
            var go = new GameObject("RowDivider", typeof(RectTransform), typeof(Image));
            go.SetActive(visible);
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = new Vector2(8f, 0f);
            rect.offsetMax = new Vector2(-8f, 1f);

            var image = go.GetComponent<Image>();
            image.color = PopupPalette.Hairline;
            image.raycastTarget = false;
        }

        private void BuildPetIcon(Transform parent)
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
        }

        private void BuildInfo(Transform parent, MenuItemInfo item)
        {
            var infoWidth = _mode == PetGridMode.Top ? TopInfoWidth : InfoWidth;
            var title = UiBuilder.MakeText(parent, _font, "Title", 8, false);
            title.text = Shorten(CapitalizeFirst(item.Title));
            title.color = Blue;
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
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

        private void BuildAction(Transform parent, MenuItemInfo item)
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
            text.text = _buttonLabel;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            go.GetComponent<Button>().interactable = item.CanSelect;
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

    }
}
