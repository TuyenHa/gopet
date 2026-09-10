using Gopet.Data.Collections;
using Gopet.Util;
using System.Collections.Concurrent;

namespace Gopet.IO
{
    public class MsgSender
    {

        protected Session session;
        protected ConcurrentQueue<Message> sendingMessage = new ConcurrentQueue<Message>();
        protected AutoResetEvent messageEvent = new AutoResetEvent(false);
        public static CopyOnWriteArrayList<MsgSender> msgSenders = new CopyOnWriteArrayList<MsgSender>();
        public static readonly Random random = new Random();
        private bool isClose = false;

        /// <summary>Đang đẩy nốt hàng đợi để đóng phiên — đẩy xong thì thoát luôn.</summary>
        private volatile bool draining = false;

        /// <summary>Bật khi luồng gửi đã thoát. <c>Session.Close()</c> chờ tín hiệu này.</summary>
        private readonly ManualResetEventSlim drained = new ManualResetEventSlim(false);

        public MsgSender(Session session)
        {
            this.session = session;
            if (session == null)
            {
                throw new ArgumentNullException();
            }
            msgSenders.Add(this);
        }

        public void addMessage(Message message)
        {
            sendingMessage.Enqueue(message);
            messageEvent.Set();
        }


        public void run()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (session.isConnected() && !isClose)
                        {
                            while (sendingMessage.TryDequeue(out Message message))
                            {
                                if (message != null)
                                {
                                    doSendMessage(message);
                                }
                            }

                            // Đang đóng phiên: hàng đợi vừa đẩy hết ở vòng trên rồi,
                            // thoát ngay thay vì ngủ tiếp 5-30 giây.
                            if (draining)
                            {
                                return;
                            }

                            messageEvent.WaitOne(random.Next(5000, 30000));
                            continue;
                        }
                    }
                    catch (Exception var6)
                    {
                    }
                    return;
                }
            }
            finally
            {
                // Phải báo dù thoát bằng đường nào, nếu không Session.Close()
                // sẽ đứng chờ hết hạn một cách vô ích.
                drained.Set();
            }
        }

        /// <summary>
        /// Yêu cầu đẩy nốt gói đang chờ rồi kết thúc. Gọi TRƯỚC khi đóng socket.
        /// </summary>
        public void requestDrain()
        {
            draining = true;
            messageEvent.Set();
        }

        /// <summary>Chờ luồng gửi thoát. Trả <c>false</c> nếu quá hạn.</summary>
        public bool waitDrained(int milliseconds)
        {
            return drained.Wait(milliseconds);
        }

        public void doSendMessage(Message m)
        {
            sbyte[] data = m.getBuffer();
            Session var10000;
            if (data != null)
            {
                // Ghi log TRƯỚC khi mã hoá: dump phải là nội dung đọc được thì
                // mới so được với dump phía client.
                Gopet.Logging.PacketLogger.Instance?.Log(
                    Gopet.Logging.PacketDirection.Out, m.id, m.isEncrypted, data);

                if (m.isEncrypted)
                {
                    data = session.tea.encrypt(data);
                }

                session.dos.WriteInt(data.Length + 1);
                session.dos.Write(((sbyte)(m.isEncrypted ? 1 : 0)).toByte());
                session.dos.Write(data);
                var10000 = session;
                var10000.sendsbyteCount += data.Length;
            }
            else
            {
                session.dos.WriteInt(0);
            }
            var10000 = session;
            var10000.sendsbyteCount += 4;
            session.dos.Flush();
        }

        /// <summary>
        /// Dừng cứng: xoá sạch hàng đợi. Muốn gói cuối tới được client thì phải
        /// <see cref="requestDrain"/> trước — <c>Session.Close()</c> làm việc đó.
        /// </summary>
        public void stop()
        {
            isClose = true;
            draining = true;
            sendingMessage.Clear();
            drained.Set();
            msgSenders.Remove(this);
            messageEvent.Set();
        }

        public void Release()
        {
            messageEvent.Set();
        }

        ~MsgSender()
        {
            stop();
        }
    }
}