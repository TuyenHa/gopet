using System;
using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Gom thời gian từng frame rồi tính fps trung bình và fps 1% thấp nhất.
    /// Thuần C# — test được bằng cách bơm mảng delta, không cần chạy Unity.
    ///
    /// <para><b>Vì sao có 1% low:</b> trung bình che mất đúng thứ người chơi cảm
    /// nhận. Danh sách virtualized dựng dòng mới lúc cuộn, và cú khựng ấy chỉ nằm ở
    /// vài frame — trung bình 60 fps mà 1% low 12 fps vẫn là giật thấy rõ.</para>
    ///
    /// <para>Bỏ qua <c>WarmupFrames</c> frame đầu: frame đầu tiên gánh cả việc nạp
    /// scene và JIT, đưa vào thống kê là bôi bẩn số đo.</para>
    /// </summary>
    public sealed class FpsSampler
    {
        public const int WarmupFrames = 10;

        private readonly List<float> _deltas = new List<float>();
        private int _seen;

        public int SampleCount => _deltas.Count;

        /// <param name="deltaSeconds">Thời gian frame, giây. Bỏ qua giá trị &lt;= 0.</param>
        public void Add(float deltaSeconds)
        {
            if (deltaSeconds <= 0f) return;

            _seen++;
            if (_seen <= WarmupFrames) return;

            _deltas.Add(deltaSeconds);
        }

        public void Reset()
        {
            _deltas.Clear();
            _seen = 0;
        }

        /// <summary>Fps trung bình trên toàn bộ mẫu. 0 khi chưa có mẫu nào.</summary>
        public float AverageFps()
        {
            if (_deltas.Count == 0) return 0f;

            var total = 0f;
            foreach (var d in _deltas) total += d;

            return _deltas.Count / total;
        }

        /// <summary>
        /// Fps của phân vị 99 về thời gian frame — tức 1% frame chậm nhất.
        /// Ít mẫu thì lấy thẳng frame chậm nhất.
        /// </summary>
        public float OnePercentLowFps()
        {
            if (_deltas.Count == 0) return 0f;

            var sorted = new List<float>(_deltas);
            sorted.Sort();

            var index = (int)Math.Floor(sorted.Count * 0.99f);
            if (index >= sorted.Count) index = sorted.Count - 1;

            return 1f / sorted[index];
        }

        public override string ToString()
        {
            return $"{SampleCount} frame | trung bình {AverageFps():F1} fps | 1% thấp {OnePercentLowFps():F1} fps";
        }
    }
}
