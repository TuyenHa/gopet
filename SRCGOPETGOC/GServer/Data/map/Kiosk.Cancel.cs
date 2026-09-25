using Gopet.Data.GopetItem;
using Gopet.Util;

namespace Gopet.Data.Map
{
    public partial class Kiosk
    {
        /// <summary>
        /// Gỡ listing của chính mình về túi đồ, hoàn 95% <c>sumVal</c> đã bán lẻ dở (nếu có).
        /// Owner check + lock theo <see cref="SellItem.Sync"/> để loại trừ với buy/expire đang
        /// chạy song song trên cùng listing — chỉ 1 trong 3 luồng được xử lý.
        /// </summary>
        public KioskResult TryCancel(Player player, int listingId)
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
                if (sellItem.hasSell)
                {
                    return KioskResult.Fail(player.Language.ItemWasSell);
                }
                if (sellItem.hasRemoved)
                {
                    kioskItems.remove(sellItem);
                    return KioskResult.Fail(player.Language.ItemWasSell);
                }

                sellItem.hasRemoved = true;
                kioskItems.remove(sellItem);

                if (sellItem.pet != null)
                {
                    player.playerData.addPet(sellItem.pet, player);
                }
                else
                {
                    player.addItemToInventory(sellItem.ItemSell);
                }

                KioskPayout.PaySeller(sellItem.user_id, sellItem.sumVal);
                HistoryManager.addHistory(new History(player).setLog(Utilities.Format("Gỡ vật phẩm về túi thành công", sellItem.getName(player))).setObj(sellItem));
            }

            // PaySeller chỉ lưu player khi sumVal > 0 (return sớm nếu = 0, trường hợp phổ biến
            // nhất khi gỡ) — lưu lại ở đây để đồ/pet vừa addItemToInventory/addPet luôn được
            // persist bất kể có tiền bán dở hay không.
            player.playerData.save();
            GopetManager.SaveMarketNow();
            return KioskResult.Success(player.Language.CancelItemKiosk);
        }
    }
}
