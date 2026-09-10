namespace Gopet.Net.Guild
{
    /// <summary>
    /// Bang hội — wrap sub-command của <c>PET_SERVICE (81) / CLAN (91) / …</c>.
    /// Xem <c>GameController.clan()</c> (line 2455) — server switch trên sub thứ 2.
    /// </summary>
    public static class GuildPackets
    {
        // Sub-command trong CLAN family (GopetCMD.cs line 71-140).
        public const sbyte SubInfo = 14;
        public const sbyte SubInfoMember = 3;
        public const sbyte SubDonate = 9;
        public const sbyte SubPlayerDonate = 10;
        public const sbyte SubSearchGuild = 13;
        public const sbyte SubTopFund = 16;
        public const sbyte SubTopGrowth = 15;
        public const sbyte SubRequestJoin = 2;
        public const sbyte SubChatHistory = 20;

        private static Message ClanBase(sbyte sub) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(91).PutSByte(sub);

        /// <summary>CLAN_INFO — bơm thông tin bang hiện tại của self.</summary>
        public static Message RequestClanInfo() => ClanBase(SubInfo);

        /// <summary>Thông tin thành viên (server đọc thêm sbyte + bool).</summary>
        public static Message RequestClanMemberInfo(sbyte index, bool detail) =>
            ClanBase(SubInfoMember).PutSByte(index).PutBool(detail);

        /// <summary>Cống hiến bang bằng vàng (không tham số).</summary>
        public static Message DonateClan() => ClanBase(SubDonate);

        /// <summary>Cống hiến số cụ thể (int amount).</summary>
        public static Message PlayerDonateClan(int amount) =>
            ClanBase(SubPlayerDonate).PutInt(amount);

        /// <summary>Tìm bang theo tên. UTF name.</summary>
        public static Message SearchGuild(string name) =>
            ClanBase(SubSearchGuild).PutUtf(name ?? string.Empty);

        /// <summary>Top quỹ bang.</summary>
        public static Message TopFund() => ClanBase(SubTopFund);

        /// <summary>Top điểm growth bang.</summary>
        public static Message TopGrowth() => ClanBase(SubTopGrowth);

        /// <summary>Xin vào bang theo clanId.</summary>
        public static Message RequestJoin(int clanId) =>
            ClanBase(SubRequestJoin).PutInt(clanId);

        /// <summary>Yêu cầu server bơm lịch sử chat bang.</summary>
        public static Message RequestChatHistory() => ClanBase(SubChatHistory);
    }
}
