using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Gopet.Net
{
    /// <summary>
    /// Kết nối TCP tới GServer: handshake, mã hoá, hai luồng đọc/ghi.
    /// Format khung gói nằm ở <see cref="PacketFramer"/>.
    ///
    /// <para><b>Không phụ thuộc UnityEngine</b> — để unit test được ngoài Unity.
    /// Phần cầu nối sang Unity nằm ở <c>Runtime/GopetClient.cs</c>.</para>
    ///
    /// <para><b>Luật thread:</b> class này không bao giờ gọi callback trên luồng nền.
    /// Gói nhận được đẩy vào <see cref="Incoming"/>; phía Unity rút ra trong
    /// <c>Update()</c>. Gọi Unity API từ luồng nền là crash.</para>
    ///
    /// Mirror của <c>GServer/Server/IO/{Session,MsgReader,MsgSender}.cs</c>.
    /// </summary>
    public sealed class GopetSocket : IDisposable
    {
        private readonly ConcurrentQueue<Message> _incoming = new ConcurrentQueue<Message>();
        private readonly BlockingCollection<Message> _outgoing = new BlockingCollection<Message>();
        private readonly PacketLogger _logger;

        private TcpClient _client;
        private NetworkStream _stream;
        private Tea _tea;
        private Thread _readThread;
        private Thread _writeThread;
        private volatile bool _connected;
        private int _failed;
        private int _disposed;

        public GopetSocket(PacketLogger logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Server từ chối kết nối mới từ CÙNG một IP trong 2 giây (<c>Server.cs:73</c>):
        /// nó vẫn accept ở tầng TCP rồi đóng ngay, không gửi gì cả.
        ///
        /// Nên logic tự kết nối lại phải chờ ít nhất chừng này, nếu không triệu chứng
        /// sẽ là "kết nối được rồi rớt ngay" lặp vô hạn mà không lời giải thích nào.
        /// </summary>
        public const int ReconnectCooldownMs = 2000;

        public bool IsConnected => _connected;

        /// <summary>
        /// Đầu cục bộ của socket, hoặc null khi chưa kết nối.
        /// Có để kiểm chứng rò socket mà không phải đoán qua bảng TCP của hệ điều hành —
        /// trên loopback rất dễ nhặt nhầm kết nối cũ còn ở TIME_WAIT.
        /// </summary>
        public IPEndPoint LocalEndPoint => _client?.Client?.LocalEndPoint as IPEndPoint;

        /// <summary>
        /// <c>true</c> nếu <see cref="Dispose"/> đã join được cả hai luồng nền trong hạn.
        /// <c>false</c> nghĩa là còn luồng sống sau khi đóng — tức là rò.
        ///
        /// Có thuộc tính này vì đếm số luồng của tiến trình không đáng tin: threadpool
        /// và JIT tự co giãn, và một luồng quá hạn join vẫn có thể thoát ngay sau đó,
        /// biến mất trước khi kịp đếm.
        /// </summary>
        public bool DisposedCleanly { get; private set; }

        /// <summary>Gói đã nhận, chờ luồng chính xử lý.</summary>
        public ConcurrentQueue<Message> Incoming => _incoming;

        /// <summary>Bắn khi kết nối đứt. Gọi từ luồng nền — chỉ đặt cờ, đừng đụng Unity API.</summary>
        public event Action<string> Disconnected;

        public void Connect(string host, int port)
        {
            if (_connected) throw new InvalidOperationException("Đã kết nối rồi.");

            _client = new TcpClient { NoDelay = true };
            _client.Connect(host, port);
            _stream = _client.GetStream();

            // Client tự sinh khoá rồi gửi lên; server dùng chính khoá đó.
            // Yếu về bảo mật nhưng là thiết kế gốc — giữ nguyên để client J2ME cũ
            // và client Unity cùng chạy được. Xem eq.java:a(long) và Session.cs:80.
            var key = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Handshake.Write(_stream, key);
            _tea = new Tea(key);

            _connected = true;

            _readThread = new Thread(ReadLoop) { IsBackground = true, Name = "Gopet Read" };
            _writeThread = new Thread(WriteLoop) { IsBackground = true, Name = "Gopet Write" };
            _readThread.Start();
            _writeThread.Start();
        }

        public void Send(Message message)
        {
            if (!_connected)
            {
                message.Dispose();
                return;
            }

            _outgoing.Add(message);
        }

        private void WriteLoop()
        {
            try
            {
                foreach (var message in _outgoing.GetConsumingEnumerable())
                {
                    if (!_connected) break;

                    var payload = message.ToWire();
                    _logger?.Log(PacketDirection.Out, message.Id, message.IsEncrypted, payload);

                    var wire = message.IsEncrypted ? _tea.Encrypt(payload) : payload;
                    if (wire == null)
                    {
                        throw new ProtocolException($"Mã hoá thất bại cho opcode {message.Id}.");
                    }

                    PacketFramer.WriteFrame(_stream, wire, message.IsEncrypted);
                    message.Dispose();
                }
            }
            catch (Exception ex)
            {
                Fail($"Lỗi luồng ghi: {ex.Message}");
            }
        }

        private void ReadLoop()
        {
            try
            {
                while (_connected)
                {
                    if (!PacketFramer.TryReadFrame(_stream, out var raw, out var wasEncrypted)) break;
                    if (raw.Length == 0) continue;

                    // Thực tế server luôn gửi plaintext, nhưng vẫn tôn trọng cờ.
                    var payload = wasEncrypted ? _tea.Decrypt(raw) : raw;
                    if (payload == null || payload.Length == 0)
                    {
                        throw new ProtocolException("Giải mã thất bại hoặc payload rỗng.");
                    }

                    _logger?.Log(PacketDirection.In, unchecked((sbyte)payload[0]), wasEncrypted, payload);
                    _incoming.Enqueue(Message.FromWire(payload, wasEncrypted));
                }

                Fail("Server đóng kết nối.");
            }
            catch (Exception ex)
            {
                Fail($"Lỗi luồng đọc: {ex.Message}");
            }
        }

        private void Fail(string reason)
        {
            // Luồng đọc và luồng ghi có thể cùng hỏng một lúc; kiểm-rồi-gán
            // để lọt cả hai thì handler bị gọi hai lần.
            if (Interlocked.Exchange(ref _failed, 1) != 0) return;
            _connected = false;

            try { Disconnected?.Invoke(reason); }
            catch { /* handler lỗi không được kéo theo socket */ }
        }

        public void Dispose()
        {
            // Unity gọi cả OnDestroy lẫn OnApplicationQuit khi thoát game, nên
            // Dispose phải chịu được gọi hai lần — lần hai mà chạy tiếp thì
            // CompleteAdding() trên collection đã dispose sẽ ném.
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            _connected = false;
            _outgoing.CompleteAdding();

            try { _stream?.Dispose(); } catch { /* đang tắt, nuốt */ }
            try { _client?.Close(); } catch { /* đang tắt, nuốt */ }

            var timeout = TimeSpan.FromSeconds(1);
            var readDone = _readThread == null || _readThread.Join(timeout);
            var writeDone = _writeThread == null || _writeThread.Join(timeout);
            DisposedCleanly = readDone && writeDone;

            _outgoing.Dispose();
        }
    }
}
