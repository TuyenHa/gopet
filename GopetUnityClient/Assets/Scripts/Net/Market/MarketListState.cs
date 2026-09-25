using System;
using System.Collections.Generic;

namespace Gopet.Net.Market
{
    /// <summary>
    /// TYPE_MARKET_LIST_STATE (sub 48) — trang kết quả tab Chợ: filter/sort/page hiện
    /// tại (server kẹp lại nếu client gửi giá trị vô lý), tổng số trang và tối đa 5 dòng.
    ///
    /// <para>Field order khớp <c>MarketService.SendListState</c>
    /// (<c>GServer/Data/Market/MarketService.cs:66-83</c>).</para>
    /// </summary>
    public sealed class MarketListState
    {
        public sbyte Filter;
        public sbyte Sort;
        public int Page;
        public int TotalPages;
        public IReadOnlyList<MarketListingRow> Rows = Array.Empty<MarketListingRow>();

        public static MarketListState Parse(Message message)
        {
            var r = message.Reader;
            var state = new MarketListState
            {
                Filter = r.ReadSByte(),
                Sort = r.ReadSByte(),
                Page = r.ReadShort(),
                TotalPages = r.ReadShort(),
            };

            var count = r.ReadSByte();
            if (count < 0) throw new ProtocolException($"TYPE_MARKET_LIST_STATE count âm: {count}.");
            var rows = new MarketListingRow[count];
            for (var i = 0; i < count; i++) rows[i] = MarketListingRow.Parse(r);
            r.ExpectFullyConsumed("TYPE_MARKET_LIST_STATE");

            state.Rows = rows;
            return state;
        }
    }

    /// <summary>
    /// TYPE_MARKET_MINE_STATE (sub 51) — toàn bộ món người xem đang treo, không phân
    /// trang. Field order khớp <c>MarketService.SendMineState</c>
    /// (<c>GServer/Data/Market/MarketService.cs:85-105</c>).
    /// </summary>
    public sealed class MarketMineState
    {
        public IReadOnlyList<MarketListingRow> Rows = Array.Empty<MarketListingRow>();

        public static MarketMineState Parse(Message message)
        {
            var r = message.Reader;
            var count = r.ReadShort();
            if (count < 0) throw new ProtocolException($"TYPE_MARKET_MINE_STATE count âm: {count}.");
            var rows = new MarketListingRow[count];
            for (var i = 0; i < count; i++) rows[i] = MarketListingRow.Parse(r);
            r.ExpectFullyConsumed("TYPE_MARKET_MINE_STATE");

            return new MarketMineState { Rows = rows };
        }
    }
}
