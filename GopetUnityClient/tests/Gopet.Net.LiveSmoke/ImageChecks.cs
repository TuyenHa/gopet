using System;
using System.Diagnostics;
using System.Linq;
using Gopet.Net;
using Gopet.Net.Images;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Đường ống ảnh, chạy trên phiên ĐÃ đăng nhập — đúng như client thật xin ảnh
    /// trong lúc chơi.
    /// </summary>
    internal static class ImageChecks
    {
        /// <summary>Có thật trong <c>GServer/assets/</c>, client J2ME đã từng xin.</summary>
        private const string RealPath = "npcs/arena.png";

        private const string MissingPath = "khong-he-ton-tai-9x8y7z.png";

        /// <summary>8 byte đầu của mọi file PNG hợp lệ.</summary>
        private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

        public static void Run(GopetSocket socket, MessageRouter router)
        {
            var clock = Stopwatch.StartNew();
            var handler = new ImageHandler(socket.Send, () => clock.ElapsedMilliseconds)
            {
                TimeoutMs = 8000
            };
            handler.RegisterOn(router);

            string timedOut = null;
            handler.TimedOut += p => timedOut = p;

            // --- Ảnh có thật ---
            ImageResponse got = null;
            handler.Request(RealPath, ImagePackets.TypeNpc, r => got = r);
            MessagePump.Until(socket, router, () => got != null, TimeSpan.FromSeconds(10), handler.Tick);

            Report.Check("N. Xin được ảnh có thật, nhận về PNG hợp lệ",
                got != null && got.Png.Length > 0 && got.Png.Take(8).SequenceEqual(PngMagic),
                got == null ? "không nhận được gói ảnh trong 10s" : $"{got.Png.Length} byte, không phải PNG");

            if (got != null)
            {
                Console.WriteLine($"       -> {got.Path}: {got.Png.Length} byte, type={got.Type}");
            }

            // --- Gộp request trùng ---
            var delivered = 0;
            for (var i = 0; i < 50; i++)
            {
                handler.Request("npcs/Tien_Nu.png", ImagePackets.TypeNpc, _ => delivered++);
            }

            var sentOne = handler.InFlightCount == 1 && handler.QueuedCount == 0;
            MessagePump.Until(socket, router, () => delivered > 0, TimeSpan.FromSeconds(10), handler.Tick);

            Report.Check("O. 50 nơi cùng xin một ảnh → 1 gói, 50 callback",
                sentOne && delivered == 50,
                $"gói đang bay lúc xin xong: {(sentOne ? 1 : -1)}, số callback: {delivered}");

            // --- Đường dẫn không tồn tại: server IM LẶNG ---
            handler.Request(MissingPath, ImagePackets.TypeNpc, _ => { });
            MessagePump.Until(socket, router, () => timedOut != null, TimeSpan.FromSeconds(12), handler.Tick);

            Report.Check("P. Đường dẫn không tồn tại → hết hạn, không treo",
                timedOut == MissingPath,
                timedOut == null
                    ? "không hết hạn sau 12s — callback sẽ treo vĩnh viễn"
                    : $"hết hạn nhầm đường dẫn: {timedOut}");
        }
    }
}
