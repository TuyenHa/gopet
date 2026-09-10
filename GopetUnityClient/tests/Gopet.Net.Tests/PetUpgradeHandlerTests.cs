using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class PetUpgradeHandlerTests
    {
        private readonly MessageRouter _router = new MessageRouter();
        private readonly PetUpgradeHandler _handler = new PetUpgradeHandler(_ => { });

        public PetUpgradeHandlerTests() => _handler.RegisterOn(_router);

        [Fact]
        public void ShowAndCompleted_RequireEmptyBodies()
        {
            var shown = false;
            var completed = false;
            _handler.ShowRequested += () => shown = true;
            _handler.Completed += () => completed = true;

            Dispatch(GopetCmd.SHOW_UPGRADE_PET, _ => { });
            Dispatch(GopetCmd.PET_UP_TIER, _ => { });

            Assert.True(shown);
            Assert.True(completed);
        }

        [Fact]
        public void PetInfoPriceAndPreview_ParseServerLayout()
        {
            PetUpgradeSelection pet = null;
            PetUpgradePrice price = null;
            PetUpgradePreview preview = null;
            _handler.PetSelected += value => pet = value;
            _handler.PriceReceived += value => price = value;
            _handler.PreviewReceived += value => preview = value;

            Dispatch(GopetCmd.PET_UPGRADE_PET_INFO, m => m
                .PutSByte(GopetCmd.PET_UPGRADE_PASSIVE).PutInt(91).PutUtf("pet/fire.png").PutSByte(2));
            Dispatch(GopetCmd.PRICE_UPGRADE_PET, m => m.PutInt(10_000).PutInt(int.MaxValue));
            Dispatch(GopetCmd.INFO_UP_TIER_PET, m => m.PutUtf("Hoả Long").PutSByte(2)
                .PutUtf("120(str) 90(agi) 80(int)").PutUtf("Tiềm năng: 40"));

            Assert.Equal(GopetCmd.PET_UPGRADE_PASSIVE, pet.Role);
            Assert.Equal(91, pet.PetId);
            Assert.Equal("pet/fire.png", pet.FrameImagePath);
            Assert.Equal(10_000, price.Gold);
            Assert.Equal(int.MaxValue, price.Coin);
            Assert.Equal("Hoả Long", preview.Title);
            Assert.Equal(2, preview.Lines.Length);
        }

        [Fact]
        public void Packets_MatchProcessPetContract()
        {
            using var select = PetUpgradePackets.SelectPet(GopetCmd.PET_UPGRADE_ACTIVE);
            var selectReader = Message.FromWire(select.ToWire(), true).Reader;
            Assert.Equal(GopetCmd.SELECT_PET_UPGRADE, selectReader.ReadSByte());
            Assert.Equal(GopetCmd.PET_UPGRADE_ACTIVE, selectReader.ReadSByte());
            selectReader.ExpectFullyConsumed("SELECT_PET_UPGRADE test");

            using var confirm = PetUpgradePackets.Confirm(7, 8, "Rồng Lửa");
            var confirmReader = Message.FromWire(confirm.ToWire(), true).Reader;
            Assert.Equal(GopetCmd.PET_UP_TIER, confirmReader.ReadSByte());
            Assert.Equal(7, confirmReader.ReadInt());
            Assert.Equal(8, confirmReader.ReadInt());
            Assert.Equal("Rồng Lửa", confirmReader.ReadUtf());
            Assert.Equal((sbyte)1, confirmReader.ReadSByte());
            confirmReader.ExpectFullyConsumed("PET_UP_TIER test");
        }

        [Fact]
        public void Preview_RejectsNegativeLineCount()
        {
            Assert.Throws<ProtocolException>(() =>
                Dispatch(GopetCmd.INFO_UP_TIER_PET, m => m.PutUtf("bad").PutSByte(-1)));
        }

        private void Dispatch(sbyte sub, System.Action<Message> write)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);
            write(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
