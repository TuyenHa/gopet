using Gopet.Data.Collections;

namespace Gopet.IO
{
    public class MsgSender
    {
        private readonly Session session;
        private readonly object gate = new();
        private readonly Queue<(sbyte[] Data, bool Encrypted, sbyte Id, long At)> queue = new();
        private readonly TaskCompletionSource drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public static CopyOnWriteArrayList<MsgSender> msgSenders = new();
        private bool stopped;
        private bool draining;
        private long queuedBytes;
        public const int MaxQueuedMessages = 1024;
        public const long MaxQueuedBytes = 16 * 1024 * 1024;
        public int QueuedMessages { get { lock (gate) return queue.Count; } }
        public long QueuedBytes { get { lock (gate) return queuedBytes; } }
        public long OldestMessageAgeMs { get { lock (gate) return queue.TryPeek(out var item) ? Environment.TickCount64 - item.At : 0; } }

        public MsgSender(Session session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            msgSenders.Add(this);
        }
        public void addMessage(Message message)
        {
            lock (gate)
            {
                if (stopped || draining || session.IsClosing) return;
                var payload = message.Freeze();
                var data = payload.Data;
                if (queue.Count < MaxQueuedMessages && data.LongLength <= MaxQueuedBytes - queuedBytes)
                {
                    queue.Enqueue((data, payload.Encrypted, payload.Id, Environment.TickCount64));
                    queuedBytes += data.Length;
                    System.Threading.Monitor.PulseAll(gate);
                    return;
                }
                // Disconnect rather than drop an arbitrary transaction packet.
                draining = true;
            }
            session.Close();
        }
        public void run()
        {
            bool failed = false;
            try
            {
                while (true)
                {
                    (sbyte[] Data, bool Encrypted, sbyte Id, long At) item;
                    lock (gate)
                    {
                        while (queue.Count == 0 && !stopped && !draining)
                            System.Threading.Monitor.Wait(gate);
                        if (stopped || !session.isSocketConnected || queue.Count == 0) return;
                        item = queue.Dequeue();
                        queuedBytes -= item.Data.Length;
                    }
                    if (Environment.TickCount64 - item.At > 10000)
                        throw new IOException("Send queue deadline exceeded");
                    Send(item.Data, item.Encrypted, item.Id);
                }
            }
            catch (Exception) { failed = true; }
            finally
            {
                stop();
                drained.TrySetResult();
                if (failed || !session.IsClosing) session.Close();
            }
        }
        public void requestDrain()
        {
            lock (gate) { draining = true; System.Threading.Monitor.PulseAll(gate); }
        }
        public bool waitDrained(int milliseconds) => drained.Task.Wait(milliseconds);
        public void doSendMessage(Message message)
        {
            var payload = message.Freeze();
            Send(payload.Data, payload.Encrypted, payload.Id);
        }
        private void Send(sbyte[] data, bool encrypted, sbyte id)
        {
            Gopet.Logging.PacketLogger.Instance?.Log(Gopet.Logging.PacketDirection.Out, id, encrypted, data);
            if (encrypted) data = session.tea.encrypt(data);
            session.dos.WriteInt(data.Length + 1);
            session.dos.Write((byte)(encrypted ? 1 : 0));
            session.dos.Write(data);
            session.dos.Flush();
            session.sendsbyteCount += data.Length + 5;
        }
        public void stop()
        {
            lock (gate)
            {
                stopped = true;
                draining = true;
                queue.Clear();
                queuedBytes = 0;
                System.Threading.Monitor.PulseAll(gate);
            }
            msgSenders.Remove(this);
        }
        public void Release() { lock (gate) System.Threading.Monitor.PulseAll(gate); }
    }
}
