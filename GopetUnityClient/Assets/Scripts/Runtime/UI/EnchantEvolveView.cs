using System;
using Gopet.Net.Pet;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup chọn nguyên liệu cho cường hoá (template + crystal) hoặc tiến hoá (2 nguyên
    /// liệu lên tier). Khung, badge tiêu đề và nút X lấy từ <see cref="GamePopupFrame"/> —
    /// cùng khung với Cửa hàng. Ô nhập và nút dựng ở <c>EnchantEvolveView.Build.cs</c>.
    ///
    /// <para>Chưa có picker nguyên liệu từ túi — hiện dùng 2 ô nhập itemId trực tiếp.
    /// Server chọn hộ nguyên liệu thì <see cref="ApplyServerMaterial"/> điền vào ô.</para>
    /// </summary>
    public sealed partial class EnchantEvolveView : MonoBehaviour
    {
        public enum Mode { Enchant, UpTier }

        private const float PopupWidth = 340f;
        private const float PopupHeight = 232f;

        private static readonly Color ErrorColor = new Color(0.86f, 0.28f, 0.28f, 1f);
        private static readonly Color OkColor = new Color(0.25f, 0.62f, 0.2f, 1f);

        private InputField _fieldA, _fieldB;
        private Text _status;

        public event Action<int, int> Confirmed; // (matA, matB)
        public event Action CloseRequested;

        public static EnchantEvolveView Create(Transform parent, Mode mode, int itemId, string itemName)
        {
            var root = new GameObject($"{mode} Popup", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            var view = root.AddComponent<EnchantEvolveView>();

            // Lớp tối là ANH EM với khung: Unity dò handler click ngược lên cây cha, nên
            // nút đóng mà nằm ở cha thì bấm vào ô nhập cũng đóng popup.
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
            dim.transform.SetParent(root.transform, false);
            UiBuilder.Stretch((RectTransform)dim.transform);
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            dim.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var font = UiBuilder.DefaultFont();
            var title = mode == Mode.Enchant ? "Cường hóa" : "Tiến hóa";
            var frame = GamePopupFrame.Create(root.transform, font, title, PopupWidth, PopupHeight);
            frame.Closed += () => view.CloseRequested?.Invoke();
            view.BuildContent(frame.Content, font, mode, JarIconTokens.Humanize(itemName ?? string.Empty));
            return view;
        }

        private void BuildContent(Transform content, Font font, Mode mode, string itemName)
        {
            // Tên đồ server trả kèm cả dải chỉ số — cho xuống 2 dòng rồi co chữ, đừng tràn.
            var name = UiBuilder.MakeText(content, font, "Item name", 14, false);
            name.text = itemName;
            name.alignment = TextAnchor.MiddleCenter;
            name.color = PopupPalette.TextDark;
            name.horizontalOverflow = HorizontalWrapMode.Wrap;
            name.verticalOverflow = VerticalWrapMode.Truncate;
            name.resizeTextForBestFit = true;
            name.resizeTextMinSize = 10;
            name.resizeTextMaxSize = 14;
            UiBuilder.SetFontStyle(name, FontStyle.Bold);
            PlaceTop(name.rectTransform, 2f, 36f, 4f);

            var hint = UiBuilder.MakeText(content, font, "Hint", 11, false);
            hint.text = mode == Mode.Enchant
                ? "Nhập ID 2 nguyên liệu (template + crystal)."
                : "Nhập ID 2 nguyên liệu để lên tier tiếp theo.";
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = PopupPalette.TextMuted;
            PlaceTop(hint.rectTransform, 40f, 16f, 4f);

            _fieldA = MakeField(content, font, mode == Mode.Enchant ? "ID nguyên liệu" : "ID nguyên liệu 1", 62f);
            _fieldB = MakeField(content, font, mode == Mode.Enchant ? "ID crystal" : "ID nguyên liệu 2", 104f);

            _status = UiBuilder.MakeText(content, font, "Status", 11, false);
            _status.alignment = TextAnchor.MiddleCenter;
            PlaceTop(_status.rectTransform, 144f, 16f, 4f);

            MakeSubmit(content, font, mode == Mode.Enchant ? "Cường hóa" : "Tiến hóa");
        }

        private void TrySubmit()
        {
            if (!int.TryParse(_fieldA.text, out var a) || !int.TryParse(_fieldB.text, out var b))
            {
                SetStatus("Nhập số hợp lệ cho cả 2 nguyên liệu.", ErrorColor);
                return;
            }
            if (a <= 0 || b <= 0)
            {
                SetStatus("ID nguyên liệu phải lớn hơn 0.", ErrorColor);
                return;
            }
            Confirmed?.Invoke(a, b);
            SetStatus("Đã gửi. Đợi server phản hồi…", OkColor);
        }

        public void ApplyServerMaterial(PetEquipMaterialSelection material)
        {
            if (material == null) return;
            var field = material.Slot == 7 ? _fieldA : _fieldB;
            field.text = material.ItemOrTemplateId.ToString();
            SetStatus($"Đã chọn {JarIconTokens.Humanize(material.Name)}", PopupPalette.TextMuted);
        }

        private void SetStatus(string text, Color color)
        {
            _status.text = text;
            _status.color = color;
        }
    }
}
