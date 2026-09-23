using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hai ô nhập của <see cref="LoginFormView"/>: nền trắng viền vàng mảnh (cùng bộ với
    /// panel), icon nét (không khung) bên trái + vạch ngăn, và ở ô mật khẩu có nút con mắt
    /// (không viền) để xem lại thứ mình vừa gõ.
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
        /// Form đăng ký có thêm ô nhập lại mật khẩu nên thông báo lỗi dời xuống dưới ô
        /// đó, thấp hơn để không đè hàng nút (nút đăng ký bắt đầu ở 0.73).
        /// </summary>
        private const float RegistrationNoticeTop = 0.59f;
        private const float RegistrationNoticeHeight = 0.12f;
        /// <summary>Hàng form đăng ký: cùng cỡ thật với form đăng nhập, khe giữa rộng hơn.</summary>
        private const float RegistrationRowHeight = RowHeight * RegistrationScale;
        private const float RegistrationRowGap = 0.05f;
        private const string ConfirmPlaceholder = "Nhập lại mật khẩu";

        /// <summary>
        /// Icon đầu ô chiếm TRỌN chiều cao ô và vuông (bề ngang do
        /// <see cref="AspectRatioFitter"/> tính), sát mép trái — đúng như mockup.
        /// Đặt bề ngang theo tỉ lệ ô như trước thì icon không vuông, và phần ô đen
        /// thừa quanh nó trông như một mảng tối lem nhem.
        /// </summary>
        private const float IconAspect = 1f;

        /// <summary>Chữ bắt đầu sau icon; icon vuông nên bề ngang nó xấp xỉ chiều cao ô.</summary>
        private const float TextLeft = 0.16f;

        /// <summary>Trắng để ô nhập nổi trên mặt panel xanh nhạt.</summary>
        private static readonly Color FieldFill = Color.white;
        /// <summary>Viền vàng mảnh — cùng tông viền panel và ô trang bị.</summary>
        private static readonly Color FieldBorder = new Color(0.93f, 0.74f, 0.30f, 1f);
        private const float FieldBorderPx = 2f;
        /// <summary>Icon nét tô xanh thương hiệu; con mắt nhạt hơn khi đang che mật khẩu.</summary>
        private static readonly Color IconTint = new Color(0.16f, 0.40f, 0.78f, 1f);
        private const float IconPadX = 10f;

        /// <summary>Chữ dark navy — đọc tốt trên nền icy blue.</summary>
        private static readonly Color FieldText = new Color(0.08f, 0.25f, 0.62f, 1f);

        private void BuildFields(Font font, RectTransform content)
        {
            var rowHeight = _registrationMode ? RegistrationRowHeight : RowHeight;
            var rowGap = _registrationMode ? RegistrationRowGap : RowGap;
            _username = MakeField(font, Frac(content, "Field_TaiKhoan", FirstRowTop, rowHeight, typeof(Image)),
                LoginSkin.IconUser, JarStrings.Vi(298));

            var passwordRow = Frac(content, "Field_MatKhau", FirstRowTop + rowHeight + rowGap, rowHeight, typeof(Image));
            _password = MakeField(font, passwordRow, LoginSkin.IconLock, JarStrings.Vi(375));
            _password.contentType = InputField.ContentType.Password;
            MakeEyeToggle(passwordRow);

            if (_registrationMode)
            {
                var confirmRow = Frac(content, "Field_NhapLaiMatKhau",
                    FirstRowTop + 2f * (rowHeight + rowGap), rowHeight, typeof(Image));
                _confirmPassword = MakeField(font, confirmRow, LoginSkin.IconLock, ConfirmPlaceholder);
                _confirmPassword.contentType = InputField.ContentType.Password;
                _notice = MakeNotice(font, Frac(content, "Notice",
                    RegistrationNoticeTop, RegistrationNoticeHeight, typeof(Text)));
                return;
            }

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
            // Ảnh của ô = VIỀN vàng; lớp "Fill" trắng lót bên trong chừa FieldBorderPx.
            var background = rect.gameObject.GetComponent<Image>();
            RoundedUiSprite.Apply(background);
            background.color = FieldBorder;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(rect, false);
            var fillImage = fill.GetComponent<Image>();
            RoundedUiSprite.Apply(fillImage);
            fillImage.color = FieldFill;
            fillImage.raycastTarget = false;
            var fillRect = (RectTransform)fill.transform;
            UiBuilder.Stretch(fillRect);
            fillRect.offsetMin = new Vector2(FieldBorderPx, FieldBorderPx);
            fillRect.offsetMax = new Vector2(-FieldBorderPx, -FieldBorderPx);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            icon.transform.SetParent(rect, false);
            var iconImage = icon.GetComponent<Image>();
            iconImage.sprite = LoginSkin.Get(iconName);
            iconImage.color = IconTint;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.enabled = iconImage.sprite != null;

            // Icon nét nhỏ hơn ô, cách mép trái IconPadX — không còn khung vuông sát mép.
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = new Vector2(0f, 0.22f);
            iconRect.anchorMax = new Vector2(0f, 0.78f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            iconRect.anchoredPosition = new Vector2(IconPadX, 0f);

            var iconFitter = icon.GetComponent<AspectRatioFitter>();
            iconFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            iconFitter.aspectRatio = IconAspect;
            MakeDivider(rect);

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

        /// <summary>Vạch dọc mảnh ngăn icon với chữ.</summary>
        private static void MakeDivider(RectTransform row)
        {
            var go = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(row, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(IconTint.r, IconTint.g, IconTint.b, 0.25f);
            image.raycastTarget = false;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(TextLeft - 0.025f, 0.25f);
            rect.anchorMax = new Vector2(TextLeft - 0.025f, 0.75f);
            rect.sizeDelta = new Vector2(1.5f, 0f);
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
