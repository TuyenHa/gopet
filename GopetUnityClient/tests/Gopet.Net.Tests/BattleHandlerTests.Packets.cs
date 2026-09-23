using Gopet.Net.Battle;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed partial class BattleHandlerTests
    {
        private static void WriteOwnedManySkills(Message m, int count)
        {
            m.PutInt(11).PutUtf("pets/me.png").PutSByte(4).PutShort(-2).PutUtf("Mèo").PutInt(3);
            for (var i = 0; i < 5; i++) m.PutInt(i + 1);
            m.PutInt(420).PutInt(9000).PutInt(500).PutInt(9000).PutSByte((sbyte)count);
            for (var i = 0; i < count; i++)
                m.PutInt(101 + i).PutUtf($"Kỹ năng {i}").PutUtf("Mô tả").PutInt(10 + i);
        }

        private static void WriteOwned(Message m, int template, string image, string name,
            int hp, int mp, int maxHp, int maxMp)
        {
            m.PutInt(template).PutUtf(image).PutSByte(4).PutShort(-2).PutUtf(name).PutInt(3);
            for (var i = 0; i < 5; i++) m.PutInt(i + 1);
            m.PutInt(hp).PutInt(mp).PutInt(maxHp).PutInt(maxMp).PutSByte(1)
                .PutInt(101).PutUtf("Cào Lv.2").PutUtf("Gây sát thương").PutInt(12);
        }

        private static void WritePassive(Message m, int template, string image, string name,
            int hp, int mp, int maxHp, int maxMp, bool skill)
        {
            m.PutInt(template).PutUtf(image).PutSByte(4).PutShort(0).PutUtf(name).PutInt(2)
                .PutInt(hp).PutInt(mp).PutInt(maxHp).PutInt(maxMp).PutSByte(skill ? 1 : 0);
            if (skill) m.PutInt(101).PutUtf("Đòn đối thủ");
        }

        private static (BattleHandler, MessageRouter) NewHandler(int userId)
        {
            var router = new MessageRouter();
            var handler = new BattleHandler(null, userId); handler.RegisterOn(router);
            return (handler, router);
        }

        private static void Dispatch(MessageRouter router, Message packet)
        {
            var body = packet.ToWire();
            var wire = new byte[body.Length + 1]; wire[0] = unchecked((byte)GopetCmd.PET_SERVICE);
            System.Array.Copy(body, 0, wire, 1, body.Length);
            router.Dispatch(Message.FromWire(wire, false));
        }

        private static JavaBinaryReader Round(Message message) => Message.FromWire(message.ToWire(), false).Reader;
        private static void AssertBody(Message sent, sbyte sub, int value)
        {
            var r = Round(sent); Assert.Equal(GopetCmd.PET_SERVICE, sent.Id);
            Assert.Equal(sub, r.ReadSByte()); Assert.Equal(value, r.ReadInt());
        }
    }
}
