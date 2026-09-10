using Gopet.Net;
using Gopet.Net.Map;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class WorldObjectHandlerTests
    {
        [Fact]
        public void GameObject_DocNpcDungWireServer()
        {
            var (handler, router) = NewHandler();
            NpcSpawn[] received = null;
            handler.NpcsReceived += value => received = value;
            using var built = Message.Create(GopetCmd.GAME_OBJECT)
                .PutSByte(0).PutInt(1).PutInt(2).PutInt(30).PutInt(40)
                .PutInt(-1).PutUtf("npcs/tranchan.png").PutInt(2)
                .PutInt(100).PutInt(120).PutInt(6).PutInt(1).PutUtf("Xin chào")
                .PutUtf("Trân Chân").PutSByte(0);
            router.Dispatch(Message.FromWire(built.ToWire(), false));
            Assert.Single(received);
            Assert.Equal(-1, received[0].Id);
            Assert.Equal("Trân Chân", received[0].Name);
            Assert.Equal(120, received[0].Y);
        }

        [Fact]
        public void PetService_DocDanhSachMobVaTuongTac()
        {
            var (handler, router) = NewHandler();
            MobSpawn[] mobs = null;
            PetInteraction interaction = null;
            handler.MobsReceived += value => mobs = value;
            handler.PetInteractionReceived += value => interaction = value;
            using var mobPacket = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.SEND_LIST_MOB_ZONE).PutInt(1).PutInt(7)
                .PutUtf("pets/wolf.png").PutUtf("Sói").PutInt(3).PutInt(50).PutInt(60)
                .PutSByte(0).PutSByte(2).PutShort(4).PutBool(true);
            router.Dispatch(Message.FromWire(mobPacket.ToWire(), false));
            Assert.Single(mobs);
            Assert.True(mobs[0].IsBoss);
            using var interact = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.ON_PET_INTERACT).PutInt(99).PutSByte(2);
            router.Dispatch(Message.FromWire(interact.ToWire(), false));
            Assert.Equal(99, interaction.UserId);
            Assert.Equal(2, interaction.Type);
        }

        private static (WorldObjectHandler, MessageRouter) NewHandler()
        {
            var router = new MessageRouter();
            var handler = new WorldObjectHandler();
            handler.RegisterOn(router);
            return (handler, router);
        }
    }
}
