using Gopet.Data.Collections;
using Gopet.Data.Dialog;
using Gopet.Data.GopetItem;
using Gopet.Data.item;

/// <summary>NPC Thợ Rèn (Thành phố Linh Thú): sửa độ bền trang bị pet bằng Đá mài sửa chữa.</summary>
public partial class MenuController
{
    public const int OP_REPAIR_EQUIP = 98;
    public const int OP_DURABILITY_HELP = 99;
    public const int MENU_REPAIR_EQUIP = 1092;
    /// <summary>Danh sách itemId đã gửi trong menu sửa — chọn theo chỉ số dòng phải ánh xạ về đúng
    /// món đã hiện, kể cả khi độ bền đổi (trận khác kết thúc) giữa lúc gửi và lúc chọn.</summary>
    public const int OBJKEY_REPAIR_EQUIP_IDS = 74;

    /// <summary>Chỉ sửa khi đứng ở Thành phố Linh Thú và không trong trận: chặn gói tự chế sửa từ
    /// xa, và chặn đua luồng với việc trừ độ bền/tính lại chỉ số ở cuối trận.</summary>
    private static bool canUseBlacksmith(Player player)
    {
        if (player.controller.getPetBattle() != null)
        {
            player.redDialog("Không thể sửa trang bị khi đang trong trận.");
            return false;
        }
        if (player.getPlace()?.map.mapID != MapManager.ID_LINH_THU_CITY)
        {
            player.redDialog("Hãy tới Thợ Rèn ở Thành phố Linh Thú để sửa trang bị.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Gửi MỌI trang bị pet (client Unity vẽ thành lưới ô vuông, chạm vào xem độ bền rồi bấm
    /// "Sửa chữa"). Món còn đầy độ bền gửi <c>canSelect=false</c>. Không kèm hộp xác nhận:
    /// popup chi tiết của client đã là bước xác nhận. Tiêu đề mang số Đá mài để client
    /// chặn bấm sửa khi hết đá.
    /// </summary>
    private static void sendRepairEquipMenu(Player player)
    {
        if (!canUseBlacksmith(player)) return;
        var items = EquipRepairService.PetEquips(player);
        player.controller.objectPerformed.put(OBJKEY_REPAIR_EQUIP_IDS, items.ConvertAll(i => i.itemId).ToArray());

        JArrayList<MenuItemInfo> infos = new();
        foreach (Item item in items)
        {
            // Mô tả kết thúc bằng "Độ bền: X/80" — client đọc số từ đây để vẽ thanh độ bền.
            var info = new MenuItemInfo(item.getTemp().getName(player),
                item.getDescription(player) + EquipDurability.Describe(item),
                item.getTemp().getIconPath(), EquipDurability.NeedsRepair(item));
            infos.add(info);
        }
        player.controller.showMenuItem(MENU_REPAIR_EQUIP, TYPE_MENU_SELECT_ELEMENT,
            $"Sửa trang bị (Đá mài: {EquipRepairService.StoneCount(player)})", infos);
    }

    private static void selectRepairEquip(Player player, int index)
    {
        if (!canUseBlacksmith(player)) return;
        var ids = player.controller.objectPerformed.ContainsKey(OBJKEY_REPAIR_EQUIP_IDS)
            ? player.controller.objectPerformed.get(OBJKEY_REPAIR_EQUIP_IDS) as int[]
            : null;
        if (ids == null || index < 0 || index >= ids.Length)
        {
            player.fastAction();
            return;
        }
        // Sửa xong thì gửi lại lưới để popup Thợ Rèn cập nhật độ bền và số Đá mài.
        if (EquipRepairService.Repair(player, ids[index]) == RepairResult.Ok)
            sendRepairEquipMenu(player);
    }
}
