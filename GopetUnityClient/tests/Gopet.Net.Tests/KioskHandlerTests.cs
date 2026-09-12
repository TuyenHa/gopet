using Gopet.Net.Kiosk;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class KioskHandlerTests
    {
        [Fact]
        public void Listing_ParsesCompleteOwnerState()
        {
            var router = new MessageRouter();
            var handler = new KioskHandler();
            handler.RegisterOn(router);
            KioskListing received = null;
            handler.ListingReceived += value => received = value;

            using var message = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.KIOSK)
                .PutSByte(4).PutInt(1)
                .PutInt(77).PutUtf("pets/fire.png").PutUtf("Rá»“ng lửa")
                .PutUtf("Cấp 12").PutInt(3600).PutSByte(6);
            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.True(received.HasListing);
            Assert.Equal(4, received.Type);
            Assert.Equal(77, received.ItemId);
            Assert.Equal("pets/fire.png", received.FrameImagePath);
            Assert.Equal(3600, received.RemainingSeconds);
            Assert.Equal(6, received.FrameCount);
        }

        [Fact]
        public void EmptyKiosk_HasNoOptionalPayload()
        {
            var router = new MessageRouter();
            var handler = new KioskHandler();
            handler.RegisterOn(router);
            KioskListing received = null;
            handler.ListingReceived += value => received = value;

            using var message = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.KIOSK).PutSByte(0).PutInt(0);
            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.False(received.HasListing);
            Assert.Equal(0, received.Type);
        }
    }
}
