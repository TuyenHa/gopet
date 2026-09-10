using System;
using Gopet.Net;
using Gopet.Net.Guider;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Mở màn hình THẬT từ server và đọc bằng đúng parser mà Unity dùng.
    ///
    /// <para>Dùng <c>CHARGE_MONEY_INFO</c> (44) vì nó mở thẳng màn ATM
    /// (<c>Player.cs:143</c> → <c>MenuController.MENU_ATM</c>) mà không phụ thuộc
    /// vị trí người chơi. Đường qua NPC thì phụ thuộc bản đồ: gửi
    /// <c>NPC_GUIDER</c> với id không có trong bản đồ hiện tại, server duyệt không
    /// thấy rồi <b>im lặng</b> (<c>GameController.cs:749-762</c>) — đã thử và mất
    /// 10 giây chờ vô ích.</para>
    /// </summary>
    internal static class GuiderChecks
    {
        /// <summary>
        /// Dựng và trả <see cref="GuiderHandler"/> đã đăng ký — caller tái dùng cho các
        /// check khác trên cùng phiên (vd <c>ShopChecks</c>). Đăng ký hai lần trên cùng
        /// router sẽ ném "sub-command đã có handler".
        /// </summary>
        public static GuiderHandler Run(GopetSocket socket, MessageRouter router)
        {
            var guider = new GuiderHandler(socket.Send);

            ListOptionScreen listOption = null;
            MenuScreen menu = null;
            InputDialogSpec inputDialog = null;
            YesNoRequest yesNo = null;

            guider.ListOptionShown += l => listOption = l;
            guider.MenuShown += m => menu = m;
            guider.InputDialogShown += d => inputDialog = d;
            guider.YesNoAsked += y => yesNo = y;
            guider.RegisterOn(router);

            // --- Mở màn hình ATM ---
            socket.Send(Message.Create(GopetCmd.CHARGE_MONEY_INFO));
            MessagePump.Until(socket, router, () => listOption != null, TimeSpan.FromSeconds(10));

            Report.Check("Q. Mở được màn hình thật, đọc đúng danh sách lựa chọn",
                listOption != null && listOption.Options.Length > 0,
                listOption == null ? "không nhận được màn hình nào trong 10s" : "danh sách rỗng");

            if (listOption == null) return guider;

            Console.WriteLine($"       -> [{listOption.ListId}] \"{listOption.Title}\" " +
                              $"nút \"{listOption.CommandText}\", {listOption.Options.Length} lựa chọn: " +
                              $"{string.Join(" | ", Preview(listOption))}");

            // --- Chọn một dòng: server phải trả về màn hình hoặc hộp thoại tiếp theo ---
            var opened = listOption;
            listOption = null;

            guider.Select(opened, 0);
            MessagePump.Until(socket, router,
                () => listOption != null || menu != null || inputDialog != null || yesNo != null,
                TimeSpan.FromSeconds(10));

            var next = Describe(listOption, menu, inputDialog, yesNo);

            Report.Check("R. Chọn một dòng → đọc được màn hình tiếp theo",
                next != null, "không nhận được màn hình nào sau khi chọn, trong 10s");

            if (next != null)
            {
                Console.WriteLine($"       -> {next}");
            }

            CheckRealMenuRows(menu);
            return guider;
        }

        /// <summary>
        /// Siết kiểm trên menu THẬT có dòng.
        ///
        /// <para>Mọi gói <c>SHOW_MENU_ITEM</c> bắt được trước đó đều rỗng, nên phần
        /// đọc TỪNG DÒNG — gồm field điều kiện <c>showDialog</c> — chỉ được phủ bằng
        /// gói tự dựng. Đây là chỗ duy nhất nó gặp dữ liệu thật, nên kiểm cho chặt.</para>
        ///
        /// <para>Không lưu được thành vector offline: gói này 1699 byte, mà packet
        /// dump cắt hex ở 256.</para>
        /// </summary>
        private static void CheckRealMenuRows(MenuScreen menu)
        {
            if (menu == null || menu.Items.Length == 0)
            {
                Console.WriteLine("       (bỏ qua check S: màn hình tiếp theo không có dòng nào)");
                return;
            }

            var wellFormed = true;
            var reason = string.Empty;

            foreach (var item in menu.Items)
            {
                if (item.Title == null || item.Description == null || item.ImagePath == null)
                {
                    wellFormed = false;
                    reason = "có dòng thiếu chuỗi — nhiều khả năng đã đọc lệch";
                    break;
                }

                // showDialog quyết định có đọc 3 chuỗi tiếp theo hay không. Đọc sai
                // là ba chuỗi đó lẫn sang dòng sau, nên bất nhất ở đây tố cáo lệch stream.
                if (item.ShowDialog != (item.DialogText != null))
                {
                    wellFormed = false;
                    reason = $"dòng \"{item.Title}\": showDialog={item.ShowDialog} nhưng dialogText={(item.DialogText == null ? "null" : "có")}";
                    break;
                }

                if (item.PaymentOptions == null)
                {
                    wellFormed = false;
                    reason = $"dòng \"{item.Title}\" không có mảng lựa chọn thanh toán";
                    break;
                }
            }

            Report.Check($"S. Menu thật {menu.Items.Length} dòng: mọi dòng đọc ra nhất quán",
                wellFormed, reason);

            if (wellFormed)
            {
                var first = menu.Items[0];
                Console.WriteLine($"       -> dòng đầu: id={first.ItemId} \"{first.Title}\" " +
                                  $"icon=\"{first.ImagePath}\" chọn được={first.CanSelect}");
            }
        }

        private static string Describe(ListOptionScreen list, MenuScreen menu,
                                       InputDialogSpec input, YesNoRequest yesNo)
        {
            if (list != null) return $"danh sách [{list.ListId}] \"{list.Title}\", {list.Options.Length} lựa chọn";
            if (menu != null) return $"menu [{menu.ListId}] \"{menu.Title}\", {menu.Items.Length} dòng";
            if (input != null) return $"hộp thoại nhập [{input.DialogId}] \"{input.Title}\", {input.Fields.Length} ô";
            if (yesNo != null) return $"hộp thoại Có/Không [{yesNo.DialogId}] \"{yesNo.Text}\"";
            return null;
        }

        private static string[] Preview(ListOptionScreen screen)
        {
            var take = Math.Min(3, screen.Options.Length);
            var preview = new string[take];
            for (var i = 0; i < take; i++) preview[i] = screen.Options[i].Text;
            return preview;
        }
    }
}
