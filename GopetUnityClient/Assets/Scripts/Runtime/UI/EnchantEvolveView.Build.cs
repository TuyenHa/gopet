using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Ô nhập xám bo góc 6 và nút xác nhận của <see cref="EnchantEvolveView"/>.</summary>
    public sealed partial class EnchantEvolveView
    {
        private const float FieldHeight = 34f;
        private const float FieldRadius = 6f;
        private const float SubmitWidth = 170f;
        private const float SubmitHeight = 36f;

        private static readonly Color FieldFill = new Color(0.90f, 0.91f, 0.93f, 1f);
        private static readonly Color FieldBorder = new Color(0.78f, 0.80f, 0.84f, 1f);
        private static readonly Color FieldText = new Color(0.16f, 0.20f, 0.28f, 1f);

        /// <summary>Ô nhập số: nền xám, viền xám đậm hơn một nấc, bo góc 6.</summary>
        private static InputField MakeField(Transform parent, Font font, string placeholder, float top)
        {
            var go = new GameObject($"Field:{placeholder}", typeof(RectTransform), typeof(Image),
                typeof(InputField));
            go.transform.SetParent(parent, false);
            PlaceTop((RectTransform)go.transform, top, FieldHeight, 8f);
            // Viền vẽ bằng 2 lớp (RoundedBorder) chứ không Outline — Outline làm góc nhoè/trắng.
            var fill = RoundedBorder.Apply(go, FieldRadius, FieldFill, FieldBorder, 1f);

            var input = go.GetComponent<InputField>();
            input.targetGraphic = fill;
            input.contentType = InputField.ContentType.IntegerNumber;

            var text = UiBuilder.MakeText(go.transform, font, "Text", 14, true);
            text.color = FieldText;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            text.rectTransform.offsetMin = new Vector2(12f, 0f);
            text.rectTransform.offsetMax = new Vector2(-12f, 0f);
            input.textComponent = text;

            var ph = UiBuilder.MakeText(go.transform, font, "Placeholder", 13, true);
            ph.text = placeholder;
            ph.color = PopupPalette.TextMuted;
            ph.alignment = TextAnchor.MiddleLeft;
            ph.rectTransform.offsetMin = new Vector2(12f, 0f);
            ph.rectTransform.offsetMax = new Vector2(-12f, 0f);
            input.placeholder = ph;
            return input;
        }

        /// <summary>Nút chính ở chân popup: viên thuốc xanh dương như nút "Tạo hình xăm".</summary>
        private void MakeSubmit(Transform parent, Font font, string label)
        {
            var go = new GameObject("Submit", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(SubmitWidth, SubmitHeight);
            rect.anchoredPosition = Vector2.zero;
            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius, PopupPalette.ButtonBlue,
                PopupPalette.HeaderBlue, 2f);

            var text = UiBuilder.MakeText(go.transform, font, "Label", 15, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            go.GetComponent<Button>().onClick.AddListener(TrySubmit);
        }

        /// <summary>Neo theo mép trên cha, kéo ngang và chừa lề <paramref name="side"/> hai bên.</summary>
        private static void PlaceTop(RectTransform rect, float top, float height, float side)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(side, -(top + height));
            rect.offsetMax = new Vector2(-side, -top);
        }
    }
}
