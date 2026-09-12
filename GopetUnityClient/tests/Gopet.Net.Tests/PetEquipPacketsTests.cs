using Gopet.Net;
using Gopet.Net.Pet;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Gói trang bị/gem/cường/tiến pet — byte-for-byte theo processPet.</summary>
    public sealed class PetEquipPacketsTests
    {
        private static (sbyte opcode, sbyte sub, int[] ints) Roundtrip(Message m, int intFieldCount)
        {
            var round = Message.FromWire(m.ToWire(), false);
            var sub = round.Reader.ReadSByte();
            var ints = new int[intFieldCount];
            for (var i = 0; i < intFieldCount; i++) ints[i] = round.Reader.ReadInt();
            return (round.Id, sub, ints);
        }

        [Fact] public void RequestHiddenStats_101_NoPlayerControlledId()
        {
            var r = Roundtrip(PetEquipPackets.RequestHiddenStats(), 0);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.HIDDEN_STATS_INFO, r.sub);
        }

        [Fact] public void Equip_29_Int()
        {
            var r = Roundtrip(PetEquipPackets.Equip(1001), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.USE_EQUIP_ITEM, r.sub);
            Assert.Equal(new[] { 1001 }, r.ints);
        }

        [Fact] public void Unequip_39_Int()
        {
            var r = Roundtrip(PetEquipPackets.Unequip(2002), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.UNEQUIP_ITEM, r.sub);
            Assert.Equal(new[] { 2002 }, r.ints);
        }

        [Fact] public void SelectEnchantMaterial_46_TwoInts()
        {
            var r = Roundtrip(PetEquipPackets.SelectEnchantMaterial(10, 20), 2);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.SELECT_METERIAL_ENCHANT, r.sub);
            Assert.Equal(new[] { 10, 20 }, r.ints);
        }

        [Fact] public void ConfirmEnchant_48_ThreeInts()
        {
            var r = Roundtrip(PetEquipPackets.ConfirmEnchant(1, 2, 3), 3);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.ENCHANT_ITEM, r.sub);
            Assert.Equal(new[] { 1, 2, 3 }, r.ints);
        }

        [Fact] public void UpTierItem_49_TwoInts()
        {
            var r = Roundtrip(PetEquipPackets.UpTierItem(11, 22), 2);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.UP_TIER_ITEM, r.sub);
            Assert.Equal(new[] { 11, 22 }, r.ints);
        }

        [Fact] public void SelectGem_73_Int()
        {
            var r = Roundtrip(PetEquipPackets.SelectGem(500), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.GEM_INVENTORY, r.sub);
            Assert.Equal(new[] { 500 }, r.ints);
        }

        [Fact] public void UnequipGem_75_Int()
        {
            var r = Roundtrip(PetEquipPackets.UnequipGem(600), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.PET_UNEQUIP_GEM_ITEM_INFO, r.sub);
            Assert.Equal(new[] { 600 }, r.ints);
        }

        [Fact] public void FastUnequipGem_78_Int()
        {
            var r = Roundtrip(PetEquipPackets.FastUnequipGem(700), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.FAST_UNQUIP_GEM, r.sub);
            Assert.Equal(new[] { 700 }, r.ints);
        }

        [Fact] public void ConfirmUnequipGem_82_Int()
        {
            var r = Roundtrip(PetEquipPackets.ConfirmUnequipGem(800), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.ON_UNQUIP_GEM, r.sub);
            Assert.Equal(new[] { 800 }, r.ints);
        }

        [Fact] public void SelectGemEnchant_80_Int()
        {
            var r = Roundtrip(PetEquipPackets.SelectGemEnchant(900), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.SELECT_GEM_ENCHANT, r.sub);
            Assert.Equal(new[] { 900 }, r.ints);
        }

        [Fact] public void ConfirmEnchantGem_76_ThreeInts()
        {
            var r = Roundtrip(PetEquipPackets.ConfirmEnchantGem(100, 200, 300), 3);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.ENCHANT_GEM_ITEM, r.sub);
            Assert.Equal(new[] { 100, 200, 300 }, r.ints);
        }

        [Fact] public void UpTierGem_79_TwoInts()
        {
            var r = Roundtrip(PetEquipPackets.UpTierGem(1, 2), 2);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.UP_TIER_GEM_ITEM, r.sub);
            Assert.Equal(new[] { 1, 2 }, r.ints);
        }

        [Fact] public void RequestGemUpTierMaterial_81_81()
        {
            var r = Roundtrip(PetEquipPackets.RequestGemUpTierMaterial(), 0);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.SELECT_GEM_UP_TIER, r.sub);
        }

        [Fact] public void RemoveGem_83_Int()
        {
            var r = Roundtrip(PetEquipPackets.RemoveGem(999), 1);
            Assert.Equal(GopetCmd.PET_SERVICE, r.opcode);
            Assert.Equal(GopetCmd.REMOVE_GEM_ITEM, r.sub);
            Assert.Equal(new[] { 999 }, r.ints);
        }
    }
}
