using System;
using System.Diagnostics;
using System.Threading;
using Gopet.Net;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>Kết nối, handshake, gửi gói mã hoá, nhận và giải mã phản hồi.</summary>
    internal static class ProtocolChecks
    {
        public static void Run(string host, int port, string dumpPath)
        {
            // Thứ tự dispose quan trọng: socket đóng TRƯỚC logger, nếu không luồng
            // nền còn sống có thể gọi Log() lên StreamWriter đã đóng.
            using var logger = new PacketLogger(dumpPath);
            using var socket = new GopetSocket(logger);

            string dropReason = null;
            socket.Disconnected += r => Volatile.Write(ref dropReason, r);

            socket.Connect(host, port);

            // Handshake sai thì server đóng ngay chứ không đợi.
            Thread.Sleep(500);
            Report.Check("A. Kết nối mở, server không đóng ngay",
                socket.IsConnected, Volatile.Read(ref dropReason) ?? "server đóng kết nối");

            socket.Send(SmokeClientInfo.Build());

            var reply = WaitForMessage(socket, TimeSpan.FromSeconds(10));
            Report.Check("B. Server giải mã được gói mã hoá (trả đúng opcode)",
                reply != null && reply.Id == GopetCmd.CLIENT_INFO,
                reply == null
                    ? Volatile.Read(ref dropReason) ?? "hết 10s không có phản hồi"
                    : $"opcode lạ: {reply.Id}");

            if (reply == null) return;

            // Server chỉ trả setClientOK sau khi đọc trọn CLIENT_INFO — nghĩa là
            // khoá TEA dựng từ 9 byte handshake đã đúng.
            var accepted = reply.Reader.ReadBool();
            reply.Reader.ExpectFullyConsumed("CLIENT_INFO reply");
            Report.Check("C. Giải mã phản hồi đúng nội dung (setClientOK = 1)",
                accepted, "server trả setClientOK = 0 — kiểm version/languageCode");

            reply.Dispose();
        }

        /// <summary>
        /// Chờ một gói. Phải TryDequeue TRƯỚC rồi mới xét IsConnected: gói cuối
        /// server gửi ngay trước khi đóng vẫn nằm trong hàng đợi, xét ngược lại
        /// là vứt mất nó.
        /// </summary>
        private static Message WaitForMessage(GopetSocket socket, TimeSpan timeout)
        {
            var clock = Stopwatch.StartNew();
            while (true)
            {
                if (socket.Incoming.TryDequeue(out var msg)) return msg;
                if (!socket.IsConnected) return null;
                if (clock.Elapsed >= timeout) return null;
                Thread.Sleep(20);
            }
        }
    }
}
