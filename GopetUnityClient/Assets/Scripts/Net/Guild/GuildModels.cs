namespace Gopet.Net.Guild
{
    public sealed class GuildClanInfo
    {
        public int ClanId;
        public string[] DescriptionLines;
    }

    public sealed class GuildListResponse
    {
        public string Title;
        public GuildListEntry[] Entries;
    }

    public sealed class GuildListEntry
    {
        public int ClanId;
        public int ReservedInt;
        public string Name;
        public string Description;
    }

    public sealed class GuildMemberListResponse
    {
        public int ClanId;
        public bool CanManage;
        public string Title;
        public GuildMember[] Members;
        public bool HasMore;
    }

    public sealed class GuildMember
    {
        public int UserId;
        public string AvatarPath;
        public string Name;
        public string FundInfo;
    }

    public sealed class GuildDonateResponse
    {
        public GuildDonateOption[] Options;
    }

    public sealed class GuildDonateOption
    {
        public int Id;
        public string Description;
    }

    public sealed class GuildTopResponse
    {
        public GuildMember[] Members;
    }

    public sealed class GuildChatHistory
    {
        public int ClanId;
        public GuildChatMessage[] Messages;
    }

    public sealed class GuildChatMessage
    {
        public string Who;
        public string Text;
    }

    public sealed class GuildChatIncoming
    {
        public string Who;
        public string Text;
    }

    public sealed class GuildSkillResponse
    {
        public sbyte Mode;
        public int PotentialSkill;
        public GuildSkillSlot[] Slots;
    }

    public sealed class GuildSkillSlot
    {
        public int State;
        public int Index;
        public string Desc1;
        public string Desc2;
        public string Desc3;
    }

    public sealed class GuildNameEntry
    {
        public int UserId;
        public string ClanName;
    }

    public sealed class GuildNameInPlace
    {
        public GuildNameEntry[] Entries;
    }
}
