using System;
using System.Collections.Generic;

namespace Gopet.Net.Images
{
    public sealed partial class ImageHandler
    {
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
