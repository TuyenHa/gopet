using System.Collections.Generic;

namespace Gopet.Net.Auth
{
    /// <summary>Một máy chủ trong danh sách server (<c>SERVER_LIST</c>, opcode 64).</summary>
    public sealed class ServerEntry
    {
        public string Name;
        public string Address;
        public int Port;

        /// <summary>Hai cờ server gửi kèm ở khối cuối gói. Hiện luôn <c>true</c>.</summary>
        public bool FlagA;

        public bool FlagB;

        public override string ToString() => $"{Name} ({Address}:{Port})";
    }

    /// <summary>
    /// Đọc gói <c>SERVER_LIST</c>. Format theo <c>Player.showListServer()</c>
    /// (<c>Player.cs:1034-1052</c>):
    ///
    /// <code>
    /// int count
    /// count x { UTF name, UTF ip, int port, int port, int port }
    /// count x { bool, bool }
    /// </code>
    ///
    /// <para><b>Cổng lặp ba lần</b> với cùng một giá trị — không phải ba cổng khác
    /// nhau. Đọc đủ ba rồi bỏ hai cái sau, đọc thiếu là lệch toàn bộ phần sau.</para>
    ///
    /// <para>Hai khối tách rời nhau: toàn bộ thông tin server trước, rồi mới tới
    /// toàn bộ cờ. Đọc xen kẽ trong một vòng lặp là sai.</para>
    /// </summary>
    public static class ServerList
    {
        /// <summary>Chặn gói dị dạng khai số lượng vô lý.</summary>
        private const int MaxServers = 256;

        public static ServerEntry[] Parse(Message message)
        {
            var reader = message.Reader;
            var count = reader.ReadInt();

            if (count < 0 || count > MaxServers)
            {
                throw new ProtocolException($"SERVER_LIST khai {count} máy chủ — ngoài khoảng hợp lệ.");
            }

            var servers = new List<ServerEntry>(count);
            for (var i = 0; i < count; i++)
            {
                var entry = new ServerEntry
                {
                    Name = reader.ReadUtf(),
                    Address = reader.ReadUtf(),
                    Port = reader.ReadInt()
                };

                reader.ReadInt(); // cùng cổng, server ghi ba lần
                reader.ReadInt();

                servers.Add(entry);
            }

            for (var i = 0; i < count; i++)
            {
                servers[i].FlagA = reader.ReadBool();
                servers[i].FlagB = reader.ReadBool();
            }

            reader.ExpectFullyConsumed("SERVER_LIST");
            return servers.ToArray();
        }
    }
}
