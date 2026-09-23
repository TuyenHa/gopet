using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nửa dưới của <see cref="LoginFormView"/>: ô "Ghi nhớ tài khoản đăng nhập" và
    /// hai nút. Sprite nút là mặt kính viền vàng TRƠN (không in chữ), nhãn vẽ bằng code.
    /// </summary>
    public sealed partial class LoginFormView
    {
        private const string RememberLabel = "Ghi nhớ tài khoản đăng nhập";

        /// <summary>Chữ dark navy — đọc tốt trên panel surface icy blue.</summary>
        private static readonly Color RememberText = new Color(0.08f, 0.25f, 0.62f, 1f);


        /// <summary>Màu lùi khi thiếu sprite nút, cũng là gốc màu viền chữ.</summary>
        private static readonly Color BlueButton = new Color(0.16f, 0.55f, 0.9f, 1f);
        private static readonly Color GreenButton = new Color(0.29f, 0.65f, 0.31f, 1f);

        private const float RememberTopWithoutNotice = 0.47f;

        private const float RememberTopWithNotice = 0.60f;

        private const float RememberHeight = 0.105f;

        // Với khung tham chiếu 800x600, 0.05 chiều cao Content xấp xỉ 15 px.
        private const float ButtonsTopWithoutNotice = 0.67f;

        private const float ButtonsTopWithNotice = 0.76f;

        private const float ButtonHeight = 0.22f;

        private void BuildActions(Font font, RectTransform content)
        {
            if (_registrationMode)
            {
                BuildRegistrationActions(font, content);
                return;
            }

            // Bình thường đặt ghi nhớ sát mật khẩu; SetNoticeLayout sẽ đẩy hàng này xuống khi có lỗi.
            var rememberRow = Frac(content, "Remember", RememberTopWithoutNotice, RememberHeight,
                typeof(Image), typeof(Toggle));
            _remember = MakeRemember(font, rememberRow);

            _submit = MakeLabeledButton(Frac(content, "Button_DangNhap", ButtonsTopWithoutNotice, ButtonHeight,
                    typeof(Image), typeof(Button)),
                font, LoginSkin.ButtonLogin, "Đăng nhập", 0f, 0.475f, BlueButton);
            _submit.onClick.AddListener(Submit);

            _register = MakeLabeledButton(Frac(content, "Button_TaoTaiKhoan", ButtonsTopWithoutNotice, ButtonHeight,
                    typeof(Image), typeof(Button)),
                font, LoginSkin.ButtonRegister, "Tạo tài khoản", 0.525f, 1f, GreenButton);
            _register.onClick.AddListener(SubmitRegister);
        }

        private void SetNoticeLayout(bool hasNotice)
        {
            if (_remember == null) return;

            var rememberTop = hasNotice ? RememberTopWithNotice : RememberTopWithoutNotice;
            var buttonsTop = hasNotice ? ButtonsTopWithNotice : ButtonsTopWithoutNotice;

            SetVerticalPosition((RectTransform)_remember.transform, rememberTop, RememberHeight);
            SetVerticalPosition((RectTransform)_submit.transform, buttonsTop, ButtonHeight);
            SetVerticalPosition((RectTransform)_register.transform, buttonsTop, ButtonHeight);
        }

        private static void SetVerticalPosition(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(rect.anchorMin.x, 1f - top - height);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1f - top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void BuildRegistrationActions(Font font, RectTransform content)
        {
            // Dưới 3 ô nhập + dòng lỗi; nút cùng cỡ thật với form đăng nhập.
            const float buttonsTop = 0.75f;
            const float buttonHeight = ButtonHeight * RegistrationScale;

            _submit = MakeLabeledButton(
                Frac(content, "Button_DangKy", buttonsTop, buttonHeight, typeof(Image), typeof(Button)),
                font, LoginSkin.ButtonRegister, "Đăng ký", 0f, 0.475f, GreenButton);
            _submit.onClick.AddListener(SubmitRegistration);

            _register = MakeLabeledButton(
                Frac(content, "Button_TroLai", buttonsTop, buttonHeight, typeof(Image), typeof(Button)),
                font, LoginSkin.ButtonLogin, "Trở lại", 0.525f, 1f, BlueButton);
            _register.onClick.AddListener(Back);
        }

        /// <summary>
        /// Nút trơn viền vàng (không in sẵn chữ) + nhãn vẽ bằng code, dùng chung cho cả
        /// form đăng nhập lẫn đăng ký. Thiếu sprite thì rơi về ô bo góc màu phẳng.
        /// </summary>
        private static Button MakeLabeledButton(RectTransform rect, Font font, string spriteName,
            string label, float left, float right, Color fallback)
        {
            rect.anchorMin = new Vector2(left, rect.anchorMin.y);
            rect.anchorMax = new Vector2(right, rect.anchorMax.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var image = rect.GetComponent<Image>();
            var sprite = LoginSkin.Get(spriteName);
            image.sprite = sprite;
            image.color = sprite == null ? fallback : Color.white;
            image.preserveAspect = sprite != null;
            if (sprite == null) RoundedUiSprite.Apply(image);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
            labelObject.transform.SetParent(rect, false);
            // Vùng chữ nằm trong mặt kính của nút, chừa viền vàng hai đầu.
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = new Vector2(0.14f, 0.28f);
            labelRect.anchorMax = new Vector2(0.86f, 0.72f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            var text = labelObject.GetComponent<Text>();
            text.font = font;
            text.text = label;
            text.color = Color.white;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            Fit(text, 0.8f);

            // MỘT lớp bóng đổ tối theo màu nút. Không dùng Outline: nó vẽ chữ thêm 4 lần
            // lệch nhau ~1px, ở cỡ chữ nhỏ mép chữ thành răng cưa.
            var shadow = labelObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(fallback.r * 0.3f, fallback.g * 0.3f, fallback.b * 0.3f, 0.6f);
            shadow.effectDistance = new Vector2(0f, -1.5f);

            return rect.GetComponent<Button>();
        }

        /// <summary>
        /// Toggle đặt trên CẢ HÀNG chứ không riêng ô vuông: vùng bấm rộng bằng cả dòng
        /// chữ, dễ trúng ngón tay hơn nhiều trên di động.
        /// </summary>
        private Toggle MakeRemember(Font font, RectTransform row)
        {
            row.GetComponent<Image>().color = Color.clear; // vô hình, chỉ để làm vùng bấm

            // Khung ô LUÔN hiện (viền vàng + nền trắng như ô nhập); Toggle chỉ bật/tắt dấu
            // tích. Trước đây khung và dấu tích chung một ảnh nên bỏ chọn là mất luôn khung.
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            box.transform.SetParent(row, false);
            var boxImage = box.GetComponent<Image>();
            RoundedUiSprite.Apply(boxImage);
            boxImage.color = FieldBorder;
            boxImage.raycastTarget = false;

            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = new Vector2(0f, 0.1f);
            boxRect.anchorMax = new Vector2(0f, 0.9f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.offsetMin = boxRect.offsetMax = Vector2.zero;
            var boxFitter = box.GetComponent<AspectRatioFitter>();
            boxFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            boxFitter.aspectRatio = 1f;

            var boxFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            boxFill.transform.SetParent(box.transform, false);
            var boxFillImage = boxFill.GetComponent<Image>();
            RoundedUiSprite.Apply(boxFillImage);
            boxFillImage.color = FieldFill;
            boxFillImage.raycastTarget = false;
            var boxFillRect = (RectTransform)boxFill.transform;
            UiBuilder.Stretch(boxFillRect);
            boxFillRect.offsetMin = new Vector2(FieldBorderPx, FieldBorderPx);
            boxFillRect.offsetMax = new Vector2(-FieldBorderPx, -FieldBorderPx);

            var check = new GameObject("Check", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(box.transform, false);
            var checkImage = check.GetComponent<Image>();
            checkImage.sprite = LoginSkin.Get(LoginSkin.CheckOn);
            checkImage.color = IconTint;
            checkImage.preserveAspect = true;
            checkImage.raycastTarget = false;
            var checkRect = (RectTransform)check.transform;
            UiBuilder.Stretch(checkRect);
            checkRect.offsetMin = new Vector2(3f, 3f);
            checkRect.offsetMax = new Vector2(-3f, -3f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(row, false);
            var text = label.GetComponent<Text>();
            text.font = font;
            text.text = RememberLabel;
            text.color = RememberText;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            Fit(text, 0.8f);

            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = new Vector2(0.13f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            var toggle = row.GetComponent<Toggle>();
            toggle.targetGraphic = row.GetComponent<Image>();
            toggle.graphic = checkImage;

            // Mặc định BẬT: trước khi có ô này hành vi là LUÔN nhớ, giữ nguyên mặc định
            // cũ để không âm thầm đổi trải nghiệm của người đã quen.
            toggle.isOn = true;
            return toggle;
        }
    }
}
