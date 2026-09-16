using System;
using System.Collections.Generic;
using System.IO;
using Gopet.Net;
using Gopet.Net.Images;
using UnityEngine;

namespace Gopet.Runtime.Assets
{
    /// <summary>
    /// Lấy ảnh theo đường dẫn: bộ nhớ → đĩa → server.
    ///
    /// <para>Phần duy nhất của đường ống ảnh cần UnityEngine. Giao thức, gộp
    /// request và cache đĩa nằm ở <c>Assets/Scripts/Net/Images/</c> để test được
    /// ngoài Editor.</para>
    ///
    /// <para><b>Hợp đồng của <see cref="Get"/>:</b> callback có thể được gọi
    /// <b>một hoặc hai lần</b> — placeholder trước, ảnh thật sau. Và lần đầu có thể
    /// xảy ra ngay trong lời gọi. UI phải chịu được cả hai điều đó.</para>
    /// </summary>
    public sealed class RemoteAssetCache : IDisposable
    {
        /// <summary>
        /// Số ảnh giải mã tối đa mỗi frame. <c>Texture2D.LoadImage</c> bắt buộc chạy
        /// trên luồng chính; mở một menu 50 icon có thể về hết trong một frame
        /// (<c>GopetClient.maxPacketsPerFrame</c> mặc định 64) và làm khựng hình nếu
        /// giải mã tất cả cùng lúc.
        /// </summary>
        public int MaxDecodesPerFrame = 4;

        private readonly Dictionary<string, Texture2D> _memory = new Dictionary<string, Texture2D>();
        private readonly Queue<PendingDecode> _decodeQueue = new Queue<PendingDecode>();
        private readonly AssetDiskStore _disk;
        private readonly ImageHandler _handler;
        private readonly GopetClient _client;
        private readonly MessageRouter _router;
        private Texture2D _placeholder;

        private struct PendingDecode
        {
            public ImageResponse Response;
            public Action<Texture2D> OnReady;
        }

        public RemoteAssetCache(GopetClient client, MessageRouter router, string cacheRoot = null,
                                long maxCacheBytes = 200L * 1024 * 1024)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _router = router ?? throw new ArgumentNullException(nameof(router));

            _disk = new AssetDiskStore(
                cacheRoot ?? Path.Combine(Application.persistentDataPath, "assetcache"),
                maxCacheBytes);

            // Retry 2 lần khi timeout: rebuild/domain-reload/reconnect thường làm mất
            // gói ảnh giữa chừng → waiter callback không bao giờ chạy → NPC chỉ còn
            // label. 3 attempts (0+2) là bù thỏa hiệp cho flake tạm mà không đè server.
            _handler = new ImageHandler(client.Send, () => (long)(Time.realtimeSinceStartup * 1000f))
            {
                MaxRetries = 2,
            };
            _handler.RegisterOn(router);
            _handler.TimedOut += path => Debug.LogWarning($"[Gopet] Hết hạn chờ ảnh: {path}");
            _handler.WaiterFailed += (path, ex) =>
            {
                Debug.LogError($"[Gopet] Lỗi khi xử lý ảnh {path}");
                Debug.LogException(ex);
            };

            // Bám vào nhịp của GopetClient thay vì tự dựng Timer — mọi thứ của
            // giao thức phải chạy trên luồng chính.
            client.Ticked += OnTick;
        }

        /// <summary>Ảnh dùng tạm trong lúc chờ tải, để UI không phải xử lý null.</summary>
        public Texture2D Placeholder => _placeholder != null ? _placeholder : _placeholder = TextureFactory.Placeholder();

        public int MemoryCount => _memory.Count;

        public int PendingDecodeCount => _decodeQueue.Count;

        /// <summary>
        /// Lấy ảnh. Xem hợp đồng gọi callback ở phần mô tả class.
        /// </summary>
        public void Get(string path, sbyte type, Action<Texture2D> onReady)
        {
            if (onReady == null) throw new ArgumentNullException(nameof(onReady));

            if (string.IsNullOrEmpty(path) || path == ImagePackets.EmptyImagePath)
            {
                // Server cố ý không trả gì cho đường dẫn này — xin là chờ vô ích.
                onReady(Placeholder);
                return;
            }

            if (_memory.TryGetValue(path, out var cached) && cached != null)
            {
                onReady(cached);
                return;
            }

            if (!ImagePackets.IsCaptcha(path) && _disk.TryRead(path, out var fromDisk))
            {
                var decoded = TextureFactory.Decode(fromDisk);
                if (decoded != null)
                {
                    Remember(path, decoded);
                    onReady(decoded);
                    return;
                }
            }

            onReady(Placeholder);
            _handler.Request(path, type, response => _decodeQueue.Enqueue(
                new PendingDecode { Response = response, OnReady = onReady }));
        }

        private void OnTick()
        {
            _handler.Tick();

            var budget = MaxDecodesPerFrame;
            while (budget-- > 0 && _decodeQueue.Count > 0)
            {
                var item = _decodeQueue.Dequeue();
                Materialize(item.Response, item.OnReady);
            }
        }

        private void Materialize(ImageResponse response, Action<Texture2D> onReady)
        {
            var texture = TextureFactory.Decode(response.Png);
            if (texture == null)
            {
                Debug.LogWarning($"[Gopet] PNG hỏng: {response.Path}");
                return;
            }

            Remember(response.Path, texture);

            // Captcha sinh riêng cho từng phiên và chỉ dùng một lần — ghi đĩa là
            // vừa phí chỗ vừa có nguy cơ dùng lại ảnh đã hết hiệu lực.
            if (!ImagePackets.IsCaptcha(response.Path))
            {
                _disk.Write(response.Path, response.Png);
            }

            onReady(texture);
        }

        /// <summary>Ghi vào bộ nhớ, huỷ texture cũ nếu đang ghi đè cùng khoá.</summary>
        private void Remember(string path, Texture2D texture)
        {
            if (_memory.TryGetValue(path, out var old) && old != null && old != texture)
            {
                UnityEngine.Object.Destroy(old);
            }

            _memory[path] = texture;
        }

        /// <summary>
        /// Gỡ SẠCH mọi đăng ký. Thiếu bước này thì dựng lại lần hai trên cùng một
        /// Router — chuyện xảy ra mỗi lần nạp lại scene — sẽ ném ngay ở constructor,
        /// vì <c>MessageRouter.Register</c> từ chối opcode đã có handler.
        /// </summary>
        public void Dispose()
        {
            _client.Ticked -= OnTick;
            _router.Unregister(GopetCmd.COMMAND_IMAGE);

            foreach (var texture in _memory.Values)
            {
                if (texture != null) UnityEngine.Object.Destroy(texture);
            }

            _memory.Clear();
            _decodeQueue.Clear();

            if (_placeholder != null)
            {
                UnityEngine.Object.Destroy(_placeholder);
                _placeholder = null;
            }
        }
    }
}
