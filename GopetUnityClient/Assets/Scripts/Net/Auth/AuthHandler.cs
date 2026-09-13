using System;
using System.Diagnostics;

namespace Gopet.Net.Auth
{
    /// <summary>
    /// Toàn bộ luồng đăng nhập, thuần C# — không phụ thuộc UnityEngine nên test
    /// được ngoài Editor.
    ///
    /// <para>Class này chỉ dịch gói tin thành sự kiện. Nó KHÔNG quyết định luồng
    /// màn hình; phần đó nằm ở tầng UI.</para>
    ///
    /// <para><b>Tự trả lời <c>CHECK_SPEED</c></b>. Server gửi gói này mỗi 15-30 giây;
    /// không trả lời thì bị đóng kết nối và triệu chứng trông hệt như lỗi mạng.
    /// Để ở đây cho khỏi quên.</para>
    /// </summary>
    public sealed class AuthHandler
    {
        /// <summary>Server không đặt tên hằng số cho opcode này — <c>Player.redDialog()</c> ghi thẳng <c>(sbyte)10</c>.</summary>
        public const sbyte RedDialog = 10;

        /// <summary>
        /// <c>Player.okDialog()</c> ghi thẳng <c>new Message(71)</c> rồi <c>putsbyte(0)</c>.
        /// Dùng ở nhánh <c>isShowMessageWhenLogin</c> của cả <c>login()</c> lẫn
        /// <c>doRegister()</c> — bật cờ đó mà không có handler thì client đứng im
        /// không hiểu vì sao.
        /// </summary>
        public const sbyte OkDialog = 71;

        private const sbyte OkDialogSub = 0;

        /// <summary>Chưa có nhịp CHECK_SPEED nào đang chờ trả lời.</summary>
        private const long NoPending = long.MinValue;

        /// <summary>Đồng hồ đơn điệu dùng chung. <c>DateTime.Now</c> nhảy khi đổi giờ hệ thống.</summary>
        private static readonly Stopwatch Clock = Stopwatch.StartNew();

        private readonly Action<Message> _send;
        private readonly Func<long> _nowMs;

        private long _speedDueAtMs = NoPending;

        /// <param name="send">Đẩy gói lên server.</param>
        /// <param name="nowMs">Nguồn thời gian, mili-giây đơn điệu. Để trống dùng đồng hồ thật; test bơm đồng hồ giả.</param>
        public AuthHandler(Action<Message> send, Func<long> nowMs = null)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
            _nowMs = nowMs ?? (() => Clock.ElapsedMilliseconds);
        }

        /// <summary>Server chấp nhận client: <c>true</c> = qua cả hai cổng chặn của CLIENT_INFO.</summary>
        public event Action<bool> ClientAccepted;

        public event Action<ServerEntry[]> ServerListReceived;

        public event Action<LoginSuccess> LoginSucceeded;

        /// <summary>Server từ chối đăng nhập, kèm câu thông báo của nó.</summary>
        public event Action<string> LoginFailed;

        /// <summary>Dialog đỏ chung — sai version, tài khoản chưa kích hoạt, chức năng bị khoá...</summary>
        public event Action<string> DialogShown;

        /// <summary>Thông báo lỗi màu đỏ, dùng cho UI sau khi đã vào game.</summary>
        public event Action<string> ErrorDialogShown;

        /// <summary>Thông báo thành công màu xanh, dùng cho UI sau khi đã vào game.</summary>
        public event Action<string> SuccessDialogShown;

        /// <summary>Đã trả lời một nhịp CHECK_SPEED. Chủ yếu để test đếm.</summary>
        public event Action SpeedChecked;

        /// <summary>
        /// Tài khoản chưa có nhân vật — server đòi tạo trước khi vào game.
        /// Đây là nhánh của lần đăng nhập đầu tiên, không phải lỗi.
        /// </summary>
        public event Action CharacterCreationRequired;

        public void RegisterOn(MessageRouter router)
        {
            router.Register(GopetCmd.CLIENT_INFO, OnClientInfo);
            router.Register(GopetCmd.SERVER_LIST, OnServerList);
            router.Register(GopetCmd.LOGIN_SUCCES, OnLoginSuccess);
            router.Register(GopetCmd.LOGIN_FAILED, OnLoginFailed);
            router.Register(GopetCmd.CHANGE_PASSWORD, OnChangePasswordRejected);
            router.Register(RedDialog, OnRedDialog);
            router.Register(GopetCmd.CREATE_CHAR, OnCreateChar);

            router.RegisterEnvelope(OkDialog);
            router.RegisterSub(OkDialog, OkDialogSub, OnOkDialog);

            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.CHECK_SPEED, OnCheckSpeed);
        }

        private void OnClientInfo(Message m)
        {
            var accepted = m.Reader.ReadBool();
            m.Reader.ExpectFullyConsumed("CLIENT_INFO reply");
            ClientAccepted?.Invoke(accepted);
        }

        private void OnServerList(Message m) => ServerListReceived?.Invoke(ServerList.Parse(m));

        private void OnLoginSuccess(Message m) => LoginSucceeded?.Invoke(LoginSuccess.Parse(m));

        private void OnLoginFailed(Message m)
        {
            var reason = m.Reader.ReadUtf();
            m.Reader.ExpectFullyConsumed("LOGIN_FAILED");
            LoginFailed?.Invoke(reason);
        }

        private void OnChangePasswordRejected(Message m)
        {
            var reason = m.Reader.ReadUtf();
            m.Reader.ExpectFullyConsumed("CHANGE_PASSWORD");
            ErrorDialogShown?.Invoke(reason);
        }

        private void OnRedDialog(Message m)
        {
            var text = m.Reader.ReadUtf();
            m.Reader.ExpectFullyConsumed("dialog đỏ");
            DialogShown?.Invoke(text);
            ErrorDialogShown?.Invoke(text);
        }

        private void OnOkDialog(Message m)
        {
            var text = m.Reader.ReadUtf();
            m.Reader.ExpectFullyConsumed("dialog thường");
            DialogShown?.Invoke(text);
            SuccessDialogShown?.Invoke(text);
        }

        private void OnCreateChar(Message m)
        {
            // sbyte + int + int, tất cả bằng 0 (GameController.createChar).
            // Không mang thông tin gì, nhưng vẫn đọc hết để bắt lệch parser.
            m.Reader.ReadSByte();
            m.Reader.ReadInt();
            m.Reader.ReadInt();
            m.Reader.ExpectFullyConsumed("CREATE_CHAR");

            CharacterCreationRequired?.Invoke();
        }

        private void OnCheckSpeed(Message m)
        {
            // Con số này là HẠN PHẢI CHỜ, không phải thông tin tham khảo.
            //
            // onClientSpeedRespose (Player.cs:331) đo thời gian trả lời:
            //   elapsed + 2s < waitMs  ->  coi là speed-hack
            // Nhánh đó ghi lịch sử rồi return sớm, để stopwatch ở trạng thái đã
            // dừng — nên nhịp checkSpeed kế tiếp lại gửi gói mới. Trả lời tức thì
            // vì vậy sinh ra một trận bão gói (đo được ~2 gói/giây) và đánh dấu
            // tài khoản là hack ở mọi nhịp. Trên server nào bỏ comment dòng
            // user.ban(...) thì đó là khoá tài khoản một tiếng.
            //
            // Cận trên: checkSpeed đóng kết nối nếu elapsed > 3 * waitMs
            // (Player.cs:296). Cửa sổ hợp lệ là [waitMs - 2s, 3 * waitMs).
            // Client J2ME trả lời đúng waitMs — bám theo nó.
            var waitMs = m.Reader.ReadInt();
            m.Reader.ExpectFullyConsumed("CHECK_SPEED");

            _speedDueAtMs = _nowMs() + waitMs;
        }

        /// <summary>
        /// Phải gọi mỗi frame (Unity: <c>Update()</c>) — nếu không, nhịp CHECK_SPEED
        /// đang chờ sẽ không bao giờ được trả lời và server đóng kết nối.
        ///
        /// Không dùng Timer hay luồng riêng: gói phải đi từ luồng chính, cùng chỗ
        /// với mọi thứ khác của giao thức.
        /// </summary>
        public void Tick()
        {
            if (_speedDueAtMs == NoPending || _nowMs() < _speedDueAtMs) return;

            _speedDueAtMs = NoPending;
            _send(AuthPackets.CheckSpeedReply());
            SpeedChecked?.Invoke();
        }
    }
}
