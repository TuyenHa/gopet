using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class CharacterCreationView : MonoBehaviour
    {
        private const string DefaultHint = "Lưu ý: Tên nhân vật không thể thay đổi sau khi tạo.";
        private static readonly Color Blue = new Color(0.16f, 0.53f, 0.91f, 1f);
        private static readonly Color Green = new Color(0.29f, 0.68f, 0.31f, 1f);
        private static readonly Color Navy = new Color(0.12f, 0.23f, 0.36f, 1f);

        private Text _notice;
        private Text _busyLabel;
        private Button _submit;
        private Button _cancel;
        public CharacterPreviewPanel Preview { get; private set; }
        public CharacterNameInput NameInput { get; private set; }
        public Button SubmitButton => _submit;
        public Button CancelButton => _cancel;
        public string NoticeText => _notice == null ? string.Empty : _notice.text;
        public bool IsBusy { get; private set; }
        public event Action<sbyte, string> Submitted;
        public event Action Cancelled;

        public static CharacterCreationView Create(Transform parent, Font font)
        {
            var go = new GameObject("CharacterCreationView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = Color.clear;
            LoginBackground.Create(go.transform);
            var view = go.AddComponent<CharacterCreationView>();
            view.Build(font);
            return view;
        }

        public void SetServerNotice(string message)
        {
            if (_notice != null)
            {
                _notice.text = string.IsNullOrWhiteSpace(message) ? DefaultHint : message;
                _notice.color = string.IsNullOrWhiteSpace(message)
                    ? new Color(0.48f, 0.54f, 0.62f, 1f) : new Color(0.85f, 0.20f, 0.20f, 1f);
            }
            NameInput?.SetServerError(message);
        }

        public void ShowNotice(string message) => SetServerNotice(message);

        public void SetBusy(bool busy)
        {
            IsBusy = busy;
            if (_submit != null) _submit.interactable = !busy && NameInput != null && NameInput.IsValid;
            if (_cancel != null) _cancel.interactable = !busy;
            if (NameInput?.Field != null) NameInput.Field.interactable = !busy;
            if (_busyLabel != null)
            {
                _busyLabel.text = busy ? "Đang tạo nhân vật..." : string.Empty;
                _busyLabel.gameObject.SetActive(busy);
            }
        }

        private void Build(Font font)
        {
            var panel = MakePanel();
            MakeRibbon(panel, font);

            var subtitle = MakeText(panel, font, "Subtitle", 17, new Color(0.42f, 0.48f, 0.56f, 1f));
            SetRect(subtitle.rectTransform, 0.10f, 0.79f, 0.90f, 0.87f);
            subtitle.text = "◆  Tạo nhân vật để bắt đầu hành trình của bạn!  ◆";

            Preview = CharacterPreviewPanel.Create(panel, Vector2.zero, font);
            SetRect((RectTransform)Preview.transform, 0.08f, 0.41f, 0.92f, 0.78f);
            Preview.GenderChanged += _ => UpdateSubmitEnabled();

            var nameTitle = MakeText(panel, font, "NameTitle", 18, Navy);
            SetRect(nameTitle.rectTransform, 0.20f, 0.33f, 0.80f, 0.40f);
            nameTitle.text = "◆ ──  Tên nhân vật  ── ◆";
            UiBuilder.SetFontStyle(nameTitle, FontStyle.Bold);

            NameInput = CharacterNameInput.Create(panel, font);
            SetRect((RectTransform)NameInput.transform, 0.12f, 0.19f, 0.88f, 0.32f);
            NameInput.Changed += _ => UpdateSubmitEnabled();

            _notice = MakeText(panel, font, "Notice", 13, new Color(0.48f, 0.54f, 0.62f, 1f));
            SetRect(_notice.rectTransform, 0.10f, 0.13f, 0.90f, 0.18f);
            _notice.text = DefaultHint;

            _submit = MakeButton(panel, font, "＋  Tạo nhân vật", 0.12f, 0.49f, Green, Color.white, false, Submit);
            _cancel = MakeButton(panel, font, "↶  Quay lại", 0.51f, 0.88f, Color.white, Blue, true, Cancel);

            _busyLabel = MakeText(panel, font, "Busy", 15, Blue);
            SetRect(_busyLabel.rectTransform, 0.25f, 0.40f, 0.75f, 0.45f);
            _busyLabel.gameObject.SetActive(false);
            UpdateSubmitEnabled();
        }

        private void Submit()
        {
            if (!IsBusy && NameInput != null && NameInput.IsValid)
                Submitted?.Invoke(Preview.SelectedGender, NameInput.Value);
        }

        private void Cancel()
        {
            if (!IsBusy) Cancelled?.Invoke();
        }

        private void UpdateSubmitEnabled()
        {
            if (_submit != null) _submit.interactable = !IsBusy && NameInput != null && NameInput.IsValid;
        }

        private RectTransform MakePanel()
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter), typeof(Shadow));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.04f);
            rect.anchorMax = new Vector2(0.5f, 0.96f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.95f, 0.98f, 1f, 0.98f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Blue;
            outline.effectDistance = new Vector2(6f, -6f);
            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0.04f, 0.20f, 0.38f, 0.25f);
            shadow.effectDistance = new Vector2(0f, -6f);
            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = 1.18f;
            return rect;
        }

        private static void MakeRibbon(Transform panel, Font font)
        {
            var go = new GameObject("TitleRibbon", typeof(RectTransform), typeof(Image), typeof(Shadow));
            go.transform.SetParent(panel, false);
            SetRect((RectTransform)go.transform, 0.25f, 0.88f, 0.75f, 0.97f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = Blue;
            go.GetComponent<Shadow>().effectColor = new Color(0.05f, 0.25f, 0.50f, 0.25f);
            var title = MakeText(go.transform, font, "Title", 28, Color.white);
            UiBuilder.Stretch(title.rectTransform);
            title.text = "Chọn nhân vật";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
        }

        private static Button MakeButton(Transform parent, Font font, string label, float left, float right,
            Color face, Color textColor, bool outlined, Action click)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Shadow));
            go.transform.SetParent(parent, false);
            SetRect((RectTransform)go.transform, left, 0.025f, right, 0.115f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = face;
            if (outlined)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = Blue;
                outline.effectDistance = new Vector2(2f, -2f);
            }
            var text = MakeText(go.transform, font, "Label", 18, textColor);
            UiBuilder.Stretch(text.rectTransform);
            text.text = label;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => click());
            return button;
        }

        private static Text MakeText(Transform parent, Font font, string name, int size, Color color)
        {
            var text = UiBuilder.MakeText(parent, font, name, size, false);
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        private static void SetRect(RectTransform rect, float x1, float y1, float x2, float y2)
        {
            rect.anchorMin = new Vector2(x1, y1);
            rect.anchorMax = new Vector2(x2, y2);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
