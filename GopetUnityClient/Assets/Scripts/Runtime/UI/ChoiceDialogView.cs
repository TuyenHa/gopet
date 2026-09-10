using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hộp thoại "một đoạn text + N nút".
    ///
    /// <para>Một class phục vụ cả ba thứ vì chúng cùng hình dạng: hộp Có/Không
    /// (2 nút), danh sách lựa chọn NPC (N nút), và màn hình
    /// <c>GUIDER_LIST_OPTION</c>. Ba class gần giống nhau chỉ tạo ba chỗ để lệch.</para>
    ///
    /// <para><b>Nhãn nút luôn lấy từ server.</b> Server gửi "Đồng ý"/"Thôi" cho dòng
    /// này và "Mua"/"Bán" cho dòng khác — hardcode "OK"/"Cancel" là hỏng ngay ở
    /// màn hình thứ hai.</para>
    /// </summary>
    public sealed class ChoiceDialogView : MonoBehaviour
    {
        private readonly List<Button> _buttons = new List<Button>();

        private Text _message;
        private Font _font;

        /// <summary>
        /// Đã chọn xong. Multi-touch có thể chạm hai nút trong CÙNG một frame; chặn
        /// ở tầng đóng màn hình là quá muộn vì cả hai đã kịp gửi gói.
        /// </summary>
        private bool _decided;

        /// <summary>Chỉ số nút được bấm, tính từ 0 theo đúng thứ tự truyền vào.</summary>
        public event Action<int> Chosen;

        public string Message => _message == null ? null : _message.text;

        public IReadOnlyList<Button> Buttons => _buttons;

        public static ChoiceDialogView Create(Transform parent, Font font)
        {
            var go = new GameObject("ChoiceDialogView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiBuilder.Panel;

            var view = go.AddComponent<ChoiceDialogView>();
            view._font = font;

            var text = new GameObject("Message", typeof(RectTransform), typeof(Text));
            text.transform.SetParent(go.transform, false);
            view._message = text.GetComponent<Text>();
            view._message.font = font;
            view._message.fontSize = 18;
            view._message.color = UiBuilder.TextMain;

            return view;
        }

        /// <param name="message">Nội dung server gửi.</param>
        /// <param name="labels">Nhãn từng nút, theo đúng thứ tự server gửi.</param>
        public void Bind(string message, IReadOnlyList<string> labels)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));

            _message.text = message;
            _decided = false;

            foreach (var button in _buttons)
            {
                if (button != null) Destroy(button.gameObject);
            }

            _buttons.Clear();

            for (var i = 0; i < labels.Count; i++)
            {
                _buttons.Add(MakeButton(labels[i], i));
            }
        }

        /// <summary>Cho test và cho phím tắt gọi thẳng, không phải qua chuột.</summary>
        public void Choose(int index)
        {
            if (index < 0 || index >= _buttons.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Nút {index} nằm ngoài hộp thoại ({_buttons.Count} nút).");
            }

            if (_decided) return;
            _decided = true;

            Chosen?.Invoke(index);
        }

        private Button MakeButton(string label, int index)
        {
            var go = new GameObject($"Button{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);

            var text = textGo.GetComponent<Text>();
            text.font = _font;
            text.text = label;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UiBuilder.TextMain;

            var button = go.GetComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => Choose(captured));
            return button;
        }
    }
}
