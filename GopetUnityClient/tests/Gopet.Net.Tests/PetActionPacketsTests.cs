using Gopet.Net;
using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Byte-for-byte kiểm 4 gói pet action. Đối chiếu jar dc.java:12-18, 53-64.</summary>
    public sealed class PetActionPacketsTests
    {
        [Theory]
        [InlineData(PetActionPackets.Kiss, (sbyte)0)]
        [InlineData(PetActionPackets.Play, (sbyte)1)]
        [InlineData(PetActionPackets.Poke, (sbyte)2)]
        public void Interact_Wire_Envelope81_Sub17_ThenType(sbyte type, sbyte expected)
        {
            using var msg = PetActionPackets.Interact(type);
            Assert.Equal(GopetCmd.PET_SERVICE, msg.Id);
            var r = Message.FromWire(msg.ToWire(), false).Reader;
            Assert.Equal((sbyte)17, r.ReadSByte());
            Assert.Equal(expected, r.ReadSByte());
        }

        /// <summary>Hồi phục là CÔNG TẮC: jar dc.a(boolean) gửi 1 để bật, 0 để tắt.</summary>
        [Theory]
        [InlineData(true, (sbyte)1)]
        [InlineData(false, (sbyte)0)]
        public void Heal_Wire_Envelope81_Sub45_ThenOnOff(bool on, sbyte expected)
        {
            using var msg = PetActionPackets.Heal(on);
            Assert.Equal(GopetCmd.PET_SERVICE, msg.Id);
            var r = Message.FromWire(msg.ToWire(), false).Reader;
            Assert.Equal((sbyte)45, r.ReadSByte());
            Assert.Equal(expected, r.ReadSByte());
        }

        [Fact]
        public void HangSoConstants_KhopGiaTriJar()
        {
            Assert.Equal(0, PetActionPackets.Kiss);
            Assert.Equal(1, PetActionPackets.Play);
            Assert.Equal(2, PetActionPackets.Poke);
        }
    }
}
