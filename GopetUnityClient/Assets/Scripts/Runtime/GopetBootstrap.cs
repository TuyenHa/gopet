using System.IO;
using Gopet.Net.Auth;
using Gopet.Net.Guider;
using Gopet.Net.Bank;
using Gopet.Net.Pet;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using Gopet.Runtime.Audio;
using Gopet.Runtime.Input;
using Gopet.Runtime.UI;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gopet.Runtime
{
    /// <summary>
    /// Ráp toàn bộ client lại và chạy: transport, handler giao thức, cache ảnh,
    /// màn đăng nhập, chồng dialog, phím tắt.
    ///
    /// <para>Đây là chỗ duy nhất biết các mảnh ghép với nhau ra sao. Mỗi mảnh tự nó
    /// đã test được rời; class này tồn tại để có thể bấm Play và thấy game thật —
    /// thứ mà P4 (ảnh hiện lên) và P5 (menu bấm được) đều cần để nghiệm thu.</para>
    /// </summary>
    public sealed partial class GopetBootstrap : MonoBehaviour
    {
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 19180;

        [Tooltip("Nhớ tài khoản giữa hai lần mở app. Tắt khi máy dùng chung.")]
        [SerializeField] private bool rememberAccount = true;

        private GopetClient _client;
        private LoginFlow _flow;
        private UiRoot _ui;
        private LoginScreens _login;
        private PixelCanvas _pixelCanvas;
        private SoundManager _sound;
        private GameSession _session;

        public GopetClient Client => _client;

        public LoginFlow Flow => _flow;

        public UiRoot Ui => _ui;

        public PixelCanvas PixelCanvasInstance => _pixelCanvas;

        private void Start()
        {
            LockLandscape();

            var canvas = CreateCanvas();
            EnsureEventSystem();

            _sound = SoundManager.Create(transform);

            // Canvas RIÊNG, không chung với `canvas` ở trên: PixelCanvas tự quản
            // scale theo pixel thật (xem class đó), trộn chung một Canvas với
            // ScaleWithScreenSize sẽ làm sai lệch phép tính bội số nguyên.
            _pixelCanvas = PixelCanvas.Create(transform);

            _client = gameObject.AddComponent<GopetClient>();

            var auth = new AuthHandler(_client.Send);
            auth.RegisterOn(_client.Router);
            _client.Ticked += auth.Tick;

            var guider = new GuiderHandler(_client.Send);
            guider.RegisterOn(_client.Router);

            var assets = new RemoteAssetCache(_client, _client.Router);

            var font = UiBuilder.BuiltinFont();

            _flow = new LoginFlow();
            _client.Ticked += _flow.Tick;

            // THỨ TỰ QUAN TRỌNG: uGUI vẽ theo thứ tự anh em, và dựng sau thì nằm trên.
            //
            // Màn đăng nhập phải dựng TRƯỚC UiRoot. Server hỏi OTP đúng lúc client
            // đang ở chặng "đang đăng nhập" (`Player.cs:400-403`), và màn chặng đó là
            // một FormView phủ kín màn hình có Image chắn cả tia raycast — dựng nó sau
            // thì hộp OTP nằm dưới, vừa không thấy vừa không bấm được.
            _login = LoginScreens.Create(canvas.transform, font);

            _ui = UiRoot.Create(canvas.transform, font);
            _ui.Initialize(guider, assets);

            var petUpgrade = new PetUpgradeHandler(_client.Send);
            petUpgrade.RegisterOn(_client.Router);
            _ui.InitializePetUpgrade(petUpgrade);

            var gems = new GemHandler(_client.Send);
            gems.RegisterOn(_client.Router);
            _ui.InitializeGems(gems);

            var animationMenus = new AnimationMenuHandler();
            animationMenus.RegisterOn(_client.Router);
            _ui.InitializeAnimationMenus(animationMenus);

            var wings = new WingHandler(_client.Send);
            wings.RegisterOn(_client.Router);
            _ui.InitializeWings(wings);

            // HUD Cửa hàng / Dịch vụ / Sự kiện: ẩn cho tới khi vào map (sau LOGIN_SUCCES).
            // Đặt SAU UiRoot để nằm trên nó — nút HUD phải bấm được cả khi có popup, và
            // Push popup của UiRoot đã tự SetActive(false) view dưới nên chồng thế này
            // không xung đột. Nếu popup shop đang mở thì nó ẩn view dưới nó (chỉ popup
            // hiện) — HUD vẫn ở trên nếu nó KHÔNG phải phần tử được push.
            var hud = ShopServiceEventHud.Create(canvas.transform, font);
            hud.gameObject.SetActive(false);
            hud.ShopClicked += () => _ui.OpenShopPopup();
            hud.GuildClicked += () => _session?.OpenGuildPopup();
            hud.ServiceClicked += () => _ui.OpenAtmPopup(() => _client.Send(BankPackets.OpenBankMenu()));
            hud.EventClicked += () => _ui.OpenDailyCheckin();

            WireFlow(auth);
            _login.Initialize(_flow, rememberAccount ? NewStore() : null, _sound);
            _login.Finished += () =>
            {
                Debug.Log($"[Gopet] Đăng nhập xong: {_flow.Success}. Vào map…");
                VerticalSplitRevealTransition.Create(transform, _login.CompleteReadyPresentation);
                _session = GameSession.Start(_client, _flow.Success, assets, guider, transform, wings);
                _ui.MenuInterceptor = _session.TryConsumeHudMenu;
                _session.LogoutRequested += LogoutToLogin;
                // Bật HUD 3 nút góc-phải NGAY sau khi vào map — trước đó ẩn để không
                // đè lên splash / màn đăng nhập.
                hud.gameObject.SetActive(true);
            };

            WireKeyboard();

            StartSplashConnection(canvas.transform, font);
        }

        private void LogoutToLogin()
        {
            _client.Disconnect();
            var active = SceneManager.GetActiveScene();
            if (active.buildIndex >= 0) SceneManager.LoadScene(active.buildIndex);
            else SceneManager.LoadScene(active.name);
        }

        /// <summary>
        /// Nối máy trạng thái đăng nhập vào transport và các sự kiện giao thức.
        ///
        /// <para><c>DialogShown</c> vào <c>OnDialog</c>, không thẳng
        /// <c>OnLoginRejected</c>: cùng một dialog đỏ/thường còn được dùng để trả
        /// lời gói <c>REGISTER</c> (nút "Tạo tài khoản"), và <c>OnDialog</c> tự tách
        /// hai trường hợp — xem <see cref="LoginFlow.OnDialog"/>. Trường hợp còn lại
        /// (sai mật khẩu, tài khoản chưa kích hoạt, sai phiên bản...) vẫn đi đúng
        /// đường cũ, và <see cref="LoginFlow"/> tự bỏ qua khi không ở chặng đăng
        /// nhập, nên dialog giữa game không bị nuốt.</para>
        /// </summary>
        private void WireFlow(AuthHandler auth)
        {
            _flow.ConnectRequested += (h, p) => _client.Connect(h, p);
            _flow.SendRequested += _client.Send;

            _client.Connected += _flow.OnConnected;
            _client.Disconnected += _flow.OnDisconnected;

            // Rớt mạng bất ngờ giữa lúc đang chơi (server đứng, mất kết nối...) đẩy
            // _flow ra khỏi Ready. GameSession và HUD (shop/dịch vụ/sự kiện/bang hội,
            // thanh sao-đậu-vàng-lúa...) không tự dọn — nếu để nguyên, chúng đè lên
            // màn đăng nhập vừa hiện lại. Tải lại scene giống hệt nút "Đăng xuất":
            // dọn sạch, chỉ còn nền + form đăng nhập.
            _flow.StageChanged += stage =>
            {
                if (stage != LoginStage.Ready && _session != null) LogoutToLogin();
            };

            auth.ClientAccepted += _flow.OnClientAccepted;
            auth.ServerListReceived += _flow.OnServerList;
            auth.LoginSucceeded += _flow.OnLoginSucceeded;
            auth.LoginFailed += _flow.OnLoginRejected;
            auth.DialogShown += _flow.OnDialog;
            // Còn ở màn đăng nhập/đăng ký thì câu thoại đã hiện dưới ô mật khẩu qua
            // OnDialog (SetNotice) — bật thêm popup ở đây là hiện trùng hai lần cùng
            // một câu. Popup chỉ dành cho lúc đã vào game (_session != null).
            auth.ErrorDialogShown += text =>
            {
                if (_ui != null && _session != null)
                {
                    _session.CancelWarpTransition();
                    _ui.ShowServerError(text);
                }
            };
            auth.SuccessDialogShown += text =>
            {
                if (_ui != null && _session != null) _ui.ShowServerSuccess(text);
            };
            auth.CharacterCreationRequired += _flow.OnCharacterRequired;
        }

        private void WireKeyboard()
        {
            var input = InputRouter.Create(transform);

            // Esc đóng dialog trên cùng. Không còn dialog nào thì để nguyên — thoát
            // game bằng Esc là hành vi bất ngờ, và trên mobile nút back cùng binding.
            input.Cancelled += () => _ui.Back();

            // Enter chỉ có nghĩa trên biểu mẫu của client; dialog của server có nhãn
            // nút riêng nên không đoán được cái nào là "mặc định".
            //
            // So sánh `!= null` chứ không dùng `?.`: toán tử điều kiện-null bỏ qua
            // "đã Destroy nhưng tham chiếu chưa null" của UnityEngine.Object.
            input.Submitted += () =>
            {
                var form = _login.Form;
                if (form != null)
                {
                    form.SubmitDefault();
                    return;
                }

                var loginForm = _login.LoginForm;
                if (loginForm != null) loginForm.SubmitDefault();
            };
        }

        private static CredentialStore NewStore()
        {
            return new CredentialStore(Path.Combine(Application.persistentDataPath, "account"));
        }
    }
}
