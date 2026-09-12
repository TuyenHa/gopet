using System;
using Gopet.Net.Auth;

namespace Gopet.Net.LiveSmoke
{
    internal sealed partial class LoginChecks
    {
        /// <summary>
        /// Giữ kết nối và trả lời CHECK_SPEED. Server hỏi mỗi 15-30 giây; không
        /// trả lời thì nó đóng kết nối, và triệu chứng trông hệt như lỗi mạng.
        /// </summary>
        private void HoldConnection(GopetSocket socket, AuthHandler auth)
        {
            Console.WriteLine($"       giữ kết nối {_holdSeconds}s để kiểm CHECK_SPEED...");

            var before = _speedReplies;
            MessagePump.Until(socket, _router, () => false, TimeSpan.FromSeconds(_holdSeconds), auth.Tick);

            Report.Check($"K. Giữ kết nối {_holdSeconds}s không bị đóng",
                socket.IsConnected, "server đã đóng kết nối");

            // Dưới ~35s thì có thể chưa tới nhịp hỏi đầu tiên, nên chỉ đòi hỏi
            // khi đã giữ đủ lâu — test không được đỏ vì lý do ngoài tầm kiểm soát.
            if (_holdSeconds >= 35)
            {
                // Server hỏi mỗi 15-30 giây, nên số nhịp phải nằm trong khoảng đó.
                //
                // CHẶN TRÊN mới là phần quan trọng: chỉ đòi "> 0" thì trả lời tức
                // thì cũng xanh, mà trả lời tức thì bị server coi là speed-hack và
                // nó bắn lại mỗi 500ms. Đúng lỗ hổng đã để lọt bug đó lần trước.
                var replies = _speedReplies - before;
                var most = _holdSeconds / 15 + 1;

                Report.Check($"L. Số nhịp CHECK_SPEED hợp lý (1..{most} trong {_holdSeconds}s)",
                    replies >= 1 && replies <= most,
                    replies == 0
                        ? "không trả lời nhịp nào"
                        : $"{replies} nhịp — quá nhiều, nhiều khả năng đang trả lời sớm");
            }
            else
            {
                Console.WriteLine($"       (bỏ qua check CHECK_SPEED: cần giữ >= 35s, đang giữ {_holdSeconds}s)");
            }
        }
    }
}
