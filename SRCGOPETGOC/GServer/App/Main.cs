
using Dapper;
using Gopet.APIs;
using Gopet.Data.Collections;
using Gopet.Data.Event;
using Gopet.Data.GopetClan;
using Gopet.Data.GopetItem;
using Gopet.Data.Map;
using Gopet.Manager;
using Gopet.Runtime;
using Gopet.Server;
using Gopet.Server.IO;
using Gopet.Shared.Helper;
using Gopet.Util;
using Newtonsoft.Json;
namespace Gopet.App
{
    public class Main
    {

        public static IServerBase server;
        public static int PORT_SERVER = ServerSetting.instance.portGopetServer;
        public static bool isNetBeans = true;
        public static int HTTP_PORT = ServerSetting.instance.portHttpServer;
        public static HttpServer APIServer;

        /**
         * hàm chính
         *
         * @param args
         * @ 
         */
        public static void StartServer(string[] args)
        {
            Thread.CurrentThread.Name = "MAIN THREAD (GOPET)";
            if (ServerSetting.instance.initLog)
            {
                initLog();
            }
            // Kiểm tra DB trước khi nạp template. Nếu cấu hình sai, hỏng ở đây
            // với thông báo rõ ràng thay vì chết giữa GopetManager.init().
            MYSQLManager.VerifyConnections();
            //        AutoMaintenance autoMaintenance = new AutoMaintenance();
            //        autoMaintenance.start(ServerSetting.instance.getHourMaintenance(), ServerSetting.instance.getMinMaintenance());
            GopetManager.init();
            HistoryManager.Instance.start();
            MapManager.init();
            ClanManager.init();
            GopetManager.loadMarket();
            BXHManager.instance.start();
            FieldManager.Init();
            initRuntime();
            RuntimeServer.instance.start();
            DailyBossEvent.Instance = new DailyBossEvent();
            //EventManager.AddEvent(DailyBossEvent.Instance);
            EventManager.Start();
            ScheduleManager.Instance.Start();
            PlayerManager.Instance.Start();
            APIServer = new HttpServer(HTTP_PORT);
            APIServer.Start();
            server = new Gopet.MServer.Server(PORT_SERVER);
            server.StartServer();
        }

        public static void initRuntime()
        {
            RuntimeServer.instance.runtimes.add(new AutoSave());
            RuntimeServer.instance.runtimes.add(new DBBackup());
            RuntimeServer.instance.runtimes.add(Maintenance.gI());
            RuntimeServer.instance.runtimes.add(new MarketExpiryTicker());
        }


        public static void initLog()
        {

        }

        private static int _shutdownStarted;

        /// <summary>
        /// Lưu market, clan, người chơi rồi dừng các cổng. Chạy đúng MỘT lần: lệnh
        /// console <c>shutdown</c> và tín hiệu dừng (<see cref="GracefulShutdown"/>) có thể
        /// cùng tới — chạy hai lần là đóng/lưu chồng lên nhau.
        /// </summary>
        public static void shutdown()
        {
            if (Interlocked.Exchange(ref _shutdownStarted, 1) == 1) return;
            MapManager.stopUpdate();
            GopetManager.saveMarket();
            server.StopServer();
            APIServer.Stop();
            RuntimeServer.isRunning = false;
            foreach (var backup in RuntimeServer.instance.runtimes.OfType<DBBackup>()) backup.Stop();
            Thread.Sleep(1000);

            foreach (Player player in PlayerManager.players)
            {
                player.session.Close();
            }
            if (server is Gopet.MServer.Server tcpServer)
                tcpServer.WaitForSessionsClosed().GetAwaiter().GetResult();

            foreach (Clan clan in ClanManager.clans)
            {
                try
                {
                    clan.save();
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
            if (!HistoryManager.Instance.Stop(TimeSpan.FromSeconds(10)))
                GopetManager.ServerMonitor.LogWarning("History backlog retained on disk for next startup");
            Gopet.Logging.Monitor.Flush(TimeSpan.FromSeconds(3));
        }
    }

}
