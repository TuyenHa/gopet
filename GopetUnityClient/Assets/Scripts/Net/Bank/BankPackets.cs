namespace Gopet.Net.Bank
{
    /// <summary>
    /// Trigger mở Ngân hàng (ATM).
    ///
    /// <para><b>Wire:</b> opcode top-level <c>CHARGE_MONEY_INFO (44)</c>, không body.</para>
    ///
    /// <para><b>Server dispatch:</b> <c>Player.cs:143</c> bắt opcode 44 <b>trước</b> khi vào
    /// <c>controller.onMessage</c>, và gọi <c>MenuController.sendMenu(MENU_ATM=1039)</c> —
    /// server trả về ListOption 3 dòng qua tầng Guider.</para>
    ///
    /// <para><b>Chú ý bẫy:</b> client J2ME jar dùng <c>cx.b(int)</c> gửi
    /// <c>en(81).a(44)</c> — opcode <c>PET_SERVICE / sub 44</c>, nhưng <c>processPet</c>
    /// <b>không</b> có case 44 → gói bị ignore. Client Unity phải gửi TOP-LEVEL 44 để
    /// Player.cs bắt được. Đây là chỗ nếu port máy móc từ jar sẽ hỏng silent.</para>
    ///
    /// <para><b>Flow sau khi mở:</b>
    /// <list type="number">
    ///   <item>Server bơm SHOW_LIST_OPTION 3 dòng: đổi vàng (mua từ item shop) / đổi vàng→đậu / đổi lúa→ngọc.</item>
    ///   <item>User chọn → <c>GuiderPackets.SelectMenuElement</c>.</item>
    ///   <item>Option 1 hoặc 2 → server bơm <c>TYPE_DIALOG_INPUT</c> hỏi số lượng.</item>
    ///   <item>User submit → <c>GuiderPackets.SubmitInputDialog</c> → server thực hiện đổi.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class BankPackets
    {
        /// <summary>Mở menu Ngân hàng. Không có body.</summary>
        public static Message OpenBankMenu()
        {
            return Message.Create(GopetCmd.CHARGE_MONEY_INFO);
        }
    }
}
