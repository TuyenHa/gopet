using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Popup ATM dùng flow server: menu 1039/1040 và input 16/32.</summary>
    public sealed class AtmPopupView : MonoBehaviour
    {
        public const int AtmMenuId = 1039;
        public const int ExchangeGoldMenuId = 1040;
        public const int GoldToCoinDialogId = 16;
        public const int LuaToCoinDialogId = 32;

        private const float PopupWidth = 440f;
        private const float PopupHeight = 270f;
        private const float TabHeight = 34f;
        private const float CloseSize = 34f;

        private static readonly Color PanelBg = new Color(0.96f, 0.98f, 1f, 1f);
        private static readonly Color PanelBorder = new Color(0.28f, 0.6f, 1f, 1f);
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color DarkText = new Color(0.14f, 0.24f, 0.44f, 1f);

        private Font _font;
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private Transform _body;
        private Action _requestAtm;

        public event Action Closed;
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public static AtmPopupView Create(Transform parent, Font font, GuiderHandler guider,
            RemoteAssetCache assets, Action requestAtm)
        {
            var frame = new GameObject("AtmPopup", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(parent, false);
            var frameRect = (RectTransform)frame.transform;
            frameRect.anchorMin = frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(PopupWidth, PopupHeight);
            frameRect.anchoredPosition = Vector2.zero;

            var background = frame.GetComponent<Image>();
            RoundedUiSprite.Apply(background);
            background.color = PanelBg;
            var outline = frame.AddComponent<Outline>();
            outline.effectColor = PanelBorder;
            outline.effectDistance = new Vector2(2f, -2f);

            var view = frame.AddComponent<AtmPopupView>();
            view._font = font ?? UiBuilder.BuiltinFont();
            view._guider = guider ?? throw new ArgumentNullException(nameof(guider));
            view._assets = assets;
            view._requestAtm = requestAtm ?? throw new ArgumentNullException(nameof(requestAtm));
            view.BuildTab();
            view.BuildBody();
            view.BuildCloseButton();
            view.ShowLoading("Đang tải thông tin ATM...");
            return view;
        }

        public void RequestAtm()
        {
            ShowLoading("Đang tải thông tin ATM...");
            _requestAtm();
        }

        public bool TryConsumeListOption(ListOptionScreen screen)
        {
            if (screen == null || screen.ListId != AtmMenuId) return false;
            ShowAtmOptions(screen);
            return true;
        }

        public bool TryConsumeMenu(MenuScreen screen)
        {
            if (screen == null || screen.ListId != ExchangeGoldMenuId) return false;
            ClearBody();
            var menu = GenericMenuView.Create(_body, _font);
            menu.ConfirmRequested += (prompt, onYes) => ConfirmRequested?.Invoke(prompt, onYes);
            menu.CloseRequested += _ => RequestAtm();
            menu.EnableEmbeddedScroll(PopupHeight - TabHeight - 20f);
            menu.Bind(screen, _assets, _guider);
            return true;
        }

        public bool TryConsumeInput(InputDialogSpec spec)
        {
            if (spec == null || (spec.DialogId != GoldToCoinDialogId && spec.DialogId != LuaToCoinDialogId))
                return false;
            ShowExchangeInput(spec);
            return true;
        }

        private void BuildTab()
        {
            var tab = new GameObject("Tab_ATM", typeof(RectTransform), typeof(Image), typeof(Button));
            tab.transform.SetParent(transform, false);
            var rect = (RectTransform)tab.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(6f, -(6f + TabHeight));
            rect.offsetMax = new Vector2(-6f, -6f);
            var image = tab.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = TabActive;

            var label = UiBuilder.MakeText(tab.transform, _font, "Label", 15, true);
            label.text = "ATM";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.color = DarkText;
            tab.GetComponent<Button>().onClick.AddListener(RequestAtm);
        }

        private void BuildBody()
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(transform, false);
            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -TabHeight - 8f);
            var image = body.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = Color.white;
            _body = body.transform;
        }

        private void BuildCloseButton()
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(CloseSize, CloseSize);
            rect.anchoredPosition = new Vector2(4f, 4f);

            var image = go.GetComponent<Image>();
            image.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
            else
            {
                image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var fallback = UiBuilder.MakeText(go.transform, _font, "X", 18, true);
                fallback.text = "×";
                fallback.alignment = TextAnchor.MiddleCenter;
                fallback.fontStyle = FontStyle.Bold;
                fallback.color = Color.white;
            }
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

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
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = UiBuilder.ButtonFace;
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
