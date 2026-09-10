namespace Gopet.Net.Pet
{
    /// <summary>
    /// 4 gói tương tác pet cho SELF (jar <c>fr.java:150-183, 295-298</c>).
    /// Server broadcast lại <c>ON_PET_INTERACT</c> cho khu vực, self bỏ qua bằng
    /// cách so <c>userId == localUserId</c> (đã xử lý ở tầng UI).
    /// </summary>
    public static class PetActionPackets
    {
        /// <summary>Hôn pet — jar <c>dc.a(0)</c>.</summary>
        public const sbyte Kiss = 0;
        /// <summary>Chơi với pet — jar <c>dc.a(1)</c>.</summary>
        public const sbyte Play = 1;
        /// <summary>Xoa đầu pet — jar <c>dc.a(2)</c>.</summary>
        public const sbyte Poke = 2;

        /// <summary>
        /// Wire: <c>PET_SERVICE 17 / sbyte type</c>.
        /// <c>type</c> = <see cref="Play"/> / <see cref="Kiss"/> / <see cref="Poke"/>.
        /// </summary>
        public static Message Interact(sbyte type)
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(17)
                .PutSByte(type);
        }

        /// <summary>
        /// Wire: <c>PET_SERVICE 45 / sbyte 1</c>. Jar <c>dc.a(true)</c>.
        /// Server hồi HP pet đầy, có thể trừ thóc tuỳ config.
        /// </summary>
        public static Message Heal()
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(45)
                .PutSByte(1);
        }
    }
}
