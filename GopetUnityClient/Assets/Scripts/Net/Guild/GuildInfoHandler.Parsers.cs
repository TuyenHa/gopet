namespace Gopet.Net.Guild
{
    public sealed partial class GuildInfoHandler
    {
        private static GuildClanInfo ParseClanInfo(JavaBinaryReader r)
        {
            var info = new GuildClanInfo { ClanId = r.ReadInt() };
            var count = r.ReadSByte();
            info.DescriptionLines = new string[count];
            for (var i = 0; i < count; i++)
                info.DescriptionLines[i] = r.ReadUtf();
            return info;
        }

        private static GuildListResponse ParseGuildList(JavaBinaryReader r)
        {
            var resp = new GuildListResponse
            {
                Title = r.ReadUtf()
            };
            r.ReadSByte(); // reserved
            r.ReadSByte(); // reserved
            var count = r.ReadInt();
            resp.Entries = new GuildListEntry[count];
            for (var i = 0; i < count; i++)
            {
                resp.Entries[i] = new GuildListEntry
                {
                    ClanId = r.ReadInt(),
                    ReservedInt = r.ReadInt(),
                    Name = r.ReadUtf(),
                    Description = r.ReadUtf()
                };
            }
            return resp;
        }

        private static GuildMemberListResponse ParseMemberList(JavaBinaryReader r)
        {
            var resp = new GuildMemberListResponse
            {
                ClanId = r.ReadInt(),
                CanManage = r.ReadBool()
            };
            r.ReadInt();   // reservedA
            resp.Title = r.ReadUtf();
            r.ReadInt();   // reservedB
            r.ReadInt();   // reservedC
            r.ReadSByte(); // reservedD
            r.ReadSByte(); // reservedE
            var count = r.ReadSByte();
            resp.Members = new GuildMember[count];
            for (var i = 0; i < count; i++)
            {
                resp.Members[i] = new GuildMember
                {
                    UserId = r.ReadInt(),
                    AvatarPath = r.ReadUtf(),
                    Name = r.ReadUtf(),
                    FundInfo = r.ReadUtf()
                };
            }
            resp.HasMore = r.ReadBool();
            return resp;
        }

        private static GuildDonateResponse ParseDonateOptions(JavaBinaryReader r)
        {
            var count = r.ReadInt();
            var resp = new GuildDonateResponse
            {
                Options = new GuildDonateOption[count]
            };
            for (var i = 0; i < count; i++)
            {
                resp.Options[i] = new GuildDonateOption
                {
                    Id = r.ReadInt(),
                    Description = r.ReadUtf()
                };
            }
            return resp;
        }

        private static GuildTopResponse ParseTopResponse(JavaBinaryReader r)
        {
            r.ReadSByte(); // reservedA
            r.ReadSByte(); // reservedB
            var count = r.ReadSByte();
            var resp = new GuildTopResponse
            {
                Members = new GuildMember[count]
            };
            for (var i = 0; i < count; i++)
            {
                resp.Members[i] = new GuildMember
                {
                    UserId = r.ReadInt(),
                    AvatarPath = r.ReadUtf(),
                    Name = r.ReadUtf(),
                    FundInfo = r.ReadUtf()
                };
            }
            return resp;
        }

        private static GuildChatHistory ParseChatHistory(JavaBinaryReader r)
        {
            var resp = new GuildChatHistory
            {
                ClanId = r.ReadInt()
            };
            r.ReadUtf(); // reserved
            var count = r.ReadSByte();
            resp.Messages = new GuildChatMessage[count];
            for (var i = 0; i < count; i++)
            {
                resp.Messages[i] = new GuildChatMessage
                {
                    Who = r.ReadUtf(),
                    Text = r.ReadUtf()
                };
            }
            return resp;
        }

        private static GuildChatIncoming ParseChatIncoming(JavaBinaryReader r)
        {
            return new GuildChatIncoming
            {
                Who = r.ReadUtf(),
                Text = r.ReadUtf()
            };
        }

        private static GuildNameInPlace ParseNameInPlace(JavaBinaryReader r)
        {
            var count = r.ReadInt();
            var resp = new GuildNameInPlace
            {
                Entries = new GuildNameEntry[count]
            };
            for (var i = 0; i < count; i++)
            {
                resp.Entries[i] = new GuildNameEntry
                {
                    UserId = r.ReadInt(),
                    ClanName = r.ReadUtf()
                };
            }
            return resp;
        }

        private static GuildSkillResponse ParseSkillInfo(JavaBinaryReader r)
        {
            var resp = new GuildSkillResponse
            {
                Mode = r.ReadSByte(),
                PotentialSkill = r.ReadInt(),
                Slots = new GuildSkillSlot[3]
            };
            for (var i = 0; i < 3; i++)
            {
                resp.Slots[i] = new GuildSkillSlot
                {
                    State = r.ReadInt(),
                    Index = r.ReadInt(),
                    Desc1 = r.ReadUtf(),
                    Desc2 = r.ReadUtf(),
                    Desc3 = r.ReadUtf()
                };
            }
            return resp;
        }
    }
}
