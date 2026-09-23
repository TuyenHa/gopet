using System;
using Gopet.Net;
using Gopet.Net.Auth;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Form đổi mật khẩu 3 field password. Verify local (new == confirm) trước khi gửi.
    /// Gói đi bằng <see cref="ChangePasswordPackets.ChangePassword"/>.
    /// </summary>
    public sealed class ChangePasswordView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action<string> ErrorRaised;

        public Action<Message> Send;

        private InputField _old, _new, _confirm;
        private Text _errorText;

        public static ChangePasswordView Create(Transform parent)
        {
            var backdrop = new GameObject("Change Password Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var view = backdrop.AddComponent<ChangePasswordView>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 260f);
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            view.BuildContent(panel.transform);
            return view;
        }

        private void BuildContent(Transform panel)
        {
            var font = UiBuilder.DefaultFont();

            var title = UiBuilder.MakeText(panel, font, "Title", 16, false);
            title.text = "Đổi mật khẩu";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UiBuilder.TextMain;
            SetRect(title.rectTransform, 8f, 24f);

            _old     = MakeField(panel, font, "Mật khẩu hiện tại",   40f);
            _new     = MakeField(panel, font, "Mật khẩu mới",        84f);
            _confirm = MakeField(panel, font, "Nhập lại mật khẩu",  128f);

            _errorText = UiBuilder.MakeText(panel, font, "Error", 11, false);
            _errorText.alignment = TextAnchor.MiddleCenter;
            _errorText.color = new Color(1f, 0.4f, 0.4f, 1f);
            SetRect(_errorText.rectTransform, 172f, 18f);

            MakeSubmit(panel, font, 200f);
        }

        private static InputField MakeField(Transform panel, Font font, string placeholder, float top)
        {
            var go = new GameObject($"Field:{placeholder}", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(panel, false);
            SetRect((RectTransform)go.transform, top, 34f);
            go.GetComponent<Image>().color = UiBuilder.Field;
            var input = go.GetComponent<InputField>();
            input.contentType = InputField.ContentType.Password;
            input.characterLimit = 32;

            var text = UiBuilder.MakeText(go.transform, font, "Text", 14, true);
            text.color = UiBuilder.TextMain;
            text.rectTransform.offsetMin = new Vector2(10f, 0f);
            text.rectTransform.offsetMax = new Vector2(-10f, 0f);
            input.textComponent = text;

            var ph = UiBuilder.MakeText(go.transform, font, "Placeholder", 13, true);
            ph.text = placeholder;
            ph.color = UiBuilder.TextMuted;
            ph.fontStyle = FontStyle.Italic;
            ph.rectTransform.offsetMin = new Vector2(10f, 0f);
            ph.rectTransform.offsetMax = new Vector2(-10f, 0f);
            input.placeholder = ph;
            return input;
        }

        private void MakeSubmit(Transform panel, Font font, float top)
        {
            var go = new GameObject("Submit", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            SetRect((RectTransform)go.transform, top, 40f);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.55f, 0.85f, 1f);
            RoundedUiSprite.Apply(img);

            var label = UiBuilder.MakeText(go.transform, font, "Label", 15, true);
            label.text = "Đổi mật khẩu";
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;

            go.GetComponent<Button>().onClick.AddListener(TrySubmit);
        }

        private void TrySubmit()
        {
            if (string.IsNullOrEmpty(_old.text) || string.IsNullOrEmpty(_new.text))
            {
                _errorText.text = "Vui lòng nhập đủ mật khẩu";
                return;
            }
            if (_new.text != _confirm.text)
            {
                _errorText.text = "Mật khẩu mới nhập lại không khớp";
                ErrorRaised?.Invoke(_errorText.text);
                return;
            }
            if (_new.text.Length < 6)
            {
                _errorText.text = "Mật khẩu mới cần ít nhất 6 ký tự";
                return;
            }

            Send?.Invoke(ChangePasswordPackets.ChangePassword(_old.text, _new.text));
            _errorText.text = "Đã gửi. Chờ server phản hồi…";
            _errorText.color = new Color(0.6f, 0.85f, 0.5f, 1f);
        }

        private static void SetRect(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(16f, -(top + height));
            rect.offsetMax = new Vector2(-16f, -top);
        }
    }
}
