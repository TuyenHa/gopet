using Gopet.Data.GopetItem;

namespace Gopet.Data.Map
{
    public partial class Kiosk
    {
        public void buy(int itemId, Player player)
        {
            if (Maintenance.gI().isIsMaintenance())
            {
                player.redDialog(player.Language.CannotBuyThisItemByMaintenance);
                return;
            }
            SellItem sellItem = searchItem(itemId);
            if (sellItem == null)
            {
                player.redDialog(player.Language.ItemWasSell);
                return;
            }
            if (!string.IsNullOrEmpty(sellItem.AssignedName) && !string.Equals(sellItem.AssignedName, player.playerData.name, StringComparison.OrdinalIgnoreCase))
            {
                player.redDialog(string.Format(player.Language.KioskAssignedToOther, sellItem.AssignedName));
                return;
            }
            if (sellItem.user_id == player.user.user_id)
            {
                player.redDialog(player.Language.CannotBuyThisItemOfYourself);
                return;
            }
            player.controller.objectPerformed.put(MenuController.OBJKEY_KIOSK_ITEM, new KeyValuePair<Kiosk, SellItem>(this, sellItem));
            if (!sellItem.IsRetail)
            {
                MenuController.showYNDialog(MenuController.DIALOG_CONFIRM_BUY_KIOSK_ITEM, player.Language.DoYouWantBuyIt, player);
            }
            else
            {
                MenuController.sendMenu(MenuController.MENU_OPTION_BUY_KIOSK_ITEM, player);
            }
        }

        public void buyRetail(int itemId, Player player, int count)
        {
            if (count <= 0)
            {
                player.redDialog(player.Language.BugWarning);
                return;
            }
            if (Maintenance.gI().isIsMaintenance())
            {
                player.redDialog(player.Language.CannotBuyThisItemByMaintenance);
                return;
            }
            SellItem sellItem = searchItem(itemId);
            if (sellItem == null)
            {
                player.redDialog(player.Language.ItemWasSell);
                return;
            }
            if (sellItem.user_id == player.user.user_id)
            {
                player.redDialog(player.Language.CannotBuyThisItemOfYourself);
                return;
            }
            if (!string.IsNullOrEmpty(sellItem.AssignedName) && !string.Equals(sellItem.AssignedName, player.playerData.name, StringComparison.OrdinalIgnoreCase))
            {
                player.redDialog(string.Format(player.Language.KioskAssignedToOther, sellItem.AssignedName));
                return;
            }
            if (sellItem.pet != null)
            {
                return;
            }
            if (sellItem.ItemSell.count == count)
            {
                confirmBuy(player, sellItem);
                return;
            }

            lock (sellItem.Sync)
            {
                if (sellItem.hasSell || sellItem.hasRemoved || !kioskItems.Contains(sellItem))
                {
                    player.redDialog(player.Language.ItemWasSell);
                    return;
                }
                if (count > sellItem.ItemSell.count)
                {
                    player.redDialog(player.Language.NotEnoughItemToBuy);
                    return;
                }
                long priceRetail = Math.Max(1, sellItem.price) / sellItem.TotalCount;
                long price = Math.Max(1, priceRetail * count);
                if (!player.TrySpendCoin(price))
                {
                    player.redDialog(player.Language.NotEnoughCoin);
                    return;
                }
                sellItem.sumVal += price;
                sellItem.ItemSell.count -= count;
                player.addItemToInventory(new Item(sellItem.ItemSell.itemTemplateId, count));
                player.okDialog(player.Language.BuyOK);
                HistoryManager.addHistory(new History(player.playerData.user_id).setObj(sellItem).setLog($"Mua thành công {sellItem.getName(player)} có sớ lượng {count}"));
            }
            // Lưu người mua (đã trừ tiền + nhận đồ) TRƯỚC market — cùng thứ tự với các mutation
            // khác, tránh crash giữa chừng làm mất tiền mà không nhận được đồ hoặc ngược lại.
            player.playerData.save();
            GopetManager.SaveMarketNow();
        }

        public void confirmBuy(Player player, SellItem sellItem)
        {
            KioskResult result = TryBuyWhole(player, sellItem.itemId);
            if (result.Ok)
            {
                player.okDialog(result.Message);
            }
            else
            {
                player.redDialog(result.Message);
            }
        }

        /// <summary>
        /// Mua trọn 1 listing (đồ nguyên cụm hoặc pet). Lock theo <see cref="SellItem.Sync"/> và
        /// kiểm tra-rồi-đặt <c>hasSell</c> để loại trừ với cancel/expire đang chạy song song
        /// trên cùng listing — chỉ 1 trong 3 luồng được xử lý.
        /// </summary>
        public KioskResult TryBuyWhole(Player buyer, int listingId)
        {
            if (Maintenance.gI().isIsMaintenance())
            {
                return KioskResult.Fail(buyer.Language.CannotBuyThisItemByMaintenance);
            }

            SellItem sellItem = searchItem(listingId);
            if (sellItem == null)
            {
                return KioskResult.Fail(buyer.Language.ItemWasSell);
            }
            if (sellItem.user_id == buyer.user.user_id)
            {
                return KioskResult.Fail(buyer.Language.CannotBuyThisItemOfYourself);
            }
            if (!string.IsNullOrEmpty(sellItem.AssignedName) && !string.Equals(sellItem.AssignedName, buyer.playerData.name, StringComparison.OrdinalIgnoreCase))
            {
                return KioskResult.Fail(string.Format(buyer.Language.KioskAssignedToOther, sellItem.AssignedName));
            }

            lock (sellItem.Sync)
            {
                if (sellItem.hasSell || sellItem.hasRemoved || !kioskItems.Contains(sellItem))
                {
                    return KioskResult.Fail(buyer.Language.ItemWasSell);
                }

                long outstanding = sellItem.RemainingPrice;
                if (!buyer.TrySpendCoin(outstanding))
                {
                    return KioskResult.Fail(buyer.Language.NotEnoughCoin);
                }

                sellItem.setHasSell(true);
                kioskItems.remove(sellItem);

                if (sellItem.ItemSell != null)
                {
                    buyer.addItemToInventory(sellItem.ItemSell);
                }
                else
                {
                    buyer.playerData.addPet(sellItem.pet, buyer);
                }

                KioskPayout.PaySeller(sellItem.user_id, sellItem.price);
                HistoryManager.addHistory(new History(sellItem.user_id).setObj(sellItem).setLog("Bán thành công vật phẩm trong ki ốt người mua là " + buyer.playerData.name));
                HistoryManager.addHistory(new History(buyer.playerData.user_id).setObj(sellItem).setLog($"Mua thành công {sellItem.getName(buyer)}"));
            }

            // PaySeller ở trên đã tự lưu người bán nếu online. Lưu thêm người mua (đã trừ tiền +
            // nhận đồ) rồi mới lưu market — seller save → buyer save → market save, đúng thứ tự
            // "lưu hết người liên quan trước, market sau" để tránh crash giữa chừng gây dupe.
            buyer.playerData.save();
            GopetManager.SaveMarketNow();
            return KioskResult.Success(buyer.Language.BuyOK);
        }
    }
}
