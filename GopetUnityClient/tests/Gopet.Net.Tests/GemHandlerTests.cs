using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class GemHandlerTests
    {
        private readonly MessageRouter _router = new MessageRouter();
        private readonly GemHandler _handler = new GemHandler(_ => { });

        public GemHandlerTests() => _handler.RegisterOn(_router);

        [Fact]
        public void Inventory_ParsesAllServerFields()
        {
            GemInventory received = null;
            _handler.InventoryReceived += value => received = value;
            Dispatch(GopetCmd.SHOW_GEM_INVENTORY, m => m.PutInt(2)
                .PutInt(10).PutInt(301).PutUtf("Hồng ngọc +2").PutInt(2)
                .PutInt(11).PutInt(302).PutUtf("Lam ngọc").PutInt(0));

            Assert.Equal(2, received.Items.Length);
            Assert.Equal(301, received.Items[0].IconId);
            Assert.Equal("Lam ngọc", received.Items[1].Name);
        }

        [Fact]
        public void UpdateAndRemove_ParseServerFields()
        {
            GemItemInfo updated = null;
            var removed = -1;
            _handler.GemUpdated += value => updated = value;
            _handler.GemRemoved += value => removed = value;
            Dispatch(GopetCmd.SEND_GEM_INFo, m => m
                .PutInt(42).PutUtf("gem/red.png").PutUtf("Hồng ngọc +3").PutSByte(3));
            Dispatch(GopetCmd.REMOVE_GEM_ITEM, m => m.PutInt(42));

            Assert.Equal(42, updated.ItemId);
            Assert.Equal("gem/red.png", updated.IconPath);
            Assert.Equal(3, updated.Level);
            Assert.Equal(42, removed);
        }

        [Fact]
        public void EnchantAndTierSelections_HaveDifferentLayouts()
        {
            GemMaterialSelection enchant = null;
            GemMaterialSelection tier = null;
            _handler.EnchantMaterialSelected += value => enchant = value;
            _handler.TierMaterialSelected += value => tier = value;
            Dispatch(GopetCmd.SELECT_GEM_ENCHANT, m => m
                .PutInt(501).PutUtf("mat.png").PutUtf("Bột ngọc").PutInt(7));
            Dispatch(GopetCmd.SELECT_GEM_UP_TIER, m => m
                .PutInt(99).PutUtf("gem.png").PutUtf("Lam ngọc").PutInt(1).PutInt(4));

            Assert.Equal(501, enchant.ItemOrTemplateId);
            Assert.Equal(7, enchant.Slot);
            Assert.Equal(0, enchant.Level);
            Assert.Equal(99, tier.ItemOrTemplateId);
            Assert.Equal(4, tier.Level);
        }

        [Fact]
        public void Inventory_RejectsUnboundedCount()
        {
            Assert.Throws<ProtocolException>(() =>
                Dispatch(GopetCmd.SHOW_GEM_INVENTORY, m => m.PutInt(513)));
        }

        private void Dispatch(sbyte sub, System.Action<Message> body)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);
            body(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
