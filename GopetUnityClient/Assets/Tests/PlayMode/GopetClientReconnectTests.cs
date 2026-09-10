using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Gopet.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nối lại sang máy chủ khác, chạy trên socket THẬT (một <see cref="TcpListener"/>
    /// trong process, không giả lập gì).
    ///
    /// <para><b>Vì sao phải có:</b> tự tay đóng một socket đang sống làm luồng đọc của
    /// nó ném và báo "mất kết nối". Lý do đó nằm lại trong hàng chờ, sống qua cả
    /// cooldown, rồi giết chính socket vừa mở xong — thành vòng lặp 2,5 giây một nhịp.
    /// Và nó chỉ nổ ở đường chạy THẬT: server phát danh sách máy chủ có IP khác máy
    /// đang nối, còn lúc dev thì danh sách trả về <c>127.0.0.1</c> nên
    /// <c>LoginFlow</c> không nối lại lần nào. Trước đây không có test nào chạm tới
    /// <see cref="GopetClient"/>.</para>
    /// </summary>
    public sealed class GopetClientReconnectTests
    {
        /// <summary>Cooldown 2 giây của server + biên của client + vài frame.</summary>
        private const float ConnectTimeout = 10f;

        private readonly List<TcpListener> _listeners = new List<TcpListener>();
        private readonly List<TcpClient> _accepted = new List<TcpClient>();

        private GameObject _host;
        private GopetClient _client;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("GopetClientHost");
            _client = _host.AddComponent<GopetClient>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);

            foreach (var accepted in _accepted) accepted.Close();
            foreach (var listener in _listeners) listener.Stop();

            _accepted.Clear();
            _listeners.Clear();
        }

        /// <summary>Một cổng nghe thật, nhận kết nối rồi GIỮ nguyên — không đọc, không đóng.</summary>
        private int Listen()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            _listeners.Add(listener);

            Accept(listener);
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        private async void Accept(TcpListener listener)
        {
            try { _accepted.Add(await listener.AcceptTcpClientAsync()); }
            catch { /* Stop() lúc dọn dẹp làm accept đang chờ ném — đúng như mong đợi */ }
        }

        private static IEnumerator WaitUntil(System.Func<bool> done, float seconds)
        {
            var deadline = Time.unscaledTime + seconds;
            while (!done() && Time.unscaledTime < deadline) yield return null;
        }

        [UnityTest]
        public IEnumerator NoiSangMayChuKhac_KhongGietSocketMoi()
        {
            _client.Connect("127.0.0.1", Listen());
            yield return WaitUntil(() => _client.IsConnected, ConnectTimeout);
            Assert.IsTrue(_client.IsConnected, "Không nối được máy chủ đầu tiên.");

            string dropped = null;
            _client.Disconnected += reason => dropped = reason;

            _client.Connect("127.0.0.1", Listen());
            yield return WaitUntil(() => _client.IsConnected, ConnectTimeout);

            // Cờ đứt cũ (nếu còn sót) chỉ ra tay ở frame SAU khi socket mới mở xong.
            for (var i = 0; i < 10; i++) yield return null;

            Assert.IsNull(dropped, $"Socket mới bị cú đứt của socket cũ giết: {dropped}");
            Assert.IsTrue(_client.IsConnected, "Nối lại xong nhưng kết nối đã chết.");
        }

        /// <summary>Cú đứt THẬT vẫn phải báo ra — sửa lỗi trên không được nuốt luôn cái này.</summary>
        [UnityTest]
        public IEnumerator ServerDongThat_VanBaoMatKetNoi()
        {
            _client.Connect("127.0.0.1", Listen());
            yield return WaitUntil(() => _client.IsConnected, ConnectTimeout);
            Assert.IsTrue(_client.IsConnected);

            // `IsConnected` chỉ cần bắt tay TCP xong ở tầng OS — không chờ
            // `AcceptTcpClientAsync()` (chạy async, có thể trễ vài frame) thật sự
            // điền vào `_accepted`. Đóng "chưa có gì để đóng" thì server không đóng
            // gì cả, và test treo tới hết ConnectTimeout mà không hiểu vì sao.
            yield return WaitUntil(() => _accepted.Count > 0, ConnectTimeout);
            Assert.IsNotEmpty(_accepted, "Server chưa kịp accept — không có gì để đóng.");

            string dropped = null;
            _client.Disconnected += reason => dropped = reason;

            foreach (var accepted in _accepted) accepted.Close();

            yield return WaitUntil(() => dropped != null, ConnectTimeout);

            Assert.IsNotNull(dropped, "Server đóng thật mà client không báo gì.");
            Assert.IsFalse(_client.IsConnected);
        }
    }
}
