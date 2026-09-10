using Gopet.Net;
using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Wire khớp <c>GopetPlace.sendListPet</c> (line 208-246) — format &gt; VERSION_133.
    /// Header: sbyte count. Each entry: int userId, int petIdTemplate, UTF frameImg,
    /// UTF name, int lvl, sbyte frameNum, short vY.
    /// </summary>
    public sealed class PetZoneHandlerTests
    {
        private static Message BuildZone(params PetZoneEntry[] entries)
        {
            var m = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.SEND_LIST_PET_ZONE);
            m.PutSByte(entries.Length);
            foreach (var e in entries)
            {
                m.PutInt(e.OwnerUserId).PutInt(e.PetIdTemplate)
                 .PutUtf(e.FrameImagePath).PutUtf(e.DisplayName).PutInt(e.Level)
                 .PutSByte(e.FrameNum).PutShort(e.VerticalOffset);
            }
            return m;
        }

        [Fact]
        public void PetZone_HaiPet_DungFieldOrder()
        {
            var router = new MessageRouter();
            var handler = new PetZoneHandler();
            handler.RegisterOn(router);

            PetZoneUpdate got = null;
            handler.PetZoneReceived += u => got = u;

            using var msg = BuildZone(
                new PetZoneEntry { OwnerUserId = 1, PetIdTemplate = 100,
                    FrameImagePath = "pets/dragon.png", DisplayName = "Dragon", Level = 5, FrameNum = 4, VerticalOffset = -8 },
                new PetZoneEntry { OwnerUserId = 2, PetIdTemplate = 101,
                    FrameImagePath = "pets/wolf.png", DisplayName = "Wolf", Level = 3, FrameNum = 2, VerticalOffset = 0 });
            router.Dispatch(Message.FromWire(msg.ToWire(), false));

            Assert.NotNull(got);
            Assert.Equal(2, got.Entries.Length);
            Assert.Equal(1, got.Entries[0].OwnerUserId);
            Assert.Equal(100, got.Entries[0].PetIdTemplate);
            Assert.Equal("pets/dragon.png", got.Entries[0].FrameImagePath);
            Assert.Equal("Dragon", got.Entries[0].DisplayName);
            Assert.Equal(5, got.Entries[0].Level);
            Assert.Equal((sbyte)4, got.Entries[0].FrameNum);
            Assert.Equal((short)-8, got.Entries[0].VerticalOffset);
            Assert.Equal(2, got.Entries[1].OwnerUserId);
            Assert.Equal("Wolf", got.Entries[1].DisplayName);
        }

        [Fact]
        public void PetZone_KhongCoPet_KhongThrow()
        {
            var router = new MessageRouter();
            var handler = new PetZoneHandler();
            handler.RegisterOn(router);

            PetZoneUpdate got = null;
            handler.PetZoneReceived += u => got = u;

            using var msg = BuildZone();
            router.Dispatch(Message.FromWire(msg.ToWire(), false));

            Assert.NotNull(got);
            Assert.Empty(got.Entries);
        }

        [Fact]
        public void PetUnfollow_DocDungUserId()
        {
            var router = new MessageRouter();
            var handler = new PetZoneHandler();
            handler.RegisterOn(router);

            PetUnfollow got = null;
            handler.PetUnfollowed += u => got = u;

            using var m = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.PET_UNFOLLOW).PutInt(42);
            router.Dispatch(Message.FromWire(m.ToWire(), false));

            Assert.Equal(42, got.OwnerUserId);
        }

        [Fact]
        public void MyPetInfo_DocDung4Int()
        {
            var router = new MessageRouter();
            var handler = new PetZoneHandler();
            handler.RegisterOn(router);

            MyPetInfo got = null;
            handler.MyPetInfoReceived += m => got = m;

            using var msg = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.MY_PET_INFO)
                .PutInt(80).PutInt(100).PutInt(45).PutInt(50);
            router.Dispatch(Message.FromWire(msg.ToWire(), false));

            Assert.Equal(80, got.Hp);
            Assert.Equal(100, got.MaxHp);
            Assert.Equal(45, got.Mp);
            Assert.Equal(50, got.MaxMp);
        }
    }
}
