using System;

namespace Gopet.Net
{
    /// <summary>
    /// Một gói tin goPet: 1 byte opcode + phần thân.
    ///
    /// Mirror của <c>GServer/Server/IO/Message.cs</c>.
    ///
    /// <para><b>Bất đối xứng mã hoá — quan trọng:</b> client gửi lên thì
    /// <b>mã hoá</b>, server gửi xuống thì <b>không</b>. Đây là hành vi thật
    /// đã kiểm chứng: server không có chỗ nào gọi <c>new Message(x, true)</c>,
    /// và <c>en.java</c> phía client cũ đặt cờ mã hoá = true cho gói đi.
    /// Vì vậy <see cref="Create"/> mặc định <see cref="IsEncrypted"/> = true.</para>
    /// </summary>
    public sealed class Message : IDisposable
    {
        /// <summary>Opcode. Là <see cref="sbyte"/> vì có opcode âm (<c>CLIENT_INFO = -36</c>).</summary>
        public sbyte Id { get; }

        /// <summary>Gói này có/đã được mã hoá TEA hay không.</summary>
        public bool IsEncrypted { get; }

        private JavaBinaryWriter _writer;
        private readonly JavaBinaryReader _reader;

        private Message(sbyte id, bool isEncrypted, JavaBinaryReader reader)
        {
            Id = id;
            IsEncrypted = isEncrypted;
            _reader = reader;
        }

        /// <summary>Tạo gói để gửi lên server. Mặc định mã hoá — xem ghi chú ở đầu class.</summary>
        public static Message Create(sbyte opcode, bool encrypted = true)
        {
            return new Message(opcode, encrypted, null);
        }

        /// <summary>
        /// Dựng gói từ payload đã nhận (và đã giải mã nếu cần).
        /// <paramref name="payload"/>[0] là opcode, phần còn lại là thân.
        /// </summary>
        public static Message FromWire(byte[] payload, bool wasEncrypted)
        {
            if (payload == null || payload.Length == 0)
            {
                throw new ProtocolException("Payload rỗng — không có opcode.");
            }

            var body = new byte[payload.Length - 1];
            Buffer.BlockCopy(payload, 1, body, 0, body.Length);

            return new Message(unchecked((sbyte)payload[0]), wasEncrypted, new JavaBinaryReader(body));
        }

        /// <summary>Reader của thân gói. Chỉ có ở gói nhận được.</summary>
        public JavaBinaryReader Reader =>
            _reader ?? throw new InvalidOperationException("Gói này được tạo để gửi, không có reader.");

        /// <summary>Writer của thân gói, tạo lười. Chỉ dùng với gói để gửi.</summary>
        public JavaBinaryWriter Writer
        {
            get
            {
                if (_reader != null)
                {
                    throw new InvalidOperationException("Gói này nhận từ server, không ghi được.");
                }

                return _writer ??= new JavaBinaryWriter();
            }
        }

        // Các hàm tắt, đặt tên theo server để đối chiếu code hai bên cho nhanh.
        public Message PutSByte(int v) { Writer.WriteSByte(v); return this; }
        public Message PutBool(bool v) { Writer.WriteBool(v); return this; }
        public Message PutShort(int v) { Writer.WriteShort(v); return this; }
        public Message PutInt(int v) { Writer.WriteInt(v); return this; }
        public Message PutLong(long v) { Writer.WriteLong(v); return this; }
        public Message PutBytes(byte[] v) { Writer.WriteBytes(v); return this; }

        /// <summary>
        /// Ghi chuỗi. Server có cả <c>putString</c> lẫn <c>putUTF</c> nhưng
        /// hai hàm đó gọi cùng một <c>writeUTF</c> — chỉ là alias, không khác gì nhau.
        /// Bên này gộp làm một để khỏi nhầm là có 2 format.
        /// </summary>
        public Message PutUtf(string v) { Writer.WriteUtf(v); return this; }

        /// <summary>Toàn bộ gói để đưa lên dây: [opcode][thân].</summary>
        public byte[] ToWire()
        {
            if (_writer == null)
            {
                return new[] { unchecked((byte)Id) };
            }

            var body = _writer.ToArray();
            var buffer = new byte[body.Length + 1];
            buffer[0] = unchecked((byte)Id);
            Buffer.BlockCopy(body, 0, buffer, 1, body.Length);
            return buffer;
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _writer = null;
        }
    }
}
