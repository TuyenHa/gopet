using System;
using System.Collections.Generic;
using Gopet.Net.Market;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "Đăng bán" — 2 cột kiểu <see cref="MailboxView"/>: rương đồ bán được bên trái,
    /// chi tiết + ô giá/số lượng bên phải (<see cref="MarketSellDetailPane"/>). Mở từ nút
    /// "Đăng bán" ở tab Gian hàng của tôi (<c>MarketPopupView.MineTab.cs</c>), push TRÊN
    /// popup Chợ trời — đóng nó thì quay lại popup Chợ trời chứ không thoát hẳn.
    ///
    /// <para>Không tự subscribe <see cref="MarketHandler"/> event, cùng khuôn
    /// <see cref="MarketPopupView"/>: <c>UiRoot.Market.cs</c> nối đúng MỘT lần rồi forward
    /// qua <see cref="BindSellable"/>/<see cref="HandleSellFailed"/>.</para>
    /// </summary>
    public sealed partial class MarketSellPopupView : MonoBehaviour
    {
        private const float Width = 520f;
        private const float Height = 360f;

        /// <summary>Phần bề ngang vùng nội dung dành cho danh sách, cùng tỉ lệ MailboxView.</summary>
        private const float ListWidthFraction = 0.44f;
        private const float SplitGap = 8f;

        internal const float RowHeight = 42f;

        private GamePopupFrame _frame;
        private MarketHandler _market;
        private RemoteAssetCache _assets;
        private Font _font;
        private PopupItemList _list;
        private MarketSellDetailPane _detail;
        private readonly List<GameObject> _rows = new List<GameObject>();
        private IReadOnlyList<MarketSellableItem> _items = Array.Empty<MarketSellableItem>();

        public event Action Closed;

        /// <summary>Toast — chủ yếu dùng khi bấm món đang khoá (<c>BlockReason</c>).</summary>
        public event Action<string> Message;

        public static MarketSellPopupView Create(Transform parent, Font font, MarketHandler market,
            RemoteAssetCache assets)
        {
            // Nền mờ + bấm ra ngoài để đóng — cùng khuôn MailboxView.Create.
            var root = new GameObject("MarketSellPopup", typeof(RectTransform), typeof(Image),
                typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, .5f);

            var view = root.AddComponent<MarketSellPopupView>();
            view._market = market ?? throw new ArgumentNullException(nameof(market));
            view._assets = assets;
            view._font = font;
            root.GetComponent<Button>().onClick.AddListener(() => view.Closed?.Invoke());

            view._frame = GamePopupFrame.Create(root.transform, font, "Đăng bán", Width, Height);
            view._frame.Closed += () => view.Closed?.Invoke();

            view.BuildList();
            view.BuildDetail();
            view._list.ShowPlaceholder("Đang tải…");
            return view;
        }

        /// <summary>Forward từ <see cref="MarketHandler.SellableReceived"/> qua <c>UiRoot</c>.
        /// Chỉ tới ĐÚNG MỘT lần lúc mở popup (đáp lại <c>RequestSellable</c>), nên không cần
        /// giữ lại món đang chọn qua lần bind sau — chưa từng có lần sau (YAGNI).</summary>
        public void BindSellable(MarketSellableState state)
        {
            _items = state?.Items ?? Array.Empty<MarketSellableItem>();
            RebuildRows();
            _detail.Show(null);
        }

        /// <summary>Server từ chối bán (action=3, ok=false): dòng đỏ trong pane + mở khoá nút.
        /// Toast đã do <c>UiRoot.OnMarketResult</c> lo chung cho mọi action.</summary>
        public void HandleSellFailed(string message)
        {
            _detail.ShowError(message);
            _detail.SetBusy(false);
        }

        private void BuildDetail()
        {
            _detail = MarketSellDetailPane.Create(_frame.Content, _font, _assets);
            var rect = (RectTransform)_detail.transform;
            // Floor để tránh cạnh cột lệch nửa pixel (mờ viền do UI scaling không nguyên).
            rect.offsetMin = new Vector2(Mathf.Floor(_frame.ContentWidth * ListWidthFraction) + SplitGap, 0f);
            rect.offsetMax = Vector2.zero;

            _detail.SellRequested += Submit;
        }

        private void Submit(MarketSellableItem item, int count, int price)
        {
            _market.Sell(item.Source, item.Id, count, price);
            _detail.SetBusy(true);
        }
    }
}
