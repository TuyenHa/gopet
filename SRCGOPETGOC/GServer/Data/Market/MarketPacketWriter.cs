using Gopet.Data.GopetItem;
using Gopet.IO;
using Gopet.Util;

namespace Gopet.Data.Market
{
    /// <summary>
    /// Ghi Row (48/51) và SellableRow (54) cho popup Chợ trời. Tên/icon dùng lại đúng nguồn mà
    /// <c>MenuController.sendMenu</c> MENU_KIOSK_* đang dùng để 2 luồng hiển thị giống hệt nhau.
    /// Xem plans/260925-2253-cho-troi-market-popup/phase-02.
    /// </summary>
    public static class MarketPacketWriter
    {
        private const int MaxDescLength = 120;

        /// <summary>Row: kioskType, listingId, isMine, name, iconPath, price, count, sellerName, secondsLeft, desc, assignedName.</summary>
        public static void WriteRow(Message m, MarketListing listing, Player viewer)
        {
            SellItem sellItem = listing.Item;
            m.putsbyte(listing.KioskType);
            m.putInt(sellItem.itemId);
            m.putbool(sellItem.user_id == viewer.user.user_id);
            m.putUTF(ResolveName(listing.KioskType, sellItem, viewer));
            m.putUTF(ResolveIconPath(sellItem));
            // Giá còn phải trả, KHÔNG phải MathPrice (đơn_giá × số_lượng_còn_lại) — TryBuyWhole
            // trừ đúng RemainingPrice, dùng field khác ở đây từng làm hiện 1 giá/trừ giá khác cho
            // listing bán lẻ dở (bug #6).
            m.putlong(sellItem.RemainingPrice);
            m.putInt(sellItem.pet != null ? 1 : sellItem.ItemSell.count);
            m.putUTF(sellItem.GetSellerDisplayName());
            m.putInt(SecondsLeft(sellItem));
            m.putUTF(Truncate(ResolveDesc(sellItem, viewer)));
            m.putUTF(sellItem.AssignedName ?? string.Empty);
        }

        /// <summary>SellableRow: source, id, name, iconPath, count, tradable, blockReason, desc.</summary>
        public static void WriteSellableRow(Message m, sbyte source, int id, string name, string iconPath, int count, string blockReason, string desc)
        {
            m.putsbyte(source);
            m.putInt(id);
            m.putUTF(name);
            m.putUTF(iconPath);
            // Đồ không xếp chồng (vd ngọc vừa tháo ra khỏi trang bị) mặc định count=0 vì không
            // có ý nghĩa số lượng thật — kẹp về 1 để client không disable nút bán mà không rõ lý do.
            m.putInt(Math.Max(1, count));
            m.putbool(string.IsNullOrEmpty(blockReason));
            m.putUTF(blockReason ?? string.Empty);
            m.putUTF(Truncate(desc));
        }

        private static string ResolveName(sbyte kioskType, SellItem sellItem, Player viewer)
        {
            if (sellItem.pet != null)
            {
                return sellItem.pet.getNameWithStar(viewer);
            }
            return kioskType == GopetManager.KIOSK_OTHER ? sellItem.ItemSell.getName(viewer) : sellItem.ItemSell.getEquipName(viewer);
        }

        private static string ResolveIconPath(SellItem sellItem)
        {
            return sellItem.pet != null ? sellItem.pet.getPetTemplate().icon : sellItem.getFrameImgPath();
        }

        private static string ResolveDesc(SellItem sellItem, Player viewer)
        {
            return sellItem.pet != null ? sellItem.pet.getDesc(viewer) : sellItem.ItemSell.getTemp().getDescription(viewer);
        }

        private static int SecondsLeft(SellItem sellItem)
        {
            long remainMs = sellItem.expireTime - Utilities.CurrentTimeMillis;
            return remainMs <= 0 ? 0 : (int)(remainMs / 1000);
        }

        private static string Truncate(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= MaxDescLength)
            {
                return text ?? string.Empty;
            }
            return text.Substring(0, MaxDescLength);
        }
    }
}
