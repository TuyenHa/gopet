using System;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Modal chọn nguyên liệu cho cường hoá (2 material: template + crystal) hoặc
    /// tiến hoá (2 material: template + tier stone).
    ///
    /// <para>Chưa có picker nguyên liệu từ túi — hiện dùng 2 InputField nhập itemId
    /// trực tiếp (dev-mode). Khi có <c>PET_INVENTORY</c> handler + item picker view
    /// đầy đủ, thay 2 field bằng 2 nút "Chọn nguyên liệu" mở picker.</para>
    /// </summary>
    public sealed class EnchantEvolveView : MonoBehaviour
    {
        public enum Mode { Enchant, UpTier }

        public event Action<int, int> Confirmed; // (matA, matB)
        public event Action CloseRequested;

        public static EnchantEvolveView Create(Transform parent, Mode mode, int itemId, string itemName)
        {
            var backdrop = new GameObject($"{mode} Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var view = backdrop.AddComponent<EnchantEvolveView>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 250f);
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            view.BuildContent(panel.transform, mode, itemName);
            return view;
        }

        private InputField _fieldA, _fieldB;
        private Text _errorText;

        private void BuildContent(Transform panel, Mode mode, string itemName)
        {
            var font = UiBuilder.BuiltinFont();
            var title = UiBuilder.MakeText(panel, font, "Title", 16, false);
            title.text = mode == Mode.Enchant ? $"Cường hoá {itemName}" : $"Tiến hoá {itemName}";
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UiBuilder.TextMain;
            SetRect(title.rectTransform, 8f, 24f);

            var hint = UiBuilder.MakeText(panel, font, "Hint", 11, false);
            hint.text = mode == Mode.Enchant
                ? "Nhập itemId 2 nguyên liệu (template + crystal)."
                : "Nhập itemId 2 nguyên liệu để tiến lên tier tiếp theo.";
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = UiBuilder.TextMuted;
            SetRect(hint.rectTransform, 40f, 30f);

            _fieldA = MakeField(panel, font, mode == Mode.Enchant ? "Material ID" : "Material 1 ID", 78f);
            _fieldB = MakeField(panel, font, mode == Mode.Enchant ? "Crystal ID" : "Material 2 ID", 118f);

            _errorText = UiBuilder.MakeText(panel, font, "Error", 11, false);
            _errorText.alignment = TextAnchor.MiddleCenter;
            _errorText.color = new Color(1f, 0.4f, 0.4f, 1f);
            SetRect(_errorText.rectTransform, 158f, 18f);

            MakeSubmit(panel, font, mode == Mode.Enchant ? "Cường hoá" : "Tiến hoá", 186f);
        }

        private static InputField MakeField(Transform panel, Font font, string placeholder, float top)
        {
            var go = new GameObject($"Field:{placeholder}", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(panel, false);
            SetRect((RectTransform)go.transform, top, 32f);
            go.GetComponent<Image>().color = UiBuilder.Field;
            var input = go.GetComponent<InputField>();
            input.contentType = InputField.ContentType.IntegerNumber;
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

        private void MakeSubmit(Transform panel, Font font, string label, float top)
        {
            var go = new GameObject("Submit", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            SetRect((RectTransform)go.transform, top, 40f);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.7f, 0.55f, 0.15f, 1f);
            RoundedUiSprite.Apply(img);
            var t = UiBuilder.MakeText(go.transform, font, "Label", 15, true);
            t.text = label;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(TrySubmit);
        }

        private void TrySubmit()
        {
            if (!int.TryParse(_fieldA.text, out var a) || !int.TryParse(_fieldB.text, out var b))
            {
                _errorText.text = "Nhập số hợp lệ cho cả 2 material.";
                return;
            }
            if (a <= 0 || b <= 0)
            {
                _errorText.text = "Material ID phải > 0.";
                return;
            }
            Confirmed?.Invoke(a, b);
            _errorText.text = "Đã gửi. Đợi server phản hồi…";
            _errorText.color = new Color(0.6f, 0.85f, 0.5f, 1f);
        }

        public void ApplyServerMaterial(PetEquipMaterialSelection material)
        {
            if (material == null) return;
            var field = material.Slot == 7 ? _fieldA : _fieldB;
            field.text = material.ItemOrTemplateId.ToString();
            _errorText.text = $"Đã chọn {material.Name}";
            _errorText.color = UiBuilder.TextMuted;
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
