namespace Gopet.Net.Market
{
    /// <summary>
    /// Gói client gửi lên trong họ <c>COMMAND_GUIDER</c> (122) cho popup "Chợ trời",
    /// sub 47..57. Thứ tự field khớp <c>GameController.cs:852-872</c>
    /// (case <c>GopetCMD.TYPE_MARKET_*</c>).
    /// </summary>
    public static class MarketPackets
    {
        public static Message RequestList(sbyte filter, sbyte sort, short page) =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_LIST)
                .PutSByte(filter).PutSByte(sort).PutShort(page);

        public static Message Buy(sbyte kioskType, int listingId) =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_BUY)
                .PutSByte(kioskType).PutInt(listingId);

        public static Message RequestMine() =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_MINE);

        public static Message Cancel(sbyte kioskType, int listingId) =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_CANCEL)
                .PutSByte(kioskType).PutInt(listingId);

        public static Message RequestSellable() =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_SELLABLE);

        /// <summary>Đăng bán trọn gói. <paramref name="price"/> là tổng giá người mua trả — dùng ở phase 5.</summary>
        public static Message Sell(sbyte source, int id, int count, int price) =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_SELL)
                .PutSByte(source).PutInt(id).PutInt(count).PutInt(price);

        /// <summary><paramref name="buyerName"/> rỗng = bỏ chỉ định (ai cũng mua được lại).</summary>
        public static Message Assign(sbyte kioskType, int listingId, string buyerName) =>
            Message.Create(GopetCmd.COMMAND_GUIDER).PutSByte(GopetCmd.TYPE_MARKET_ASSIGN)
                .PutSByte(kioskType).PutInt(listingId).PutUtf(buyerName ?? string.Empty);
    }
}
