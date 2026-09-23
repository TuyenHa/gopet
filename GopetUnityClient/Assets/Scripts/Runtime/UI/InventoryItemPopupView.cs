using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Chi tiết một ô rương đồ, dùng cùng chrome sáng của popup Hành trang.</summary>
    public sealed class InventoryItemPopupView : MonoBehaviour
    {
        public static InventoryItemPopupView Create(Transform parent, MenuItemInfo item,
            RemoteAssetCache assets, Action use)
        {
            var overlay = new GameObject("Inventory item details", typeof(RectTransform),
                typeof(Image), typeof(Button));
            overlay.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)overlay.transform);
            overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.56f);

            var view = overlay.AddComponent<InventoryItemPopupView>();
            overlay.GetComponent<Button>().onClick.AddListener(() => Destroy(view.gameObject));
            view.Build(item, assets, use);
            return view;
        }

        private void Build(MenuItemInfo item, RemoteAssetCache assets, Action use)
        {
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(transform, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(510f, 224f);
            RoundedUiSprite.Apply(card.GetComponent<Image>());
            card.GetComponent<Image>().color = new Color(0.97f, 0.985f, 1f, 1f);
            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            BuildIcon(card.transform, item, assets);
            BuildText(card.transform, item);
            BuildClose(card.transform);
            BuildButtons(card.transform, item.CanSelect, use);
        }

        private static void BuildIcon(Transform parent, MenuItemInfo item, RemoteAssetCache assets)
        {
            var box = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(parent, false);
            var rect = (RectTransform)box.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(88f, 88f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            RoundedUiSprite.Apply(box.GetComponent<Image>());
            box.GetComponent<Image>().color = new Color(0.9f, 0.92f, 0.95f, 1f);

            var icon = new GameObject("Image", typeof(RectTransform), typeof(RawImage));
            icon.transform.SetParent(box.transform, false);
            UiBuilder.Stretch((RectTransform)icon.transform);
            var raw = icon.GetComponent<RawImage>();
            raw.raycastTarget = false;
            if (assets == null || string.IsNullOrEmpty(item.ImagePath)) return;
            var expected = item.ImagePath;
            assets.Get(expected, ImagePackets.TypeIcon, raw, texture =>
            {
                if (raw != null && item.ImagePath == expected) raw.texture = texture;
            });
        }

        private static void BuildText(Transform parent, MenuItemInfo item)
        {
            var title = UiBuilder.MakeText(parent, UiBuilder.DefaultFont(), "Title", 19, false);
            title.text = item.Title ?? "Vật phẩm";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.color = new Color(0.14f, 0.17f, 0.22f, 1f);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.sizeDelta = new Vector2(365f, 32f);
            titleRect.anchoredPosition = new Vector2(122f, -20f);

            var line = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            UiBuilder.PlaceRow((RectTransform)line.transform, 51f, 1f, 122f);
            line.GetComponent<Image>().color = new Color(0.8f, 0.84f, 0.89f, 1f);

            var info = UiBuilder.MakeText(parent, UiBuilder.DefaultFont(), "Info", 15, false);
            info.text = $"• {item.Description ?? "Chưa có mô tả"}\n• Mã vật phẩm: {item.ItemId}\n• Có thể sử dụng: {(item.CanSelect ? "Có" : "Không")}";
            info.color = new Color(0.2f, 0.23f, 0.28f, 1f);
            info.alignment = TextAnchor.UpperLeft;
            info.horizontalOverflow = HorizontalWrapMode.Wrap;
            info.verticalOverflow = VerticalWrapMode.Overflow;
            var infoRect = info.rectTransform;
            infoRect.anchorMin = new Vector2(0f, 0f);
            infoRect.anchorMax = new Vector2(1f, 1f);
            infoRect.offsetMin = new Vector2(122f, 56f);
            infoRect.offsetMax = new Vector2(-22f, -62f);
        }

        private void BuildClose(Transform parent)
        {
            var close = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            close.transform.SetParent(parent, false);
            var rect = (RectTransform)close.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(34f, 34f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            var image = close.GetComponent<Image>();
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null) image.sprite = sprite;
            else
            {
                image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                RoundedUiSprite.Apply(image);
            }
            var label = UiBuilder.MakeText(close.transform, UiBuilder.DefaultFont(), "Label", 28, true);
            label.text = "×";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            close.GetComponent<Button>().onClick.AddListener(() => Destroy(gameObject));
        }

        private void BuildButtons(Transform parent, bool canUse, Action use)
        {
            MakeButton(parent, "Dùng", new Vector2(-78f, 18f), canUse,
                new Color(0.25f, 0.34f, 0.46f, 1f), () =>
                {
                    use?.Invoke();
                    Destroy(gameObject);
                });
            MakeButton(parent, "Hủy", new Vector2(72f, 18f), true,
                new Color(0.9f, 0.9f, 0.91f, 1f), () => Destroy(gameObject));
        }

        private static void MakeButton(Transform parent, string label, Vector2 position, bool enabled,
            Color color, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(132f, 40f);
            rect.anchoredPosition = position;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = enabled ? color : new Color(color.r, color.g, color.b, 0.45f);
            var button = go.GetComponent<Button>();
            button.interactable = enabled;
            button.onClick.AddListener(() => onClick());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 17, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.color = label == "Hủy" ? new Color(0.25f, 0.28f, 0.34f, 1f) : Color.white;
        }
    }
}
