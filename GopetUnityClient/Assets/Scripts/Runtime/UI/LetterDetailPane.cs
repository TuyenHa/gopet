using System;
using Gopet.Net.Social;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Ô đọc thư nằm ở NỬA PHẢI popup hộp thư: tiêu đề, nội dung cuộn được, và hai nút
    /// "Đánh dấu đã đọc" (xanh) / "Xoá" (đỏ) ghim đáy.
    ///
    /// <para>Thay cho overlay tối phủ kín màn trước đây. Đọc thư mà phải đóng một lớp nữa
    /// mới quay lại danh sách là mất mạch; ở đây danh sách vẫn nằm bên trái, bấm thư nào
    /// là bên phải đổi theo.</para>
    /// </summary>
    public sealed class LetterDetailPane : MonoBehaviour
    {
        private const float TitleHeight = 22f;
        private const float ButtonsHeight = 28f;
        /// <summary>Khe giữa vùng nội dung và hàng nút.</summary>
        private const float ButtonsGap = 6f;
        private const float Padding = 8f;

        private static readonly Color DeleteRed = new Color(0.86f, 0.27f, 0.27f, 1f);

        private Text _title;
        private Text _body;
        private ScrollRect _bodyScroll;
        private GameObject _buttons;
        private Button _markButton;
        private Text _placeholder;
        private Letter _letter;

        /// <summary>Người chơi bấm "Đánh dấu đã đọc". Tham số là <c>LetterId</c>.</summary>
        public event Action<int> MarkRequested;

        /// <summary>Người chơi bấm "Xoá". Mang cả lá thư để bên nhận biết loại mà hỏi cho đúng.</summary>
        public event Action<Letter> RemoveRequested;

        public static LetterDetailPane Create(RectTransform parent, Font font)
        {
            var go = new GameObject("LetterDetail", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);

            var pane = go.AddComponent<LetterDetailPane>();
            pane.Build(font);
            pane.Show(null);
            return pane;
        }

        /// <summary>Đổ một lá thư vào ô. <c>null</c> = chưa chọn thư nào, chỉ hiện lời mời.</summary>
        public void Show(Letter letter)
        {
            _letter = letter;
            var has = letter != null;

            _title.gameObject.SetActive(has);
            _body.gameObject.SetActive(has);
            _buttons.SetActive(has);
            _placeholder.gameObject.SetActive(!has);
            if (!has) return;

            _title.text = letter.Title ?? string.Empty;
            _body.text = letter.Content ?? string.Empty;
            // Đã đọc rồi thì không cần đánh dấu nữa — để nút đó sáng là mời bấm vô ích.
            _markButton.interactable = !letter.IsMark;

            // Đổi thư thì cuộn về đầu, không thì thư mới mở ra ở lưng chừng chỗ cuộn của
            // thư trước.
            _bodyScroll.verticalNormalizedPosition = 1f;
        }

        private void Build(Font font)
        {
            RoundedBorder.Apply(gameObject, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            _placeholder = UiBuilder.MakeText(transform, font, "Placeholder", 12, true);
            _placeholder.alignment = TextAnchor.MiddleCenter;
            _placeholder.color = PopupPalette.TextMuted;
            _placeholder.text = "Chọn một thư bên trái để đọc.";
            _placeholder.raycastTarget = false;

            _title = UiBuilder.MakeText(transform, font, "Title", 13, false);
            _title.alignment = TextAnchor.MiddleLeft;
            _title.color = PopupPalette.TextDark;
            _title.fontStyle = FontStyle.Bold;
            _title.supportRichText = false;
            _title.horizontalOverflow = HorizontalWrapMode.Wrap;
            _title.verticalOverflow = VerticalWrapMode.Truncate;
            UiBuilder.PlaceRow(_title.rectTransform, Padding, TitleHeight, Padding);

            BuildBody(font);
            BuildButtons(font);
        }

        /// <summary>Vùng nội dung cuộn được, cắt bằng <see cref="RectMask2D"/> như khung danh sách.</summary>
        private void BuildBody(Font font)
        {
            var viewport = new GameObject("Body", typeof(RectTransform), typeof(Image),
                typeof(RectMask2D), typeof(ScrollRect));
            viewport.transform.SetParent(transform, false);
            var rect = (RectTransform)viewport.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(Padding, Padding + ButtonsHeight + ButtonsGap);
            rect.offsetMax = new Vector2(-Padding, -(Padding + TitleHeight + 4f));
            // Graphic trong suốt: ScrollRect chỉ nhận kéo khi có thứ bắt được raycast phủ
            // hết vùng cuộn.
            viewport.GetComponent<Image>().color = Color.clear;

            // Chính ô chữ LÀ nội dung cuộn, không bọc thêm một lớp "Content" nữa:
            // ContentSizeFitter tự đặt chiều cao rect theo chiều cao chữ sau mỗi lượt
            // layout, nên thư dài bao nhiêu thì cuộn được bấy nhiêu.
            //
            // Trước đây chỗ này tự đọc `preferredHeight` rồi gán sizeDelta. Cách đó hỏng
            // ngầm: `preferredHeight` của Text ngắt dòng phụ thuộc BỀ RỘNG rect, mà lúc
            // vừa dựng xong bề rộng có thể chưa được tính — ra một con số sai, và thư dài
            // thì bị cụt đúng lúc cần cuộn nhất.
            _body = UiBuilder.MakeText(viewport.transform, font, "Text", 11, false);
            _body.alignment = TextAnchor.UpperLeft;
            _body.color = PopupPalette.TextDark;
            // Nội dung do NGƯỜI KHÁC soạn — tắt rich text, xem PopupTextRow.
            _body.supportRichText = false;
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            var bodyRect = _body.rectTransform;
            // Neo giãn NGANG (bề rộng bám vùng cuộn) nhưng ghim mép trên: chiều cao do
            // ContentSizeFitter quyết định, nên không được neo giãn dọc.
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            var fitter = _body.gameObject.AddComponent<ContentSizeFitter>();
            // Chỉ khớp chiều DỌC: bề ngang đã do neo giãn lo, để CSF đụng vào là nó bóp
            // rect theo bề ngang một dòng chữ và chữ hết ngắt dòng.
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _bodyScroll = viewport.GetComponent<ScrollRect>();
            _bodyScroll.viewport = rect;
            _bodyScroll.content = bodyRect;
            _bodyScroll.horizontal = false;
            _bodyScroll.vertical = true;
            _bodyScroll.movementType = ScrollRect.MovementType.Clamped;
            _bodyScroll.scrollSensitivity = 20f;
        }

        private void BuildButtons(Font font)
        {
            _buttons = new GameObject("Buttons", typeof(RectTransform));
            _buttons.transform.SetParent(transform, false);
            var rect = (RectTransform)_buttons.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(Padding, Padding);
            rect.offsetMax = new Vector2(-Padding, Padding + ButtonsHeight);

            _markButton = PopupButtonRow.MakeButton(_buttons.transform, font, "Đánh dấu đã đọc",
                0, PopupPalette.ButtonBlue, Color.white,
                () => { if (_letter != null) MarkRequested?.Invoke(_letter.LetterId); });

            PopupButtonRow.MakeButton(_buttons.transform, font, "Xoá", 1, DeleteRed, Color.white,
                () => { if (_letter != null) RemoveRequested?.Invoke(_letter); });
        }
    }
}
