using System;
using Gopet.Runtime.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Màn đăng nhập: logo GOPET treo trên panel xanh HUD, hai ô nhập, ô ghi
    /// nhớ, hai nút. Mọi kích thước đặt theo TỈ LỆ panel (<see cref="Frac"/>) để không
    /// vỡ khi <c>referenceResolution</c> thay đổi. Không dùng <see cref="PixelCanvas"/>
    /// (hệ 320×240) vì bộ art này vẽ độ phân giải cao.</summary>
    public sealed partial class LoginFormView : MonoBehaviour
    {
        /// <summary>Chiều cao panel so với chiều cao màn hình.</summary>
        private const float PanelHeightFrac = 0.64f;

        /// <summary>Tâm panel THẤP hơn tâm màn hình để logo (chồm lên đỉnh) không bị cắt.</summary>
        private const float PanelCenterY = 0.45f;
        private const float PanelAspect = 561f / 471f;
        /// <summary>Chừa lề trong panel để không lấn viền xanh.</summary>
        private const float PadX = 0.10f;

        private const float PadTop = 0.15f;

        private const float PadBottom = 0.08f;

        private SoundManager _sound;
        private bool _registrationMode;
        private InputField _username;
        private InputField _password;
        private Text _notice;
        private Toggle _remember;
        private Button _submit;
        private Button _register;

        public event Action<string, string> SubmitRequested;

        public event Action<string, string> RegisterRequested;

        /// <summary>Bấm "Đăng ký" trên biến thể form đăng ký.</summary>
        public event Action<string, string> SubmitRegistrationRequested;

        /// <summary>Bấm "Trở lại" trên biến thể form đăng ký.</summary>
        public event Action BackRequested;

        public string Username => _username.text;

        public string Password => _password.text;

        /// <summary>Có nhớ tài khoản cho lần đăng nhập THÀNH CÔNG kế tiếp hay không.</summary>
        public bool Remember => _remember.isOn;

        public string NoticeText => _notice == null ? null : _notice.text;

        /// <param name="sound">Để <c>null</c> nếu không cần tiếng bấm nút.</param>
        public static LoginFormView Create(Transform parent, Font font, SoundManager sound = null)
        {
            return CreateInternal(parent, font, sound, false);
        }

        /// <summary>
        /// Dựng form đăng ký bằng chính nền, panel, logo và ô nhập của form đăng nhập.
        /// </summary>
        public static LoginFormView CreateRegistration(Transform parent, Font font, SoundManager sound = null)
        {
            return CreateInternal(parent, font, sound, true);
        }

        private static LoginFormView CreateInternal(Transform parent, Font font, SoundManager sound,
            bool registrationMode)
        {
            var go = new GameObject("LoginFormView", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);

            var view = go.AddComponent<LoginFormView>();
            view._sound = sound;
            view._registrationMode = registrationMode;
            view.Build(font);
            return view;
        }

        public void SetCredentials(string username, string password)
        {
            _username.text = username ?? string.Empty;
            _password.text = password ?? string.Empty;
        }

        public void SetNotice(string text)
        {
            var message = text ?? string.Empty;
            var hasNotice = !string.IsNullOrWhiteSpace(message);

            _notice.text = message;
            _notice.gameObject.SetActive(hasNotice);
            SetNoticeLayout(hasNotice);
        }

        /// <summary>Khoá form trong lúc chờ LOGIN_SUCCES để tránh gửi lặp nhưng vẫn giữ hình.</summary>
        public void SetInteractionEnabled(bool enabled)
        {
            _username.interactable = enabled;
            _password.interactable = enabled;
            if (_remember != null) _remember.interactable = enabled;
            _submit.interactable = enabled;
            _register.interactable = enabled;
        }

        /// <summary>Cho phím Enter gọi thẳng, không phải qua chuột.</summary>
        public void SubmitDefault() => Submit();

        private void Submit()
        {
            if (_submit != null && !_submit.interactable) return;
            _sound?.PlayEffect("s_button");
            SubmitRequested?.Invoke(_username.text, _password.text);
        }

        private void SubmitRegister()
        {
            if (_register != null && !_register.interactable) return;
            _sound?.PlayEffect("s_button");
            RegisterRequested?.Invoke(_username.text, _password.text);
        }

        private void SubmitRegistration()
        {
            if (_submit != null && !_submit.interactable) return;
            _sound?.PlayEffect("s_button");
            SubmitRegistrationRequested?.Invoke(_username.text, _password.text);
        }

        private void Back()
        {
            if (_register != null && !_register.interactable) return;
            _sound?.PlayEffect("s_button");
            BackRequested?.Invoke();
        }

        /// <summary>
        /// Panel neo giữa màn hình, cao theo tỉ lệ chiều cao khung và rộng theo đúng
        /// tỉ lệ ảnh gốc — <see cref="AspectRatioFitter"/> lo phần rộng nên panel không
        /// bao giờ bị kéo méo.
        /// </summary>
        private void Build(Font font)
        {
            // Nền là con ĐẦU TIÊN nên nằm dưới panel, và tự biến mất cùng view khi
            // chuyển chặng — không phải dọn tay ở nhánh cây khác.
            LoginBackground.Create(transform);

            var panel = MakePanel();

            // Logo nằm TRÊN panel, chồng lấn một phần: trong bộ art nó là mảnh rời.
            MakeLogo(panel);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(panel, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(PadX, PadBottom);
            contentRect.anchorMax = new Vector2(1f - PadX, 1f - PadTop);
            contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;

            BuildFields(font, contentRect);
            BuildActions(font, contentRect);
            SetNotice(null);
        }

        /// <summary>
        /// Khung xanh bo góc + mặt trong nhạt — cùng bộ với <see cref="CharacterHud"/> và
        /// <see cref="NotificationTicker"/>. Bỏ sprite cam-kem cũ để form đồng bộ HUD map
        /// và splash winter. Logo GOPET vẫn treo trên đỉnh vì có <c>MakeLogo</c>.
        /// </summary>
        private RectTransform MakePanel()
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            go.transform.SetParent(transform, false);

            var frame = go.GetComponent<Image>();
            RoundedUiSprite.Apply(frame);
            frame.color = new Color(0.18f, 0.55f, 0.9f, 1f);
            frame.raycastTarget = true; // chắn cú chạm rơi xuống nền phía sau

            var surface = new GameObject("Surface", typeof(RectTransform), typeof(Image));
            surface.transform.SetParent(go.transform, false);
            var surfImg = surface.GetComponent<Image>();
            RoundedUiSprite.Apply(surfImg);
            surfImg.color = new Color(0.91f, 0.96f, 1f, 0.98f);
            surfImg.raycastTarget = false;
            var srect = (RectTransform)surface.transform;
            srect.anchorMin = Vector2.zero; srect.anchorMax = Vector2.one;
            srect.offsetMin = new Vector2(4f, 4f); srect.offsetMax = new Vector2(-4f, -4f);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, PanelCenterY - PanelHeightFrac / 2f);
            rect.anchorMax = new Vector2(0.5f, PanelCenterY + PanelHeightFrac / 2f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = PanelAspect;

            return rect;
        }

        private void MakeLogo(RectTransform panel)
        {
            var go = new GameObject("Logo", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            go.transform.SetParent(panel, false);

            var image = go.GetComponent<Image>();
            image.sprite = LoginSkin.Get(LoginSkin.Logo);
            image.raycastTarget = false;
            image.enabled = image.sprite != null; // thiếu art thì đừng để một ô trắng lơ lửng

            // Cao bằng 30% panel, nằm ngay TRÊN đỉnh panel; bề ngang do fitter tính.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1.00f);
            rect.anchorMax = new Vector2(0.5f, 1.30f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = AspectOf(image.sprite, 350f / 185f);
        }

        private static float AspectOf(Sprite sprite, float fallback)
        {
            if (sprite == null || sprite.rect.height <= 0f) return fallback;

            return sprite.rect.width / sprite.rect.height;
        }

        /// <summary>Neo một hàng theo TỈ LỆ chiều cao vùng chứa, đo từ trên xuống.</summary>
        private static RectTransform Frac(Transform parent, string name, float top, float height, params Type[] extra)
        {
            var types = new Type[extra.Length + 1];
            types[0] = typeof(RectTransform);
            Array.Copy(extra, 0, types, 1, extra.Length);

            var go = new GameObject(name, types);
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f - top - height);
            rect.anchorMax = new Vector2(1f, 1f - top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
