namespace Gopet.Net.Chat
{
    /// <summary>
    /// Chat 2 kênh cross-map:
    /// <list type="bullet">
    ///   <item><b>Cộng đồng</b>: <c>PET_SERVICE (81) / CHAT_GLOBAL (10) / UTF text</c>.
    ///     Server tính phí <c>GOLD_NEED_CHAT_GLOBAL</c>, broadcast toàn server.
    ///     Server phản hồi <c>PET_SERVICE / CHAT_GLOBAL / UTF name / UTF text</c>.</item>
    ///   <item><b>Bang hội</b>: gửi <c>PET_SERVICE (81) / CLAN (91) / GUILD_PLAYER_CHAT (?) / int clanId / UTF text</c>.
    ///     Server broadcast cho thành viên bang.</item>
    /// </list>
    ///
    /// <para><b>Ghi chú Guild:</b> jar chỉ có sub-command GUILD_CHAT (=20) để REQUEST xem
    /// lịch sử chat bang. Việc SEND text vào bang dùng sub GUILD_PLAYER_CHAT. Phase 5 mới
    /// đưa vào 2 cái này; view/dispatch full ở Phase 8.</para>
    /// </summary>
    public static class ChatChannelPackets
    {
        public const sbyte ChatGlobal = 10;
        public const sbyte Clan = 91;
        public const sbyte GuildChatRequest = 20;
        public const sbyte GuildPlayerChatSend = 21;    // GUILD_PLAYER_CHAT — verify Phase 8

        /// <summary>Gửi 1 dòng chat cộng đồng (tính phí gold).</summary>
        public static Message SendGlobal(string text)
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(ChatGlobal)
                .PutUtf(text ?? string.Empty);
        }

        /// <summary>Yêu cầu server bơm lịch sử chat bang xuống.</summary>
        public static Message RequestGuildHistory()
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(Clan)
                .PutSByte(GuildChatRequest);
        }

        /// <summary>
        /// Gửi 1 dòng chat vào bang. Server đọc: <c>PET_SERVICE / CLAN / GUILD_PLAYER_CHAT
        /// (21) / int clanId / UTF text</c> — xem <c>GameController.cs:2491-2493</c>.
        /// </summary>
        public static Message SendGuildChat(int clanId, string text)
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(Clan)
                .PutSByte(GuildPlayerChatSend)
                .PutInt(clanId)
                .PutUtf(text ?? string.Empty);
        }
    }
}
