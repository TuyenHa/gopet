

using Gopet.IO;
using Gopet.Manager;
using Gopet.Util;
using System.Net.Sockets;
namespace Gopet.IO
{
    public class Session : ISession
    {
        public IHandleMessage messageHandler;
        public BinaryWriter dos;
        public BinaryReader dis;
        public Socket sc;
        public bool isSocketConnected;
        private MsgSender sender;
        private MsgReader reader;
        public int sendsbyteCount;
        public int recvsbyteCount;
        public TEA tea;
        public String currentIp;
        public int currentPort;
        public bool clientOK = false;
        public long msgCount = 0;
        private Thread sendThread;
        private Thread readThread;
        public Session(Socket socket)
        {
            sc = socket;
        }

        public void setClientOK(bool ok)
        {
            Message ms = new Message((sbyte)-36);
            ms.writer().writeSByte(ok ? 1 : 0);
            ms.writer().flush();
            sendMessage(ms);
            clientOK = true;
        }

        public bool isConnected()
        {
            return this.isSocketConnected;
        }


        public void run()
        {
            try
            {
                isSocketConnected = true;
                this.messageHandler = new Player(this);
                NetworkStream networkStream = new NetworkStream(sc);
                dis = new BinaryReader(networkStream);
                dos = new BinaryWriter(networkStream);
                readKey();
                setSender(new MsgSender(this));
                setReader(new MsgReader(this));
                sendThread = new Thread(this.sender.run);
                sendThread.IsBackground = true;
                sendThread.Name = "SEND THREAD " + sc.RemoteEndPoint.ToString();
                sendThread.Start();
                readThread = new Thread(this.reader.run);
                readThread.IsBackground = true;
                readThread.Name = "READ THREAD " + sc.RemoteEndPoint.ToString();
                readThread.Start();
                ThreadManager.AddThread(sendThread);
                ThreadManager.AddThread(readThread);
            }
            catch (Exception e)
            {
                e.printStackTrace();
                Close();
            }
        }

        public void readKey()
        {
            byte[] keys = new byte[9];
            dis.Read(keys, 0, 9);
            long key = readKey(keys.sbytes());
            tea = new TEA(key);
        }

        private long readKey(sbyte[] var10000)
        {
            long time = 0;
            time ^= var10000[1] & 255;
            time <<= 8;
            time ^= var10000[2] & 255;
            time <<= 8;
            time ^= var10000[3] & 255;
            time <<= 8;
            time ^= var10000[4] & 255;
            time <<= 8;
            time ^= var10000[5] & 255;
            time <<= 8;
            time ^= var10000[6] & 255;
            time <<= 8;
            time ^= var10000[7] & 255;
            time <<= 8;
            time ^= var10000[8] & 255;
            return time;
        }

        public void setHandler(IHandleMessage messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public void setSender(MsgSender s)
        {
            this.sender = s;
        }

        public void setReader(MsgReader r)
        {
            this.reader = r;
        }

        protected void setKey(long key)
        {
            this.tea = new TEA(key);
        }

        public void sendMessage(Message message)
        {
            this.sender.addMessage(message);
        }

        public static int socketCount = 0;

        bool ISession.clientOK { get => clientOK; set => clientOK = value; }
        Socket ISession.CSocket { get => sc; set => sc = value; }

        /// <summary>Hạn chờ đẩy nốt hàng đợi lúc đóng phiên.</summary>
        private const int DrainTimeoutMs = 1000;

        public void Close()
        {
            // Đẩy nốt gói còn trong hàng đợi TRƯỚC khi giết luồng gửi.
            //
            // Trước đây Close() Interrupt() thẳng, rồi Exit() gọi sender.stop() xoá
            // sạch hàng đợi — nên gói gửi ngay trước lúc đóng bị mất: server ghi log
            // là đã gửi nhưng không client nào nhận được. Thấy rõ nhất ở dialog
            // "Phiên bản cũ rồi..." — người chơi chỉ thấy rớt mạng, không rõ lý do.
            //
            // Vì vậy vài chỗ trước đây phải Thread.Sleep() trước Close() để gói kịp
            // đi. Nay không cần nữa.
            if (sender != null && Thread.CurrentThread != sendThread)
            {
                sender.requestDrain();
                sender.waitDrained(DrainTimeoutMs);
            }

            this.sendThread?.Interrupt();
            this.sendThread = null;
            this.readThread = null;
            ThreadPool.QueueUserWorkItem(Exit);
            GC.Collect();
        }

        public void Exit(object state)
        {
            try
            {
                currentIp = null;
                currentPort = -1;
                isSocketConnected = false;
                sender?.stop();

                // FIN chứ không RST: đóng thẳng có thể làm hệ điều hành vứt phần
                // dữ liệu còn nằm trong đệm gửi.
                try { sc?.Shutdown(SocketShutdown.Send); }
                catch (Exception) { /* phía kia đóng trước, hoặc chưa từng kết nối */ }

                dos?.Close();
                dos = null;
                dis?.Close();
                dis = null;
                sc?.Close();
                sc = null;
                sendsbyteCount = 0;
                recvsbyteCount = 0;
                messageHandler?.onDisconnected();
            }
            catch (Exception var2)
            {
                var2.printStackTrace();
            }
        }
    }
}