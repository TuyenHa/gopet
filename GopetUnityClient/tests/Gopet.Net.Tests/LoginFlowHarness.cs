using System.Collections.Generic;
using Gopet.Net.Auth;
using Gopet.UiLogic;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Nối <see cref="LoginFlow"/> vào hai cái sổ ghi — gói đã gửi và địa chỉ đã xin
    /// nối — rồi đưa luồng tới những chặng mà test hay bắt đầu từ đó.
    ///
    /// <para>Một chỗ duy nhất mô tả "đăng nhập bình thường trông thế nào". Mỗi file
    /// test tự dựng lấy thì lúc thứ tự thay đổi sẽ phải sửa nhiều nơi, và nơi nào
    /// quên sửa thì test vẫn xanh với kịch bản đã lỗi thời.</para>
    /// </summary>
    internal sealed class LoginFlowHarness
    {
        public readonly List<sbyte> Sent = new List<sbyte>();
        public readonly List<string> Connects = new List<string>();

        /// <summary>Đồng hồ giả, mili-giây. Test tự đẩy tới để kiểm hạn chờ.</summary>
        public long NowMs;

        public readonly LoginFlow Flow;

        public LoginFlowHarness()
        {
            Flow = new LoginFlow(nowMs: () => NowMs);

            Flow.SendRequested += m =>
            {
                Sent.Add(m.Id);
                m.Dispose();
            };

            Flow.ConnectRequested += (host, port) => Connects.Add($"{host}:{port}");
        }

        public LoginStage Stage => Flow.Stage;

        public string Notice => Flow.Notice;

        public int CountSent(sbyte opcode) => Sent.FindAll(id => id == opcode).Count;

        public static ServerEntry[] OneServer(string address = "127.0.0.1", int port = 19180)
        {
            return new[] { new ServerEntry { Name = "Máy chủ 1", Address = address, Port = port } };
        }

        /// <summary>Tới chặng nhập tài khoản, nối lại nếu máy chủ được chọn ở địa chỉ khác.</summary>
        public void ReachCredentials(string serverAddress = "127.0.0.1")
        {
            Flow.Start("127.0.0.1", 19180);
            Flow.OnConnected();
            Flow.OnClientAccepted(true);
            Flow.OnServerList(OneServer(serverAddress));

            // Đúng một máy chủ thì OnServerList tự chọn luôn — chỉ gọi ChooseServer
            // khi luồng THẬT SỰ dừng ở màn chọn (helper này dùng OneServer(), nhưng
            // để không âm thầm gọi trùng nếu sau này ai đó đổi sang nhiều máy chủ).
            if (Flow.Stage == LoginStage.ChoosingServer) Flow.ChooseServer(0);

            if (Flow.Stage != LoginStage.Connecting) return;

            Flow.OnConnected();
            Flow.OnClientAccepted(true);
        }

        /// <summary>Tới chặng đã gửi <c>LOGIN</c> và đang chờ server trả lời.</summary>
        public void LogIn()
        {
            ReachCredentials();
            Flow.SubmitCredentials("gopettest", "abc12345");
        }
    }
}
