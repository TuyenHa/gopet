using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Modal mất kết nối, dùng cùng bảng màu và panel bo góc của popup thành phố.</summary>
    public sealed class ConnectionPopupView : MonoBehaviour
    {
        private const float Width = 360f;
        private const float Height = 190f;

        private Text _detail;
        private Button _retry;

        public event Action RetryRequested;

        public string Detail => _detail == null ? null : _detail.text;
        public Button RetryButton => _retry;

        public static ConnectionPopupView Create(Transform parent, Font font)
        {
            var root = new GameObject("ConnectionPopup", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0.03f, 0.1f, 0.2f, 0.22f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image), typeof(Outline));
            frame.transform.SetParent(root.transform, false);
            var frameRect = (RectTransform)frame.transform;
            frameRect.anchorMin = frameRect.anchorMax = frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(Width, Height);

            var frameImage = frame.GetComponent<Image>();
            RoundedUiSprite.Apply(frameImage);
            frameImage.color = new Color(0.96f, 0.98f, 1f, 1f);
            frame.GetComponent<Outline>().effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            frame.GetComponent<Outline>().effectDistance = new Vector2(2f, -2f);

            var view = root.AddComponent<ConnectionPopupView>();
            MakeLabel(frame.transform, font, "Title", "Mất kết nối", 25, FontStyle.Bold,
                new Vector2(16f, -50f), new Vector2(-16f, -14f), new Color(0.14f, 0.24f, 0.44f, 1f));
            view._detail = MakeLabel(frame.transform, font, "Detail", string.Empty, 15, FontStyle.Normal,
                new Vector2(22f, -110f), new Vector2(-22f, -58f), UiBuilder.TextMuted);
            view._detail.alignment = TextAnchor.MiddleCenter;
            view._detail.horizontalOverflow = HorizontalWrapMode.Wrap;
            view._detail.verticalOverflow = VerticalWrapMode.Overflow;
            view._retry = MakeRetryButton(frame.transform, font, view.RequestRetry);
            return view;
        }

        public void Show(string detail)
        {
            _detail.text = string.IsNullOrWhiteSpace(detail) ? "Không thể kết nối tới máy chủ." : detail;
            _retry.interactable = true;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void RequestRetry()
        {
            if (!_retry.interactable) return;

            _retry.interactable = false;
            RetryRequested?.Invoke();
        }

        private static Text MakeLabel(Transform parent, Font font, string name, string value, int size,
            FontStyle style, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var label = UiBuilder.MakeText(parent, font, name, size, stretch: false);
            var rect = (RectTransform)label.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            label.text = value;
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, style);
            label.color = color;
            return label;
        }

        private static Button MakeRetryButton(Transform parent, Font font, Action retry)
        {
            var go = new GameObject("Retry", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(152f, 38f);
            rect.anchoredPosition = new Vector2(0f, 16f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.35f, 0.65f, 1f, 1f);
            var label = UiBuilder.MakeText(go.transform, font, "Label", 17, stretch: true);
            label.text = "Thử lại";
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = Color.white;

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => retry());
            return button;
        }
    }
}
