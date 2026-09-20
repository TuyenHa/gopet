using System;
using System.Threading;
using Gopet.Net;
using Gopet.Net.Auth;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Chạy trọn luồng đăng nhập với GServer thật, qua đúng đường mà Unity dùng:
    /// <see cref="MessageRouter"/> + <see cref="AuthHandler"/>, gói được rút khỏi
    /// hàng đợi trên luồng chính giống <c>GopetClient.Update()</c>.
    /// </summary>
    internal sealed partial class LoginChecks
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly int _holdSeconds;

        /// <summary>Router mới cho mỗi lượt — đăng ký lại lên router cũ sẽ ném "opcode đã có handler".</summary>
        private MessageRouter _router;

        private bool? _accepted;
        private ServerEntry[] _servers;
        private LoginSuccess _success;
        private string _failure;
        private int _speedReplies;
        private bool _needsCharacter;

        public LoginChecks(string host, int port, string username, string password, int holdSeconds)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _holdSeconds = holdSeconds;
        }

        public void Run()
        {
            // Tài khoản mới chưa có nhân vật: server đòi tạo rồi ĐÓNG kết nối,
            // nên phải vào lại lần hai. Đây là nhánh thật của lần đăng nhập đầu
            // tiên, không phải lỗi — client Unity cũng sẽ gặp.
            if (Attempt()) return;

            if (!_needsCharacter)
            {
                Report.Fail("G-L. Đăng nhập", "lượt đầu dừng giữa chừng mà không phải vì thiếu nhân vật");
                return;
            }

            Console.WriteLine("       tài khoản chưa có nhân vật — tạo rồi đăng nhập lại");
            ResetState();

            // Lượt hai KHÔNG được đòi tạo nhân vật nữa. Không kiểm chỗ này thì
            // check I/J/K/L lặng lẽ không chạy mà harness vẫn in "LIVE SMOKE OK".
            if (!Attempt())
            {
                Report.Fail("G-L. Đăng nhập",
                    "lượt hai vẫn chưa đăng nhập được — các check I/J/K/L đã không chạy");
            }
        }

        private void ResetState()
        {
            _accepted = null;
            _servers = null;
            _success = null;
            _failure = null;
            _needsCharacter = false;
        }

        /// <summary>Một lượt: kết nối, CLIENT_INFO, danh sách server, đăng nhập.</summary>
        private bool Attempt()
        {
            // Server chặn kết nối lại trong 2 giây với cùng một IP.
            Thread.Sleep(GopetSocket.ReconnectCooldownMs + 500);

            using var socket = new GopetSocket();

            _router = new MessageRouter();
            var auth = new AuthHandler(socket.Send);
            auth.ClientAccepted += ok => _accepted = ok;
            auth.ServerListReceived += list => _servers = list;
            auth.LoginSucceeded += ok => _success = ok;
            auth.LoginFailed += reason => _failure = reason;
            auth.DialogShown += text => _failure = text;
            auth.SpeedChecked += () => _speedReplies++;
            auth.CharacterCreationRequired += () => _needsCharacter = true;
            auth.RegisterOn(_router);

            // Server đẩy INIT_PLAYER + MAP_UPDATE ngay sau LOGIN_SUCCES, trong cùng nhịp
            // pump của gói login — nên MapHandler phải lắp TRƯỚC khi bơm login, không thì
            // gói bị Dispatch bỏ (OnUnhandled im lặng) và Verify không có gì để đọc.
            var mapRec = MapMovementChecks.Register(socket, _router);

            socket.Connect(_host, _port);

            socket.Send(new ClientInfo().ToMessage());
            MessagePump.Until(socket, _router, () => _accepted != null || _failure != null,
                              TimeSpan.FromSeconds(10), auth.Tick);
            Report.Check("G. CLIENT_INFO được chấp nhận",
                _accepted == true, _failure ?? "không có phản hồi -36");

            socket.Send(AuthPackets.RequestServerList());
            MessagePump.Until(socket, _router, () => _servers != null, TimeSpan.FromSeconds(10), auth.Tick);
            Report.Check("H. Nhận và đọc được danh sách máy chủ",
                _servers != null && _servers.Length > 0,
                _servers == null ? "không nhận được SERVER_LIST" : "danh sách rỗng");

            if (_servers != null && _servers.Length > 0)
            {
                Console.WriteLine($"       -> {string.Join(", ", (object[])_servers)}");
            }

            socket.Send(AuthPackets.Login(_username, _password, "ref-live-smoke", "1.4.3"));
            MessagePump.Until(socket, _router, () => _success != null || _failure != null || _needsCharacter,
                              TimeSpan.FromSeconds(15), auth.Tick);

            if (_needsCharacter)
            {
                // Server tạo xong sẽ tự đóng kết nối; lượt sau mới có LOGIN_SUCCES.
                socket.Send(AuthPackets.CreateCharacter(NewCharacterName(), 0));
                MessagePump.Until(socket, _router, () => !socket.IsConnected, TimeSpan.FromSeconds(10), auth.Tick);
                return false;
            }

            Report.Check("I. Đăng nhập thành công, đọc đủ 5 field LOGIN_SUCCES",
                _success != null,
                _failure ?? "không nhận được LOGIN_SUCCES trong 15s");

            if (_success == null)
            {
                return true;
            }

            Console.WriteLine($"       -> {_success}");
            Report.Check("J. LOGIN_SUCCES có user_id và tên hợp lệ",
                _success.UserId > 0 && !string.IsNullOrEmpty(_success.Name),
                $"user_id={_success.UserId} name=\"{_success.Name}\"");

            // Xin ảnh và mở menu trên chính phiên đã đăng nhập — đúng như client thật làm.
            ImageChecks.Run(socket, _router);
            var guider = GuiderChecks.Run(socket, _router);
            MapMovementChecks.Verify(socket, _router, mapRec, _success.UserId);
            ShopChecks.Run(socket, _router, guider);
            TeleportMenuChecks.Run(socket, _router);
            WarpChecks.Run(socket, _router, mapRec);
            ChallengePlaceChecks.Run(socket, _router, mapRec);
            MarketPlaceChecks.Run(socket, _router, mapRec);
            BattleChecks.Run(socket, _router, guider, mapRec.Handler, _success.UserId);
            HoldConnection(socket, auth);
            return true;
        }

        /// <summary>Tên nhân vật: <c>^[a-z0-9]+$</c>, 5-20 ký tự (<c>GameController.cs:718</c>).</summary>
        private string NewCharacterName()
        {
            return "smoke" + DateTime.UtcNow.ToString("HHmmss");
        }

    }
}
