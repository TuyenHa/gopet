using System;
using Gopet.Net;
using Gopet.Net.Guider;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Xác nhận sửa lỗi <c>GuiderPackets.RequestShop</c>: bản cũ gửi <c>REQUEST_SHOP</c>
    /// (2) làm opcode TOP-LEVEL, nhưng server chỉ đọc nó như sub-command trong bao
    /// <c>PET_SERVICE</c> (81) (<c>GameController.cs</c> <c>processPet</c>, dòng 1029) —
    /// gửi sai bọc thì server im lặng, bấm shop trên map không mở được gì.
    ///
    /// <para>Check này gửi packet THẬT mà <c>BuildingDispatcher</c> dùng cho building
    /// type 27 (Vũ khí, shopId=1) và đòi server trả về màn hình — bằng chứng sống rằng
    /// bọc <c>PET_SERVICE</c> đúng.</para>
    /// </summary>
    internal static class ShopChecks
    {
        /// <param name="guider">
        /// Handler ĐÃ đăng ký (từ <see cref="GuiderChecks.Run"/>) — đăng ký handler
        /// <c>COMMAND_GUIDER</c> lần hai trên cùng router sẽ ném "sub-command đã có handler".
        /// </param>
        public static void Run(GopetSocket socket, MessageRouter router, GuiderHandler guider)
        {
            MenuScreen menu = null;
            guider.MenuShown += m => menu = m;

            // shopId=1 = SHOP_WEAPON — cùng packet BuildingDispatcher.Dispatch(27) tạo ra.
            socket.Send(GuiderPackets.RequestShop(1));
            MessagePump.Until(socket, router, () => menu != null, TimeSpan.FromSeconds(5));

            // Chỉ đòi NHẬN ĐƯỢC màn hình đúng chỗ — đây là bằng chứng bọc PET_SERVICE
            // đúng (trước khi sửa: opcode 2 trần → server im lặng, timeout 5s không gì cả).
            // Item rỗng là chuyện DB seed của môi trường test, KHÔNG phải lỗi wire —
            // không đòi hỏi ở check này.
            Report.Check("W. REQUEST_SHOP bọc PET_SERVICE đúng — server mở được shop Vũ khí",
                menu != null,
                "không nhận được SHOW_MENU_ITEM trong 5s — nghi wire format sai bọc lại");

            if (menu != null)
            {
                Console.WriteLine($"       -> menu [{menu.ListId}] \"{menu.Title}\", {menu.Items.Length} item" +
                                  (menu.Items.Length == 0 ? " (DB test không seed item shop — không phải lỗi wire)" : ""));
            }
        }
    }
}
