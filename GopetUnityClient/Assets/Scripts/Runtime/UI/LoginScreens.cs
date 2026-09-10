using System;
using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hiện đúng màn hình cho chặng hiện tại của <see cref="LoginFlow"/>: đang nối,
    /// chọn máy chủ, nhập tài khoản, tạo nhân vật, mất kết nối.
    ///
    /// <para><b>Không có màn 2FA ở đây.</b> Server hỏi OTP bằng một
    /// <c>TYPE_DIALOG_INPUT</c> thường nên nó đi qua <see cref="UiRoot"/> như mọi hộp
    /// nhập khác, nằm đè lên màn đăng nhập. Dựng riêng màn OTP là chép lại đường đã có.</para>
    ///
    /// <para>Class này chỉ dựng hình; thứ tự các bước nằm trong <see cref="LoginFlow"/>.</para>
    /// </summary>
    public sealed partial class LoginScreens : MonoBehaviour
    {
        private const int ButtonPrimary = 0;
        private const int ButtonSecondary = 1;

        private LoginFlow _flow;
        private CredentialStore _store;
        private Font _font;
        private SoundManager _sound;
        private FormView _form;
        private ChoiceDialogView _servers;
        private LoginFormView _loginForm;
        private LoginFormView _registrationForm;
        private CharacterCreationView _characterCreation;
        private bool _presentationEnabled = true;

        /// <summary>Tài khoản vừa gửi. Giữ riêng vì lúc đăng nhập xong biểu mẫu đã bị huỷ.</summary>
        private string _sentUsername = string.Empty;
        private string _sentPassword = string.Empty;

        /// <summary>Ghi lại lúc gửi <c>SubmitRequested</c> — mặc định nhớ, khớp hành vi trước khi có ô chọn này.</summary>
        private bool _remember = true;

        /// <summary>Đăng nhập xong. Tầng trên chuyển sang màn chơi.</summary>
        public event Action Finished;

        public FormView Form => _form;
        public ChoiceDialogView ServerPicker => _servers;
        public LoginFormView LoginForm => _loginForm;
        public LoginFormView RegistrationForm => _registrationForm;
        public CharacterCreationView CharacterCreation => _characterCreation;

        public void SetPresentationEnabled(bool enabled)
        {
            if (_presentationEnabled == enabled) return;

            _presentationEnabled = enabled;
            if (!enabled)
            {
                ClearViews();
                return;
            }

            OnStageChanged(_flow.Stage);
        }

        public static LoginScreens Create(Transform parent, Font font)
        {
            var go = new GameObject("LoginScreens", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);

            var screens = go.AddComponent<LoginScreens>();
            screens._font = font;
            return screens;
        }

        /// <param name="store">Để <c>null</c> nếu không muốn nhớ tài khoản giữa hai lần mở app.</param>
        /// <param name="sound">Để <c>null</c> nếu không cần tiếng bấm nút trên <see cref="LoginFormView"/>.</param>
        public void Initialize(LoginFlow flow, CredentialStore store = null, SoundManager sound = null)
        {
            if (_flow != null) throw new InvalidOperationException("LoginScreens đã khởi tạo; gọi lần hai là đăng ký trùng.");

            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _store = store;
            _sound = sound;

            _flow.StageChanged += OnStageChanged;
            _flow.RegisterReplyReceived += OnRegisterReply;
            OnStageChanged(_flow.Stage);
        }

        private void OnStageChanged(LoginStage stage)
        {
            if (!_presentationEnabled)
            {
                ClearViews();
                return;
            }

            // Chụp màn login trước khi nó bị huỷ. Người nghe Finished dựng map ngay,
            // còn VerticalSplitRevealTransition dùng ảnh vừa chụp để mở map theo trục dọc.
            if (stage == LoginStage.Ready)
            {
                Finished?.Invoke();
                return;
            }

            // Giữ nguyên khung login trong lúc đợi server để không lộ camera xanh.
            // Chỉ khoá điều khiển nhằm chặn người chơi gửi LOGIN nhiều lần.
            if (stage == LoginStage.LoggingIn && _loginForm != null)
            {
                _loginForm.SetInteractionEnabled(false);
                return;
            }

            // Đăng nhập bị từ chối: dùng lại form và dữ liệu vừa nhập, chỉ mở khoá
            // và cập nhật lý do thay vì phá form rồi dựng lại.
            if (stage == LoginStage.EnteringCredentials && _loginForm != null)
            {
                _loginForm.SetInteractionEnabled(true);
                _loginForm.SetNotice(_flow.Notice);
                return;
            }

            // Giữ nguyên ô tên sau khi server từ chối (ví dụ tên đã tồn tại), chỉ thay thông báo.
            if (stage == LoginStage.CreatingCharacter && _characterCreation != null)
            {
                _characterCreation.SetBusy(false);
                _characterCreation.SetServerNotice(_flow.Notice);
                return;
            }

            // CREATE_CHAR làm server đóng socket có chủ đích. Giữ view và báo bận trong suốt
            // reconnect + auto-login thay vì xoá màn hình khi LoginFlow đi qua Connecting.
            if ((stage == LoginStage.Connecting || stage == LoginStage.Handshaking || stage == LoginStage.LoggingIn)
                && _characterCreation != null && _characterCreation.IsBusy)
            {
                _characterCreation.SetBusy(true);
                return;
            }

            ClearViews();

            switch (stage)
            {
                case LoginStage.Connecting:
                case LoginStage.Handshaking:
                case LoginStage.LoggingIn:
                    // Không dựng form “Đang kết nối”. Nhánh JAR chuyển thẳng sang
                    // map sau khi nhận dữ liệu vị trí spawn, nên trước đó giữ nền trống.
                    return;

                case LoginStage.ChoosingServer:
                    ShowServerPicker();
                    return;

                case LoginStage.EnteringCredentials:
                    ShowLogin();
                    return;

                case LoginStage.CreatingCharacter:
                    ShowCreateCharacter();
                    return;

                case LoginStage.Disconnected:
                    // Notice luôn có câu gì đó: LoginFlow đã thay null bằng "không rõ
                    // lý do". Màn hình trắng trơn là cách chắc chắn để người chơi
                    // tưởng game treo.
                    ShowMessage("Mất kết nối", "Thử lại");
                    return;

            }
        }

        /// <param name="retryLabel">Nhãn nút thử lại, hoặc <c>null</c> cho màn chỉ báo tin.</param>
        private void ShowMessage(string title, string retryLabel)
        {
            _form = FormView.Create(transform, _font);
            _form.Bind(title, Array.Empty<string>(),
                retryLabel == null ? Array.Empty<string>() : new[] { retryLabel });
            _form.SetNotice(_flow.Notice);

            if (retryLabel == null) return;

            _form.DefaultButton = ButtonPrimary;
            _form.Pressed += _ => _flow.Retry();
        }

        private void ShowServerPicker()
        {
            var names = new string[_flow.Servers.Length];
            for (var i = 0; i < names.Length; i++) names[i] = _flow.Servers[i].Name;

            _servers = ChoiceDialogView.Create(transform, _font);
            _servers.Bind("Chọn máy chủ", names);
            _servers.Chosen += index => _flow.ChooseServer(index);
        }

        private void ShowCreateCharacter()
        {
            Debug.Log("[Gopet] Server yêu cầu tạo nhân vật — hiển thị màn chọn giới tính và tên.");
            var view = CharacterCreationView.Create(transform, _font);
            _characterCreation = view;
            if (!string.IsNullOrWhiteSpace(_flow.Notice)) view.SetServerNotice(_flow.Notice);
            view.Submitted += (gender, name) =>
            {
                if (_flow.SubmitCharacter(name, gender))
                {
                    view.SetBusy(true);
                }
                else
                {
                    view.SetServerNotice(_flow.Notice);
                }
            };
            view.Cancelled += () => _flow.Reconnect();

            // Compatibility surface for existing automation that still drives FormView. It is
            // inactive and never part of the presentation; all new UI input goes through view.
            _form = FormView.Create(transform, _font);
            _form.Bind("Tạo nhân vật", new[] { "Tên nhân vật" }, new[] { "Nam", "Nữ" });
            _form.gameObject.SetActive(false);
            _form.Pressed += index =>
            {
                if (_flow.SubmitCharacter(_form.TextOf(0), (sbyte)index)) view.SetBusy(true);
                else view.SetServerNotice(_flow.Notice);
            };
        }

        private void ClearViews()
        {
            Discard(_form == null ? null : _form.gameObject);
            Discard(_servers == null ? null : _servers.gameObject);
            Discard(_loginForm == null ? null : _loginForm.gameObject);
            Discard(_registrationForm == null ? null : _registrationForm.gameObject);
            Discard(_characterCreation == null ? null : _characterCreation.gameObject);

            _form = null;
            _servers = null;
            _loginForm = null;
            _registrationForm = null;
            _characterCreation = null;
        }

        /// <summary>Đóng source của hiệu ứng vào map sau khi nó đã chụp được frame login.</summary>
        public void CompleteReadyPresentation()
        {
            if (_flow == null || _flow.Stage != LoginStage.Ready) return;
            ClearViews();
            Remember();
        }

        /// <summary>Tắt trước rồi mới huỷ: <c>Destroy</c> chỉ có hiệu lực cuối frame, và
        /// trong khoảng đó màn hình cũ vẫn nhận được click.</summary>
        private static void Discard(GameObject view)
        {
            if (view == null) return;

            view.SetActive(false);
            Destroy(view);
        }

        private void OnDestroy()
        {
            if (_flow == null) return;

            _flow.StageChanged -= OnStageChanged;
            _flow.RegisterReplyReceived -= OnRegisterReply;
        }
    }
}
