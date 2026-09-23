
using Gopet.Manager;
using Gopet.Util;
using System.Diagnostics;

namespace Gopet
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Máy chủ goPet";
            Gopet.App.Main.StartServer(args);
            // Sau khi khởi động xong mới bắt tín hiệu dừng: dừng giữa lúc nạp dữ liệu
            // thì chưa có gì để lưu.
            Gopet.App.GracefulShutdown.Register();
            CommandManager.StartReadingKeys();
        }
        
        static Program()
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
    }
}