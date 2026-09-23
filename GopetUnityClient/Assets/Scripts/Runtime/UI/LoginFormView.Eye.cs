using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Nút con mắt KHÔNG viền ở ô mật khẩu của <see cref="LoginFormView"/>.</summary>
    public sealed partial class LoginFormView
    {
        /// <summary>Con mắt nhạt khi đang che mật khẩu, đậm (<see cref="IconTint"/>) khi đang hiện.</summary>
        private static readonly Color EyeHidden = new Color(0.16f, 0.40f, 0.78f, 0.45f);
        private Image _eyeIcon;

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

            // Con mắt KHÔNG viền: chỉ nét, tô nhạt khi đang che, đậm khi đang hiện.
            var image = go.GetComponent<Image>();
            image.sprite = LoginSkin.Get(LoginSkin.IconEye);
            image.color = EyeHidden;
            image.preserveAspect = true;
            _eyeIcon = image;

            // Vuông và neo mép phải, giống icon đầu ô — để khung chữ nhật thì con mắt
            // bị kéo bẹt theo bề ngang ô.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 0.2f);
            rect.anchorMax = new Vector2(1f, 0.8f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = new Vector2(-IconPadX, 0f);

            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = IconAspect;

            go.GetComponent<Button>().onClick.AddListener(ToggleReveal);
        }

        private void ToggleReveal()
        {
            var hidden = _password.contentType == InputField.ContentType.Password;
            _password.contentType = hidden ? InputField.ContentType.Standard : InputField.ContentType.Password;
            if (_eyeIcon != null) _eyeIcon.color = hidden ? IconTint : EyeHidden;

            // InputField chỉ vẽ lại khi text đổi; không ép thì chữ vẫn hiện dấu sao.
            _password.ForceLabelUpdate();

            // Form đăng ký: con mắt hiện/che luôn ô nhập lại để so hai ô bằng mắt được.
            if (_confirmPassword == null) return;
            _confirmPassword.contentType = _password.contentType;
            _confirmPassword.ForceLabelUpdate();
        }
    }
}
