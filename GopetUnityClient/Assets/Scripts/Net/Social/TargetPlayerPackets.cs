namespace Gopet.Net.Social
{
    /// <summary>
    /// Các thao tác nhắm vào 1 người chơi khác — mở info, thách đấu, PK, xin đồ pet.
    /// </summary>
    public static class TargetPlayerPackets
    {
        // GET_PLAYER_INFO sub types — jar getInfo() phân biệt loại info nào cần.
        // 0/1/2 chưa reverse chi tiết; default 0 = thông tin cơ bản.
        public const sbyte InfoBasic = 0;

        /// <summary>PLAYER_CHALLENGE (12) TOP-LEVEL — thách đấu 1v1 pet.</summary>
        public static Message Challenge(int userId) =>
            Message.Create(GopetCmd.PLAYER_CHALLENGE).PutInt(userId);

        /// <summary>PET_SERVICE / GET_PLAYER_INFO (55) — xem info người chơi khác.</summary>
        public static Message RequestInfo(int userId, sbyte type = InfoBasic) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.GET_PLAYER_INFO)
                .PutSByte(type).PutInt(userId);

        /// <summary>PET_SERVICE / PLAYER_PK (96) — mời PK map hiện tại.</summary>
        public static Message SendPk(int userId) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.PLAYER_PK).PutInt(userId);

        /// <summary>PET_SERVICE / EQUIP_INFO (28) với userId khác — xem đồ pet của họ.</summary>
        public static Message ViewEquipment(int userId) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.EQUIP_INFO).PutInt(userId);
    }
}
