

using Gopet.Data.Collections;
using System.Net.Sockets;
using Gopet.Util;
using Gopet.IO;
using Gopet.Server.IO;
using System.Net;
using System.Diagnostics;
using System;
using System.Collections.Concurrent;
namespace Gopet.MServer
{
    public class Server : IServerBase
    {
        private TcpListener _listener;

        public CopyOnWriteArrayList<Session> sessions { get; } = new();
        public bool IsRunning { get; set; } = false;

        private readonly SemaphoreSlim handshakes = new(64, 64);
        private readonly object sessionGate = new();

        private ConcurrentDictionary<string, DateTime> ConnectionWait = new();


        public Server(int port)
        {
            // Mặc định 0.0.0.0 (giữ nguyên hành vi cũ, production cần vậy).
            // Máy dev đặt 127.0.0.1 trong config/server.json.
            var bind = ServerSetting.instance.gameBindAddress;
            if (!IPAddress.TryParse(bind, out var address))
            {
                throw new ArgumentException(
                    $"gameBindAddress không phải địa chỉ IP hợp lệ: \"{bind}\". " +
                    "Dùng 0.0.0.0 để mở ra ngoài, 127.0.0.1 để chỉ nghe cục bộ.");
            }

            IPEndPoint iPEndPoint = new IPEndPoint(address, port);
            _listener = new TcpListener(iPEndPoint);
        }

        public Thread RunnerThread { get; protected set; }



        public void StartServer()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                _listener.Start();

                // Nói rõ đang nghe ở đâu: "cổng game mở ra toàn mạng" là điều
                // người vận hành phải biết mà không cần đi đọc config.
                var ep = (IPEndPoint)_listener.LocalEndpoint;
                var scope = IPAddress.IsLoopback(ep.Address) ? "chỉ cục bộ" : "MỌI giao diện mạng";
                GopetManager.ServerMonitor.LogWarning(
                    $"Cổng game nghe ở {ep.Address}:{ep.Port} ({scope})");
                RunnerThread = new Thread(Runner);
                RunnerThread.Name = "Listener Thread";
                RunnerThread.IsBackground = true;
                RunnerThread.Start();
            }
        }

        private void Runner()
        {
            while (IsRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    if (ConnectionWait.TryGetValue(clientIP, out var dateTime) && dateTime > DateTime.Now)
                    {
                        client.Close();
                        continue;
                    }
                    else ConnectionWait[clientIP] = DateTime.Now.AddSeconds(2);
                    this.CleanupConnectionWait();
                    if (!handshakes.Wait(0)) { client.Close(); continue; }
                    ThreadPool.QueueUserWorkItem(setupClient, client);
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
        }
        private void setupClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            try
            {
                Session session;
                lock (sessionGate)
                {
                    if (!IsRunning) { client.Close(); return; }
                    session = new Session(client.Client, closed => sessions.Remove(closed));
                    sessions.Add(session);
                }
                session.run();
            }
            finally { handshakes.Release(); }
        }

        private void CleanupConnectionWait()
        {
            foreach (var key in ConnectionWait.Keys)
            {
                if (ConnectionWait[key] <= DateTime.Now)
                {
                    ConnectionWait.TryRemove(key, out _);
                }
            }
        }

        public void StopServer()
        {
            lock (sessionGate)
            {
                IsRunning = false;
                _listener.Stop();
                foreach (var session in sessions) session.Close();
            }
        }

        public Task WaitForSessionsClosed() => Task.WhenAll(sessions.Select(s => s.Completion));
    }
}
