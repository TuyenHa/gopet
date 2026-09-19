using System;
using System.Collections.Generic;

namespace Gopet.Net.Images
{
    /// <summary>
    /// Xin ảnh từ server: gộp request trùng, giới hạn số gói đang bay, và tự bỏ
    /// cuộc khi hết hạn.
    ///
    /// <para><b>Vì sao phải có hạn chờ riêng:</b> <c>requestImg</c> im lặng <c>return</c>
    /// ở SÁU nhánh (<c>GameController.cs:872-944</c>): đường dẫn bằng
    /// <c>EMPTY_IMG_PATH</c>, chuỗi rỗng sau khi tra <c>itemAssetsIcon</c>, không có
    /// file, <c>gameType != 0</c>, <c>type</c> là 10/11, hoặc captcha chưa sinh.
    /// Ngoài ra <c>loadAssets</c> ném lỗi thì bị nuốt luôn. Server KHÔNG bao giờ báo
    /// lỗi. Không tự đặt hạn thì callback treo vĩnh viễn và UI đứng ở placeholder
    /// mãi mãi.</para>
    ///
    /// <para>Thuần C#, không UnityEngine — phần dựng <c>Texture2D</c> nằm ở
    /// <c>Runtime/Assets/RemoteAssetCache.cs</c>.</para>
    /// </summary>
    public sealed class ImageHandler
    {
        /// <summary>Số gói đang bay tối đa. Xin cả một menu 50 icon cùng lúc sẽ nghẽn hàng đợi gửi.</summary>
        public int MaxInFlight = 8;

        /// <summary>Hạn chờ mỗi ảnh.</summary>
        public long TimeoutMs = 10000;

        /// <summary>Số lần THỬ LẠI khi timeout trước khi bỏ cuộc (fire TimedOut).
        /// Mặc định 0 = giữ hành vi cũ (không retry). Đặt >0 để chịu được thất lạc
        /// tạm thời sau khi rebuild / reconnect (client mất waiter response cũ).</summary>
        public int MaxRetries;

        private sealed class Pending
        {
            public sbyte Type;
            public long DeadlineMs;
            public bool Sent;
            public int Retries;
            public readonly List<Action<ImageResponse>> Waiters = new List<Action<ImageResponse>>();
        }

        private readonly Action<Message> _send;
        private readonly Func<long> _nowMs;
        private readonly Dictionary<string, Pending> _pending = new Dictionary<string, Pending>();
        private readonly List<string> _queue = new List<string>();
        private int _inFlight;

        public ImageHandler(Action<Message> send, Func<long> nowMs)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
            _nowMs = nowMs ?? throw new ArgumentNullException(nameof(nowMs));
        }

        /// <summary>Bắn khi hết hạn mà server vẫn im. Tham số là đường dẫn đã xin.</summary>
        public event Action<string> TimedOut;

        /// <summary>Bắn khi một nơi chờ ném lỗi lúc nhận ảnh. Không dừng những nơi còn lại.</summary>
        public event Action<string, Exception> WaiterFailed;

        public int InFlightCount => _inFlight;

        public int QueuedCount => _queue.Count;

        public void RegisterOn(MessageRouter router)
        {
            router.Register(GopetCmd.COMMAND_IMAGE, OnImage);
        }

        /// <summary>
        /// Xin một ảnh. Nhiều nơi cùng xin một đường dẫn chỉ sinh MỘT gói tin —
        /// mở một menu 50 icon giống nhau không được thành 50 gói.
        /// </summary>
        public void Request(string path, sbyte type, Action<ImageResponse> onLoaded)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Đường dẫn rỗng.", nameof(path));
            if (onLoaded == null) throw new ArgumentNullException(nameof(onLoaded));

            if (_pending.TryGetValue(path, out var existing))
            {
                existing.Waiters.Add(onLoaded);
                return;
            }

            // DeadlineMs đặt lúc GỬI chứ không phải lúc xin: nằm chờ trong hàng đợi
            // không được tính vào hạn chờ của server.
            var entry = new Pending { Type = type };
            entry.Waiters.Add(onLoaded);
            _pending[path] = entry;
            _queue.Add(path);

            PumpQueue();
        }

        /// <summary>
        /// Gọi mỗi frame: đẩy hàng đợi và dọn những cái quá hạn.
        /// Quên gọi thì ảnh không bao giờ hết hạn và hàng đợi không bao giờ nhích.
        /// </summary>
        public void Tick()
        {
            PumpQueue();

            var now = _nowMs();
            List<string> expired = null;

            foreach (var pair in _pending)
            {
                if (pair.Value.Sent && now >= pair.Value.DeadlineMs)
                {
                    (expired ??= new List<string>()).Add(pair.Key);
                }
            }

            if (expired == null) return;

            foreach (var path in expired)
            {
                var entry = _pending[path];
                _inFlight--;
                if (entry.Retries < MaxRetries)
                {
                    // Retry: reset Sent + re-queue, giữ nguyên Waiters. PumpQueue
                    // dưới sẽ bắn lại request (deadline mới đặt lúc gửi).
                    entry.Retries++;
                    entry.Sent = false;
                    _queue.Add(path);
                }
                else
                {
                    // Trước đây _pending.Remove xoá luôn Waiters → waiter callback (từ
                    // RemoteAssetCache.Get) không bao giờ chạy → NPC đọng placeholder
                    // vĩnh viễn. Bắn synthetic response (Png=null) trước khi xoá để
                    // downstream biết "load fail" và có thể swap sang fallback texture.
                    var failed = new ImageResponse { GameType = 0, Type = entry.Type, Path = path, Png = null };
                    _pending.Remove(path);
                    NotifyAll(entry, failed);
                    TimedOut?.Invoke(path);
                }
            }

            PumpQueue();
        }

        private void PumpQueue()
        {
            while (_queue.Count > 0 && _inFlight < MaxInFlight)
            {
                var path = _queue[0];
                _queue.RemoveAt(0);

                // Có thể đã hết hạn hoặc đã xong trong lúc còn xếp hàng.
                if (!_pending.TryGetValue(path, out var entry) || entry.Sent) continue;

                entry.Sent = true;
                entry.DeadlineMs = _nowMs() + TimeoutMs;
                _inFlight++;
                _send(ImagePackets.Request(path, entry.Type));
            }
        }

        /// <summary>
        /// Gọi mọi nơi đang chờ. Một waiter ném lỗi (chạm GameObject đã Destroy
        /// chẳng hạn) KHÔNG được kéo theo 49 waiter còn lại, và cũng không được
        /// nổ ngược lên vòng dispatch của frame.
        /// </summary>
        private void NotifyAll(Pending entry, ImageResponse response)
        {
            foreach (var waiter in entry.Waiters)
            {
                try
                {
                    waiter(response);
                }
                catch (Exception ex)
                {
                    WaiterFailed?.Invoke(response.Path, ex);
                }
            }
        }

        private void OnImage(Message m)
        {
            var response = ImageResponse.Parse(m);

            // Ghép theo originPath server dội lại, không phải đường đã giải.
            if (!_pending.TryGetValue(response.Path, out var entry)) return;

            _pending.Remove(response.Path);

            // Chỉ trừ khi entry NÀY thực sự đang bay.
            //
            // Kịch bản làm lệch: xin ảnh -> hết hạn (đã trừ một lần) -> xin lại
            // đúng lúc đủ 8 gói đang bay nên nó nằm chờ trong hàng đợi (Sent=false)
            // -> gói muộn của lần xin trước tới, khớp entry mới. Trừ vô điều kiện
            // ở đây là trừ hai lần cho một lần cộng, _inFlight tụt dần xuống âm và
            // MaxInFlight mất tác dụng vĩnh viễn.
            if (entry.Sent) { _inFlight--; }
            else { _queue.Remove(response.Path); }

            NotifyAll(entry, response);
            PumpQueue();
        }
    }
}
