using Gopet.Net;
using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class PetProfileHandlerTests
    {
        [Fact]
        public void Magic_DocDayDuProfileSkillVaTattoo()
        {
            var router = new MessageRouter();
            var handler = new PetProfileHandler();
            handler.RegisterOn(router);
            PetProfile received = null;
            handler.ProfileReceived += value => received = value;

            using var message = ProfilePacket();
            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(42, received.UserId);
            Assert.Equal("Mèo (1★)", received.Name);
            Assert.Equal(10, received.Level);
            Assert.Equal(100, received.Str);
            Assert.Equal(500, received.MaxHp);
            Assert.Equal("Cào", received.Skills[0].Name);
            Assert.Equal(12, received.Skills[0].MpCost);
            Assert.Equal("Nhanh nhẹn", received.Tattoos[0].Name);
            Assert.Equal(7, received.PotentialPoints);
            Assert.Equal(4, received.FrameCount);
        }

        [Fact]
        public void GymVaPotential_DocDungBaOption()
        {
            var router = new MessageRouter();
            var handler = new PetProfileHandler();
            handler.RegisterOn(router);
            PetGymState gym = null;
            PetPotentialUpdate update = null;
            handler.GymReceived += value => gym = value;
            handler.PotentialUpdated += value => update = value;

            using var gymMessage = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.GYM)
                .PutInt(9).PutUtf("pet.png").PutUtf("Mèo").PutSByte(2).PutInt(8)
                .PutLong(20).PutLong(30).PutLong(0).PutInt(10).PutInt(11).PutInt(12)
                .PutSByte(3);
            WriteOptions(gymMessage);
            gymMessage.PutSByte(4);
            router.Dispatch(Message.FromWire(gymMessage.ToWire(), false));

            using var updateMessage = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.UP_TIEM_NANG).PutInt(9).PutInt(11).PutInt(11).PutInt(12);
            WriteOptions(updateMessage);
            router.Dispatch(Message.FromWire(updateMessage.ToWire(), false));

            Assert.Equal(3, gym.PotentialPoints);
            Assert.Equal(3, gym.Options.Length);
            Assert.Equal(11, update.Str);
            Assert.Equal("stat-2", update.Options[2].Description);
        }

        [Fact]
        public void TattooMaterial_DocDungSubBay()
        {
            var router = new MessageRouter();
            var handler = new PetProfileHandler();
            handler.RegisterOn(router);
            TattooMaterialSelection received = null;
            handler.TattooMaterialSelected += value => received = value;

            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.TATTOO)
                .PutSByte(7).PutInt(88).PutUtf("tattoo.png").PutUtf("Mực phép");
            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Equal(88, received.ItemId);
            Assert.Equal("Mực phép", received.Name);
        }

        [Fact]
        public void Packets_DungEnvelopeVaWire()
        {
            using var profile = PetProfilePackets.RequestProfile();
            var r1 = Message.FromWire(profile.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.MAGIC, r1.ReadSByte());

            using var gym = PetProfilePackets.RequestGym();
            var r2 = Message.FromWire(gym.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.GYM, r2.ReadSByte());

            using var add = PetProfilePackets.AddPotential(17, 2);
            var r3 = Message.FromWire(add.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.UP_TIEM_NANG, r3.ReadSByte());
            Assert.Equal(17, r3.ReadInt());
            Assert.Equal((sbyte)2, r3.ReadSByte());
        }

        [Fact]
        public void TattooScreen_DocDungDanhSachSlot()
        {
            var router = new MessageRouter();
            var handler = new PetProfileHandler();
            handler.RegisterOn(router);
            TattooScreen received = null;
            handler.TattooScreenReceived += value => received = value;
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.TATTOO)
                .PutSByte(GopetCmd.TATTOO_INIT_SCREEN).PutInt(2)
                .PutInt(7).PutUtf("Nhanh nhẹn").PutSByte(1).PutUtf("tattoos/7.png")
                .PutInt(0).PutUtf("Chưa mở").PutSByte(2).PutUtf("tattoos/-1.png");

            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Equal(2, received.Slots.Length);
            Assert.Equal(7, received.Slots[0].TattooId);
            Assert.Equal((sbyte)2, received.Slots[1].Position);
        }

        [Fact]
        public void TattooPackets_DungThuTuVatLieuEnchant()
        {
            using var select = TattooPackets.SelectEnchantMaterial(2);
            var first = Message.FromWire(select.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.TATTOO, first.ReadSByte());
            Assert.Equal(GopetCmd.TATTOO_ENCHANT_SELECT_MATERIAL, first.ReadSByte());
            Assert.Equal((sbyte)2, first.ReadSByte());

            using var confirm = TattooPackets.ConfirmEnchant(1, 2, 3);
            var second = Message.FromWire(confirm.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.TATTOO, second.ReadSByte());
            Assert.Equal(GopetCmd.TATTOO_ENCHANT, second.ReadSByte());
            Assert.Equal(1, second.ReadInt());
            Assert.Equal(2, second.ReadInt());
            Assert.Equal(3, second.ReadInt());
        }

        private static Message ProfilePacket()
        {
            return Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.MAGIC)
                .PutInt(42).PutInt(9).PutSByte(1).PutUtf("pet.png").PutUtf("Mèo (1★)")
                .PutSByte(2).PutInt(10).PutLong(20).PutLong(30).PutLong(0)
                .PutInt(100).PutInt(80).PutInt(60).PutInt(150).PutInt(75)
                .PutInt(450).PutInt(90).PutInt(500).PutInt(100)
                .PutSByte(1).PutInt(3).PutUtf("Cào").PutUtf("Gây sát thương").PutInt(12)
                .PutInt(7).PutInt(1).PutInt(1).PutUtf("Nhanh nhẹn").PutSByte(1)
                .PutUtf("").PutSByte(1).PutSByte(4);
        }

        private static void WriteOptions(Message message)
        {
            for (var i = 0; i < 3; i++)
                message.PutInt(10 + i).PutInt(20 + i).PutSByte(0)
                    .PutUtf($"stat-{i}").PutSByte(1);
        }
    }
}
