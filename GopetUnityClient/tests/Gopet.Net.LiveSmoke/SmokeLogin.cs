using System;
using System.Threading;
using Gopet.Net;
using Gopet.Net.Auth;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Đăng nhập tối giản, dùng cho harness cần MỘT phiên sẵn sàng mà KHÔNG kèm các
    /// check khác — vd tài khoản thứ hai cho PvP (<see cref="PvpBattleChecks"/>).
    ///
    /// <para>Cố ý KHÔNG dùng chung code với <c>LoginChecks.Attempt</c> — đường đó gắn
    /// chặt với luồng A-M/T-DD đã verify xanh, tách riêng ra để không rủi ro làm hỏng
    /// nó khi thêm ca dùng mới.</para>
    /// </summary>
    internal static class SmokeLogin
    {
        internal sealed class Session : IDisposable
        {
            public GopetSocket Socket;
            public MessageRouter Router;
            public AuthHandler Auth;
            public LoginSuccess Success;
            public void Dispose() => Socket?.Dispose();
        }

        /// <summary>Đăng nhập, tự tạo nhân vật nếu tài khoản chưa có (kết nối lại 1 lần, giống LoginChecks).</summary>
        public static Session Connect(string host, int port, string username, string password)
        {
            var first = Attempt(host, port, username, password, out var needsCharacter);
            if (first != null) return first;
            return needsCharacter ? Attempt(host, port, username, password, out _) : null;
        }

        private static Session Attempt(string host, int port, string username, string password,
            out bool needsCharacter)
        {
            needsCharacter = false;
            Thread.Sleep(GopetSocket.ReconnectCooldownMs + 500);

            var socket = new GopetSocket();
            var router = new MessageRouter();
            var auth = new AuthHandler(socket.Send);

            bool? accepted = null;
            LoginSuccess success = null;
            string failure = null;
            var needsCreate = false;

            auth.ClientAccepted += ok => accepted = ok;
            auth.LoginSucceeded += ok => success = ok;
            auth.LoginFailed += reason => failure = reason;
            auth.DialogShown += text => failure = text;
            auth.CharacterCreationRequired += () => needsCreate = true;
            auth.RegisterOn(router);

            socket.Connect(host, port);
            socket.Send(new ClientInfo().ToMessage());
            MessagePump.Until(socket, router, () => accepted != null || failure != null,
                TimeSpan.FromSeconds(10), auth.Tick);
            if (accepted != true) { socket.Dispose(); return null; }

            socket.Send(AuthPackets.Login(username, password, "ref-live-smoke-2", "1.4.3"));
            MessagePump.Until(socket, router, () => success != null || failure != null || needsCreate,
                TimeSpan.FromSeconds(15), auth.Tick);

            if (needsCreate)
            {
                socket.Send(AuthPackets.CreateCharacter("pvp" + DateTime.UtcNow.ToString("HHmmssff"), 0));
                MessagePump.Until(socket, router, () => !socket.IsConnected, TimeSpan.FromSeconds(10), auth.Tick);
                needsCharacter = true;
                socket.Dispose();
                return null;
            }

            if (success == null) { socket.Dispose(); return null; }

            return new Session { Socket = socket, Router = router, Auth = auth, Success = success };
        }
    }
}
