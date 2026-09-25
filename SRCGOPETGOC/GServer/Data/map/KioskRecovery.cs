using System.Collections.Concurrent;
using Dapper;
using Gopet.Data.GopetItem;
using Gopet.Util;
using MySqlConnector;
using Newtonsoft.Json;

namespace Gopet.Data.Map
{
    /// <summary>
    /// Trả đồ/tiền ki ốt hết hạn (bảng <c>kiosk_recovery</c>) về cho người bán.
    /// Expire tick (Runtime thread) chỉ INSERT recovery rồi <see cref="MarkPending"/>; việc cộng vào túi
    /// luôn chạy trên thread xử lý gói tin của chính player (<see cref="DeliverIfPending"/> gọi từ
    /// GameController.onMessage) hoặc lúc login — tránh đua với subCountItem/addItem của player.
    /// </summary>
    public static class KioskRecovery
    {
        private static readonly ConcurrentDictionary<int, byte> _pending = new();

        /// <summary>Đánh dấu user đang online có recovery mới; gói tin kế tiếp của họ sẽ nhận.</summary>
        public static void MarkPending(int userId) => _pending[userId] = 0;

        /// <summary>Gọi trên thread của player: nếu có recovery chờ thì nhận ngay và báo cho người chơi.</summary>
        public static void DeliverIfPending(Player player)
        {
            if (player?.user == null || player.playerData == null || !_pending.TryRemove(player.user.user_id, out _))
            {
                return;
            }
            try
            {
                int delivered;
                using (var conn = MYSQLManager.create())
                {
                    delivered = Deliver(player, conn);
                }
                if (delivered > 0)
                {
                    player.playerData.save();
                    player.okDialog(player.Language.KioskExpiredReturned);
                }
            }
            catch (Exception e)
            {
                // Để lại cờ để lần sau thử lại; dòng recovery vẫn còn trong DB nên không mất đồ.
                MarkPending(player.user.user_id);
                e.printStackTrace();
            }
        }

        /// <summary>
        /// Nhận toàn bộ dòng recovery của player: trả đồ/pet, cộng <see cref="KioskPayout.SellerShare"/>
        /// của sumVal gốc, rồi xoá ĐÚNG dòng vừa xử lý (không DELETE theo user_id — tick có thể vừa
        /// INSERT dòng mới sau câu SELECT). Trả về số dòng đã nhận.
        /// </summary>
        public static int Deliver(Player player, MySqlConnection conn)
        {
            int userId = player.user.user_id;
            var rows = conn.Query("SELECT * FROM `kiosk_recovery` where user_id = @user_id", new { user_id = userId }).ToList();
            foreach (var row in rows)
            {
                SellItem sellItem = JsonConvert.DeserializeObject<SellItem>(row.item);
                if (sellItem.pet == null)
                {
                    player.addItemToInventory(sellItem.ItemSell);
                }
                else
                {
                    player.playerData.addPet(sellItem.pet, player);
                }
                if (sellItem.sumVal > 0)
                {
                    player.addCoin(KioskPayout.SellerShare(sellItem.sumVal));
                }
                conn.Execute("DELETE FROM `kiosk_recovery` WHERE `kioskType` = @kioskType AND `user_id` = @user_id AND `item` = @item LIMIT 1",
                    new { kioskType = Convert.ToSByte(row.kioskType), user_id = userId, item = (string)row.item });
            }
            return rows.Count;
        }
    }
}
