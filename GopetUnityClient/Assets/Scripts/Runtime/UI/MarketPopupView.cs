using System;
using Gopet.Net.Market;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "Chợ trời" toàn map — 2 tab (Chợ / Gian hàng của tôi), style đồng bộ
    /// <see cref="ShopPopupView"/> (<see cref="GamePopupFrame"/> + <see cref="PopupTabRail"/>).
    /// Server đã gộp mọi ki-ốt (<c>MarketPlace.kiosks</c>) thành một danh sách nên popup
    /// chạy được ở BẤT KỲ map nào, không cast <c>MarketPlace</c>.
    ///
    /// <para><b>Không tự subscribe <see cref="MarketHandler"/> event.</b> Cùng khuôn
    /// <see cref="DailyCheckinView"/>: <see cref="UiRoot"/> nối handler đúng MỘT lần trong
    /// <c>BindMarket</c> rồi forward vào <see cref="BindList"/>/<see cref="BindMine"/> qua
    /// null-check — tránh subscription treo lơ lửng mỗi lần popup bị Destroy và mở lại.</para>
    ///
    /// <para>Đăng bán (tab Gian hàng) thuộc phase 5 — nút "Đăng bán" chỉ bắn
    /// <see cref="SellRequested"/>, chưa tự mở popup 2 cột.</para>
    ///
    /// <para>Phần dựng tab Chợ nằm ở <c>MarketPopupView.MarketTab.cs</c>, tab Gian hàng ở
    /// <c>MarketPopupView.MineTab.cs</c>.</para>
    /// </summary>
    public sealed partial class MarketPopupView : MonoBehaviour
    {
        private const float PopupWidth = 520f;
        private const float PopupHeight = 380f;

        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private MarketHandler _market;
        private RemoteAssetCache _assets;
        private Font _font;
        private long _playerCoin = long.MaxValue;

        public event Action Closed;
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;
        public event Action<string> Message;

        /// <summary>Bấm "Đăng bán" ở tab Gian hàng. Phase 5 hook để mở popup thật; UiRoot
        /// hiện chỉ toast tạm — xem <c>UiRoot.Market.cs</c>.</summary>
        public event Action SellRequested;

        public static MarketPopupView Create(Transform parent, Font font, MarketHandler market,
            RemoteAssetCache assets)
        {
            var frame = GamePopupFrame.Create(parent, font, "Chợ trời", PopupWidth, PopupHeight);
            frame.gameObject.name = "MarketPopup";

            var view = frame.gameObject.AddComponent<MarketPopupView>();
            view._frame = frame;
            view._market = market ?? throw new ArgumentNullException(nameof(market));
            view._assets = assets;
            view._font = font;
            frame.Closed += () => view.Closed?.Invoke();

            view._rail = PopupTabRail.Create(frame.Content, font, frame.ContentWidth,
                new[] { "Chợ", "Gian hàng của tôi" });
            view._rail.Selected += view.SelectTab;

            view.BuildMarketTab(frame.Content, frame.ContentWidth);
            view.BuildMineTab(frame.Content, frame.ContentWidth);

            view._rail.Select(0);
            return view;
        }

        /// <summary>Cập nhật ngọc hiện có — chỉ để chặn bấm Mua khi rõ ràng không đủ (UX).
        /// Server vẫn là nguồn thật, xem <c>MarketPopupView.MarketTab.cs.TryBuy</c>.</summary>
        public void SetPlayerCoin(long coin) => _playerCoin = coin;

        /// <summary>Forward từ <see cref="MarketHandler.ListReceived"/> qua <c>UiRoot</c>.</summary>
        public void BindList(MarketListState state) => ApplyListState(state);

        /// <summary>Forward từ <see cref="MarketHandler.MineReceived"/> qua <c>UiRoot</c>.</summary>
        public void BindMine(MarketMineState state) => ApplyMineState(state);

        private void SelectTab(int index)
        {
            _marketPanel.gameObject.SetActive(index == 0);
            _minePanel.gameObject.SetActive(index == 1);

            if (index == 0) RequestList();
            else RequestMine();
        }

        /// <summary>Trang nội dung nằm dưới khay tab, chiếm hết chỗ còn lại — cùng khuôn
        /// <c>DailyCheckinView.MakePage</c>.</summary>
        private static RectTransform MakeTabPanel(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            return rect;
        }
    }
}
