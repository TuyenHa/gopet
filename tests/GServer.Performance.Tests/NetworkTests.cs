using Gopet.IO;
using System.Net;
using System.Net.Sockets;

static class NetworkTests
{
    public static void WireVariants()
    {
        foreach (bool iwin in new[] {false, true})
        {
            Message.isiWin = iwin;
            try
            {
                using var stream = new MemoryStream();
                var session = new Session(null) { dos = new BinaryWriter(stream), tea = new TEA(1740000000000L) };
                var sender = new MsgSender(session);
                try
                {
                    var message = new Message(-7); message.putShort(0x1234); message.cleanup();
                    sender.doSendMessage(message);
                    var expected = iwin ? new byte[] {0,0,0,6,0,40,255,249,18,52} : new byte[] {0,0,0,4,0,249,18,52};
                    if (!stream.ToArray().SequenceEqual(expected)) throw new Exception("legacy frame changed");
                }
                finally { sender.stop(); }
            }
            finally { Message.isiWin = false; }
        }
        using var encrypted = new MemoryStream();
        var encryptedSession = new Session(null) { dos = new BinaryWriter(encrypted), tea = new TEA(1740000000000L) };
        var encryptedSender = new MsgSender(encryptedSession);
        try
        {
            encryptedSender.doSendMessage(new Message(1, true));
            if (!Convert.ToHexString(encrypted.ToArray()).Equals("0000000D010000000114A09825A2FB7CC7"))
                throw new Exception("encrypted vector changed");
            encrypted.Position = 0;
            if (!Gopet.Net.PacketFramer.TryReadFrame(encrypted, out var payload, out bool flag) || !flag
                || !new Gopet.Net.Tea(1740000000000L).Decrypt(payload).SequenceEqual(new byte[] {1}))
                throw new Exception("Unity decoder rejected server frame");
        }
        finally { encryptedSender.stop(); }
    }
    public static void FinalPacket()
    {
        int baseline = Session.socketCount;
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        using var client = new TcpClient();
        client.Connect((IPEndPoint)listener.LocalEndpoint);
        var socket = listener.AcceptSocket(); listener.Stop();
        var handler = new CountingHandler(); int removed = 0;
        var session = new Session(socket, _ => Interlocked.Increment(ref removed)); session.setHandler(handler);
        try
        {
            client.GetStream().Write(new byte[] {0,1,2,3,4,5,6,7,8});
            session.run();
            session.sendMessage(new Message(42)); session.Close();
            client.ReceiveTimeout = 3000;
            var bytes = new byte[6]; client.GetStream().ReadExactly(bytes);
            if (!bytes.SequenceEqual(new byte[] {0,0,0,2,0,42})) throw new Exception("final packet lost");
            if (!session.Completion.Wait(3000) || handler.Count != 1 || removed != 1 || Session.socketCount != baseline)
                throw new Exception("socket lifetime did not return to baseline");
        }
        finally { session.Close(); session.Completion.Wait(3000); }
    }
    public static void DisconnectWaitsForHandler()
    {
        using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
        var handler = new CountingHandler { Action = () => { entered.Set(); release.Wait(3000); } };
        var session = new Session(null) { isSocketConnected = true, messageHandler = handler,
            dis = new BinaryReader(new MemoryStream(new byte[] {0,0,0,2,0,42})) };
        var reader = new Thread(new MsgReader(session).run) { IsBackground = true }; reader.Start();
        try
        {
            if (!entered.Wait(1000)) throw new Exception("handler not invoked");
            session.Close();
            if (session.Completion.Wait(150) || handler.Count != 0) throw new Exception("saved/disconnected during message processing");
        }
        finally { release.Set(); reader.Join(3000); session.Close(); session.Completion.Wait(3000); }
        if (handler.Count != 1) throw new Exception("disconnect count wrong");
    }
    sealed class CountingHandler : IHandleMessage
    {
        public int Count; public Action Action;
        public void onMessage(Message message) => Action?.Invoke();
        public void onDisconnected() => Interlocked.Increment(ref Count);
    }
}
