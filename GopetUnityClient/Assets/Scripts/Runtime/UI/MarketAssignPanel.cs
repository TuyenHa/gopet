using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Form nhỏ "Chỉ định người mua" mở đè lên tab Gian hàng của tôi.
    ///
    /// <para><b>Không dùng lại <see cref="PopupInputForm"/></b>: form đó chặn submit rỗng
    /// và báo lỗi (<c>PopupInputForm.Submit</c>), trong khi yêu cầu ở đây ngược lại —
    /// "để trống = bỏ chỉ định" là một hành động HỢP LỆ, không phải lỗi nhập liệu. Vẫn tái
    /// dùng <see cref="PopupField"/>/<see cref="PopupButtonRow"/> để giữ đúng style.</para>
    /// </summary>
    public sealed class MarketAssignPanel : MonoBehaviour
    {
        private const float PanelWidth = 260f;
        private const float PanelHeight = 148f;

        private InputField _input;
        private Text _fee;

        /// <summary>Tên đã nhập (rỗng = bỏ chỉ định).</summary>
        public event Action<string> Submitted;

        public event Action Cancelled;

        public static MarketAssignPanel Create(RectTransform parent, Font font)
        {
            var go = new GameObject("AssignPanel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            rect.anchoredPosition = Vector2.zero;
            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius, PopupPalette.Panel,
                PopupPalette.Border, 2f);

            var panel = go.AddComponent<MarketAssignPanel>();
            panel.Build(font);
            go.SetActive(false);
            return panel;
        }

        /// <summary>Mở form với tên đang chỉ định (nếu có) và đúng dòng phí theo loại món.</summary>
        public void Show(bool isPet, string currentAssignedName)
        {
            _input.text = currentAssignedName ?? string.Empty;
            _fee.text = isPet ? "Phí: 15.000 vàng (pet)" : "Phí: 10.000 vàng (đồ)";
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void Build(Font font)
        {
            var title = UiBuilder.MakeText(transform, font, "Title", 12, false);
            title.text = "Chỉ bán cho người chơi";
            title.alignment = TextAnchor.MiddleCenter;
            title.color = PopupPalette.TextDark;
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiBuilder.PlaceRow(title.rectTransform, 8f, 34f, 14f);

            _input = PopupField.Create(transform, font, "Tên:", 48f, 28f, 24);
            _input.lineType = InputField.LineType.SingleLine;
            // Trống = bỏ chỉ định; nói ở gợi ý trong ô để tiêu đề ngắn gọn.
            PopupField.SetPlaceholder(_input, font, "Để trống: ai cũng mua được");

            _fee = UiBuilder.MakeText(transform, font, "Fee", 10, false);
            _fee.alignment = TextAnchor.MiddleCenter;
            _fee.color = PopupPalette.TextMuted;
            UiBuilder.PlaceRow(_fee.rectTransform, 82f, 16f, 14f);

            PopupButtonRow.Create(transform, font, 108f, "Đồng ý", Submit,
                "Huỷ", () => { Hide(); Cancelled?.Invoke(); });
        }

        private void Submit()
        {
            var name = _input.text == null ? string.Empty : _input.text.Trim();
            Hide();
            Submitted?.Invoke(name);
        }
    }
}
