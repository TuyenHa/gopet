using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Phần dựng hình của <see cref="MenuItemRow"/>. Tách khỏi file chính để mỗi file
    /// dưới 200 dòng.
    /// </summary>
    public sealed partial class MenuItemRow
    {
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
            row._divider = MakeDivider(go.transform);
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

        private static GameObject MakeDivider(Transform parent)
        {
            var go = new GameObject("Divider", typeof(RectTransform), typeof(Image));
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
            go.SetActive(false);
            return go;
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
