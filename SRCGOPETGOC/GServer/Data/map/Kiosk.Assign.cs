using Dapper;
using Gopet.Data.GopetItem;

namespace Gopet.Data.Map
{
    public partial class Kiosk
    {
        /// <summary>
        /// Đặt/đổi/xoá người được chỉ định mua riêng cho 1 listing. Tên rỗng = bỏ chỉ định
        /// (miễn phí). Đổi sang tên khác tên hiện tại thu phí vàng
        /// <see cref="KioskPayout.AssignFee"/> (pet 15.000, đồ khác 10.000).
        /// </summary>
        public KioskResult TrySetAssignedName(Player player, int listingId, string name)
        {
            SellItem sellItem = searchItem(listingId);
            if (sellItem == null)
            {
                return KioskResult.Fail(player.Language.ItemWasSell);
            }
            if (sellItem.user_id != player.user.user_id)
            {
                return KioskResult.Fail(player.Language.KioskNotOwner);
            }

            lock (sellItem.Sync)
            {
                if (sellItem.hasSell || sellItem.hasRemoved)
                {
                    return KioskResult.Fail(player.Language.ItemWasSell);
                }

                name = name?.Trim();
                bool goldCharged = false;
                if (string.IsNullOrEmpty(name))
                {
                    sellItem.AssignedName = null;
                }
                else
                {
                    // So sánh không phân biệt hoa/thường — "Abc" và "abc" là cùng 1 người chơi,
                    // trước đây so bằng == nên tự-chỉ-định-mình-khác-hoa-thường lọt qua, và đổi
                    // thuần hoa/thường bị tính là "đổi người" nên bị thu phí oan.
                    if (string.Equals(name, player.playerData.name, StringComparison.OrdinalIgnoreCase))
                    {
                        return KioskResult.Fail(player.Language.KioskCannotAssignSelf);
                    }
                    if (!string.Equals(name, sellItem.AssignedName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!PlayerNameExists(name))
                        {
                            return KioskResult.Fail(player.Language.PlayerNotFound);
                        }
                        int fee = KioskPayout.AssignFee(sellItem.pet != null);
                        if (!player.checkGold(fee))
                        {
                            return KioskResult.Fail(player.Language.NotEnoughGold);
                        }
                        player.mineGold(fee);
                        goldCharged = true;
                    }
                    sellItem.AssignedName = name;
                }

                if (goldCharged)
                {
                    player.playerData.save();
                }
            }

            GopetManager.SaveMarketNow();
            return KioskResult.Success(player.Language.UpdateOK);
        }

        /// <summary>Dùng lại ở <see cref="MenuController.SellKioskItem"/> (luồng NPC treo mới + chỉ
        /// định) để validate tên trước khi thu phí — tránh mất vàng oan nếu tên không tồn tại.</summary>
        public static bool PlayerNameExists(string name)
        {
            if (PlayerManager.get(name) != null)
            {
                return true;
            }
            using var conn = MYSQLManager.create();
            return conn.QueryFirstOrDefault<int?>("SELECT `user_id` FROM `player` WHERE `name` = @name LIMIT 1", new { name }) != null;
        }
    }
}
