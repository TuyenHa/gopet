namespace Gopet.IO
{
    public class Message
    {
        public sbyte id;
        private DataOutputStream<MemoryStream> dos;
        private DataInputStream dis;
        public bool isEncrypted;
        public static bool isiWin = false;
        private readonly object freezeGate = new();
        private sbyte[]? frozenPayload;
        private bool frozenEncrypted;
        private sbyte frozenId;

        public Message(int command) : this(command, false)
        {

        }

        public Message(int command, bool isEncrypted)
        {
            this.isEncrypted = false;
            this.id = (sbyte)(command & 0xFF);
            this.isEncrypted = isEncrypted;
        }

        public Message(sbyte[] data)
        {
            this.isEncrypted = false;
            sbyte[] msgData = new sbyte[data.Length - 1];
            Buffer.BlockCopy(data, 1, msgData, 0, msgData.Length);
            this.id = data[0];
            this.dis = new DataInputStream(msgData);
        }

        public sbyte[] getBuffer()
        {
            return (sbyte[])Freeze().Data.Clone();
        }

        // Only the sender may share this immutable payload. TEA encrypt allocates its output.
        internal (sbyte[] Data, bool Encrypted, sbyte Id) Freeze()
        {
            lock (freezeGate)
            {
                if (frozenPayload == null)
                {
                    int prefix = dos != null && isiWin ? 3 : 1;
                    int length = dos == null ? 0 : checked((int)dos.BaseStream.Length);
                    frozenPayload = new sbyte[length + prefix];
                    if (prefix == 3)
                    {
                        frozenPayload[0] = 40;
                        frozenPayload[1] = (sbyte)(id >>> 8 & 255);
                        frozenPayload[2] = id;
                    }
                    else frozenPayload[0] = id;
                    if (length > 0)
                        Buffer.BlockCopy(dos!.BaseStream.GetBuffer(), 0, frozenPayload, prefix, length);
                    frozenEncrypted = isEncrypted;
                    frozenId = id;
                    dos?.BaseStream.Dispose();
                }
                return (frozenPayload, frozenEncrypted, frozenId);
            }
        }

        public DataInputStream reader()
        {
            return this.dis;
        }

        public DataOutputStream<MemoryStream> writer()
        {
            if (frozenPayload != null) throw new InvalidOperationException("Message has already been finalized");
            if (this.dos == null)
            {
                this.dos = new DataOutputStream<MemoryStream>(new MemoryStream());
            }

            return this.dos;
        }

        public void putsbyte(int value)
        {

            this.writer().writeByte(value);

        }

        public void putString(string text)
        {

            this.writer().writeUTF(text);

        }

        public void putUTF(string text)
        {

            this.writer().writeUTF(text);

        }

        public void putInt(int value)
        {

            this.writer().writeInt(value);

        }

        public void putbool(bool value)
        {

            this.writer().writeBool(value);

        }

        public void putShort(int value)
        {

            this.writer().writeShort(value);

        }

        public void putlong(long value)
        {

            this.writer().writeLong(value);

        }

        public void cleanup()
        {
            try
            {
                if (this.dis != null)
                {
                    this.dis.Close();
                }

                if (this.dos != null)
                {
                    this.dos.Close();
                }
            }
            catch (IOException var2)
            {
            }
        }

        public sbyte readsbyte()
        {
            return reader().readsbyte();
        }

        public short readShort()
        {
            return reader().readShort();
        }

        public int readUnsignedsbyte()
        {
            return reader().readUnsignedByte();
        }

        public int readUnsignedShort()
        {
            return reader().readUnsignedShort();
        }

        public String readUTF()
        {
            return reader().readUTF();
        }

        public long readlong()
        {
            return reader().readlong();
        }
        public int readInt()
        {
            return reader().readInt();
        }

        public void Close()
        {
            cleanup();
        }
    }

}
