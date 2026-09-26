using System.Collections.Generic;
using Gopet.Data.GopetItem;
using Gopet.Data.Map;
using Gopet.IO;
using Gopet.Util;

namespace Gopet.Data.Market
{
    /// <summary>
    /// Nối protocol popup Chợ trời (COMMAND_GUIDER sub 47..57) với dữ liệu kiosk thật
    /// (<see cref="MarketPlace.kiosks"/>, dùng chung với NPC map 22). Không giới hạn map đứng.
    /// List/Buy/Cancel/Assign ở đây; Sellable/Sell tách sang MarketService.Sellable.cs.
    /// Xem plans/260925-2253-cho-troi-market-popup/phase-02-server-market-protocol.md.
    /// </summary>
    public static partial class MarketService
    {
        private const sbyte ActionBuy = 1;
        private const sbyte ActionCancel = 2;
        private const sbyte ActionSell = 3;
        private const sbyte ActionAssign = 4;

        // Key riêng trong objectPerformed (per-player) để nhớ filter/sort/page lần LIST gần nhất,
        // dùng lại sau khi BUY để gửi lại đúng trang cho client — tránh trùng OBJKEY_* của MenuController.
        private const int ObjKeyListState = 4_812_047;
        private static readonly (sbyte Filter, sbyte Sort, short Page) DefaultListState = (-1, 0, 0);

        // Rate-limit key riêng cho sub 47 (LIST) và 53 (SELLABLE) — 2 sub không có race/lock bảo
        // vệ như buy/cancel nên spam gói liên tục (macro/bot) tốn CPU quét+serialize toàn bộ chợ
        // hoặc toàn bộ túi đồ mỗi lần. 300ms đủ mượt cho thao tác tay (đổi filter/mở popup) nhưng
        // chặn được spam packet.
        private const int ObjKeyListRateLimitMs = 4_812_050;
        private const int ObjKeySellableRateLimitMs = 4_812_051;
        private const long RateLimitMs = 300;

        /// <summary>True nếu đã đủ lâu kể từ lần gọi trước (hoặc chưa từng gọi) — đồng thời cập
        /// nhật mốc thời gian cho lần này. Dùng chung cho HandleList/HandleSellable.</summary>
        private static bool TryConsumeRateLimit(Player player, int objKey)
        {
            long now = Utilities.CurrentTimeMillis;
            if (player.controller.objectPerformed.TryGetValue(objKey, out dynamic lastRaw) && lastRaw is long last && now - last < RateLimitMs)
            {
                return false;
            }
            player.controller.objectPerformed[objKey] = now;
            return true;
        }

        public static void HandleList(Player player, sbyte filter, sbyte sort, short page)
        {
            if (!TryConsumeRateLimit(player, ObjKeyListRateLimitMs)) return;
            filter = ClampFilter(filter);
            sort = ClampSort(sort);
            page = ClampPage(page);
            SetListState(player, filter, sort, page);
            SendListState(player, filter, sort, page);
        }

        public static void HandleBuy(Player player, sbyte kioskType, int listingId)
        {
            Kiosk kiosk = MarketPlace.getKiosk(kioskType);
            KioskResult result = kiosk == null ? KioskResult.Fail(player.Language.KioskInvalidCategory) : kiosk.TryBuyWhole(player, listingId);
            SendResult(player, ActionBuy, result.Ok, result.Message);

            var state = GetListState(player);
            SendListState(player, state.Filter, state.Sort, state.Page);
        }

        public static void HandleMine(Player player)
        {
            SendMineState(player);
        }

        public static void HandleCancel(Player player, sbyte kioskType, int listingId)
        {
            Kiosk kiosk = MarketPlace.getKiosk(kioskType);
            KioskResult result = kiosk == null ? KioskResult.Fail(player.Language.KioskInvalidCategory) : kiosk.TryCancel(player, listingId);
            SendResult(player, ActionCancel, result.Ok, result.Message);
            SendMineState(player);
        }

        public static void HandleAssign(Player player, sbyte kioskType, int listingId, string buyerName)
        {
            Kiosk kiosk = MarketPlace.getKiosk(kioskType);
            KioskResult result = kiosk == null ? KioskResult.Fail(player.Language.KioskInvalidCategory) : kiosk.TrySetAssignedName(player, listingId, buyerName);
            SendResult(player, ActionAssign, result.Ok, result.Message);
            SendMineState(player);
        }

        private static void SendListState(Player player, sbyte filter, sbyte sort, short page)
        {
            MarketQuery.Result result = MarketQuery.Page(AllListings(), player.user.user_id, player.playerData.name, filter, sort, page);

            Message m = new Message(GopetCMD.COMMAND_GUIDER);
            m.putsbyte(GopetCMD.TYPE_MARKET_LIST_STATE);
            m.putsbyte(filter);
            m.putsbyte(sort);
            m.putShort((short)result.Page);
            m.putShort((short)result.TotalPages);
            m.putsbyte((sbyte)result.Rows.Count);
            foreach (MarketListing row in result.Rows)
            {
                MarketPacketWriter.WriteRow(m, row, player);
            }
            m.cleanup();
            player.session.sendMessage(m);
        }

        private static void SendMineState(Player player)
        {
            List<MarketListing> mine = new();
            foreach (MarketListing listing in AllListings())
            {
                if (listing.Item.user_id == player.user.user_id)
                {
                    mine.Add(listing);
                }
            }

            Message m = new Message(GopetCMD.COMMAND_GUIDER);
            m.putsbyte(GopetCMD.TYPE_MARKET_MINE_STATE);
            m.putShort((short)mine.Count);
            foreach (MarketListing row in mine)
            {
                MarketPacketWriter.WriteRow(m, row, player);
            }
            m.cleanup();
            player.session.sendMessage(m);
        }

        private static void SendResult(Player player, sbyte action, bool ok, string message)
        {
            Message m = new Message(GopetCMD.COMMAND_GUIDER);
            m.putsbyte(GopetCMD.TYPE_MARKET_RESULT);
            m.putsbyte(action);
            m.putbool(ok);
            m.putUTF(message ?? string.Empty);
            m.cleanup();
            player.session.sendMessage(m);
        }

        private static IEnumerable<MarketListing> AllListings()
        {
            foreach (Kiosk kiosk in MarketPlace.kiosks)
            {
                foreach (SellItem item in kiosk.kioskItems)
                {
                    yield return new MarketListing(kiosk.kioskType, item);
                }
            }
        }

        private static sbyte ClampFilter(sbyte filter) => filter >= -1 && filter <= GopetManager.KIOSK_OTHER ? filter : (sbyte)-1;
        private static sbyte ClampSort(sbyte sort) => sort is >= 0 and <= 2 ? sort : (sbyte)0;
        private static short ClampPage(short page) => page < 0 ? (short)0 : page;

        private static (sbyte Filter, sbyte Sort, short Page) GetListState(Player player)
        {
            if (player.controller.objectPerformed.TryGetValue(ObjKeyListState, out dynamic cached) && cached is ValueTuple<sbyte, sbyte, short> state)
            {
                return state;
            }
            return DefaultListState;
        }

        private static void SetListState(Player player, sbyte filter, sbyte sort, short page)
        {
            player.controller.objectPerformed[ObjKeyListState] = (filter, sort, page);
        }
    }
}
