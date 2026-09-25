using Gopet.Data.GopetItem;
using Gopet.Data.item;
using Gopet.Data.Market;

namespace Gopet.Data.Map
{
    public partial class Kiosk
    {
        /// <summary>
        /// Treo bán 1 vật phẩm/pet lên đúng ki ốt suy ra từ (source, loại template) — xem
        /// <see cref="MarketItemCategory"/>. Validate lại toàn bộ từ dữ liệu server hiện tại
        /// (tìm lại theo id, không dùng object client cache) rồi mới trừ khỏi túi đồ + thêm
        /// listing. Không tự gửi dialog.
        /// </summary>
        public static KioskResult TryList(Player player, ListRequest req, string assignedName = null)
        {
            if (Maintenance.gI().isIsMaintenance())
            {
                return KioskResult.Fail(player.Language.KioskMaintenanceBlockList);
            }
            if (req.Price < KioskPayout.MinPrice || req.Price > KioskPayout.MaxPrice)
            {
                return KioskResult.Fail(player.Language.KioskInvalidPrice);
            }
            if (req.Count < 1)
            {
                return KioskResult.Fail(player.Language.KioskInvalidCount);
            }

            return req.Source == MarketItemCategory.SourcePet
                ? TryListPet(player, req, assignedName)
                : TryListItem(player, req, assignedName);
        }

        private static KioskResult TryListItem(Player player, ListRequest req, string assignedName)
        {
            Item item = player.controller.selectItemByItemId(req.ItemOrPetId, req.Source);
            if (item == null)
            {
                return KioskResult.Fail(player.Language.KioskItemNotFound);
            }
            // BlockReason gom đủ canTrade/petEuipId/gemInfo/expire — dùng chung với SELLABLE
            // (popup Đăng bán) để 2 luồng từ chối/chấp nhận nhất quán, và bắt luôn đồ hết hạn
            // (trước đây TryListItem không kiểm expire, chỉ SELLABLE kiểm nên treo lệch).
            string blockReason = MarketItemCategory.BlockReason(player, item);
            if (blockReason != null)
            {
                return KioskResult.Fail(blockReason);
            }
            // Đồ không thể xếp chồng (vd ngọc vừa tháo ra) mặc định count=0 vì không có ý nghĩa
            // số lượng thật — coi như 1 đơn vị để không bị từ chối oan bởi check count bên dưới.
            int availableCount = item.Template.isStackable ? item.count : 1;
            if (req.Count > availableCount)
            {
                return KioskResult.Fail(player.Language.KioskInvalidCount);
            }

            sbyte? kioskType = MarketItemCategory.ResolveKioskType(req.Source, item.Template.getType());
            Kiosk target = kioskType.HasValue ? MarketPlace.getKiosk(kioskType.Value) : null;
            if (target == null)
            {
                return KioskResult.Fail(player.Language.KioskInvalidCategory);
            }

            Item toList;
            if (!item.Template.isStackable || req.Count >= item.count)
            {
                player.playerData.removeItem(req.Source, item);
                toList = item;
            }
            else
            {
                player.controller.subCountItem(item, req.Count, req.Source);
                toList = new Item(item.itemTemplateId) { count = req.Count };
                toList.SourcesItem.Add(ItemSource.COPY_PHI_CHỢ);
                // Bản copy phải giữ expire/canTrade của item gốc — thiếu 2 trường này thì đồ hết
                // hạn/khóa bị copy-split (bán 1 phần từ 1 cụm) sẽ mất trạng thái, bán được sai luật.
                toList.expire = item.expire;
                toList.canTrade = item.canTrade;
            }

            SellItem listing = target.addKioskItem(toList, req.Price, player, assignedName);
            // Item đã trừ khỏi túi (removeItem/subCountItem ở trên) — lưu player TRƯỚC market để
            // nếu crash giữa 2 lần lưu, tệ nhất là mất đồ (đã hết trong túi + chưa kịp lên chợ)
            // chứ không nhân đôi (đồ vẫn còn trong túi DB cũ + đã lên chợ).
            player.playerData.save();
            GopetManager.SaveMarketNow();
            return KioskResult.Success(player.Language.UpdateOK, listing);
        }

        private static KioskResult TryListPet(Player player, ListRequest req, string assignedName)
        {
            Pet pet = player.controller.selectPetByItemId(req.ItemOrPetId);
            if (pet == null)
            {
                return KioskResult.Fail(player.Language.KioskItemNotFound);
            }
            string blockReason = MarketItemCategory.BlockReason(player, pet);
            if (blockReason != null)
            {
                return KioskResult.Fail(blockReason);
            }

            Kiosk target = MarketPlace.getKiosk(GopetManager.KIOSK_PET);
            player.playerData.pets.remove(pet);
            SellItem listing = target.addKioskItem(pet, req.Price, player, assignedName);
            player.playerData.save();
            GopetManager.SaveMarketNow();
            return KioskResult.Success(player.Language.UpdateOK, listing);
        }
    }
}
