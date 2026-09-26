using System;
using System.Collections.Generic;
using System.Globalization;
using Gopet.Net.Market;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>Tab "Chợ": 7 chip lọc + sort xoay vòng, 5 dòng/trang, pager, nút Mua.</summary>
    public sealed partial class MarketPopupView
    {
        private const float FilterGap = 4f;
        private const float PagerGap = 4f;

        private RectTransform _marketPanel;
        private MarketFilterBar _filterBar;
        private PopupItemList _marketList;
        private MarketPagerBar _pager;
        private readonly List<MarketListingRowView> _marketRows = new List<MarketListingRowView>();

        private sbyte _filter = -1;
        private sbyte _sort;
        private int _page;
        private int _totalPages = 1;

        private void BuildMarketTab(RectTransform content, float contentWidth)
        {
            _marketPanel = MakeTabPanel(content, "Tab_Market");

            _filterBar = MarketFilterBar.Create(_marketPanel, _font, contentWidth);
            _filterBar.FilterChanged += OnFilterChanged;
            _filterBar.SortChanged += OnSortChanged;

            _pager = MarketPagerBar.Create(_marketPanel, _font);
            _pager.PageRequested += OnPageRequested;

            var listArea = new GameObject("ListArea", typeof(RectTransform));
            listArea.transform.SetParent(_marketPanel, false);
            var areaRect = (RectTransform)listArea.transform;
            areaRect.anchorMin = Vector2.zero;
            areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(0f, MarketPagerBar.Height + PagerGap);
            areaRect.offsetMax = new Vector2(0f, -(MarketFilterBar.Height + FilterGap));

            _marketList = PopupItemList.Create(areaRect, _font);
            _marketList.ShowPlaceholder("Đang tải…");
        }

        private void OnFilterChanged(sbyte filter)
        {
            _filter = filter;
            _page = 0;
            RequestList();
        }

        private void OnSortChanged(sbyte sort)
        {
            _sort = sort;
            _page = 0;
            RequestList();
        }

        private void OnPageRequested(int page)
        {
            _page = page;
            RequestList();
        }

        private void RequestList() => _market.RequestList(_filter, _sort, (short)_page);

        private void ApplyListState(MarketListState state)
        {
            if (state == null) return;
            // Chỉ so filter/sort — KHÔNG so Page: server có thể tự clamp Page (vd mua hết món
            // cuối trang cuối làm TotalPages giảm) nên Page trả về lệch với _page đã gửi là bình
            // thường, không phải gói trễ. So cả Page ở đây từng làm rớt gói LIST_STATE hợp lệ,
            // danh sách đứng hình ở trang cũ đã hết hàng.
            if (state.Filter != _filter || state.Sort != _sort) return;
            _page = state.Page;

            _totalPages = Math.Max(1, state.TotalPages);
            _pager.SetPage(_page, _totalPages);
            RebuildMarketRows(state.Rows);
        }

        private void RebuildMarketRows(IReadOnlyList<MarketListingRow> rows)
        {
            foreach (var row in _marketRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _marketRows.Clear();

            for (var i = 0; i < rows.Count; i++)
            {
                var listing = rows[i];
                var view = MarketListingRowView.Create(_marketList.Rows, _font);
                view.Bind(listing, _assets, SellerText(listing));
                view.SetSeparatorVisible(i < rows.Count - 1);
                ((RectTransform)view.transform).anchoredPosition =
                    new Vector2(0f, -i * MarketListingRowView.Height);

                if (!listing.IsMine)
                {
                    var captured = listing;
                    view.AddButton("Mua", (MarketListingRowView.Height - 24f) / 2f, 24f,
                        () => TryBuy(captured));
                }

                _marketRows.Add(view);
            }

            _marketList.SetRowsHeight(rows.Count * MarketListingRowView.Height);
            _marketList.ShowPlaceholder(rows.Count == 0 ? "Chợ chưa có ai bày món nào." : null);
        }

        /// <summary>Đồ của mình luôn hiện "Người bán: Bạn"; đồ chỉ định cho mình còn hiện
        /// thêm nhãn — server đã lọc hết dòng chỉ định cho NGƯỜI KHÁC khỏi gói này rồi
        /// (<c>MarketQuery.IsVisibleToViewer</c>), nên còn AssignedName mà không phải
        /// IsMine nghĩa là chắc chắn đang chỉ định cho người xem.</summary>
        private static string SellerText(MarketListingRow row)
        {
            if (row.IsMine) return "Người bán: Bạn";
            var text = "Người bán: " + row.SellerName;
            return row.HasAssignedBuyer ? text + " · Chỉ định cho bạn" : text;
        }

        private void TryBuy(MarketListingRow row)
        {
            if (row.Price > _playerCoin)
            {
                Message?.Invoke($"Bạn cần {FormatCoin(row.Price)} ngọc để mua món này.");
                return;
            }

            void Send() => _market.Buy(row.KioskType, row.ListingId);

            if (ConfirmRequested != null)
            {
                ConfirmRequested(new MenuSelection.ConfirmPrompt
                {
                    Text = $"Mua {JarIconTokens.Strip(row.Name)} với giá {FormatCoin(row.Price)} ngọc?",
                    ConfirmLabel = "Mua",
                    CancelLabel = "Huỷ",
                }, Send);
            }
            else
            {
                Send();
            }
        }

        private static string FormatCoin(long value) =>
            value.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '.');
    }
}
