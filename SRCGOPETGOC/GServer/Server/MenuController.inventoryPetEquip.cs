using Gopet.Data.Collections;
using Gopet.Data.Dialog;
using Gopet.Data.GopetItem;
using Gopet.Data.item;

/// <summary>
/// Rương đồ hiện cả trang bị pet (kèm độ bền), nối vào CUỐI danh sách túi thường. Bấm "Dùng"
/// trên một món: pet đang theo đang mặc thì tháo, còn lại thì mặc cho pet đang theo — đi qua
/// đúng <c>useEquipItem</c>/<c>unEquipItem</c> của màn trang bị pet.
/// </summary>
public partial class MenuController
{
    /// <summary>Ảnh chụp (id dòng đầu, itemId từng dòng) của các dòng trang bị pet đã gửi.</summary>
    public const int OBJKEY_INVENTORY_PET_EQUIP_ROWS = 75;

    /// <summary>Chữ client Unity đọc để đổi nút "Dùng" thành "Tháo" / khoá nút.</summary>
    public const string PET_EQUIP_WORN_BY_ACTIVE = " (Pet đang theo mặc)";
    public const string PET_EQUIP_WORN_BY_OTHER = " (Pet khác đang mặc)";

    /// <summary>
    /// Id dòng đi tiếp sau túi thường (<paramref name="firstRowId"/> trở đi), nên chọn dòng túi
    /// thường vẫn tra theo chỉ số như cũ. Món đang nằm trên pet KHÁC thì không cho chọn:
    /// <c>unEquipItem</c> chỉ gỡ khỏi pet đang theo.
    /// </summary>
    private static void appendPetEquipsForView(Player player, JArrayList<MenuItemInfo> menuList, int firstRowId)
    {
        var items = player.playerData.getInventoryOrCreate(GopetManager.EQUIP_PET_INVENTORY).ToArray();
        int activePetId = player.getPet()?.petId ?? int.MinValue;
        var ids = new int[items.Length];
        for (int k = 0; k < items.Length; k++)
        {
            Item item = items[k];
            ids[k] = item.itemId;
            bool wornByOther = item.petEuipId > 0 && item.petEuipId != activePetId;
            string worn = item.petEuipId > 0
                ? (wornByOther ? PET_EQUIP_WORN_BY_OTHER : PET_EQUIP_WORN_BY_ACTIVE)
                : "";
            var info = new MenuItemInfo(item.getName(player),
                item.getDescription(player) + EquipDurability.Describe(item) + worn,
                item.getTemp().getIconPath(), !wornByOther);
            info.setHasId(true);
            info.setItemId(firstRowId + k);
            menuList.add(info);
        }
        player.controller.objectPerformed[OBJKEY_INVENTORY_PET_EQUIP_ROWS] = (firstRowId, ids);
    }

    /// <summary>Trả true khi <paramref name="rowId"/> là dòng trang bị pet (đã xử lý xong).</summary>
    private static bool selectInventoryPetEquip(Player player, int rowId)
    {
        if (!player.controller.objectPerformed.ContainsKey(OBJKEY_INVENTORY_PET_EQUIP_ROWS)) return false;
        (int firstRowId, int[] ids) = ((int, int[]))player.controller.objectPerformed[OBJKEY_INVENTORY_PET_EQUIP_ROWS];
        int k = rowId - firstRowId;
        if (k < 0 || k >= ids.Length) return false;

        Item item = player.controller.selectItemEquipByItemId(ids[k]);
        if (item == null)
        {
            player.fastAction();
            return true;
        }
        int activePetId = player.getPet()?.petId ?? int.MinValue;
        int before = item.petEuipId;
        bool unequip = before > 0 && before == activePetId;
        if (unequip) player.controller.unEquipItem(item.itemId);
        else player.controller.useEquipItem(item.itemId);

        // Thất bại thì useEquipItem/unEquipItem đã tự báo lỗi; chỉ báo khi thực sự đổi.
        if (item.petEuipId != before)
        {
            string name = item.getTemp().getName(player);
            player.okDialog(unequip ? $"Đã tháo {name} khỏi pet." : $"Đã mặc {name} cho pet.");
        }

        // Gửi lại Rương đồ để nhãn "đang mặc" và nút Dùng/Tháo khớp trạng thái mới.
        showInventory(player, GopetManager.NORMAL_INVENTORY, MENU_NORMAL_INVENTORY, player.Language.Inventory);
        return true;
    }
}
