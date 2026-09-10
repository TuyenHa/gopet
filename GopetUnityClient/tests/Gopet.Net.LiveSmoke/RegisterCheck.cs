using System;
using System.Threading;
using Gopet.Net;
using Gopet.Net.Auth;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Kiểm gói <c>REGISTER</c> đi tới nơi và được server hiểu.
    ///
    /// <para><b>Không kiểm đăng ký thành công</b> — không kiểm được: <c>doRegister</c>
    /// mở đầu bằng <c>if (true) { redDialog("Chức năng này bị khóa..."); return; }</c>
    /// (<c>Player.cs:191</c>). Server gốc bắt đăng ký qua web.</para>
    ///
    /// <para>Nhận đúng dialog "bị khoá" đã chứng minh gói được đóng đúng dạng và
    /// server đọc được nó — phần còn lại là chính sách, không phải giao thức.</para>
    /// </summary>
    internal static class RegisterCheck
    {
        public static void Run(string host, int port)
        {
            Thread.Sleep(GopetSocket.ReconnectCooldownMs + 500);

            using var socket = new GopetSocket();
            var router = new MessageRouter();

            bool? accepted = null;
            string dialog = null;

            var auth = new AuthHandler(socket.Send);
            auth.ClientAccepted += ok => accepted = ok;
            auth.DialogShown += text => dialog = text;
            auth.RegisterOn(router);

            socket.Connect(host, port);

            socket.Send(new ClientInfo().ToMessage());
            MessagePump.Until(socket, router, () => accepted != null, TimeSpan.FromSeconds(10));

            // Tên/mật khẩu hợp lệ theo AuthRules, để nếu server có mở khoá thì
            // nó đi tiếp được chứ không dừng vì dữ liệu sai.
            socket.Send(AuthPackets.Register("smoketestacc", "abc12345"));
            MessagePump.Until(socket, router, () => dialog != null, TimeSpan.FromSeconds(10));

            Report.Check("M. Gói REGISTER được server đọc và phản hồi",
                dialog != null, "không nhận được phản hồi nào trong 10s");

            if (dialog != null)
            {
                Console.WriteLine($"       -> \"{dialog}\"");
            }
        }
    }
}
