using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hai ô nhập của <see cref="LoginFormView"/>: nền tối bo góc, icon vuông bên
    /// trái, và ở ô mật khẩu có thêm nút con mắt để xem lại thứ mình vừa gõ.
    /// </summary>
    public sealed partial class LoginFormView
    {
        /// <summary>
        /// Chiều cao một ô nhập, theo tỉ lệ vùng nội dung. Đo từ mockup: ô cao 13.2%
        /// panel, mà vùng nội dung chỉ chiếm 77% panel nên quy đổi ra ~0.17.
        /// </summary>
        private const float RowHeight = 0.175f;

        /// <summary>Mép trên ô tài khoản; phần trống phía dưới hai ô dành cho thông báo lỗi.</summary>
        private const float FirstRowTop = 0.02f;

        /// <summary>Khoảng hở giữa hai ô nhập.</summary>
        private const float RowGap = 0.045f;

        private const float NoticeTop = 0.435f;

        private const float NoticeHeight = 0.135f;

        /// <summary>
        /// Icon đầu ô chiếm TRỌN chiều cao ô và vuông (bề ngang do
        /// <see cref="AspectRatioFitter"/> tính), sát mép trái — đúng như mockup.
        /// Đặt bề ngang theo tỉ lệ ô như trước thì icon không vuông, và phần ô đen
        /// thừa quanh nó trông như một mảng tối lem nhem.
        /// </summary>
        private const float IconAspect = 1f;

        /// <summary>Chữ bắt đầu sau icon; icon vuông nên bề ngang nó xấp xỉ chiều cao ô.</summary>
        private const float TextLeft = 0.16f;

        /// <summary>Nền ô nhập icy blue, đồng bộ HUD map.</summary>
        private static readonly Color FieldFill = new Color(0.82f, 0.90f, 0.98f, 1f);

        /// <summary>Chữ dark navy — đọc tốt trên nền icy blue.</summary>
        private static readonly Color FieldText = new Color(0.08f, 0.25f, 0.62f, 1f);

        private void BuildFields(Font font, RectTransform content)
        {
            _username = MakeField(font, Frac(content, "Field_TaiKhoan", FirstRowTop, RowHeight, typeof(Image)),
                LoginSkin.IconUser, JarStrings.Vi(298));

            var passwordRow = Frac(content, "Field_MatKhau", FirstRowTop + RowHeight + RowGap, RowHeight, typeof(Image));
            _password = MakeField(font, passwordRow, LoginSkin.IconLock, JarStrings.Vi(375));
            _password.contentType = InputField.ContentType.Password;
            MakeEyeToggle(passwordRow);

            // Đặt lỗi sát bên dưới mật khẩu để người chơi thấy ngay nơi cần sửa.
            // Vùng riêng đủ cao cho thông báo dài tự xuống dòng mà không đè lên hàng thao tác.
            _notice = MakeNotice(font, Frac(content, "Notice", NoticeTop, NoticeHeight, typeof(Text)));
        }

        private static Text MakeNotice(Font font, RectTransform rect)
        {
            var text = rect.gameObject.GetComponent<Text>();
            text.font = font;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = new Color(0.85f, 0.25f, 0.2f, 1f);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Fit(text, 0.75f);
            return text;
        }

        private InputField MakeField(Font font, RectTransform rect, string iconName, string placeholder)
        {
            // Nền procedural icy blue thay sprite dark navy cũ → sync HUD blue frame.
            var background = rect.gameObject.GetComponent<Image>();
            RoundedUiSprite.Apply(background);
            background.color = FieldFill;

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            icon.transform.SetParent(rect, false);
            var iconImage = icon.GetComponent<Image>();
            iconImage.sprite = LoginSkin.Get(iconName);
            iconImage.raycastTarget = false;
            iconImage.enabled = iconImage.sprite != null;

            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = new Vector2(0f, 0f);
            iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;

            var iconFitter = icon.GetComponent<AspectRatioFitter>();
            iconFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            iconFitter.aspectRatio = IconAspect;

            // Chữ bắt đầu SAU icon, chừa thêm một khoảng thở; mép phải chừa chỗ cho
            // nút con mắt của ô mật khẩu (ô tài khoản thừa ra một chút cũng không sao).
            var text = MakeFieldText(font, rect, "Text");
            var placeholderText = MakeFieldText(font, rect, "Placeholder");
            placeholderText.text = placeholder;
            placeholderText.color = new Color(FieldText.r, FieldText.g, FieldText.b, 0.5f);

            var field = rect.gameObject.AddComponent<InputField>();
            field.textComponent = text;
            field.placeholder = placeholderText;
            return field;
        }

        private static Text MakeFieldText(Font font, RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(TextLeft, 0.12f);
            rect.anchorMax = new Vector2(0.87f, 0.88f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var text = go.GetComponent<Text>();
            text.font = font;
            text.color = FieldText;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            Fit(text, 1f);
            return text;
        }

        /// <summary>
        /// Nút con mắt: đổi qua lại giữa che và hiện mật khẩu.
        ///
        /// <para>Có ích thật chứ không phải trang trí — bàn gõ tiếng Việt kiểu Telex
        /// nuốt phím trong ô nhập (đã trả giá ở P1: gõ <c>test1234</c> ra <c>tét1234</c>),
        /// mà ô bị che thì không thể nhìn ra mình gõ hỏng ở đâu.</para>
        /// </summary>
        private void MakeEyeToggle(RectTransform row)
        {
            var go = new GameObject("ToggleReveal", typeof(RectTransform), typeof(Image), typeof(Button), typeof(AspectRatioFitter));
            go.transform.SetParent(row, false);

            var image = go.GetComponent<Image>();
            image.sprite = LoginSkin.Get(LoginSkin.IconEye);
            image.color = image.sprite == null ? new Color(1f, 1f, 1f, 0.35f) : Color.white;

            // Vuông và neo mép phải, giống icon đầu ô — để khung chữ nhật thì con mắt
            // bị kéo bẹt theo bề ngang ô.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 0.14f);
            rect.anchorMax = new Vector2(1f, 0.86f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = IconAspect;

            go.GetComponent<Button>().onClick.AddListener(ToggleReveal);
        }

        private void ToggleReveal()
        {
            var hidden = _password.contentType == InputField.ContentType.Password;
            _password.contentType = hidden ? InputField.ContentType.Standard : InputField.ContentType.Password;

            // InputField chỉ vẽ lại khi text đổi; không ép thì chữ vẫn hiện dấu sao.
            _password.ForceLabelUpdate();
        }

        /// <param name="fill">Phần chiều cao ô mà chữ được phép chiếm.</param>
        private static void Fit(Text text, float fill)
        {
            // Cỡ chữ theo TỈ LỆ khung, không đặt cứng: panel co giãn theo màn hình nên
            // một con số pixel cố định sẽ vừa trên máy này và tràn trên máy khác.
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 1;
            text.resizeTextMaxSize = Mathf.Max(2, Mathf.RoundToInt(48f * fill));
        }
    }
}
