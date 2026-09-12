using System;
using System.Text;
using Gopet.Net.Auth;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Ô nhập tên nhân vật, lọc theo đúng luật server và báo lỗi ngay dưới ô.</summary>
    public sealed class CharacterNameInput : MonoBehaviour
    {
        private InputField _field;
        private Text _errorLabel;
        private bool _guard;

        public bool IsValid { get; private set; }
        public string Value => _field == null ? string.Empty : _field.text ?? string.Empty;
        public InputField Field => _field;
        public string ErrorText => _errorLabel == null ? string.Empty : _errorLabel.text;
        public event Action<string> Changed;

        public static CharacterNameInput Create(Transform parent, Font font)
        {
            var go = new GameObject("CharacterNameInput", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var root = (RectTransform)go.transform;
            root.anchorMin = new Vector2(0.12f, 0f);
            root.anchorMax = new Vector2(0.88f, 0f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = Vector2.zero;

            var view = go.AddComponent<CharacterNameInput>();
            view.Build(font);
            return view;
        }

        public void SetServerError(string message)
        {
            if (_errorLabel == null) return;
            _errorLabel.text = message ?? string.Empty;
            _errorLabel.color = new Color(1f, 0.42f, 0.42f, 1f);
            _errorLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        }

        public static string Sanitize(string raw)
        {
            raw = raw ?? string.Empty;
            var builder = new StringBuilder(raw.Length);
            foreach (var c in raw)
            {
                if (c >= 'A' && c <= 'Z') builder.Append((char)(c + ('a' - 'A')));
                else if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) builder.Append(c);
            }
            return builder.ToString();
        }

        private void Build(Font font)
        {
            var fieldGo = new GameObject("Field", typeof(RectTransform), typeof(Image), typeof(InputField));
            fieldGo.transform.SetParent(transform, false);
            var fieldRect = (RectTransform)fieldGo.transform;
            fieldRect.anchorMin = new Vector2(0f, 0.22f);
            fieldRect.anchorMax = Vector2.one;
            fieldRect.offsetMin = Vector2.zero;
            fieldRect.offsetMax = Vector2.zero;
            var image = fieldGo.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.98f, 0.99f, 1f, 1f);
            var outline = fieldGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.68f, 0.73f, 0.79f, 1f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var text = UiBuilder.MakeText(fieldGo.transform, font, "Text", 18, true);
            text.color = new Color(0.14f, 0.24f, 0.36f, 1f);
            text.alignment = TextAnchor.MiddleLeft;
            AddHorizontalPadding(text.rectTransform, 48f);
            var placeholder = UiBuilder.MakeText(fieldGo.transform, font, "Placeholder", 18, true);
            placeholder.text = "Tên nhân vật (5-20, a-z 0-9)";
            placeholder.color = new Color(0.58f, 0.63f, 0.70f, 0.82f);
            placeholder.alignment = TextAnchor.MiddleLeft;
            AddHorizontalPadding(placeholder.rectTransform, 48f);
            MakeUserIcon(fieldGo.transform);

            _field = fieldGo.GetComponent<InputField>();
            _field.textComponent = text;
            _field.placeholder = placeholder;
            _field.characterLimit = AuthRules.MaxCharacterNameLength;
            _field.contentType = InputField.ContentType.Alphanumeric;
            _field.onValueChanged.AddListener(OnValueChanged);

            _errorLabel = UiBuilder.MakeText(transform, font, "Error", 13, true);
            var errorRect = (RectTransform)_errorLabel.transform;
            errorRect.anchorMin = new Vector2(0f, 0f);
            errorRect.anchorMax = new Vector2(1f, 0.20f);
            errorRect.offsetMin = new Vector2(4f, -5f);
            errorRect.offsetMax = new Vector2(-4f, -5f);
            _errorLabel.color = new Color(1f, 0.42f, 0.42f, 1f);
            _errorLabel.gameObject.SetActive(false);

            ValidateAndNotify(string.Empty);
        }

        private void OnValueChanged(string value)
        {
            if (_guard) return;

            var clean = Sanitize(value);
            if (!string.Equals(clean, value, StringComparison.Ordinal))
            {
                _guard = true;
                _field.text = clean;
                _guard = false;
            }

            ValidateAndNotify(clean);
        }

        private void ValidateAndNotify(string value)
        {
            IsValid = AuthRules.IsValidCharacterName(value, out var error);
            if (value.Length < AuthRules.MinCharacterNameLength)
                error = $"Tên nhân vật phải từ {AuthRules.MinCharacterNameLength} đến {AuthRules.MaxCharacterNameLength} ký tự.";

            _errorLabel.text = error ?? string.Empty;
            _errorLabel.gameObject.SetActive(value.Length > 0 && !IsValid && !string.IsNullOrEmpty(error));
            Changed?.Invoke(value);
        }

        private static void AddHorizontalPadding(RectTransform rect, float left)
        {
            rect.offsetMin = new Vector2(left, 0f);
            rect.offsetMax = new Vector2(-14f, 0f);
        }

        private static void MakeUserIcon(Transform parent)
        {
            var head = new GameObject("UserHead", typeof(RectTransform), typeof(Image));
            head.transform.SetParent(parent, false);
            var headRect = (RectTransform)head.transform;
            headRect.anchorMin = headRect.anchorMax = new Vector2(0f, 0.63f);
            headRect.anchoredPosition = new Vector2(25f, 0f);
            headRect.sizeDelta = new Vector2(10f, 10f);
            RoundedUiSprite.Apply(head.GetComponent<Image>());
            head.GetComponent<Image>().color = new Color(0.58f, 0.63f, 0.70f, 0.85f);
            head.GetComponent<Image>().raycastTarget = false;

            var body = new GameObject("UserBody", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(parent, false);
            var bodyRect = (RectTransform)body.transform;
            bodyRect.anchorMin = bodyRect.anchorMax = new Vector2(0f, 0.35f);
            bodyRect.anchoredPosition = new Vector2(25f, 0f);
            bodyRect.sizeDelta = new Vector2(20f, 10f);
            RoundedUiSprite.Apply(body.GetComponent<Image>());
            body.GetComponent<Image>().color = new Color(0.58f, 0.63f, 0.70f, 0.85f);
            body.GetComponent<Image>().raycastTarget = false;
        }
    }
}
