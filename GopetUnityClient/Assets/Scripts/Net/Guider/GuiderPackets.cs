using System;

namespace Gopet.Net.Guider
{
    /// <summary>
    /// Các gói client gửi lên trong họ <c>COMMAND_GUIDER</c> (122), cộng thêm câu
    /// trả lời Có/Không đi qua <c>SERVER_MESSAGE</c> (45).
    ///
    /// <para>Thứ tự field theo <c>GameController.guider()</c>
    /// (<c>GameController.cs:744-800</c>).</para>
    /// </summary>
    public static class GuiderPackets
    {
        /// <summary>Sub-command server dùng khi GỬI danh sách lựa chọn NPC. Cùng giá trị với <see cref="GopetCmd.SELECT_OPTION"/>.</summary>
        public const sbyte NpcOption = 5;

        /// <summary>
        /// Đã chọn một dòng của màn hình <paramref name="listId"/>.
        ///
        /// <para><paramref name="echo"/> là giá trị server GỬI XUỐNG cho dòng đó
        /// (<c>itemId</c>, hoặc chỉ số dòng khi dòng không có id riêng) — không phải
        /// vị trí dòng trên màn hình. Xem <c>GuiderHandler.Select</c>.</para>
        /// </summary>
        public static Message SelectMenuElement(int listId, int echo)
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.SELECT_MENU_ELEMENT)
                .PutInt(listId)
                .PutInt(echo);
        }

        /// <summary>
        /// Chọn dòng KÈM lựa chọn thanh toán. Đường riêng, không phải
        /// <see cref="SelectMenuElement"/> — server đọc thêm một <c>sbyte</c> chế độ
        /// rồi mới tới hai chỉ số (<c>GameController.cs:776-787</c>).
        /// </summary>
        public static Message SelectMenuElementWithPayment(int listId, int echo, int paymentIndex)
        {
            const sbyte modeSelectWithPayment = 2;

            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.GUIDER_TYPE_PAY)
                .PutInt(listId)
                .PutSByte(modeSelectWithPayment)
                .PutInt(echo)
                .PutInt(paymentIndex);
        }

        /// <summary>Bắt chuyện với NPC — server trả về danh sách lựa chọn.</summary>
        public static Message TalkToNpc(int npcId)
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.NPC_GUIDER)
                .PutInt(npcId);
        }

        /// <summary>
        /// Yêu cầu server mở cửa hàng theo id (<c>SHOP_WEAPON=1</c>, <c>SHOP_ARMOUR=2</c>,
        /// <c>SHOP_HAT=3</c>, <c>SHOP_FOOD=4</c>, …).
        ///
        /// <para><b>KHÔNG phải opcode top-level.</b> <c>GopetCmd.REQUEST_SHOP</c> chỉ được
        /// server đọc như SUB-COMMAND bên trong bao <c>PET_SERVICE</c> (81) —
        /// <c>GameController.onMessage</c> case <c>PET_SERVICE</c> gọi
        /// <c>processPet(subCmd, message)</c>, và <c>case GopetCMD.REQUEST_SHOP</c> nằm
        /// TRONG switch của <c>processPet</c> (<c>GameController.cs:951,1029</c>), không phải
        /// switch ngoài trên <c>message.id</c>. Gửi opcode 2 trần sẽ rơi vào default của
        /// switch ngoài — server im lặng, không mở shop. Cùng pattern với GYM/MAGIC/REQUEST_SHOP_SKIN
        /// (xem <c>BuildingDispatcher.cs</c> case 31/17).</para>
        ///
        /// <para>Server trả gói <c>SHOW_MENU_ITEM</c> qua kênh <c>COMMAND_GUIDER</c>, và
        /// <see cref="GuiderHandler.MenuShown"/> bắn <see cref="MenuScreen"/> như mọi
        /// menu khác — hạ tầng menu có sẵn xử lý được ngay, không cần đường riêng.</para>
        /// </summary>
        public static Message RequestShop(sbyte shopId)
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.REQUEST_SHOP)
                .PutSByte(shopId);
        }

        public static Message SelectNpcOption(int npcId, int optionId)
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.SELECT_OPTION)
                .PutInt(npcId)
                .PutInt(optionId);
        }

        /// <summary>
        /// Trả lời hộp thoại nhập liệu. Số ô phải khớp đúng cái server đã hỏi;
        /// server chặn ở 32 và ngắt kết nối nếu vượt.
        /// </summary>
        public static Message SubmitInputDialog(int dialogId, string[] texts)
        {
            if (texts == null) throw new ArgumentNullException(nameof(texts));
            if (texts.Length > InputDialogSpec.MaxFields)
            {
                throw new ArgumentException(
                    $"Gửi {texts.Length} ô nhập, server chỉ chấp nhận tối đa {InputDialogSpec.MaxFields}.",
                    nameof(texts));
            }

            var m = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_DIALOG_INPUT)
                .PutInt(dialogId)
                .PutInt(texts.Length);

            foreach (var text in texts)
            {
                m.PutUtf(text ?? string.Empty);
            }

            return m;
        }

        /// <summary>
        /// Trả lời Có/Không. Đi trong <c>SERVER_MESSAGE</c> chứ không phải
        /// <c>COMMAND_GUIDER</c> — đây là chỗ rất dễ đặt nhầm.
        /// </summary>
        public static Message AnswerYesNo(int dialogId, bool yes)
        {
            return Message.Create(GopetCmd.SERVER_MESSAGE)
                .PutSByte(GopetCmd.SEND_YES_NO)
                .PutInt(dialogId)
                .PutBool(yes);
        }

        /// <summary>Chạm image dialog để server mở bước tiếp theo (captcha mở form nhập mã).</summary>
        public static Message SelectImageDialog(int dialogId)
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.GUIDER_IMGDIALOG)
                .PutInt(dialogId);
        }

        /// <summary>Tab "Nhiệm vụ tiếp theo": PET_SERVICE/SHOW_LIST_TASK kèm byte tab = 1.
        /// Server đáp menu 1093 (gặp NPC nào, ở map nào). Gói trống (jar) vẫn là menu 1034.</summary>
        public static Message RequestNextTaskGuide()
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.SHOW_LIST_TASK)
                .PutSByte(1);
        }

        /// <summary>Xin trạng thái lịch điểm danh (mở tab). Server đáp TYPE_DAILY_CHECKIN_STATE.</summary>
        public static Message RequestDailyCheckin()
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_DAILY_CHECKIN_OPEN);
        }

        /// <summary>Bấm điểm danh hôm nay.</summary>
        public static Message DoDailyCheckin()
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_DAILY_CHECKIN_DO);
        }

        /// <summary>Xin danh mục khung cảnh màn đấu. Server đáp TYPE_BATTLE_BG_STATE.</summary>
        public static Message RequestBattleScenes()
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_BATTLE_BG_OPEN);
        }

        /// <summary>Mua khung cảnh (server tự trừ vàng, chọn luôn rồi gửi lại STATE).</summary>
        public static Message BuyBattleScene(int sceneId)
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_BATTLE_BG_BUY)
                .PutSByte((sbyte)sceneId);
        }

        /// <summary>Chọn khung cảnh đã sở hữu.</summary>
        public static Message SelectBattleScene(int sceneId)
        {
            return Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_BATTLE_BG_SELECT)
                .PutSByte((sbyte)sceneId);
        }
    }
}
