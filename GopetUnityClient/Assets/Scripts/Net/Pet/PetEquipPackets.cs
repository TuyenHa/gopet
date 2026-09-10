namespace Gopet.Net.Pet
{
    /// <summary>
    /// Gói client gửi cho luồng trang bị pet + gem + cường/tiến hoá.
    /// Opcode server tra <c>GameController.cs:1026-1172</c> + <c>GopetCMD.cs:53-128</c>.
    ///
    /// <para><b>Tất cả là sub-command của PET_SERVICE (81).</b> Server chỉ dispatch các
    /// opcode này bên trong <c>processPet</c>; gửi chúng top-level sẽ bị bỏ qua.</para>
    ///
    /// <para><b>Enchant asymmetry:</b> server phân biệt gem/tattoo/enchant thường bằng
    /// tham số <c>bool isGem</c> ở cuối. Client dùng opcode khác nhau (ENCHANT_ITEM=48
    /// cho thường, ENCHANT_GEM_ITEM=76 cho gem) — server tự set bool tương ứng.</para>
    /// </summary>
    public static class PetEquipPackets
    {
        private static Message PetService(sbyte sub) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);

        // === Query ===

        /// <summary>Yêu cầu server bơm EQUIP_INFO cho <paramref name="userId"/>. Truyền chính user id để xem đồ mình.</summary>
        public static Message RequestEquipInfo(int userId) =>
            PetService(GopetCmd.EQUIP_INFO).PutInt(userId);

        /// <summary>PET_INVENTORY (5) — server bơm menu list pets sở hữu; user chọn để đưa ra làm active pet.</summary>
        public static Message RequestPetInventory() =>
            PetService(GopetCmd.PET_INVENTORY);

        /// <summary>
        /// NORMAL_INVENTORY (30) — server bơm menu items trong túi; dùng khi tap slot rỗng để
        /// chọn item để mặc. Server auto-equip khi user chọn (nếu item hợp).
        /// </summary>
        public static Message RequestNormalInventory() =>
            PetService(GopetCmd.NORMAL_INVENTORY);

        /// <summary>SHOW_GEM_INVENTORY (74) — mở menu gem trong túi.</summary>
        public static Message RequestGemInventory() =>
            PetService(GopetCmd.SHOW_GEM_INVENTORY);

        // === Equip/Unequip ===

        /// <summary>Trang bị item lên pet đang theo. Opcode 29, wire: int itemId.</summary>
        public static Message Equip(int itemId) =>
            PetService(GopetCmd.USE_EQUIP_ITEM).PutInt(itemId);

        /// <summary>Tháo item khỏi pet. Opcode 39, wire: int itemId.</summary>
        public static Message Unequip(int itemId) =>
            PetService(GopetCmd.UNEQUIP_ITEM).PutInt(itemId);

        // === Cường hoá (enchant) — thường ===

        /// <summary>Chọn nguyên liệu cường hoá. Opcode 46, wire: int itemId, int materialItemId.</summary>
        public static Message SelectEnchantMaterial(int itemId, int materialItemId) =>
            PetService(GopetCmd.SELECT_METERIAL_ENCHANT)
                .PutInt(itemId).PutInt(materialItemId);

        /// <summary>
        /// Xác nhận cường hoá. Opcode 48, wire: int equipItemId, int materialTemplateId,
        /// int materialTempCrystal. Server <c>confirmEnchantItem(id, mat, crystal, isGem=false)</c>.
        /// Chỉ 2 material (mat thường + crystal), không phải 3 — reversal correction.
        /// </summary>
        public static Message ConfirmEnchant(int itemId, int materialTemplateId, int materialCrystalId) =>
            PetService(GopetCmd.ENCHANT_ITEM)
                .PutInt(itemId).PutInt(materialTemplateId).PutInt(materialCrystalId);

        // === Destroy (huỷ đồ) ===

        /// <summary>REMOVE_ITEM_EQUIP (56) — huỷ đồ. Server pop YN dialog xác nhận trước khi thật xoá.</summary>
        public static Message RequestDestroyEquip(int itemId) =>
            PetService(GopetCmd.REMOVE_ITEM_EQUIP).PutInt(itemId);

        // === Tiến hoá (up-tier) ===

        /// <summary>Tiến hoá item. Opcode 49, wire: int itemId, int materialItemId. Server tính material chính-phụ.</summary>
        public static Message UpTierItem(int itemId, int materialItemId) =>
            PetService(GopetCmd.UP_TIER_ITEM)
                .PutInt(itemId).PutInt(materialItemId);

        // === Gem ===

        /// <summary>Chọn gem để gắn. Opcode 73 (GEM_INVENTORY), wire: int socketItemId.</summary>
        public static Message SelectGem(int socketItemId) =>
            PetService(GopetCmd.GEM_INVENTORY).PutInt(socketItemId);

        /// <summary>Tháo 1 gem. Opcode 75 (PET_UNEQUIP_GEM_ITEM_INFO), wire: int gemItemId.</summary>
        public static Message UnequipGem(int gemItemId) =>
            PetService(GopetCmd.PET_UNEQUIP_GEM_ITEM_INFO).PutInt(gemItemId);

        /// <summary>Tháo gem nhanh (không confirm). Wire: int itemId.</summary>
        public static Message FastUnequipGem(int itemId) =>
            PetService(GopetCmd.FAST_UNQUIP_GEM).PutInt(itemId);

        /// <summary>Xác nhận tháo 1 gem. Opcode 74 (ON_UNQUIP_GEM), wire: int itemId.</summary>
        public static Message ConfirmUnequipGem(int itemId) =>
            PetService(GopetCmd.ON_UNQUIP_GEM).PutInt(itemId);

        /// <summary>Chọn item để cường hoá bằng gem. Opcode 80 (SELECT_GEM_ENCHANT), wire: int gemItemId.</summary>
        public static Message SelectGemEnchant(int gemItemId) =>
            PetService(GopetCmd.SELECT_GEM_ENCHANT).PutInt(gemItemId);

        /// <summary>Xác nhận cường hoá gem. Opcode 76 (ENCHANT_GEM_ITEM), wire: int itemId, int material1, int material2.</summary>
        public static Message ConfirmEnchantGem(int itemId, int material1Id, int material2Id) =>
            PetService(GopetCmd.ENCHANT_GEM_ITEM)
                .PutInt(itemId).PutInt(material1Id).PutInt(material2Id);

        /// <summary>Mở menu chọn gem nguyên liệu để tiến hoá.</summary>
        public static Message RequestGemUpTierMaterial() => PetService(GopetCmd.SELECT_GEM_UP_TIER);

        /// <summary>Tiến hoá gem. Opcode 79 (UP_TIER_GEM_ITEM), wire: int itemId, int materialId.</summary>
        public static Message UpTierGem(int itemId, int materialItemId) =>
            PetService(GopetCmd.UP_TIER_GEM_ITEM)
                .PutInt(itemId).PutInt(materialItemId);

        /// <summary>Xoá gem. Opcode 83 (REMOVE_GEM_ITEM), wire: int gemItemId.</summary>
        public static Message RemoveGem(int gemItemId) =>
            PetService(GopetCmd.REMOVE_GEM_ITEM).PutInt(gemItemId);
    }
}
