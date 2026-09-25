using System;
using Gopet.Net.Market;

namespace Gopet.Runtime.UI
{
    /// <summary>Popup "Chợ trời" (kiosk toàn map), mở từ icon HUD cạnh Cửa hàng.</summary>
    public sealed partial class UiRoot
    {
        private MarketHandler _marketHandler;
        private MarketPopupView _marketPopup;
        private MarketSellPopupView _marketSellPopup;
        private long _playerCoin = long.MaxValue;

        /// <summary>
        /// Nối <see cref="MarketHandler"/> đã đăng ký trong <c>GameSession</c> (cạnh
        /// <c>KioskHandler</c>) — gọi ngay khi vào map, trước khi icon HUD "Chợ trời" bấm
        /// được. Không đi qua <c>GuiderHandler</c> vì các sub 47..57 KHÔNG phải
        /// <c>MenuScreen</c>/<c>ListOptionScreen</c> mà nó biết đọc.
        ///
        /// <para>Chỉ forward state — popup TỰ quyết định có bind hay không (so khớp
        /// filter/sort/page hiện tại, xem <c>MarketPopupView.MarketTab.cs</c>).</para>
        /// </summary>
        public void BindMarket(MarketHandler market)
        {
            _marketHandler = market ?? throw new ArgumentNullException(nameof(market));
            _marketHandler.ListReceived += state => _marketPopup?.BindList(state);
            _marketHandler.MineReceived += state => _marketPopup?.BindMine(state);
            _marketHandler.SellableReceived += state => _marketSellPopup?.BindSellable(state);
            _marketHandler.ResultReceived += OnMarketResult;
        }

        /// <summary>
        /// Cập nhật số ngọc hiện có — chỉ dùng để chặn nút Mua khi rõ ràng không đủ
        /// (UX tốt hơn, đỡ round-trip). Server vẫn là nguồn thật quyết định giao dịch.
        /// </summary>
        public void SetPlayerCoin(long coin)
        {
            _playerCoin = coin;
            _marketPopup?.SetPlayerCoin(coin);
        }

        public void OpenMarketPopup()
        {
            if (_marketPopup != null) return;
            if (_marketHandler == null)
            {
                ShowToast("Chợ trời chưa sẵn sàng, thử lại sau.");
                return;
            }

            _marketPopup = MarketPopupView.Create(transform, _font, _marketHandler, _assets);
            _marketPopup.SetPlayerCoin(_playerCoin);
            _marketPopup.Closed += () => Close(_marketPopup);
            // Xác nhận Mua/Gỡ phải hỏi trước; không nối thì nút bấm vào im lặng.
            _marketPopup.ConfirmRequested += ShowConfirm;
            _marketPopup.Message += ShowToast;
            _marketPopup.SellRequested += OpenMarketSellPopup;

            Push(_marketPopup, _marketPopup.gameObject);
        }

        /// <summary>
        /// Popup "Đăng bán" (2 cột: rương đồ / chi tiết), push TRÊN popup Chợ trời — đóng
        /// nó (X, bấm ra ngoài, hoặc bán thành công) thì quay lại popup Chợ trời chứ
        /// không thoát hẳn cả hai. Mở là xin ngay danh sách đồ bán được (sub 53).
        /// </summary>
        private void OpenMarketSellPopup()
        {
            if (_marketSellPopup != null || _marketHandler == null) return;

            _marketSellPopup = MarketSellPopupView.Create(transform, _font, _marketHandler, _assets);
            _marketSellPopup.Closed += () => Close(_marketSellPopup);
            _marketSellPopup.Message += ShowToast;

            Push(_marketSellPopup, _marketSellPopup.gameObject);
            _marketHandler.RequestSellable();
        }

        private void OnMarketResult(MarketResult result)
        {
            if (result == null) return;
            if (!string.IsNullOrEmpty(result.Message)) ShowToast(result.Message);

            if (result.Action != MarketResult.ActionSell || _marketSellPopup == null) return;
            // ok: server đã gửi kèm MINE_STATE (51) để tab Gian hàng tự refresh — đóng
            // popup Đăng bán là đủ. fail: giữ popup mở, thêm dòng đỏ trong pane phải.
            if (result.Ok) Close(_marketSellPopup);
            else _marketSellPopup.HandleSellFailed(result.Message);
        }
    }
}
