using Gopet.Data.Collections;
using Gopet.Data.GopetItem;
using Gopet.Util;

namespace Gopet.Data.Map
{

    /// <summary>
    /// 1 sạp bán (1 trong 6 loại: nón/vũ khí/giáp/ngọc/pet/vật phẩm khác). Chứa dữ liệu +
    /// thao tác thêm/tìm listing. Thao tác nghiệp vụ (treo/mua/hủy/chỉ định/hết hạn) tách sang
    /// các file partial để mỗi file dưới 200 dòng:
    /// <see cref="Kiosk"/> (đây, dữ liệu + addKioskItem/searchItem),
    /// Kiosk.List.cs (TryList), Kiosk.Trade.cs (buy/buyRetail/confirmBuy/TryBuyWhole),
    /// Kiosk.Assign.cs (TrySetAssignedName), Kiosk.Expire.cs (update/expire).
    /// </summary>
    public partial class Kiosk
    {

        public sbyte kioskType { get; set; }

        public CopyOnWriteArrayList<SellItem> kioskItems = new();

        public Kiosk(sbyte kioskType_)
        {
            kioskType = kioskType_;
        }

        public SellItem addKioskItem(Item item, int price, Player player, string assignedName = null)
        {
            if (item == null)
            {
                throw new NullReferenceException("item is null");
            }
            if (!item.wasSell)
            {
                item.wasSell = true;
            }
            SellItem sellItem = new SellItem(item, price, GopetManager.HOUR_UPLOAD_ITEM)
            {
                AssignedName = assignedName
            };
            addKioskItem(sellItem, player);
            HistoryManager.addHistory(new History(player).setLog(Utilities.Format("Treo vật phẩm %s với giá %s ngoc", item.getTemp().getName(player), Utilities.FormatNumber(price))).setObj(item));
            return sellItem;
        }

        public SellItem addKioskItem(Pet pet, int price, Player player, string assignedName = null)
        {
            if (kioskType != GopetManager.KIOSK_PET)
            {
                return null;
            }
            if (!pet.wasSell)
            {
                pet.wasSell = true;
            }
            SellItem sellItem = new SellItem(price, pet, GopetManager.HOUR_UPLOAD_ITEM)
            {
                AssignedName = assignedName
            };
            addKioskItem(sellItem, player);
            HistoryManager.addHistory(new History(player).setLog(Utilities.Format("Treo pet %s với giá %s ngoc", pet.getPetTemplate().getName(player), Utilities.FormatNumber(price))).setObj(pet));
            return sellItem;
        }

        sealed class SellItemComparer : IComparer<SellItem>
        {
            public int Compare(SellItem obj1, SellItem obj2)
            {
                return obj1.itemId - obj2.itemId;
            }
        }

        private void addKioskItem(SellItem item, Player player)
        {
            item.user_id = player.user.user_id;
            item.SellerName = player.playerData.name;
            kioskItems.Add(item);
            while (true)
            {
                item.itemId = Utilities.nextInt(1, int.MaxValue - 2);
                bool flag = true;
                foreach (SellItem item1 in kioskItems)
                {
                    if (item1 != item)
                    {
                        if (item1.itemId == item.itemId)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            kioskItems.Sort(new SellItemComparer());
        }

        public SellItem searchItem(int itemId)
        {
            int left = 0;
            int right = kioskItems.Count - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                SellItem midItem = kioskItems.get(mid);
                if (midItem.itemId == itemId)
                {
                    return midItem;
                }
                if (midItem.itemId < itemId)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            return null;
        }

        public void setKioskItem(CopyOnWriteArrayList<SellItem> sellItem)
        {
            kioskItems = sellItem;
        }

        public SellItem getItemByUserId(int user_id)
        {
            foreach (SellItem kioskItem in kioskItems)
            {
                if (kioskItem.user_id == user_id)
                {
                    return kioskItem;
                }
            }
            return null;
        }

        public sbyte getKioskType()
        {
            return kioskType;
        }
    }
}
