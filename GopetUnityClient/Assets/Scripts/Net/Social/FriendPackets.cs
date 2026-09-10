namespace Gopet.Net.Social
{
    /// <summary>
    /// Kết bạn / block — LETTER_COMMAND (121) sub 3/4 gửi lên.
    /// Server dispatch trong <c>GameController.letter</c> (line 466-514).
    /// </summary>
    public static class FriendPackets
    {
        public const sbyte RequestAddById = 3;
        public const sbyte RequestAddByName = 4;

        /// <summary>Kết bạn theo userId (đã tra thấy). Wire: 121 / sbyte 3 / int userId.</summary>
        public static Message AddFriendById(int userId) =>
            Message.Create((sbyte)121).PutSByte(RequestAddById).PutInt(userId);

        /// <summary>Kết bạn theo tên (tra database). Wire: 121 / sbyte 4 / UTF name.</summary>
        public static Message AddFriendByName(string name) =>
            Message.Create((sbyte)121).PutSByte(RequestAddByName).PutUtf(name ?? string.Empty);
    }
}
