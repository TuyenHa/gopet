namespace Gopet.Net.Market
{
    /// <summary>
    /// TYPE_MARKET_RESULT (sub 56) — kết quả một hành động (Mua/Gỡ/Bán/Chỉ định). Server
    /// LUÔN gửi kèm gói state mới (48 sau Mua, 51 sau Gỡ/Bán/Chỉ định) ngay sau gói này nên
    /// UiRoot chỉ cần bắn toast, view tự làm mới khi state tới.
    ///
    /// <para>Field order khớp <c>MarketService.SendResult</c>
    /// (<c>GServer/Data/Market/MarketService.cs:107-116</c>).</para>
    /// </summary>
    public sealed class MarketResult
    {
        public const sbyte ActionBuy = 1;
        public const sbyte ActionCancel = 2;
        public const sbyte ActionSell = 3;
        public const sbyte ActionAssign = 4;

        public sbyte Action;
        public bool Ok;
        public string Message;

        public static MarketResult Parse(Message message)
        {
            var r = message.Reader;
            var result = new MarketResult
            {
                Action = r.ReadSByte(),
                Ok = r.ReadBool(),
                Message = r.ReadUtf(),
            };
            r.ExpectFullyConsumed("TYPE_MARKET_RESULT");
            return result;
        }
    }
}
