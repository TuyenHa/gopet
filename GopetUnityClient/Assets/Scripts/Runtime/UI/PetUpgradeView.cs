using System;
using Gopet.Net;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Màn chọn hai pet, xem trước và xác nhận tiến hoá.</summary>
    public sealed class PetUpgradeView : MonoBehaviour
    {
        private Text _activeText;
        private Text _materialText;
        private Text _priceText;
        private Text _previewText;
        private InputField _name;
        private Button _previewButton;
        private Button _confirmButton;
        private int _activeId;
        private int _materialId;

        public event Action<sbyte> SelectRequested;
        public event Action<int, int> PreviewRequested;
        public event Action<int, int, string> ConfirmRequested;
        public event Action CloseRequested;

        public int ActivePetId => _activeId;
        public int MaterialPetId => _materialId;
        public string PreviewText => _previewText != null ? _previewText.text : string.Empty;

        public static PetUpgradeView Create(Transform parent, Font font)
        {
            var backdrop = new GameObject("PetUpgradeView", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(390f, 430f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());
            panel.GetComponent<Image>().color = UiBuilder.Panel;

            var view = backdrop.AddComponent<PetUpgradeView>();
            view.Build(panel.transform, font ?? UiBuilder.BuiltinFont());
            return view;
        }

        public void ApplySelection(PetUpgradeSelection selection)
        {
            if (selection == null) return;
            if (selection.Role == GopetCmd.PET_UPGRADE_ACTIVE)
            {
                _activeId = selection.PetId;
                _activeText.text = $"Pet chính: #{selection.PetId}";
            }
            else if (selection.Role == GopetCmd.PET_UPGRADE_PASSIVE)
            {
                _materialId = selection.PetId;
                _materialText.text = $"Pet nguyên liệu: #{selection.PetId}";
            }
            RefreshButtons();
        }

        public void ApplyPrice(PetUpgradePrice price)
        {
            if (price != null) _priceText.text = $"Chi phí: {price.Gold:N0} vàng";
        }

        public void ApplyPreview(PetUpgradePreview preview)
        {
            if (preview == null) return;
            _previewText.text = preview.Title + (preview.Lines.Length == 0
                ? string.Empty
                : "\n" + string.Join("\n", preview.Lines));
            _confirmButton.interactable = _activeId > 0 && _materialId > 0;
        }

        private void Build(Transform panel, Font font)
        {
            var title = TextRow(panel, font, "Tiến hoá pet", 14f, 32f, 19);
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            ButtonRow(panel, font, "Đóng", 14f, 34f, 54f, () => CloseRequested?.Invoke());

            _activeText = TextRow(panel, font, "Pet chính: chưa chọn", 62f, 30f, 14);
            ButtonRow(panel, font, "Chọn pet chính", 96f, 40f, 16f,
                () => SelectRequested?.Invoke(GopetCmd.PET_UPGRADE_ACTIVE));
            _materialText = TextRow(panel, font, "Pet nguyên liệu: chưa chọn", 142f, 30f, 14);
            ButtonRow(panel, font, "Chọn pet nguyên liệu", 176f, 40f, 16f,
                () => SelectRequested?.Invoke(GopetCmd.PET_UPGRADE_PASSIVE));

            _priceText = TextRow(panel, font, "Đang tải chi phí…", 222f, 26f, 13);
            _priceText.color = new Color(1f, 0.82f, 0.3f, 1f);
            _name = MakeNameField(panel, font, 252f);
            _previewButton = ButtonRow(panel, font, "Xem trước", 294f, 38f, 16f,
                () => PreviewRequested?.Invoke(_activeId, _materialId));
            _previewText = TextRow(panel, font, "Chọn đủ hai pet để xem kết quả.", 337f, 46f, 12);
            _previewText.alignment = TextAnchor.UpperCenter;
            _previewText.color = UiBuilder.TextMuted;
            _confirmButton = ButtonRow(panel, font, "Tiến hoá", 386f, 34f, 16f, Confirm);
            RefreshButtons();
        }

        private void Confirm()
        {
            var petName = (_name.text ?? string.Empty).Trim();
            if (petName.Length < 5 || petName.Length >= 30)
            {
                _previewText.text = "Tên pet mới phải dài từ 5 đến 29 ký tự.";
                return;
            }
            ConfirmRequested?.Invoke(_activeId, _materialId, petName);
        }

        private void RefreshButtons()
        {
            var ready = _activeId > 0 && _materialId > 0 && _activeId != _materialId;
            if (_previewButton != null) _previewButton.interactable = ready;
            if (_confirmButton != null) _confirmButton.interactable = false;
            if (_activeId > 0 && _activeId == _materialId)
                _previewText.text = "Pet chính và pet nguyên liệu phải khác nhau.";
        }

        private static Text TextRow(Transform parent, Font font, string value, float top, float height, int size)
        {
            var text = UiBuilder.MakeText(parent, font, value, size, false);
            text.text = value;
            UiBuilder.PlaceRow(text.rectTransform, top, height, 16f);
            return text;
        }

        private static Button ButtonRow(Transform parent, Font font, string label, float top,
            float height, float side, Action clicked)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, top, height, side);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, font, "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => clicked());
            return button;
        }

        private static InputField MakeNameField(Transform parent, Font font, float top)
        {
            var go = new GameObject("PetName", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, top, 36f, 16f);
            go.GetComponent<Image>().color = UiBuilder.Field;
            var input = go.GetComponent<InputField>();
            input.characterLimit = 29;
            input.textComponent = UiBuilder.MakeText(go.transform, font, "Text", 14, true);
            input.textComponent.rectTransform.offsetMin = new Vector2(10f, 0f);
            input.textComponent.rectTransform.offsetMax = new Vector2(-10f, 0f);
            var placeholder = UiBuilder.MakeText(go.transform, font, "Placeholder", 13, true);
            placeholder.text = "Tên pet mới (5–29 ký tự)";
            placeholder.color = UiBuilder.TextMuted;
            placeholder.rectTransform.offsetMin = new Vector2(10f, 0f);
            input.placeholder = placeholder;
            return input;
        }
    }
}
