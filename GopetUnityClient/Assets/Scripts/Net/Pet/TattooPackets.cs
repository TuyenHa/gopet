namespace Gopet.Net.Pet
{
    public static class TattooPackets
    {
        public static Message RequestScreen() => Command(GopetCmd.TATTOO_INIT_SCREEN);
        public static Message SelectGenerationItem() => Command(GopetCmd.SELECT_ITEM_GEM_TATTO);
        public static Message SelectRemoveItem(int tattooId) =>
            Command(GopetCmd.SELECT_ITEM_REMOVE_TATOO).PutInt(tattooId);
        public static Message SelectEnchantMaterial(sbyte slot) =>
            Command(GopetCmd.TATTOO_ENCHANT_SELECT_MATERIAL).PutSByte(slot);
        public static Message ConfirmEnchant(int tattooId, int material1, int material2) =>
            Command(GopetCmd.TATTOO_ENCHANT).PutInt(tattooId).PutInt(material1).PutInt(material2);

        private static Message Command(sbyte sub) => Message.Create(GopetCmd.PET_SERVICE)
            .PutSByte(GopetCmd.TATTOO).PutSByte(sub);
    }
}
