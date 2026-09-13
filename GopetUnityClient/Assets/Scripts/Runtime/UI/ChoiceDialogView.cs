using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Hộp thoại chọn một đoạn text với N nút, dùng chung cho confirm và menu.</summary>
    public sealed class ChoiceDialogView : MonoBehaviour
    {
        private const float PanelWidth = 420f;
        private const float PanelHeight = 190f;
        private const float ButtonHeight = 42f;
        private const float ButtonGap = 12f;

        private readonly List<Button> _buttons = new List<Button>();
        private Text _message;
        private Transform _panel;
        private Font _font;
        private bool _decided;

        public event Action<int> Chosen;
        public event Action Closed;
        public string Message => _message == null ? null : _message.text;
        public IReadOnlyList<Button> Buttons => _buttons;

        public static ChoiceDialogView Create(Transform parent, Font font)
        {
            var backdrop = new GameObject("ChoiceDialogView", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            var backdropImage = backdrop.GetComponent<Image>();
            backdropImage.color = new Color(0f, 0f, 0f, 0.42f);
            backdropImage.raycastTarget = true;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            var panelImage = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(panelImage);
            panelImage.color = new Color(0.97f, 0.985f, 1f, 1f);
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.26f, 0.58f, 0.95f, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            var view = backdrop.AddComponent<ChoiceDialogView>();
            view._font = font;
            view._panel = panel.transform;

            var text = new GameObject("Message", typeof(RectTransform), typeof(Text));
            text.transform.SetParent(view._panel, false);
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = new Vector2(0f, 0.42f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(24f, 12f);
            textRect.offsetMax = new Vector2(-24f, -22f);
            view._message = text.GetComponent<Text>();
            view._message.font = font;
            view._message.fontSize = 18;
            view._message.alignment = TextAnchor.MiddleCenter;
            view._message.horizontalOverflow = HorizontalWrapMode.Wrap;
            view._message.verticalOverflow = VerticalWrapMode.Truncate;
            view._message.color = new Color(0.12f, 0.16f, 0.23f, 1f);
            view.BuildCloseButton();
            return view;
        }

        public void Bind(string message, IReadOnlyList<string> labels)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            _message.text = message ?? string.Empty;
            _decided = false;
            foreach (var button in _buttons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _buttons.Clear();

            var count = labels.Count;
            while (count > 1 && string.IsNullOrWhiteSpace(labels[count - 1])) count--;
            if (count == 0) count = 1;
            for (var i = 0; i < count; i++)
            {
                var label = i < labels.Count ? labels[i] : string.Empty;
                if (string.IsNullOrWhiteSpace(label)) label = i == 0 ? "OK" : "Huỷ";
                _buttons.Add(MakeButton(label, i, count));
            }
        }

        public void Choose(int index)
        {
            if (index < 0 || index >= _buttons.Count)
                throw new ArgumentOutOfRangeException(nameof(index), $"Nút {index} nằm ngoài hộp thoại ({_buttons.Count} nút).");
            if (_decided) return;
            _decided = true;
            Chosen?.Invoke(index);
        }

        private Button MakeButton(string label, int index, int count)
        {
            var go = new GameObject($"Button{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            var width = Mathf.Min(170f, (PanelWidth - 48f - ButtonGap * (count - 1)) / count);
            rect.sizeDelta = new Vector2(width, ButtonHeight);
            var totalWidth = count * width + (count - 1) * ButtonGap;
            rect.anchoredPosition = new Vector2(-totalWidth / 2f + width / 2f + index * (width + ButtonGap), 16f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = index == 0 ? UiBuilder.ButtonFace : new Color(0.86f, 0.88f, 0.92f, 1f);
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 16, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = index == 0 ? Color.white : new Color(0.14f, 0.18f, 0.25f, 1f);

            var button = go.GetComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => Choose(captured));
            return button;
        }

        private void BuildCloseButton()
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-8f, -8f);
            rect.sizeDelta = new Vector2(32f, 32f);

            var image = go.GetComponent<Image>();
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            else
            {
                RoundedUiSprite.Apply(image);
                image.color = new Color(0.9f, 0.12f, 0.1f, 1f);
                var fallback = UiBuilder.MakeText(go.transform, _font, "Label", 20, true);
                fallback.text = "×";
                fallback.alignment = TextAnchor.MiddleCenter;
                fallback.color = Color.white;
            }
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_decided) return;
                _decided = true;
                Closed?.Invoke();
            });
        }
    }
}
