using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Lấy mẫu đường đi của nhân vật theo pattern bản jar (<c>ew.java:436-497</c>): cứ
    /// tick lại push vị trí hiện tại; nếu 3 điểm cuối thẳng hàng thì xoá điểm giữa
    /// (tối ưu độ dài mảng gửi qua mạng). Sau 2 giây flush thành mảng gửi server.
    ///
    /// <para><b>Không phải pathfinding</b> — jar không tính đường tối ưu, chỉ ghi lại
    /// những gì người chơi đã bước qua. Tối ưu collinear giữ mảng ngắn khi đi thẳng.</para>
    ///
    /// <para><b>Wire format của <see cref="Flush"/></b>: <c>[x0, y0, x1, y1, ...]</c> interleaved,
    /// khớp <c>GopetPlace.sendMove</c> phía server.</para>
    /// </summary>
    public sealed class PathSampler
    {
        /// <summary>Khoảng flush khớp <c>GameController.TIME_MOVE_SEND</c> phía server.</summary>
        public const long FlushIntervalMs = 2000;

        /// <summary>
        /// Trần số điểm giữ trong bộ đệm. Server verify <c>readArrayLength(2, 64)</c>
        /// trên MẢNG INT interleaved, tức tối đa <b>32 cặp</b> (x,y). Giữ 31 cặp ở đây,
        /// chừa 1 slot cho điểm cuối mà <see cref="Flush"/> luôn cộng thêm — tổng ≤ 32.
        ///
        /// <para>Không cap ⇒ zig-zag 2 giây ở 60 fps đủ để đẩy mảng lên hàng trăm int,
        /// <see cref="Gopet.Net.Map.MapHandler.SendMove"/> ném ArgumentException, người chơi
        /// đứng hình câm giữa map. Đây là bug bị bắt ở console runtime, không phải test.</para>
        /// </summary>
        private const int MaxSamples = 31;

        private readonly List<(int x, int y)> _points = new List<(int, int)>();
        private long _startedAtMs;
        private bool _recording;

        public bool Recording => _recording;

        public int SampleCount => _points.Count;

        /// <summary>Bắt đầu ghi mẫu từ <paramref name="nowMs"/>. Xoá dữ liệu cũ.</summary>
        public void Start(long nowMs)
        {
            _points.Clear();
            _startedAtMs = nowMs;
            _recording = true;
        }

        /// <summary>
        /// Thêm mẫu (x, y). Nếu 3 điểm cuối (bao gồm mẫu mới) thẳng hàng — xoá điểm giữa
        /// để mảng không phình lên với người đi đường thẳng. Trùng điểm cuối cũng bị nuốt.
        /// </summary>
        public void Sample(int x, int y)
        {
            if (!_recording) return;

            var n = _points.Count;
            if (n >= 1 && _points[n - 1] == (x, y)) return; // trùng điểm cuối, bỏ qua

            if (n >= 2)
            {
                var a = _points[n - 2];
                var b = _points[n - 1];
                if (Collinear(a, b, (x, y))) _points.RemoveAt(n - 1);
            }

            // Đầy bộ đệm → drop điểm cũ nhất (queue). Server chỉ dùng 2 số cuối để đặt
            // vị trí; các điểm giữa là để các client khác nội suy đường đi. Cắt đầu
            // vẫn giữ được đích đúng và phần đường mới nhất — có ý nghĩa gameplay.
            if (_points.Count >= MaxSamples) _points.RemoveAt(0);

            _points.Add((x, y));
        }

        /// <summary>Đã tới lúc flush chưa (>= 2s kể từ Start)?</summary>
        public bool ShouldFlush(long nowMs) => _recording && nowMs - _startedAtMs >= FlushIntervalMs;

        /// <summary>
        /// Chốt điểm cuối (finalX, finalY), trả mảng interleaved rồi dừng ghi.
        /// Đảm bảo tối thiểu 2 điểm để khớp verify <c>readArrayLength(2, 64)</c> của server.
        /// </summary>
        public int[] Flush(int finalX, int finalY)
        {
            _recording = false;
            if (_points.Count == 0 || _points[_points.Count - 1] != (finalX, finalY))
            {
                _points.Add((finalX, finalY));
            }
            if (_points.Count == 1) _points.Add((finalX, finalY)); // thêm lần nữa để đủ 2

            // Safety net trùng logic Sample: nếu ai đó gọi Flush bỏ qua đường Sample
            // (test/gọi tay), vẫn chốt được mảng hợp lệ.
            while (_points.Count > MaxSamples + 1) _points.RemoveAt(0);

            var arr = new int[_points.Count * 2];
            for (var i = 0; i < _points.Count; i++)
            {
                arr[i * 2] = _points[i].x;
                arr[i * 2 + 1] = _points[i].y;
            }
            _points.Clear();
            return arr;
        }

        /// <summary>Ba điểm thẳng hàng ⇔ cross product = 0 (không phụ thuộc floating point).</summary>
        private static bool Collinear((int x, int y) a, (int x, int y) b, (int x, int y) c)
        {
            return (b.x - a.x) * (c.y - a.y) == (c.x - a.x) * (b.y - a.y);
        }
    }
}
