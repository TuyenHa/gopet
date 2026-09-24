namespace Gopet.IO
{
    public class MsgReader
    {

        protected Session session;
        public MsgReader(Session session)
        {
            this.session = session;
        }

        public void run()
        {
            while (true)
            {
                try
                {
                    if (session.isConnected())
                    {
                        Message message = readMessage();
                        if (message != null)
                        {
                            session.Dispatch(message);
                            continue;
                        }
                    }
                }
                catch (Exception var5)
                {
                    session.Close();
#if DEBUG
                    //throw var5;
#endif
                }

                if (session.isConnected())
                {
                    session.Close();
                }
                return;
            }
        }

        private Message readMessage()
        {
            int Length = 0;
            int hi = session.dis.ReadJavaInt();

            if (hi == -1)
            {
                return null;
            }
            else
            {
                Length = hi - 1;
                if (hi < 1 || hi > 10001)
                {
                    throw new IOException("Dữ liệu quá lớn");
                }
                sbyte isEncrypted = session.dis.ReadSByte();
                byte[] data = new byte[Length];
                int sbyteRead = 0;

                while (sbyteRead < Length)
                {
                    int len = session.dis.Read(data, sbyteRead, Length - sbyteRead);
                    if (len == 0) throw new EndOfStreamException("Truncated packet payload");
                    sbyteRead += len;
                }

                if (Length == 0)
                {
                    return null;
                }
                else
                {
                    Message msg;
                    sbyte[] payload;
                    if (isEncrypted == 1)
                    {
                        payload = session.tea.decrypt(data.sbytes());
                        if (payload == null)
                        {
                            throw new IOException("Giải mã gói tin thất bại");
                        }
                    }
                    else
                    {
                        payload = data.sbytes();
                    }

                    // Ghi log SAU khi giải mã, cùng lý do với phía gửi.
                    if (payload.Length > 0)
                    {
                        Gopet.Logging.PacketLogger.Instance?.Log(
                            Gopet.Logging.PacketDirection.In, payload[0], isEncrypted == 1, payload);
                    }

                    msg = new Message(payload);
                    Session var10000 = session;
                    var10000.recvsbyteCount += 4 + Length;
                    return msg;
                }
            }
        }

    }
}
