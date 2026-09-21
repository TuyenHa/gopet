using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nửa dưới của <see cref="LoginFormView"/>: ô "Ghi nhớ tài khoản đăng nhập" và
    /// hai nút. Nút dùng sprite ĐÃ CÓ SẴN CHỮ trong bộ art nên không dựng Text đè lên —
    /// vẽ chữ thứ hai lên trên chỉ tạo ra hai lớp chữ lệch nhau.
    /// </summary>
    public sealed partial class LoginFormView
    {
        private const string RememberLabel = "Ghi nhớ tài khoản đăng nhập";

        /// <summary>Chữ dark navy — đọc tốt trên panel surface icy blue.</summary>
        private static readonly Color RememberText = new Color(0.08f, 0.25f, 0.62f, 1f);

        private static readonly Color BoxOff = new Color(1f, 1f, 1f, 0.55f);

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

            _submit = MakeButton(Frac(content, "Button_DangNhap", ButtonsTopWithoutNotice, ButtonHeight,
                    typeof(Image), typeof(Button)),
                LoginSkin.ButtonLogin, 0f, 0.475f, new Color(0.16f, 0.55f, 0.9f, 1f));
            _submit.onClick.AddListener(Submit);

            _register = MakeButton(Frac(content, "Button_TaoTaiKhoan", ButtonsTopWithoutNotice, ButtonHeight,
                    typeof(Image), typeof(Button)),
                LoginSkin.ButtonRegister, 0.525f, 1f, new Color(0.29f, 0.65f, 0.31f, 1f));
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
            const float buttonsTop = 0.73f;
            const float buttonHeight = 0.22f;

            _submit = MakeRelabeledSkinButton(
                Frac(content, "Button_DangKy", buttonsTop, buttonHeight, typeof(Image), typeof(Button)),
                font, LoginSkin.ButtonRegister, "Đăng ký", 0f, 0.475f,
                new Color(0.29f, 0.65f, 0.31f, 1f));
            _submit.onClick.AddListener(SubmitRegistration);

            _register = MakeRelabeledSkinButton(
                Frac(content, "Button_TroLai", buttonsTop, buttonHeight, typeof(Image), typeof(Button)),
                font, LoginSkin.ButtonLogin, "Trở lại", 0.525f, 1f,
                new Color(0.16f, 0.55f, 0.9f, 1f));
            _register.onClick.AddListener(Back);
        }

        /// <summary>
        /// Dùng đúng sprite bóng/gradient của hai nút login. Chữ trong PNG gốc được
        /// phủ bằng một lát màu sạch lấy từ chính sprite trước khi đặt nhãn mới.
        /// </summary>
        private static Button MakeRelabeledSkinButton(RectTransform rect, Font font, string spriteName,
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

            if (sprite != null)
            {
                var coverObject = new GameObject("OriginalTextCover", typeof(RectTransform), typeof(RawImage));
                coverObject.transform.SetParent(rect, false);
                var coverRect = (RectTransform)coverObject.transform;
                coverRect.anchorMin = new Vector2(0.09f, 0.06f);
                coverRect.anchorMax = new Vector2(0.91f, 0.94f);
                coverRect.offsetMin = coverRect.offsetMax = Vector2.zero;

                var sourceRect = sprite.rect;
                var texture = sprite.texture;
                var cover = coverObject.GetComponent<RawImage>();
                cover.texture = texture;
                cover.uvRect = new Rect(
                    (sourceRect.x + sourceRect.width * 0.105f) / texture.width,
                    sourceRect.y / texture.height,
                    sourceRect.width * 0.007f / texture.width,
                    sourceRect.height / texture.height);
                cover.raycastTarget = false;
            }
            else
            {
                RoundedUiSprite.Apply(image);
                image.color = fallback;
            }

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
            labelObject.transform.SetParent(rect, false);
            // Khung chữ lấy đúng tỉ lệ chữ ĐÃ IN SẴN trong sprite nút đăng nhập:
            // đo button-login.png (277x100) thì chữ cao 32px (0.32) và rộng 0.60 mặt nút,
            // nên nhãn mới hiện ra cùng cỡ với nút đăng nhập thay vì to hơn.
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = new Vector2(0.15f, 0.32f);
            labelRect.anchorMax = new Vector2(0.85f, 0.68f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            var text = labelObject.GetComponent<Text>();
            text.font = font;
            text.text = label;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            Fit(text, 0.8f);

            var shadow = labelObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.20f, 0.32f, 0.65f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return rect.GetComponent<Button>();
        }

        /// <summary>
        /// Toggle đặt trên CẢ HÀNG chứ không riêng ô vuông: vùng bấm rộng bằng cả dòng
        /// chữ, dễ trúng ngón tay hơn nhiều trên di động.
        /// </summary>
        private Toggle MakeRemember(Font font, RectTransform row)
        {
            row.GetComponent<Image>().color = Color.clear; // vô hình, chỉ để làm vùng bấm

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(row, false);
            var boxImage = box.GetComponent<Image>();
            boxImage.sprite = LoginSkin.Get(LoginSkin.CheckOn);
            boxImage.color = boxImage.sprite == null ? BoxOff : Color.white;
            boxImage.raycastTarget = false;

            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = new Vector2(0f, 0f);
            boxRect.anchorMax = new Vector2(0.1f, 1f);
            boxRect.offsetMin = boxRect.offsetMax = Vector2.zero;

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
            toggle.graphic = boxImage;

            // Mặc định BẬT: trước khi có ô này hành vi là LUÔN nhớ, giữ nguyên mặc định
            // cũ để không âm thầm đổi trải nghiệm của người đã quen.
            toggle.isOn = true;
            return toggle;
        }

        /// <param name="left">Mép trái nút, theo tỉ lệ bề ngang vùng nội dung.</param>
        /// <param name="right">Mép phải nút.</param>
        /// <param name="fallback">Màu dùng khi thiếu sprite — vẫn phân biệt được hai nút.</param>
        private static Button MakeButton(RectTransform rect, string spriteName, float left, float right, Color fallback)
        {
            rect.anchorMin = new Vector2(left, rect.anchorMin.y);
            rect.anchorMax = new Vector2(right, rect.anchorMax.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var image = rect.GetComponent<Image>();
            image.sprite = LoginSkin.Get(spriteName);
            image.color = image.sprite == null ? fallback : Color.white;
            image.preserveAspect = image.sprite != null;

            return rect.GetComponent<Button>();
        }
    }
}
