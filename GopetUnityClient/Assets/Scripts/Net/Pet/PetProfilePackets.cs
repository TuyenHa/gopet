namespace Gopet.Net.Pet
{
    public static class PetProfilePackets
    {
        private static Message PetService(sbyte sub) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);

        public static Message RequestProfile() => PetService(GopetCmd.MAGIC);

        public static Message RequestGym() => PetService(GopetCmd.GYM);

        public static Message AddPotential(int value, sbyte statIndex) =>
            PetService(GopetCmd.UP_TIEM_NANG).PutInt(value).PutSByte(statIndex);
    }
}
