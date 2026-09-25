using Dapper;
using MySqlConnector;

namespace Gopet.Data.Map
{
    /// <summary>
    /// Công thức tiền/phí dùng chung cho mọi luồng ki ốt (mua trọn, hủy bán dở, hết hạn
    /// online/offline, chỉ định người mua). Gom về 1 chỗ để tránh mỗi nơi tính thuế 1 kiểu —
    /// trước bản vá này Kiosk.update() tính rồi bỏ (mất tiền), Player.cs cộng 100% sumVal lúc
    /// offline recovery (không trừ thuế), còn cancel/buy lại trừ đúng.
    /// </summary>
    public static class KioskPayout
    {
        public const int MinPrice = 1;
        public const int MaxPrice = 2_000_000_000;

        /// <summary>Phần người bán nhận được sau khi trừ thuế <see cref="GopetManager.KIOSK_PER_SELL"/>%.</summary>
        public static long SellerShare(long grossValue)
        {
            if (grossValue <= 0) return 0;
            return (long)Math.Round(grossValue / 100.0 * (100 - GopetManager.KIOSK_PER_SELL));
        }

        /// <summary>Phí vàng để chỉ định người mua duy nhất cho 1 listing.</summary>
        public static int AssignFee(bool isPet)
        {
            return isPet ? GopetManager.PRICE_ASSIGNED_PET : GopetManager.PRICE_ASSIGNED_ITEM;
        }

        /// <summary>
        /// Trả <see cref="SellerShare"/> của <paramref name="grossValue"/> cho người bán
        /// <paramref name="sellerId"/>: cộng thẳng nếu đang online, UPDATE DB nếu offline.
        /// Dùng chung cho TryBuyWhole/TryCancel/expire (buy trả 95% price, cancel/expire trả 95% sumVal).
        /// </summary>
        public static void PaySeller(int sellerId, long grossValue)
        {
            long share = SellerShare(grossValue);
            if (share <= 0) return;

            Player sellPlayer = PlayerManager.get(sellerId);
            if (sellPlayer != null)
            {
                sellPlayer.addCoin(share);
                sellPlayer.playerData.save();
                return;
            }

            using var conn = MYSQLManager.create();
            conn.Execute("Update `player` set coin = coin + @share where user_id = @user_id",
                new { share, user_id = sellerId });
        }
    }
}
