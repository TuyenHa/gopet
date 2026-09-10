using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Màn hình tạo nhân vật: preview giới tính, tên, thông báo và reconnect feedback.</summary>
    public sealed class CharacterCreationView : MonoBehaviour
    {
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
            go.GetComponent<Image>().color = UiBuilder.JarBackground;

            var view = go.AddComponent<CharacterCreationView>();
            view.Build(font);
            return view;
        }

        public void SetServerNotice(string message)
        {
            if (_notice != null)
            {
                _notice.text = message ?? string.Empty;
                _notice.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
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
                _busyLabel.text = busy ? "Đang tạo nhân vật…" : string.Empty;
                _busyLabel.gameObject.SetActive(busy);
            }
        }

        private void Build(Font font)
        {
            var title = UiBuilder.MakeText(transform, font, "Title", 24, false);
            SetTop(title, 18f, 54f, 0.1f, 0.9f);
            title.text = "Chọn nhân vật";
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;

            var previewGo = CharacterPreviewPanel.Create(transform, new Vector2(460f, 190f));
            Preview = previewGo;
            var previewRect = (RectTransform)previewGo.transform;
            previewRect.anchorMin = previewRect.anchorMax = new Vector2(0.5f, 0.62f);
            previewRect.anchoredPosition = Vector2.zero;
            Preview.GenderChanged += _ => UpdateSubmitEnabled();

            NameInput = CharacterNameInput.Create(transform, font);
            var nameRect = (RectTransform)NameInput.transform;
            nameRect.anchorMin = nameRect.anchorMax = new Vector2(0.5f, 0.23f);
            nameRect.anchoredPosition = Vector2.zero;
            NameInput.Changed += _ => UpdateSubmitEnabled();

            _notice = UiBuilder.MakeText(transform, font, "Notice", 14, false);
            var noticeRect = (RectTransform)_notice.transform;
            noticeRect.anchorMin = new Vector2(0.12f, 0.14f);
            noticeRect.anchorMax = new Vector2(0.88f, 0.20f);
            noticeRect.offsetMin = noticeRect.offsetMax = Vector2.zero;
            _notice.alignment = TextAnchor.MiddleCenter;
            _notice.color = UiBuilder.TextMuted;
            _notice.gameObject.SetActive(false);

            _submit = MakeButton(transform, font, "Tạo nhân vật", new Vector2(0.26f, 0.06f), () =>
            {
                if (IsBusy || NameInput == null || !NameInput.IsValid) return;
                Submitted?.Invoke(Preview.SelectedGender, NameInput.Value);
            });
            _cancel = MakeButton(transform, font, "Quay lại", new Vector2(0.18f, 0.06f), () =>
            {
                if (!IsBusy) Cancelled?.Invoke();
            });
            ((RectTransform)_submit.transform).anchorMin = ((RectTransform)_submit.transform).anchorMax = new Vector2(0.39f, 0.08f);
            ((RectTransform)_cancel.transform).anchorMin = ((RectTransform)_cancel.transform).anchorMax = new Vector2(0.61f, 0.08f);

            _busyLabel = UiBuilder.MakeText(transform, font, "Busy", 15, false);
            var busyRect = (RectTransform)_busyLabel.transform;
            busyRect.anchorMin = new Vector2(0.1f, 0.29f);
            busyRect.anchorMax = new Vector2(0.9f, 0.35f);
            busyRect.offsetMin = busyRect.offsetMax = Vector2.zero;
            _busyLabel.alignment = TextAnchor.MiddleCenter;
            _busyLabel.color = new Color(1f, 0.8f, 0.2f, 1f);
            _busyLabel.gameObject.SetActive(false);

            _notice.text = "Tài khoản chưa có nhân vật. Đặt tên rồi chọn giới tính.";
            _notice.gameObject.SetActive(true);
            UpdateSubmitEnabled();
        }

        private void UpdateSubmitEnabled()
        {
            if (_submit != null) _submit.interactable = !IsBusy && NameInput != null && NameInput.IsValid;
        }

        private static void SetTop(Text text, float top, float height, float minX, float maxX)
        {
            var rect = (RectTransform)text.transform;
            rect.anchorMin = new Vector2(minX, 1f);
            rect.anchorMax = new Vector2(maxX, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, -(top + height));
            rect.offsetMax = new Vector2(0f, -top);
        }

        private static Button MakeButton(Transform parent, Font font, string label, Vector2 size, Action click)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size.x * 1000f, size.y * 800f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, font, "Label", 17, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => click());
            return button;
        }
    }
}
