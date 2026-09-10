using System;
using System.IO;
using Gopet.Net;
using UnityEngine;

namespace Gopet.Runtime
{
    /// <summary>
    /// Cầu nối giữa <see cref="GopetSocket"/> (thuần C#, luồng nền) và vòng đời Unity.
    ///
    /// <para><b>Việc duy nhất của class này:</b> rút gói tin từ hàng đợi và dispatch
    /// trên luồng chính. Logic giao thức nằm trong handler đăng ký với
    /// <see cref="Router"/>, không nằm ở đây.</para>
    /// </summary>
    public sealed class GopetClient : MonoBehaviour
    {
        /// <summary>Chờ thêm quá cooldown của server một chút, cho lệch đồng hồ hai máy.</summary>
        private const float ReconnectMarginSeconds = 0.5f;

        [Header("Kết nối")]
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 19180;

        [Header("Gỡ lỗi")]
        [Tooltip("Ghi hex dump mọi gói tin — công cụ chính để bắt desync. " +
                 "CẢNH BÁO: dump chứa gói LOGIN, tức MẬT KHẨU dạng thô. " +
                 "Chỉ ghi ở Editor và bản development; bản phát hành bỏ qua cờ này.")]
        [SerializeField] private bool enablePacketLog = true;

        [Tooltip("Số gói xử lý tối đa mỗi frame. Chặn việc một đợt gói dồn làm đứng hình.")]
        [SerializeField] private int maxPacketsPerFrame = 64;

        /// <summary><c>volatile</c>: luồng đọc của socket đọc nó — xem <see cref="OpenSocket"/>.</summary>
        private volatile GopetSocket _socket;

        private PacketLogger _logger;
        private volatile string _pendingDisconnectReason;

        private bool _connectPending;
        private float _connectAllowedAt;

        /// <summary>Đăng ký handler ở đây, trước khi gọi <see cref="Connect()"/>.</summary>
        public MessageRouter Router { get; } = new MessageRouter();

        public bool IsConnected => _socket != null && _socket.IsConnected;

        /// <summary>Cả hai bắn trên luồng chính. Lý do đứt có thể rỗng.</summary>
        public event Action Connected;
        public event Action<string> Disconnected;

        /// <summary>
        /// Bắn mỗi frame sau khi đã dispatch xong gói tin. Đăng ký <c>AuthHandler.Tick</c>
        /// và <c>LoginFlow.Tick</c> vào đây: việc theo thời gian phải bám móc này chứ
        /// không tự dựng Timer — mọi thứ của giao thức phải đi từ luồng chính.
        /// </summary>
        public event Action Ticked;

        public void Connect() => Connect(host, port);

        /// <summary>
        /// Xếp lịch nối tới một máy chủ; việc nối làm ở <c>Update()</c>, vì hai lý do.
        ///
        /// <para>1. Server từ chối nối lại trong <see cref="GopetSocket.ReconnectCooldownMs"/>
        /// mili-giây với cùng một IP. Nối ngay sau khi rớt — chuyện xảy ra ngay sau khi
        /// tạo nhân vật, vì server tự đóng — bị đá ra, và trông hệt như server chết.</para>
        ///
        /// <para>2. Nối hỏng thì báo qua <see cref="Disconnected"/> chứ không ném: người
        /// gọi là màn đăng nhập, nó cần một câu để hiện chứ không cần exception.</para>
        /// </summary>
        public void Connect(string targetHost, int targetPort)
        {
            host = targetHost;
            port = targetPort;

            Cleanup();
            _connectPending = true;
        }

        public void Send(Message message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning($"[Gopet] Bỏ qua gói opcode {message.Id} — chưa kết nối.");
                message.Dispose();
                return;
            }

            _socket.Send(message);
        }

        private void Update()
        {
            if (_connectPending && Time.unscaledTime >= _connectAllowedAt) OpenSocket();

            // Ticked phải bắn KỂ CẢ khi chưa hoặc đã mất kết nối: handler dựa vào nó
            // để tính hết hạn. Return sớm ở đây là request đang chờ không bao giờ hết
            // hạn, và callback kẹt vĩnh viễn sau mỗi lần rớt mạng.
            // Chụp snapshot: socket có thể bị callback Disconnected thay thế giữa phép
            // kiểm tra null và TryDequeue (đặc biệt đúng lúc CREATE_CHAR đóng kết nối).
            var socket = _socket;
            if (socket == null)
            {
                Ticked?.Invoke();
                return;
            }

            var processed = 0;
            while (processed < maxPacketsPerFrame && socket.Incoming.TryDequeue(out var message))
            {
                Router.Dispatch(message);
                processed++;
            }

            Ticked?.Invoke();

            var reason = _pendingDisconnectReason;
            if (reason != null)
            {
                _pendingDisconnectReason = null;
                Debug.LogWarning($"[Gopet] Mất kết nối: {reason}");
                Cleanup();
                Disconnected?.Invoke(reason);
            }
        }

        private void OpenSocket()
        {
            _connectPending = false;

            // Đai an toàn thứ hai: luồng đọc của socket cũ báo muộn hơn cả lúc Dispose
            // trả về thì lý do đó vẫn không dính sang socket mới.
            _pendingDisconnectReason = null;

            // Logger sống theo cả phiên chạy app: PacketLogger mở file với
            // append=false, dựng lại lúc nối lại là xoá trắng dump của lần trước.
            // `Debug.isDebugBuild` là true trong Editor và ở bản development, false ở
            // bản phát hành — nên bản đến tay người chơi không ghi mật khẩu ra đĩa.
            if (enablePacketLog && Debug.isDebugBuild && _logger == null)
            {
                var path = Path.Combine(Application.persistentDataPath, "packet-dump-unity.log");
                _logger = new PacketLogger(path);
                Debug.Log($"[Gopet] Packet log: {path}");
            }

            var socket = new GopetSocket(_logger);
            _socket = socket;

            // Chạy trên luồng nền — chỉ ghi lại lý do, xử lý ở Update().
            //
            // BỎ QUA cú đứt của socket KHÔNG còn là socket hiện tại. Tự tay đóng một
            // socket đang sống (đổi máy chủ, nối lại) làm luồng đọc của nó ném và báo
            // "mất kết nối"; lý do đó nằm lại trong hàng chờ, sống qua cả cooldown, rồi
            // giết chính socket vừa mở xong — vòng lặp 2,5 giây một nhịp. Và nó nổ ở
            // ĐÚNG đường chạy thật: server phát danh sách máy chủ có IP khác máy đang
            // nối; lúc dev thì danh sách trả về 127.0.0.1 nên không nối lại lần nào.
            socket.Disconnected += reason =>
            {
                if (!ReferenceEquals(_socket, socket)) return;

                _pendingDisconnectReason = reason;
            };

            try
            {
                socket.Connect(host, port);
                Debug.Log($"[Gopet] Đã kết nối {host}:{port}");
                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Gopet] Kết nối thất bại: {ex.Message}");
                Cleanup();
                Disconnected?.Invoke(ex.Message);
            }
        }

        private void OnDestroy()
        {
            Cleanup();
            _logger?.Dispose();
            _logger = null;
        }

        private void OnApplicationQuit() => OnDestroy();

        private void Cleanup()
        {
            var socket = _socket;
            if (socket == null) return;

            // Gỡ khỏi _socket TRƯỚC khi Dispose: Dispose làm luồng đọc ném và gọi
            // handler Disconnected, và handler đó phân biệt "đứt thật" với "ta tự đóng"
            // bằng đúng phép so sánh này.
            _socket = null;
            socket.Dispose();

            // Mọi lần nối tiếp theo phải chờ hết cooldown của server.
            _connectAllowedAt = Time.unscaledTime
                                + GopetSocket.ReconnectCooldownMs / 1000f
                                + ReconnectMarginSeconds;
        }
    }
}
