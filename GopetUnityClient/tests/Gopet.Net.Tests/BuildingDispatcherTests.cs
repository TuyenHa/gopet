using Gopet.Net;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Khoá đúng 7 building thật có trên map 11 (theo
    /// <c>plans/260907-2210.../reports/decode-map11-entities.md</c> + đối chiếu trực tiếp
    /// <c>dc.java:45-51</c>/<c>eg.java:298-320</c>/<c>GameController.cs</c> processPet)
    /// sinh ra opcode server khớp thật.
    ///
    /// <para><b>Bug đã sửa:</b> <c>REQUEST_SHOP</c> KHÔNG phải opcode top-level — server
    /// chỉ đọc nó như sub-command trong bao <c>PET_SERVICE</c> (81)
    /// (<c>GameController.cs</c> case PET_SERVICE → <c>processPet(subCmd, msg)</c> → switch
    /// case REQUEST_SHOP ở dòng 1029 nằm TRONG processPet, không phải switch ngoài trên
    /// <c>message.id</c>). Bản cũ gửi opcode 2 trần → server im lặng.</para>
    /// </summary>
    public sealed class BuildingDispatcherTests
    {
        [Theory]
        [InlineData(27, (sbyte)1)]  // Vũ khí — SHOP_WEAPON
        [InlineData(28, (sbyte)2)]  // Giáp — SHOP_ARMOUR
        [InlineData(29, (sbyte)3)]  // Mũ — SHOP_HAT
        [InlineData(30, (sbyte)4)]  // Thức ăn — SHOP_FOOD
        public void Dispatch_4Shop_BoiPetServiceEnvelope_DungShopId(int buildingType, sbyte shopId)
        {
            var action = BuildingDispatcher.Dispatch(buildingType);

            Assert.Equal(BuildingAction.Kind.Send, action.Type);
            var round = Message.FromWire(action.Packet.ToWire(), false);

            // Opcode NGOÀI phải là PET_SERVICE (81) — không phải REQUEST_SHOP (2) trần,
            // vì server chỉ đọc REQUEST_SHOP như sub-command bên trong processPet.
            Assert.Equal(GopetCmd.PET_SERVICE, round.Id);
            Assert.Equal(GopetCmd.REQUEST_SHOP, round.Reader.ReadSByte());
            Assert.Equal(shopId, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.Remaining);
        }

        [Fact]
        public void Dispatch_Gym_BoiPetServiceEnvelope_KhongBody()
        {
            var action = BuildingDispatcher.Dispatch(31);

            Assert.Equal(BuildingAction.Kind.Send, action.Type);
            var round = Message.FromWire(action.Packet.ToWire(), false);
            Assert.Equal(GopetCmd.PET_SERVICE, round.Id);
            Assert.Equal(GopetCmd.GYM, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.Remaining);
        }

        [Fact]
        public void Dispatch_PetFollowHeal_CoPet_LaLocalMenu()
        {
            var action = BuildingDispatcher.Dispatch(32, hasPet: true);
            Assert.Equal(BuildingAction.Kind.LocalMenu, action.Type);
        }

        [Fact]
        public void Dispatch_PetFollowHeal_KhongPet_TraToastDungChuJar()
        {
            // jar case 32: cg.a_("Bạn không dẫn theo pet") khi chưa có pet theo — khoá
            // nguyên văn chuỗi vì đây là copy 1-1 từ client cũ.
            var action = BuildingDispatcher.Dispatch(32, hasPet: false);
            Assert.Equal(BuildingAction.Kind.Toast, action.Type);
            Assert.Equal("Bạn không dẫn theo pet", action.ToastText);
        }

        [Fact]
        public void Dispatch_Atm_KhongCoTrongBang_TraNoop()
        {
            // type 9 = ATM: server requestBank() rỗng → BuildingDispatcher không có case
            // riêng → rơi vào default (Noop). Khoá hành vi để không crash khi bấm.
            var action = BuildingDispatcher.Dispatch(9);
            Assert.Equal(BuildingAction.Kind.Noop, action.Type);
        }

        [Theory]
        [InlineData(27)]
        [InlineData(28)]
        [InlineData(29)]
        [InlineData(30)]
        [InlineData(31)]
        [InlineData(32)]
        public void LabelOf_27To32_RongKhopJarEgJava(int buildingType)
        {
            // eg.java:138-155 — case 27-32 đều label rỗng (jar không hiện tên trên các
            // nhà 4 shop/gym/pet-heal này). MapBuildingView chỉ hiện nhãn khi LabelOf
            // khác rỗng, nên khớp jar là đúng — không phải thiếu sót cần thêm nhãn.
            Assert.Equal(string.Empty, BuildingDispatcher.LabelOf(buildingType));
        }

        [Fact]
        public void Dispatch_LoaiKhongXacDinh_TraNoop_KhongNem()
        {
            var action = BuildingDispatcher.Dispatch(999);
            Assert.Equal(BuildingAction.Kind.Noop, action.Type);
        }
    }
}
