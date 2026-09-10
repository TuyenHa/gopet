using System;
using Xunit;

namespace Gopet.Net.Tests
{


    public sealed class MessageRouterTests
    {
        [Fact]
        public void DinhTuyenOpcodeDungRieng()
        {
            var router = new MessageRouter();
            var hit = 0;
            router.Register(GopetCmd.LOGIN_SUCCES, _ => hit++);

            router.Dispatch(Message.FromWire(new[] { unchecked((byte)GopetCmd.LOGIN_SUCCES) }, false));

            Assert.Equal(1, hit);
        }

        [Fact]
        public void DinhTuyenSubCommandCuaOpcodeBaoNgoai()
        {
            var router = new MessageRouter();
            var received = 0;

            router.RegisterEnvelope(GopetCmd.COMMAND_GUIDER);
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.SHOW_MENU_ITEM, m => received = m.Reader.ReadInt());

            // [opcode 122][sub 8][int 777]
            var payload = new byte[]
            {
                unchecked((byte)GopetCmd.COMMAND_GUIDER),
                unchecked((byte)GopetCmd.SHOW_MENU_ITEM),
                0, 0, 3, 9
            };

            router.Dispatch(Message.FromWire(payload, false));

            Assert.Equal(777, received);
        }

        [Fact]
        public void OpcodeKhongCoHandler_GoiOnUnhandled()
        {
            var router = new MessageRouter();
            sbyte unhandled = 0;
            router.OnUnhandled = m => unhandled = m.Id;

            router.Dispatch(Message.FromWire(new byte[] { 99 }, false));

            Assert.Equal((sbyte)99, unhandled);
        }

        [Fact]
        public void DangKyTrungOpcode_NemLoi()
        {
            var router = new MessageRouter();
            router.Register(GopetCmd.LOGIN_SUCCES, _ => { });

            Assert.Throws<InvalidOperationException>(
                () => router.Register(GopetCmd.LOGIN_SUCCES, _ => { }));
        }

        [Fact]
        public void RegisterSub_ChuaKhaiBaoBaoNgoai_NemLoi()
        {
            var router = new MessageRouter();

            Assert.Throws<InvalidOperationException>(
                () => router.RegisterSub(GopetCmd.PET_SERVICE, 1, _ => { }));
        }

        [Fact]
        public void HandlerNemLoi_ChuyenSangOnError()
        {
            var router = new MessageRouter();
            Exception caught = null;
            router.OnError = (_, ex) => caught = ex;
            router.Register(GopetCmd.LOGIN_SUCCES, _ => throw new ProtocolException("lệch"));

            router.Dispatch(Message.FromWire(new[] { unchecked((byte)GopetCmd.LOGIN_SUCCES) }, false));

            Assert.IsType<ProtocolException>(caught);
        }
    }
}
