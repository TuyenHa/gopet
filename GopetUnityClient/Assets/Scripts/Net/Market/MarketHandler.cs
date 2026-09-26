using System;

namespace Gopet.Net.Market
{
    /// <summary>
    /// Kênh gói tin popup "Chợ trời" (<c>COMMAND_GUIDER</c> sub 47..57). Cùng khuôn
    /// <see cref="Gopet.Net.Guider.GuiderHandler"/>: thuần C# (test được ngoài Unity),
    /// giữ cả phần GỬI LÊN (RequestList/Buy/…) để <c>MarketPopupView</c> không phải tự
    /// lắp <see cref="Message"/> — đúng khuôn <c>GuiderHandler.Select</c>/<c>RequestShop</c>.
    ///
    /// <para><b>Không dùng lại GuiderHandler</b>: các sub 47..57 không phải
    /// <c>MenuScreen</c>/<c>ListOptionScreen</c> mà GuiderHandler biết đọc — đăng ký
    /// chung một envelope (<see cref="MessageRouter.RegisterEnvelope"/> nhận trùng vô
    /// hại, xem cách <c>KioskHandler</c> làm với <c>PET_SERVICE</c>) nhưng sub-command
    /// khác nhau nên không đụng nhau.</para>
    /// </summary>
    public sealed class MarketHandler
    {
        private readonly Action<Message> _send;

        public MarketHandler(Action<Message> send)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public event Action<MarketListState> ListReceived;
        public event Action<MarketMineState> MineReceived;
        public event Action<MarketSellableState> SellableReceived;
        public event Action<MarketResult> ResultReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));

            router.RegisterEnvelope(GopetCmd.COMMAND_GUIDER);
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_MARKET_LIST_STATE,
                m => ListReceived?.Invoke(MarketListState.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_MARKET_MINE_STATE,
                m => MineReceived?.Invoke(MarketMineState.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_MARKET_SELLABLE_STATE,
                m => SellableReceived?.Invoke(MarketSellableState.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_MARKET_RESULT,
                m => ResultReceived?.Invoke(MarketResult.Parse(m)));
        }

        public void RequestList(sbyte filter, sbyte sort, short page) => _send(MarketPackets.RequestList(filter, sort, page));

        public void Buy(sbyte kioskType, int listingId) => _send(MarketPackets.Buy(kioskType, listingId));

        public void RequestMine() => _send(MarketPackets.RequestMine());

        public void Cancel(sbyte kioskType, int listingId) => _send(MarketPackets.Cancel(kioskType, listingId));

        public void RequestSellable() => _send(MarketPackets.RequestSellable());

        public void Sell(sbyte source, int id, int count, int price) => _send(MarketPackets.Sell(source, id, count, price));

        public void Assign(sbyte kioskType, int listingId, string buyerName) => _send(MarketPackets.Assign(kioskType, listingId, buyerName));
    }
}
