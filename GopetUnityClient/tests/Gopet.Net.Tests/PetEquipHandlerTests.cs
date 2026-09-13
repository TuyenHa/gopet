using Gopet.Net;
using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>EQUIP_INFO recv wire (GameController.cs:1825-1923) — header + N item + frameNum.</summary>
    public sealed class PetEquipHandlerTests
    {
        private static Message BuildEquipInfo(int userId, int petId, params PetEquipItem[] items)
        {
            var m = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.EQUIP_INFO)
                .PutInt(userId)
                .PutInt(petId)
                .PutUtf("pets/dragon.png")
                .PutUtf("Baby Dragon (5★)")
                .PutInt(50)          // lvl
                .PutInt(100)         // str
                .PutInt(80)          // agi
                .PutInt(60);         // int
            m.PutInt(items.Length);
            foreach (var it in items) WriteItem(m, it);
            m.PutSByte(4);           // frameNum
            return m;
        }

        private static void WriteItem(Message m, PetEquipItem item)
        {
            m.PutInt(item.ItemId)
             .PutUtf(item.FrameImagePath)
             .PutUtf("???")
             .PutUtf(item.DisplayName)
             .PutInt(item.Type)
             .PutInt(item.PetEquipId);
            for (var j = 0; j < 11; j++) m.PutInt(j + 1);
            m.PutSByte(0).PutSByte(item.Level).PutBool(item.HasGem);
            if (item.HasGem)
            {
                m.PutLong(item.GemTimeUnequip).PutInt(item.GemSecondsRemaining);
            }
        }

        [Fact]
        public void EquipInfo_KhongItem_HeaderDayVaFrameNumDung()
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);

            PetEquipInfo got = null;
            handler.EquipInfoReceived += i => got = i;

            using var msg = BuildEquipInfo(42, 100);
            router.Dispatch(Message.FromWire(msg.ToWire(), false));

            Assert.NotNull(got);
            Assert.Equal(42, got.UserId);
            Assert.Equal(100, got.PetId);
            Assert.Equal("Baby Dragon (5★)", got.PetName);
            Assert.Equal(50, got.Level);
            Assert.Equal(100, got.Str);
            Assert.Empty(got.Items);
            Assert.Equal((sbyte)4, got.FrameNumber);
        }

        [Fact]
        public void EquipInfo_ItemKhongGem_ParseVaSlotMap()
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);

            PetEquipInfo got = null;
            handler.EquipInfoReceived += i => got = i;

            var hat = new PetEquipItem { ItemId = 1, FrameImagePath = "items/hat.png",
                DisplayName = "Nón +5", Type = 3, PetEquipId = 100, Level = 5, HasGem = false };
            var weapon = new PetEquipItem { ItemId = 2, FrameImagePath = "items/sword.png",
                DisplayName = "Kiếm +3", Type = 1, PetEquipId = 100, Level = 3, HasGem = false };
            using var msg = BuildEquipInfo(42, 100, hat, weapon);
            router.Dispatch(Message.FromWire(msg.ToWire(), false));

            Assert.Equal(2, got.Items.Length);
            Assert.Equal(EquipSlot.Hat, got.Items[0].Slot);
            Assert.Equal(EquipSlot.Weapon, got.Items[1].Slot);
        }

        [Fact]
        public void EquipInfo_ItemCoGem_DocLongVaInt()
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);

            PetEquipInfo got = null;
            handler.EquipInfoReceived += i => got = i;

            var gemArmor = new PetEquipItem { ItemId = 3, FrameImagePath = "items/armor.png",
                DisplayName = "Giáp Ngọc", Type = 2, PetEquipId = 100, Level = 7,
                HasGem = true, GemTimeUnequip = 1_700_000_000_000L, GemSecondsRemaining = 3600 };
            using var msg = BuildEquipInfo(42, 100, gemArmor);
            router.Dispatch(Message.FromWire(msg.ToWire(), false));

            var it = got.Items[0];
            Assert.True(it.HasGem);
            Assert.Equal(1_700_000_000_000L, it.GemTimeUnequip);
            Assert.Equal(3600, it.GemSecondsRemaining);
            Assert.Equal(EquipSlot.Armor, it.Slot);
        }

        [Fact]
        public void EquipInfo_ItemCountAm_Nem()
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);

            var m = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.EQUIP_INFO)
                .PutInt(1).PutInt(1).PutUtf("").PutUtf("").PutInt(1).PutInt(1).PutInt(1).PutInt(1)
                .PutInt(-1);

            Assert.Throws<ProtocolException>(() => router.Dispatch(Message.FromWire(m.ToWire(), false)));
        }

        [Fact]
        public void RequestEquipInfo_Wire_81_28_Int()
        {
            using var m = PetEquipPackets.RequestEquipInfo(555);
            Assert.Equal(GopetCmd.PET_SERVICE, m.Id);
            var r = Message.FromWire(m.ToWire(), false).Reader;
            Assert.Equal(GopetCmd.EQUIP_INFO, r.ReadSByte());
            Assert.Equal(555, r.ReadInt());
        }

        [Theory]
        [InlineData(GopetCmd.USE_EQUIP_ITEM, true)]
        [InlineData(GopetCmd.UNEQUIP_ITEM, false)]
        public void EquipDelta_DocTrangThai(sbyte sub, bool equipped)
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);
            PetEquipDelta received = null;
            handler.EquipChanged += value => received = value;

            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub)
                .PutSByte(1).PutInt(77);
            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.True(received.Accepted);
            Assert.Equal(77, received.ItemId);
            Assert.Equal(equipped, received.Equipped);
        }

        [Fact]
        public void RemoveEquip_DocItemId()
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);
            PetEquipDelta received = null;
            handler.EquipChanged += value => received = value;

            using var message = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.REMOVE_ITEM_EQUIP).PutInt(91);
            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.True(received.Removed);
            Assert.Equal(91, received.ItemId);
        }

        [Fact]
        public void EnchantMaterial_DocRouteDongQuaHelperCmd()
        {
            var router = new MessageRouter();
            var handler = new PetEquipHandler();
            handler.RegisterOn(router);
            PetEquipMaterialSelection received = null;
            handler.EnchantMaterialSelected += value => received = value;
            using var message = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.SELECT_METERIAL_ENCHANT_PET_INFO)
                .PutInt(31).PutUtf("items/31.png").PutUtf("Bạc").PutInt(7);

            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Equal(31, received.ItemOrTemplateId);
            Assert.Equal("Bạc", received.Name);
            Assert.Equal(7, received.Slot);
        }
    }
}
