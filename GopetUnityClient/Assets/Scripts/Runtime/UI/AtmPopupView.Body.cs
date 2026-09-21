using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nội dung bên trong popup Dịch vụ: danh sách hình thức quy đổi, ô nhập số lượng
    /// và màn chờ. Tách khỏi file chính để mỗi file dưới 200 dòng.
    /// </summary>
    public sealed partial class AtmPopupView
    {
        private void ShowAtmOptions(ListOptionScreen screen)
        {
            ClearBody();
            var title = MakeText("Title", "Chọn hình thức quy đổi", 15, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(title.rectTransform, 8f, 28f, 12f);

            for (var i = 0; i < screen.Options.Length; i++)
            {
                var optionIndex = i;
                var optionLabel = AtmOptionLabel(i, screen.Options[i].Text);
                var button = MakeButton("Option_" + i, optionLabel, 42f + i * 52f, 44f);
                button.onClick.AddListener(() =>
                {
                    ShowLoading("Đang mở " + optionLabel.ToLowerInvariant() + "...");
                    _guider.Select(screen, optionIndex);
                });
            }
        }

        private void ShowExchangeInput(InputDialogSpec spec)
        {
            ClearBody();
            var title = MakeText("ExchangeTitle", spec.Title, 15, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(title.rectTransform, 10f, 32f, 12f);

            var fieldLabel = spec.Fields != null && spec.Fields.Length > 0
                ? spec.Fields[0].Label
                : "Số lượng:";
            var label = MakeText("FieldLabel", fieldLabel, 13, FontStyle.Normal);
            UiBuilder.PlaceRow(label.rectTransform, 48f, 22f, 18f);

            var fieldGo = new GameObject("AmountField", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(InputField));
            fieldGo.transform.SetParent(_body, false);
            UiBuilder.PlaceRow((RectTransform)fieldGo.transform, 72f, 42f, 18f);
            var fieldImage = fieldGo.GetComponent<Image>();
            RoundedUiSprite.Apply(fieldImage);
            fieldImage.color = Color.white;
            var fieldOutline = fieldGo.GetComponent<Outline>();
            fieldOutline.effectColor = new Color(0.55f, 0.61f, 0.7f, 1f);
            fieldOutline.effectDistance = new Vector2(1f, -1f);

            var inputText = UiBuilder.MakeText(fieldGo.transform, _font, "Text", 15, true);
            inputText.color = DarkText;
            inputText.supportRichText = false;
            inputText.rectTransform.offsetMin = new Vector2(12f, 0f);
            inputText.rectTransform.offsetMax = new Vector2(-12f, 0f);

            var placeholder = UiBuilder.MakeText(fieldGo.transform, _font, "Placeholder", 14, true);
            placeholder.text = "Nhập số lượng";
            placeholder.color = new Color(0.55f, 0.6f, 0.68f, 1f);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.rectTransform.offsetMin = new Vector2(12f, 0f);
            placeholder.rectTransform.offsetMax = new Vector2(-12f, 0f);

            var input = fieldGo.GetComponent<InputField>();
            input.textComponent = inputText;
            input.placeholder = placeholder;
            input.contentType = InputField.ContentType.IntegerNumber;
            input.lineType = InputField.LineType.SingleLine;

            var error = MakeText("Validation", string.Empty, 12, FontStyle.Normal);
            error.color = new Color(0.78f, 0.2f, 0.2f, 1f);
            error.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(error.rectTransform, 116f, 22f, 12f);

            var submit = MakeButton("Submit", "Đổi", 142f, 44f, 120f);
            submit.onClick.AddListener(() =>
            {
                var value = input.text == null ? string.Empty : input.text.Trim();
                if (!long.TryParse(value, out var amount) || amount <= 0)
                {
                    error.text = "Vui lòng nhập số lượng lớn hơn 0.";
                    return;
                }

                error.text = string.Empty;
                submit.interactable = false;
                _guider.SubmitInput(spec.DialogId, new[] { value });
            });
            input.Select();
            input.ActivateInputField();
        }

        private void ShowLoading(string message)
        {
            ClearBody();
            var text = MakeText("Loading", message, 14, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            UiBuilder.Stretch(text.rectTransform);
        }

        private Text MakeText(string name, string value, int size, FontStyle style)
        {
            var text = UiBuilder.MakeText(_body, _font, name, size, false);
            text.text = value;
            text.color = DarkText;
            text.fontStyle = style;
            return text;
        }

        private Button MakeButton(string name, string label, float top, float height, float width = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_body, false);
            var rect = (RectTransform)go.transform;
            if (width <= 0f)
            {
                UiBuilder.PlaceRow(rect, top, height, 16f);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = new Vector2(0f, -top);
            }
            // Xanh sáng của bảng màu popup chứ không phải navy xám của
            // UiBuilder.ButtonFace — popup này dùng chung tông với cửa hàng.
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 6f);
            image.color = PopupPalette.ButtonBlue;
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            return go.GetComponent<Button>();
        }

        private void ClearBody()
        {
            if (_body == null) return;
            for (var i = _body.childCount - 1; i >= 0; i--)
                Destroy(_body.GetChild(i).gameObject);
        }

        private static string AtmOptionLabel(int index, string serverLabel)
        {
            switch (index)
            {
                case 0: return "Đổi số dư → vàng";
                case 1: return "Đổi vàng → ngọc";
                case 2: return "Đổi lượng → ngọc";
                default: return string.IsNullOrWhiteSpace(serverLabel) ? "Quy đổi" : serverLabel;
            }
        }
    }
}
