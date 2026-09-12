using System;

namespace Gopet.Net.Guild
{
    /// <summary>
    /// Dispatches all <c>PET_SERVICE (81) / CLAN (91) / sub</c> responses.
    /// Parsing lives in the partial file <c>GuildInfoHandler.Parsers.cs</c>.
    /// </summary>
    public sealed partial class GuildInfoHandler
    {
        public int ClanId { get; private set; }

        public event Action<int> ClanIdChanged;
        public event Action<GuildClanInfo> ClanInfoReceived;
        public event Action<GuildListResponse> GuildListReceived;
        public event Action<GuildMemberListResponse> MemberListReceived;
        public event Action<GuildDonateResponse> DonateOptionsReceived;
        public event Action<GuildTopResponse> TopFundReceived;
        public event Action<GuildChatHistory> ChatHistoryReceived;
        public event Action<GuildChatIncoming> ChatMessageReceived;
        public event Action<GuildSkillResponse> SkillInfoReceived;
        public event Action<GuildNameInPlace> NameInPlaceReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, 91, OnClanEnvelope);
        }

        private void OnClanEnvelope(Message message)
        {
            var r = message.Reader;
            if (r.Remaining < 1) return;
            var sub = r.ReadSByte();
            switch (sub)
            {
                case GopetCmd.GUILD_LIST:
                    GuildListReceived?.Invoke(ParseGuildList(r));
                    break;
                case GopetCmd.CLAN_INFO_MEMBER:
                    MemberListReceived?.Invoke(ParseMemberList(r));
                    break;
                case GopetCmd.DONATE_CLAN:
                    DonateOptionsReceived?.Invoke(ParseDonateOptions(r));
                    break;
                case GopetCmd.CLAN_INFO:
                    var info = ParseClanInfo(r);
                    if (info.ClanId != ClanId)
                    {
                        ClanId = info.ClanId;
                        ClanIdChanged?.Invoke(info.ClanId);
                    }
                    ClanInfoReceived?.Invoke(info);
                    break;
                case GopetCmd.GUILD_TOP_GROWTH_POINT:
                case GopetCmd.GUILD_TOP_FUND:
                    TopFundReceived?.Invoke(ParseTopResponse(r));
                    break;
                case GopetCmd.GUILD_CHAT:
                    ChatHistoryReceived?.Invoke(ParseChatHistory(r));
                    break;
                case GopetCmd.GUILD_ON_PLAYER_CHAT:
                    ChatMessageReceived?.Invoke(ParseChatIncoming(r));
                    break;
                case GopetCmd.GUILD_NAME_IN_PLACE:
                    NameInPlaceReceived?.Invoke(ParseNameInPlace(r));
                    break;
                case GopetCmd.GUILD_CLAN_SKILL:
                    SkillInfoReceived?.Invoke(ParseSkillInfo(r));
                    break;
            }
        }
    }
}
