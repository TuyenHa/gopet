using System;
using System.Collections.Generic;
using System.Linq;
using Gopet.Data.GopetItem;
using Gopet.Util;

namespace Gopet.Data.item
{
    public enum RepairResult { Ok, NotFound, NotNeeded, NoStone }

    /// <summary>
    /// Sửa trang bị pet ở Thợ Rèn: tiêu 1 Đá mài sửa chữa, đầy độ bền. Phần luật thuần
    /// (<see cref="TryRepair"/>) tách khỏi Player để test được.
    /// </summary>
    public static class EquipRepairService
    {
        public const string HelpText =
            "Trang bị pet (nón, kiếm, giày, bao tay, giáp) có độ bền tối đa 80. Mỗi trận thắng trừ 1, " +
            "thua trừ 2. Về 0 thì món đó HỎNG và mất chỉ số riêng (bonus set vẫn giữ).\n" +
            "Mang tới ta cùng 1 Đá mài sửa chữa để sửa đầy 1 món.\n" +
            "Đá mài có khi đánh quái, hạ boss và quà điểm danh.";

        /// <summary>
        /// Khoá theo người chơi: bấm sửa liên tiếp/gói trùng không tiêu 2 Đá mài cho một lần sửa —
        /// lần sau thấy món đã đầy và trả <see cref="RepairResult.NotNeeded"/>.
        /// </summary>
        public static RepairResult TryRepair(Item item, object playerLock, Func<bool> consumeStone)
        {
            if (item == null || !EquipDurability.Applies(item)) return RepairResult.NotFound;
            lock (playerLock)
            {
                if (!EquipDurability.NeedsRepair(item)) return RepairResult.NotNeeded;
                if (!consumeStone()) return RepairResult.NoStone;
                EquipDurability.Repair(item);
            }
            return RepairResult.Ok;
        }

        /// <summary>Mọi trang bị pet (kể cả còn đầy độ bền), món hỏng/mòn nhiều lên đầu.</summary>
        public static List<Item> PetEquips(Player player) =>
            player.playerData.getInventoryOrCreate(GopetManager.EQUIP_PET_INVENTORY)
                .Where(EquipDurability.Applies)
                .OrderBy(i => i.durability)
                .ToList();

        public static int StoneCount(Player player) =>
            player.playerData.getInventoryOrCreate(GopetManager.NORMAL_INVENTORY)
                .Where(i => i.itemTemplateId == GopetManager.REPAIR_STONE_ID)
                .Sum(i => i.count);

        public static RepairResult Repair(Player player, int itemId)
        {
            Item item = player.controller.selectItemEquipByItemId(itemId);
            var result = TryRepair(item, player.playerData.EquipRepairLock, () => ConsumeStone(player));
            switch (result)
            {
                case RepairResult.Ok:
                    RefreshPetStats(player, item);
                    player.okDialog(Utilities.Format("Đã sửa xong %s (độ bền %s/%s).",
                        item.getTemp().getName(player), item.durability, EquipDurability.Max));
                    break;
                case RepairResult.NoStone:
                    player.redDialog("Cần 1 Đá mài sửa chữa. Đá mài có khi đánh quái, hạ boss và quà điểm danh.");
                    break;
                case RepairResult.NotNeeded:
                    player.okDialog("Món này còn nguyên độ bền, không cần sửa.");
                    break;
                default:
                    player.fastAction();
                    break;
            }
            return result;
        }

        /// <summary>Tiêu 1 viên, ưu tiên chồng KHOÁ giao dịch trước để giữ lại đá bán được.</summary>
        private static bool ConsumeStone(Player player)
        {
            Item stone = player.playerData.getInventoryOrCreate(GopetManager.NORMAL_INVENTORY)
                .Where(i => i.itemTemplateId == GopetManager.REPAIR_STONE_ID && i.count > 0)
                .OrderBy(i => i.canTrade)
                .FirstOrDefault();
            if (stone == null) return false;
            player.controller.subCountItem(stone, 1, GopetManager.NORMAL_INVENTORY);
            return true;
        }

        /// <summary>Món đang mặc: tính lại chỉ số pet ngay để thấy chỉ số trở lại.</summary>
        private static void RefreshPetStats(Player player, Item item)
        {
            if (item.petEuipId < 0) return;
            Pet pet = player.playerData.petSelected?.petId == item.petEuipId
                ? player.playerData.petSelected
                : player.controller.selectPetByItemId(item.petEuipId);
            if (pet == null) return;
            pet.applyInfo(player); // tự gửi MY_PET_INFO ở cuối hàm
        }
    }
}
