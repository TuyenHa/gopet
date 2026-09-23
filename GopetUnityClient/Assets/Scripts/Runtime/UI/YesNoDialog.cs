using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Dialog Có/Không đơn giản. Dùng cho huỷ đồ, confirm chi phí, etc.
    /// Khác <see cref="ChoiceDialogView"/> ở chỗ này là client-side thuần, không đến từ server.
    /// </summary>
    public sealed class YesNoDialog : MonoBehaviour
    {
        public event Action Confirmed;
        public event Action Cancelled;

        public static YesNoDialog Create(Transform parent, string message, string yesLabel = "Có", string noLabel = "Không")
        {
            var backdrop = new GameObject("YN Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var view = backdrop.AddComponent<YesNoDialog>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.Cancelled?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420f, 210f);
            // Viền 2 lớp như GamePopupFrame — Outline cũ làm viền nhoè và góc bị trắng.
            RoundedBorder.Apply(panel, 14f, PopupPalette.Panel, PopupPalette.Border, 2f);

            view.BuildContent(panel.transform, message, yesLabel, noLabel);
            view.BuildClose(panel.transform);
            return view;
        }

        private void BuildContent(Transform panel, string message, string yesLabel, string noLabel)
        {
            var font = UiBuilder.DefaultFont();
            var text = UiBuilder.MakeText(panel, font, "Message", 14, false);
            text.text = message;
            text.alignment = TextAnchor.MiddleCenter;
            // Nền trắng → chữ tối.
            text.color = PopupPalette.TextDark;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            // Kéo giãn cả hai chiều: từ trên hàng nút (12 + 40 + 12) tới dưới nút X. Bản cũ
            // neo cả hai mép theo đỉnh panel mà mép dưới lại cao hơn mép trên → cao âm,
            // chữ không bao giờ hiện.
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(28f, 64f);
            rect.offsetMax = new Vector2(-28f, -40f);

            MakeBtn(panel, font, yesLabel, new Color(0.75f, 0.3f, 0.3f, 1f), 0f, () => Confirmed?.Invoke());
            MakeBtn(panel, font, noLabel,  UiBuilder.ButtonFace,             1f, () => Cancelled?.Invoke());
        }

        /// <summary>Nút X dùng chung với khung popup — sát góc trên-phải, không nhoè.</summary>
        private void BuildClose(Transform panel) =>
            GamePopupFrame.CreateCloseButton(panel, UiBuilder.DefaultFont(), () => Cancelled?.Invoke());

        private static void MakeBtn(Transform panel, Font font, string label, Color color, float side, Action onClick)
        {
            var go = new GameObject($"Btn:{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(side, 0f);
            rect.anchorMax = new Vector2(side, 0f);
            rect.pivot = new Vector2(side, 0f);
            rect.sizeDelta = new Vector2(140f, 40f);
            rect.anchoredPosition = new Vector2(side == 0f ? 16f : -16f, 12f);
            var img = go.GetComponent<Image>();
            img.color = color;
            RoundedUiSprite.Apply(img);
            var t = UiBuilder.MakeText(go.transform, font, "Label", 14, true);
            t.text = label;
            UiBuilder.SetFontStyle(t, FontStyle.Bold);
            t.alignment = TextAnchor.MiddleCenter;
            // Nút nền màu đậm (đỏ/xanh) → chữ trắng.
            t.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
        }
    }
}
