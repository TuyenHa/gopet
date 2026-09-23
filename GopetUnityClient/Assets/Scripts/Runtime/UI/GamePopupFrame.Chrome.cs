using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Băng chân và nút đóng của <see cref="GamePopupFrame"/>. Tách khỏi file chính để
    /// mỗi file dưới 200 dòng.
    /// </summary>
    public sealed partial class GamePopupFrame
    {
        private void BuildFooter(Font font, string text)
        {
            var go = new GameObject("Footer", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            // Hẹp hơn hẳn vùng nội dung và căn giữa, đúng như ảnh mẫu — kéo dài hết bề
            // ngang thì nó trông như một dòng danh sách nữa chứ không phải chú thích.
            // Bề rộng ôm theo nội dung: câu dài ngắn khác nhau giữa các popup, đóng
            // cứng một con số thì hoặc cụt chữ hoặc thừa khoảng trắng hai bên.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, FooterHeight);
            rect.anchoredPosition = new Vector2(0f, FooterBottom);

            var bg = go.GetComponent<Image>();
            RoundedUiSprite.Apply(bg);
            bg.color = new Color(1f, 1f, 1f, 0.92f);
            bg.raycastTarget = false;

            // Xếp dấu chân và chữ thành một cụm rồi căn giữa cả cụm. Trước đây chữ căn
            // giữa cả băng còn icon neo cứng bên trái, nên câu dài là chữ chạy sát vào
            // icon, không còn khe nào.
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 14, 0, 0);
            layout.spacing = FooterIconGap;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var paw = PawBadge.CreateIcon(go.transform, FooterIconSize, Vector2.zero);
            if (paw != null)
            {
                var iconLayout = paw.AddComponent<LayoutElement>();
                iconLayout.preferredWidth = FooterIconSize;
                iconLayout.preferredHeight = FooterIconSize;
            }

            _footer = UiBuilder.MakeText(go.transform, font, "Label", 12, false);
            _footer.alignment = TextAnchor.MiddleCenter;
            _footer.color = PopupPalette.TextDark;
            _footer.raycastTarget = false;
            _footer.text = text;
            _footer.gameObject.AddComponent<LayoutElement>().preferredHeight = FooterHeight;
        }

        private void BuildCloseButton(Font font)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(transform, false);

            // Tâm nút NẰM TRONG popup, sát góc trên-phải: đo trên ảnh mẫu là lùi vào 6
            // và xuống 2 so với góc. Đẩy tâm ra ngoài góc thì hai phần ba nút lơ lửng
            // ngoài popup và đè lên cụm icon HUD phía trên.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(CloseSize, CloseSize);
            rect.anchoredPosition = new Vector2(-6f, -2f);

            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                // Không có sprite: nền tròn đỏ + chữ × trắng.
                img.sprite = CircleUiSprite.Get();
                img.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var xLabel = UiBuilder.MakeText(go.transform, font, "X", 18, true);
                xLabel.alignment = TextAnchor.MiddleCenter;
                xLabel.text = "×";
                xLabel.color = Color.white;
                UiBuilder.SetFontStyle(xLabel, FontStyle.Bold);
            }

            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }
    }
}
