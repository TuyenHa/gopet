using Gopet.Data.GopetItem;
using System.Collections.Generic;

namespace Gopet.Data.item
{
    /// <summary>
    /// Trừ độ bền trang bị pet khi trận kết thúc (gọi từ <c>PetBattle.win</c>, điểm chốt duy nhất
    /// của mọi loại trận). Trận bị bỏ (<c>Close()</c>: đổi map, rớt mạng) không tới đây nên
    /// không trừ.
    /// </summary>
    public static class EquipWearService
    {
        public static void Apply(Player player, Pet pet, bool lost)
        {
            if (player?.controller == null || pet?.equip == null) return;
            var warned = new List<string>();
            var broke = new List<string>();
            // Cùng khoá với sửa ở Thợ Rèn: sửa ghi 80 rồi Wear ghi đè số cũ là mất 1 Đá mài.
            lock (player.playerData.EquipRepairLock)
            {
                foreach (int itemId in pet.equip.ToArray())
                {
                    Item item = player.controller.selectItemEquipByItemId(itemId);
                    if (item == null) continue;
                    switch (EquipDurability.Wear(item, lost))
                    {
                        case WearResult.Warned: warned.Add(item.getTemp().getName(player)); break;
                        case WearResult.Broke: broke.Add(item.getTemp().getName(player)); break;
                    }
                }
            }
            // Cả bộ thường cùng độ bền nên hay vượt mốc cùng lúc: gộp một thông báo mỗi trận.
            if (broke.Count > 0)
                player.Popup($"Trang bị đã hỏng và mất chỉ số: {string.Join(", ", broke)}. " +
                             "Dùng Đá mài sửa chữa ở Thợ Rèn (Thành phố Linh Thú) để sửa.");
            else if (warned.Count > 0)
                player.Popup($"Trang bị sắp hỏng (độ bền ≤ {EquipDurability.WarnAt}/{EquipDurability.Max}): " +
                             $"{string.Join(", ", warned)}. Mang tới Thợ Rèn ở Thành phố Linh Thú để sửa.");
            // sendMyPetInfo của win() không tự tính lại chỉ số — món vừa hỏng phải áp lại ngay.
            if (broke.Count > 0) pet.applyInfo(player);
        }
    }
}
