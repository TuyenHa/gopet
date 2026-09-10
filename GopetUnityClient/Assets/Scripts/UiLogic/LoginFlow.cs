using System;
using Gopet.Net;
using Gopet.Net.Auth;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Máy trạng thái của luồng đăng nhập. Thuần C#, không UnityEngine — chỗ duy
    /// nhất biết thứ tự các bước, và test được ngoài Editor.
    ///
    /// <para>Thứ tự lấy từ lượt chạy thật với GServer (<c>LoginChecks</c> của
    /// LiveSmoke): nối → <c>CLIENT_INFO</c> → xin <c>SERVER_LIST</c> → chọn máy chủ
    /// → <c>LOGIN</c> → <c>LOGIN_SUCCES</c>, hoặc rẽ sang tạo nhân vật.</para>
    ///
    /// <para><b>2FA không có chặng riêng.</b> Server hỏi OTP bằng một
    /// <c>TYPE_DIALOG_INPUT</c> thường, đi qua đúng đường dialog generic.</para>
    /// </summary>
    public sealed partial class LoginFlow
    {
        private readonly ClientInfo _clientInfo;
        private readonly string _refCode;

        private string _host;
        private int _port;

        private string _username;
        private string _password;

        /// <summary>Đã qua màn chọn máy chủ — lần bắt tay sau không hỏi lại nữa.</summary>
        private bool _serverChosen;

        /// <summary>Server ĐÓNG kết nối ngay sau khi tạo nhân vật (<c>GameController.cs:740</c>).</summary>
        private bool _expectingCloseAfterCreate;

        /// <summary>Câu server dùng để từ chối, giữ lại QUA cú đóng kết nối theo sau nó.</summary>
        private string _rejection;

        /// <summary>
        /// Câu từ chối đó thuộc về MÀN HÌNH nào.
        ///
        /// <para>"Sai mật khẩu" thuộc màn đăng nhập, "tên đã có người dùng" thuộc màn
        /// tạo nhân vật — và giữa hai lần hiện nó, người chơi phải đăng nhập lại một
        /// lượt. Gộp chung một ô thì lần đăng nhập ở giữa xoá mất câu của màn kia.</para>
        /// </summary>
        private LoginStage _rejectionStage;

        /// <summary>Được phép tự đăng nhập lại mà không hỏi. Xem <see cref="Resume"/>.</summary>
        private bool _autoLogin;

        /// <param name="nowMs">Nguồn thời gian đơn điệu. Để trống dùng đồng hồ thật; test bơm đồng hồ giả.</param>
        public LoginFlow(ClientInfo clientInfo = null, string refCode = "", Func<long> nowMs = null)
        {
            _clientInfo = clientInfo ?? new ClientInfo();
            _refCode = refCode ?? string.Empty;
            _nowMs = nowMs ?? (() => Clock.ElapsedMilliseconds);
        }

        public LoginStage Stage { get; private set; } = LoginStage.Idle;
        public ServerEntry[] Servers { get; private set; }
        public LoginSuccess Success { get; private set; }

        /// <summary>Câu cần hiện cho người dùng ở chặng hiện tại. Có thể <c>null</c>.</summary>
        public string Notice { get; private set; }

        /// <summary>Còn kết nối hay không. Khác <see cref="Stage"/>: chặng là MÀN HÌNH, cái này là đường dây.</summary>
        public bool IsConnected { get; private set; }

        public event Action<LoginStage> StageChanged;
        public event Action<Message> SendRequested;

        /// <summary>Xin tầng transport nối tới đây. Tham số: host, port.</summary>
        public event Action<string, int> ConnectRequested;

        public void Start(string host, int port)
        {
            _host = host;
            _port = port;
            _serverChosen = false;
            Connect();
        }

        public void OnConnected()
        {
            IsConnected = true;

            // Nối lại NGẦM (sau khi bị từ chối) giữ nguyên màn hình đang hiện: đá
            // người chơi sang màn "đang kết nối" là xoá mất câu server vừa nói.
            if (Stage != LoginStage.EnteringCredentials && Stage != LoginStage.CreatingCharacter)
            {
                Enter(LoginStage.Handshaking, null);
            }

            SendRequested?.Invoke(_clientInfo.ToMessage());
        }

        public void OnClientAccepted(bool accepted)
        {
            if (!accepted)
            {
                Enter(LoginStage.Disconnected, "Máy chủ từ chối phiên bản client này.");
                return;
            }

            // Bắt tay lần hai: hỏi lại danh sách là bắt người dùng chọn hai lần.
            if (_serverChosen)
            {
                Resume();
                return;
            }

            SendRequested?.Invoke(AuthPackets.RequestServerList());
        }

        /// <param name="reason">Có thể <c>null</c>: mạng đứt hay server chết thì không có thông điệp nào.</param>
        public void OnDisconnected(string reason)
        {
            IsConnected = false;

            // Rớt mạng giữa lúc chờ hồi âm REGISTER thì hồi âm đó không bao giờ tới
            // — treo cờ mãi mãi sẽ chặn hết mọi lần thử đăng ký sau này.
            _awaitingRegisterReply = false;

            if (_expectingCloseAfterCreate)
            {
                _expectingCloseAfterCreate = false;
                _autoLogin = true;
                Connect();
                return;
            }

            // Vừa bị từ chối rồi bị đóng: nối lại NGẦM để người chơi gõ tiếp, giữ
            // nguyên màn hình lẫn câu của server. "Mất kết nối" ở đây là giấu lý do thật.
            if (_rejection != null)
            {
                Reconnect();
                return;
            }

            Enter(LoginStage.Disconnected, NoticeForDisconnect(reason));
        }

        private static string NoticeForDisconnect(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Mất kết nối, không rõ lý do.";

            if (reason.IndexOf("no connection could be made", StringComparison.OrdinalIgnoreCase) >= 0
                || reason.IndexOf("connection refused", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Không thể kết nối tới máy chủ. Vui lòng thử lại.";
            }

            return reason;
        }

        /// <summary>
        /// Nối lại từ <see cref="LoginStage.Disconnected"/>. Người dùng bấm, nên nối
        /// xong đăng nhập luôn — trừ khi lần trước bị TỪ CHỐI, lúc đó để họ sửa đã.
        /// </summary>
        public void Retry()
        {
            if (_host == null) throw new InvalidOperationException("Chưa gọi Start() nên không biết nối đi đâu.");

            _autoLogin = _rejection == null;
            Connect();
        }

        private void Connect()
        {
            Enter(LoginStage.Connecting, null);
            Reconnect();
        }

        /// <summary>Nối lại mà KHÔNG đổi màn hình — dùng khi màn đang hiện có câu cần giữ.</summary>
        public void Reconnect()
        {
            IsConnected = false;
            ConnectRequested?.Invoke(_host, _port);
        }

        /// <summary>
        /// Đổi chặng. Chặng VÀ câu đều không đổi thì không bắn sự kiện — view dựng lại
        /// màn hình theo sự kiện này, và dựng lại là xoá sạch những gì vừa gõ.
        /// </summary>
        private void Enter(LoginStage stage, string notice)
        {
            // Đặt đồng hồ TRƯỚC lần thoát sớm: chặng không đổi thì view khỏi dựng lại,
            // nhưng hạn chờ vẫn phải đúng.
            ArmTimeout(stage == LoginStage.Handshaking || stage == LoginStage.LoggingIn);

            if (stage == Stage && notice == Notice) return;

            Stage = stage;
            Notice = notice;
            StageChanged?.Invoke(stage);
        }

    }
}
