namespace Gopet.Net.Guild
{
    /// <summary>
    /// Bang hội — wrap sub-command của <c>PET_SERVICE (81) / CLAN (91) / …</c>.
    /// Xem <c>GameController.clan()</c> (line 2455) — server switch trên sub thứ 2.
    /// </summary>
    public static class GuildPackets
    {
        public const sbyte SubGuildList = 1;
        public const sbyte SubRequestJoin = 2;
        public const sbyte SubInfoMember = 3;
        public const sbyte SubKickMember = 6;
        public const sbyte SubDonate = 9;
        public const sbyte SubPlayerDonate = 10;
        public const sbyte SubSearchGuild = 13;
        public const sbyte SubInfo = 14;
        public const sbyte SubTopGrowth = 15;
        public const sbyte SubTopFund = 16;
        public const sbyte SubChatHistory = 20;
        public const sbyte SubSendChat = 21;
        public const sbyte SubUnlockSkillSlot = 25;
        public const sbyte SubRentSkill = 26;
        public const sbyte SubShowClanSkill = 27;

        private static Message ClanBase(sbyte sub) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(91).PutSByte(sub);

        public static Message RequestGuildList() => ClanBase(SubGuildList);

        public static Message RequestJoin(int clanId) =>
            ClanBase(SubRequestJoin).PutInt(clanId);

        public static Message RequestClanMemberInfo(sbyte index, bool detail) =>
            ClanBase(SubInfoMember).PutSByte(index).PutBool(detail);

        public static Message KickMember(int userId) =>
            ClanBase(SubKickMember).PutInt(userId);

        public static Message DonateClan() => ClanBase(SubDonate);

        public static Message PlayerDonateClan(int amount) =>
            ClanBase(SubPlayerDonate).PutInt(amount);

        public static Message SearchGuild(string name) =>
            ClanBase(SubSearchGuild).PutUtf(name ?? string.Empty);

        public static Message RequestClanInfo() => ClanBase(SubInfo);

        public static Message TopGrowth() => ClanBase(SubTopGrowth);

        public static Message TopFund() => ClanBase(SubTopFund);

        public static Message RequestChatHistory() => ClanBase(SubChatHistory);

        public static Message SendChat(int clanId, string text) =>
            ClanBase(SubSendChat).PutInt(clanId).PutUtf(text ?? string.Empty);

        public static Message UnlockSkillSlot() => ClanBase(SubUnlockSkillSlot);

        public static Message RentSkill(int index) =>
            ClanBase(SubRentSkill).PutInt(index);

        public static Message ShowClanSkill(int userId) =>
            ClanBase(SubShowClanSkill).PutInt(userId);
    }
}
