using Gopet.Net;
using Gopet.Net.Auth;
using Gopet.Net.Guild;
using Gopet.Net.Map;
using Gopet.Net.MiniGame;
using Gopet.Net.Pet;
using Gopet.Net.Social;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Byte-for-byte packets Phase 8 + fill-in Phase 5/7. Rất nhiều gói nhỏ nên gom 1 file.</summary>
    public sealed class P8PacketsTests
    {
        private static (sbyte opcode, JavaBinaryReader r) Roundtrip(Message m)
        {
            var round = Message.FromWire(m.ToWire(), false);
            return (round.Id, round.Reader);
        }

        // === P8.1/P8.2 Target player ===

        [Fact] public void Challenge_12_Int()
        {
            var (op, r) = Roundtrip(TargetPlayerPackets.Challenge(999));
            Assert.Equal(GopetCmd.PLAYER_CHALLENGE, op);
            Assert.Equal(999, r.ReadInt());
        }

        [Fact] public void RequestInfo_81_55_SbyteInt()
        {
            var (op, r) = Roundtrip(TargetPlayerPackets.RequestInfo(1234));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal(GopetCmd.GET_PLAYER_INFO, r.ReadSByte());
            Assert.Equal((sbyte)0, r.ReadSByte());
            Assert.Equal(1234, r.ReadInt());
        }

        [Fact] public void SendPk_81_96_Int()
        {
            var (op, r) = Roundtrip(TargetPlayerPackets.SendPk(42));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)96, r.ReadSByte());
            Assert.Equal(42, r.ReadInt());
        }

        [Fact] public void ViewEquipment_81_28_Int()
        {
            var (op, r) = Roundtrip(TargetPlayerPackets.ViewEquipment(7));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal(GopetCmd.EQUIP_INFO, r.ReadSByte());
            Assert.Equal(7, r.ReadInt());
        }

        [Fact] public void AddFriendById_121_3_Int()
        {
            var (op, r) = Roundtrip(FriendPackets.AddFriendById(500));
            Assert.Equal((sbyte)121, op);
            Assert.Equal(FriendPackets.RequestAddById, r.ReadSByte());
            Assert.Equal(500, r.ReadInt());
        }

        [Fact] public void AddFriendByName_121_4_Utf()
        {
            var (op, r) = Roundtrip(FriendPackets.AddFriendByName("gopettest"));
            Assert.Equal((sbyte)121, op);
            Assert.Equal(FriendPackets.RequestAddByName, r.ReadSByte());
            Assert.Equal("gopettest", r.ReadUtf());
        }

        // === P8.3 Guild ===

        [Fact] public void GuildClanInfo_81_91_14()
        {
            var (op, r) = Roundtrip(GuildPackets.RequestClanInfo());
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)91, r.ReadSByte());
            Assert.Equal(GuildPackets.SubInfo, r.ReadSByte());
        }

        [Fact] public void GuildPlayerDonate_81_91_10_Int()
        {
            var (op, r) = Roundtrip(GuildPackets.PlayerDonateClan(1000));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)91, r.ReadSByte());
            Assert.Equal(GuildPackets.SubPlayerDonate, r.ReadSByte());
            Assert.Equal(1000, r.ReadInt());
        }

        [Fact] public void GuildSearch_81_91_13_Utf()
        {
            var (op, r) = Roundtrip(GuildPackets.SearchGuild("BestGuild"));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)91, r.ReadSByte());
            Assert.Equal(GuildPackets.SubSearchGuild, r.ReadSByte());
            Assert.Equal("BestGuild", r.ReadUtf());
        }

        [Fact] public void GuildRequestJoin_81_91_2_Int()
        {
            var (op, r) = Roundtrip(GuildPackets.RequestJoin(15));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)91, r.ReadSByte());
            Assert.Equal(GuildPackets.SubRequestJoin, r.ReadSByte());
            Assert.Equal(15, r.ReadInt());
        }

        // === P8.4 Channel + password ===

        [Fact] public void GetChannelInfo_7_NoBody()
        {
            var (op, _) = Roundtrip(ChannelPackets.GetChannelInfo());
            Assert.Equal(GopetCmd.ON_PLAYER_GET_CHANNEL_INFO, op);
        }

        [Fact] public void ChangeChannel_24_4Int()
        {
            var (op, r) = Roundtrip(ChannelPackets.ChangeChannel(11, 0, 1, 2));
            Assert.Equal(GopetCmd.ON_PLAYER_CHANGE_CHANNEL, op);
            Assert.Equal(11, r.ReadInt());
            Assert.Equal(0, r.ReadInt());
            Assert.Equal(1, r.ReadInt());
            Assert.Equal(2, r.ReadInt());
        }

        [Fact] public void ChangePassword_93_Wire()
        {
            var (op, r) = Roundtrip(ChangePasswordPackets.ChangePassword("old123", "newSecure456"));
            Assert.Equal(GopetCmd.CHANGE_NEW_PASSWORD, op);
            Assert.Equal((sbyte)2, r.ReadSByte());
            Assert.Equal((sbyte)7, r.ReadSByte());
            Assert.Equal("old123", r.ReadUtf());
            Assert.Equal("newSecure456", r.ReadUtf());
        }

        // === P8.7 Mini-game ===

        [Theory]
        [InlineData(MiniGamePackets.GameCaro)]
        [InlineData(MiniGamePackets.GameCoTuong)]
        [InlineData(MiniGamePackets.GameTienLen)]
        [InlineData(MiniGamePackets.GamePhom)]
        public void OpenMiniGame_81_2_SbyteType(sbyte gameType)
        {
            var (op, r) = Roundtrip(MiniGamePackets.OpenMiniGame(gameType));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)2, r.ReadSByte());
            Assert.Equal(gameType, r.ReadSByte());
        }

        // === P7.1 fill Pet inventory + P7.3 Destroy ===

        [Fact] public void RequestPetInventory_81_5()
        {
            var (op, r) = Roundtrip(PetEquipPackets.RequestPetInventory());
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal(GopetCmd.PET_INVENTORY, r.ReadSByte());
        }

        [Fact] public void RequestNormalInventory_81_30()
        {
            var (op, r) = Roundtrip(PetEquipPackets.RequestNormalInventory());
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal(GopetCmd.NORMAL_INVENTORY, r.ReadSByte());
        }

        [Fact] public void RequestGemInventory_81_74()
        {
            var (op, r) = Roundtrip(PetEquipPackets.RequestGemInventory());
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal(GopetCmd.SHOW_GEM_INVENTORY, r.ReadSByte());
        }

        [Fact] public void RequestDestroyEquip_81_56_Int()
        {
            var (op, r) = Roundtrip(PetEquipPackets.RequestDestroyEquip(777));
            Assert.Equal(GopetCmd.PET_SERVICE, op);
            Assert.Equal((sbyte)56, r.ReadSByte());
            Assert.Equal(777, r.ReadInt());
        }
    }
}
