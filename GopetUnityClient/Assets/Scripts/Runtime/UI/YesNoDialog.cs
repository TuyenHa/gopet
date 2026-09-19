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
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.96f, 0.98f, 1f, 1f);
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            view.BuildContent(panel.transform, message, yesLabel, noLabel);
            view.BuildClose(panel.transform);
            return view;
        }

        private void BuildContent(Transform panel, string message, string yesLabel, string noLabel)
        {
            var font = UiBuilder.BuiltinFont();
            var text = UiBuilder.MakeText(panel, font, "Message", 14, false);
            text.text = message;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(28f, 86f);
            rect.offsetMax = new Vector2(-28f, -46f);

            MakeBtn(panel, font, yesLabel, new Color(0.75f, 0.3f, 0.3f, 1f), 0f, () => Confirmed?.Invoke());
            MakeBtn(panel, font, noLabel,  UiBuilder.ButtonFace,             1f, () => Cancelled?.Invoke());
        }

        private void BuildClose(Transform panel)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(34f, 34f);
            rect.anchoredPosition = new Vector2(-18f, -18f);
            var image = go.GetComponent<Image>();
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null) image.sprite = sprite;
            else
            {
                image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                RoundedUiSprite.Apply(image);
            }
            var label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "X", 18, true);
            label.text = "×";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => Cancelled?.Invoke());
        }

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
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.black;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
        }
    }
}
