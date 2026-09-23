using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hộp thoại nhập liệu (<c>TYPE_DIALOG_INPUT</c>): tiêu đề + N ô nhập có nhãn,
    /// hai nút Đồng ý / Huỷ ở đáy và nút X đóng ở góc phải trên.
    ///
    /// <para>Số ô do server quyết định và câu trả lời phải gửi <b>đúng bấy nhiêu</b>
    /// chuỗi — server dựng <c>InputReader</c> theo số ô nó đã hỏi
    /// (<c>GameController.cs:788-795</c>).</para>
    ///
    /// <para>Layout đi theo pattern của <see cref="ChoiceDialogView"/>: backdrop mờ
    /// phủ kín + panel neo giữa. Trước đây view chỉ dựng ảnh nền mà không neo/không
    /// đặt kích thước, người chơi bấm vào NPC ra "hình vuông màu xanh" trống rỗng.</para>
    ///
    /// <para><b>Bẫy đã trả giá:</b> bộ gõ tiếng Việt (Telex) nuốt phím trong ô nhập —
    /// gõ <c>test1234</c> ra <c>tét1234</c>. Đã dính khi nhập vào emulator ở P1, và
    /// ô nhập của Unity dính y hệt. Khi test bằng tay phải chuyển sang bàn phím
    /// tiếng Anh.</para>
    /// </summary>
    public sealed partial class InputDialogView : MonoBehaviour
    {
        private const float PanelWidth = 480f;
        private const float PanelPadding = 20f;
        private const float TitleHeight = 34f;
        private const float FieldHeight = 44f;
        private const float FieldGap = 10f;
        private const float ButtonRowHeight = 44f;
        private const float ButtonGap = 12f;
        private const float LabelWidth = 140f;

        private readonly List<InputField> _fields = new List<InputField>();

        private RectTransform _panel;
        private Text _title;
        private Font _font;
        private bool _decided;

        public int DialogId { get; private set; }

        public IReadOnlyList<InputField> Fields => _fields;

        /// <summary>Người dùng xác nhận. Mảng trả về đúng số ô server đã hỏi.</summary>
        public event Action<int, string[]> Submitted;

        /// <summary>Người dùng bấm Huỷ hoặc X. UiRoot đóng view qua sự kiện này.</summary>
        public event Action Closed;

        public static InputDialogView Create(Transform parent, Font font)
        {
            var backdrop = new GameObject("InputDialogView", typeof(RectTransform), typeof(Image));
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
            panelRect.sizeDelta = new Vector2(PanelWidth, ComputePanelHeight(0));

            var panelImage = panel.GetComponent<Image>();
            // Đồng bộ popup cửa hàng: nền trắng sáng, bo góc và viền xanh.
            RoundedUiSprite.Apply(panelImage);
            panelImage.color = new Color(0.96f, 0.98f, 1f, 1f);
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            var view = backdrop.AddComponent<InputDialogView>();
            view._font = font;
            view._panel = panelRect;

            view._title = UiBuilder.MakeText(view._panel, font, "Title", 18, false);
            view._title.alignment = TextAnchor.MiddleCenter;
            view._title.color = new Color(0.14f, 0.18f, 0.25f, 1f);
            UiBuilder.PlaceRow((RectTransform)view._title.transform, PanelPadding, TitleHeight, PanelPadding);

            view.BuildCloseButton();
            return view;
        }

        public void Bind(InputDialogSpec spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            DialogId = spec.DialogId;
            _title.text = spec.Title ?? string.Empty;
            _decided = false;

            foreach (var field in _fields)
            {
                if (field != null) Destroy(field.gameObject);
            }
            _fields.Clear();

            var fieldCount = spec.Fields != null ? spec.Fields.Length : 0;
            _panel.sizeDelta = new Vector2(PanelWidth, ComputePanelHeight(fieldCount));

            var y = PanelPadding + TitleHeight + FieldGap;
            for (var i = 0; i < fieldCount; i++)
            {
                _fields.Add(MakeField(spec.Fields[i], y));
                y += FieldHeight + FieldGap;
            }

            BuildActionButtons(y);
        }

        /// <summary>Cho test và cho phím Enter gọi thẳng.</summary>
        public void Submit()
        {
            if (_decided) return;
            _decided = true;

            var texts = new string[_fields.Count];
            for (var i = 0; i < _fields.Count; i++)
            {
                // Ô trống phải gửi chuỗi rỗng chứ không phải null: server đọc đủ
                // số chuỗi nó đã hỏi, thiếu một cái là lệch cả gói.
                texts[i] = _fields[i].text ?? string.Empty;
            }

            Submitted?.Invoke(DialogId, texts);
        }

        private static float ComputePanelHeight(int fieldCount)
        {
            var content = TitleHeight + FieldGap + fieldCount * (FieldHeight + FieldGap) + ButtonRowHeight;
            return PanelPadding * 2f + content;
        }

        private InputField MakeField(InputDialogSpec.Field definition, float top)
        {
            var row = new GameObject($"Field_{definition.Label}", typeof(RectTransform));
            row.transform.SetParent(_panel, false);
            UiBuilder.PlaceRow((RectTransform)row.transform, top, FieldHeight, PanelPadding);

            var label = UiBuilder.MakeText(row.transform, _font, "Label", 15, false);
            label.text = definition.Label ?? string.Empty;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = new Color(0.14f, 0.18f, 0.25f, 1f);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(LabelWidth, 0f);
            labelRect.anchoredPosition = Vector2.zero;

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(row.transform, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = new Vector2(0f, 0f);
            boxRect.anchorMax = new Vector2(1f, 1f);
            boxRect.offsetMin = new Vector2(LabelWidth + 8f, 4f);
            boxRect.offsetMax = new Vector2(0f, -4f);
            // Bo góc 5 cho ô nhập; ô vuông cạnh sắc lạc hẳn giữa các panel bo tròn.
            RoundedUiSprite.Apply(box.GetComponent<Image>(), 5f);
            box.GetComponent<Image>().color = new Color(0.90f, 0.92f, 0.95f, 1f);

            var text = UiBuilder.MakeText(box.transform, _font, "Text", 16, true);
            text.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            text.supportRichText = false;

            var field = box.AddComponent<InputField>();
            field.textComponent = text;

            // Server gửi kiểu ô nhập; khác 0 nghĩa là ô số, để bàn phím mobile mở
            // đúng bàn phím số thay vì bàn phím chữ.
            field.contentType = definition.InputType != 0
                ? InputField.ContentType.IntegerNumber
                : InputField.ContentType.Standard;

            return field;
        }

    }
}
