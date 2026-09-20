using Gopet.Net;
using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Wire của học/thay kỹ năng pet. Gửi sai `skillId` là THAY nhầm kỹ năng của người chơi,
    /// nên khoá đúng byte: PET_SERVICE(81) + sub 31 + int, không thừa không thiếu.
    /// </summary>
    public sealed class PetProfilePacketsTests
    {
        [Theory]
        // -1 = học vào ô trống; server đọc thành Player.skillId_learn == -1.
        [InlineData(PetProfilePackets.LearnNewSlot)]
        // id kỹ năng đang có = thay chính kỹ năng đó.
        [InlineData(101)]
        [InlineData(130)]
        public void LearnSkill_DungSubVaMangIdNguyenVan(int skillId)
        {
            using var message = PetProfilePackets.LearnSkill(skillId);
            Assert.Equal(GopetCmd.PET_SERVICE, message.Id);

            var reader = Message.FromWire(message.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.MAGIC_LEARN_SKILL, reader.ReadSByte());
            Assert.Equal(skillId, reader.ReadInt());
            Assert.Equal(0, reader.Remaining);
        }
    }
}
