using Gopet.Data.GopetItem;
using Gopet.Util;

namespace Gopet.Data.Market
{
    /// <summary>
    /// Suy ra loại ki ốt (GopetManager.KIOSK_*) từ nguồn treo bán (source) và loại template
    /// của item/pet. Dùng chung cho <see cref="Gopet.Data.Map.Kiosk.TryList"/> (phase 1) và
    /// popup Đăng bán (phase 2) để tránh mỗi nơi tự lặp lại bảng ánh xạ này.
    /// </summary>
    public static class MarketItemCategory
    {
        /// <summary>Rương trang bị pet (nón/giáp/vũ khí/găng/giày) — GopetManager.EQUIP_PET_INVENTORY.</summary>
        public const sbyte SourceEquip = GopetManager.EQUIP_PET_INVENTORY;
        /// <summary>Túi đồ thường — GopetManager.NORMAL_INVENTORY.</summary>
        public const sbyte SourceNormal = GopetManager.NORMAL_INVENTORY;
        /// <summary>Túi ngọc — GopetManager.GEM_INVENTORY.</summary>
        public const sbyte SourceGem = GopetManager.GEM_INVENTORY;
        /// <summary>Pet: không phải 1 inventory thật, chỉ dùng để đánh dấu nguồn treo bán là pet.</summary>
        public const sbyte SourcePet = -1;

        /// <summary>Trả về loại ki ốt tương ứng, null nếu source/templateType không hợp lệ.</summary>
        public static sbyte? ResolveKioskType(sbyte source, int templateType)
        {
            switch (source)
            {
                case SourcePet:
                    return GopetManager.KIOSK_PET;
                case SourceGem:
                    return GopetManager.KIOSK_GEM;
                case SourceNormal:
                    return GopetManager.KIOSK_OTHER;
                case SourceEquip:
                    return ResolveEquipKioskType(templateType);
                default:
                    return null;
            }
        }

        private static sbyte? ResolveEquipKioskType(int templateType)
        {
            switch (templateType)
            {
                case GopetManager.PET_EQUIP_HAT:
                    return GopetManager.KIOSK_HAT;
                case GopetManager.PET_EQUIP_WEAPON:
                    return GopetManager.KIOSK_WEAPON;
                case GopetManager.PET_EQUIP_ARMOUR:
                case GopetManager.PET_EQUIP_GLOVE:
                case GopetManager.PET_EQUIP_SHOE:
                    return GopetManager.KIOSK_AMOUR;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Lý do 1 item không treo bán được ở popup Chợ, null nếu bán được bình thường. Theo
        /// đúng thứ tự kiểm tra của <see cref="Gopet.Data.Map.Kiosk.TryList"/> để SELLABLE hiển
        /// thị nhất quán với việc treo thật sẽ thành công hay bị từ chối.
        /// </summary>
        public static string BlockReason(Player player, Item item)
        {
            if (!item.Template.canTrade || !item.canTrade)
            {
                return player.Language.KioskItemLocked;
            }
            if (item.petEuipId > 0)
            {
                return player.Language.PleaseDoUpToKioskItemHasPetEquip;
            }
            if (item.gemInfo != null)
            {
                return player.Language.PleaseUnequipGem;
            }
            if (item.expire >= 0 && item.expire < Utilities.CurrentTimeMillis)
            {
                return player.Language.ItemExpiredCannotSell;
            }
            return null;
        }

        /// <summary>Lý do 1 pet không treo bán được: pet dùng thử, đang theo cùng người chơi,
        /// hoặc đang mang trang bị (bán sẽ để trang bị treo lơ lửng với petEuipId trỏ tới pet đã
        /// bán mất — item.petEuipId>0 nhưng chủ item không còn giữ pet đó nữa).</summary>
        public static string BlockReason(Player player, Pet pet)
        {
            if (pet.Expire != null)
            {
                return player.Language.YouCannotSellPetTry;
            }
            if (player.playerData.petSelected == pet)
            {
                return player.Language.KioskCannotSellActivePet;
            }
            if (pet.equip.Count > 0)
            {
                return player.Language.KioskPetHasEquip;
            }
            return null;
        }
    }
}
