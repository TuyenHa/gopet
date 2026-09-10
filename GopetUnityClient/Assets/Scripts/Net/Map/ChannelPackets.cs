namespace Gopet.Net.Map
{
    /// <summary>
    /// Chuyển kênh / xem info kênh. TOP-LEVEL opcodes, không wrap trong PET_SERVICE.
    /// Wire khớp <c>GameController.cs:306-315</c>:
    /// <list type="bullet">
    ///   <item>GET_CHANNEL_INFO (7) — không body.</item>
    ///   <item>CHANGE_CHANNEL (24) — 4 int (mapId, placeId, zoneId, channelId).</item>
    /// </list>
    /// </summary>
    public static class ChannelPackets
    {
        /// <summary>Yêu cầu server bơm info các kênh hiện có (danh sách + số người).</summary>
        public static Message GetChannelInfo() =>
            Message.Create(GopetCmd.ON_PLAYER_GET_CHANNEL_INFO);

        /// <summary>
        /// Đổi sang kênh khác. 4 int: mapId, placeId, zoneId, channelId.
        /// Cảnh báo: server dùng gói này để re-add player vào place mới; sai tham số
        /// có thể tạo double-init (xem <see cref="GameSession"/> comment về không gửi thừa
        /// ON_PLAYER_CHANGE_CHANNEL lúc đăng nhập).
        /// </summary>
        public static Message ChangeChannel(int mapId, int placeId, int zoneId, int channelId) =>
            Message.Create(GopetCmd.ON_PLAYER_CHANGE_CHANNEL)
                .PutInt(mapId).PutInt(placeId).PutInt(zoneId).PutInt(channelId);
    }
}
