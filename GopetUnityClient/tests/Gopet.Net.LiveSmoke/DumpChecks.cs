using System;
using System.IO;
using System.Linq;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Kiểm nội dung packet dump — không phải chỉ kiểm file có tồn tại không.
    ///
    /// File luôn tồn tại: constructor của <c>PacketLogger</c> tạo nó và ghi header
    /// ngay lúc dựng. Check dựa vào <c>File.Exists</c> sẽ xanh cả khi không gói nào
    /// được ghi, tức là không kiểm gì cả.
    /// </summary>
    internal static class DumpChecks
    {
        /// <summary>
        /// Byte đúng của gói CLIENT_INFO trên dây. Hằng số cố ý viết tay: so với giá
        /// trị tự sinh lại từ chính code đang test thì chỉ là vòng tròn tự nhất quán.
        /// Đổi <see cref="ClientInfoPacket"/> mà quên cập nhật đây thì test đỏ — đúng ý.
        /// </summary>
        private const string ExpectedClientInfoHex =
            "dc00000000040005312e342e330010756e6974792d6c6976652d736d6f6b65" +
            "00000140000000f000027669" +
            "0005322e342e39";

        private sealed class Line
        {
            public string Direction;
            public string Opcode;
            public string Encrypted;
            public string Length;
            public string Hex;
        }

        public static void Run(string clientDump, string serverDump)
        {
            var ours = Parse(clientDump).FirstOrDefault(l => l.Direction == "OUT");

            Report.Check("D. Dump client ghi đúng byte của gói đã gửi",
                ours != null
                    && ours.Opcode == "-36" && ours.Encrypted == "1"
                    && ours.Length == (ExpectedClientInfoHex.Length / 2).ToString()
                    && ours.Hex == ExpectedClientInfoHex,
                ours == null ? "không có dòng OUT nào" : $"opcode={ours.Opcode} enc={ours.Encrypted} len={ours.Length} hex={ours.Hex}");

            if (ours == null) return;

            if (!File.Exists(serverDump))
            {
                Report.Fail("F. Dump hai đầu khớp từng byte", $"không thấy dump server: {serverDump}");
                return;
            }

            // Cùng một gói, hai điểm quan sát: OUT của client phải là IN của server.
            // Đây là bằng chứng trực tiếp cho "không đổi một byte nào trên dây".
            var theirs = Parse(serverDump).LastOrDefault(l => l.Direction == "IN" && l.Hex == ours.Hex);

            Report.Check("F. Dump hai đầu khớp từng byte (client OUT ≡ server IN)",
                theirs != null && theirs.Length == ours.Length && theirs.Opcode == ours.Opcode,
                theirs == null
                    ? "server không ghi nhận gói nào có cùng hex — server chưa nhận, hoặc byte đã lệch"
                    : $"lệch: server len={theirs.Length} opcode={theirs.Opcode}");
        }

        private static Line[] Parse(string path)
        {
            // Phải mở với FileShare.ReadWrite: dump của server đang được ghi, và
            // chia sẻ file là hai chiều — bên đọc không cho phép ghi thì hệ điều
            // hành từ chối, dù bên ghi đã cho phép đọc.
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                                              FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);

            // StreamWriter của .NET ghi BOM, nên dòng header bắt đầu bằng BOM chứ không phải '#'.
            return ReadLines(reader)
                .Select(l => l.TrimStart('﻿'))
                .Where(l => l.Length > 0 && !l.StartsWith("#"))
                .Select(l => l.Split('\t'))
                .Where(p => p.Length >= 6)
                .Select(p => new Line { Direction = p[1], Opcode = p[2], Encrypted = p[3], Length = p[4], Hex = p[5] })
                .ToArray();
        }

        private static System.Collections.Generic.IEnumerable<string> ReadLines(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null) { yield return line; }
        }
    }
}
