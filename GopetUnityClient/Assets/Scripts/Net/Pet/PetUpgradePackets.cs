namespace Gopet.Net.Pet
{
    /// <summary>Gói chiều lên của màn tiến hoá pet, bọc trong PET_SERVICE.</summary>
    public static class PetUpgradePackets
    {
        private static Message Begin(sbyte sub) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);

        public static Message SelectPet(sbyte role) =>
            Begin(GopetCmd.SELECT_PET_UPGRADE).PutSByte(role);

        public static Message RequestPrice() => Begin(GopetCmd.PRICE_UPGRADE_PET);

        public static Message RequestPreview(int activePetId, int materialPetId) =>
            Begin(GopetCmd.INFO_UP_TIER_PET).PutInt(activePetId).PutInt(materialPetId);

        public static Message Confirm(int activePetId, int materialPetId, string name) =>
            Begin(GopetCmd.PET_UP_TIER)
                .PutInt(activePetId)
                .PutInt(materialPetId)
                .PutUtf(name ?? string.Empty)
                .PutSByte(1); // server hiện chỉ chấp nhận MONEY_TYPE_GOLD = 1
    }
}
