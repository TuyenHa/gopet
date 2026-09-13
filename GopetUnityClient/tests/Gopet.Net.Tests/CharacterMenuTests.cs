using Gopet.Net;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Menu char Unity; opcode gửi khớp fr.java + dc.java.</summary>
    public sealed class CharacterMenuTests
    {
        [Fact]
        public void CoDung18Muc()
        {
            // Chat khu vực/cộng đồng đã có ở thanh dưới nên bị ẩn khỏi menu.
            Assert.Equal(18, CharacterMenu.Entries.Count);
            Assert.DoesNotContain(CharacterMenu.Entries,
                entry => entry.Action == CharacterMenuAction.PlaceChat);
            Assert.DoesNotContain(CharacterMenu.Entries,
                entry => entry.Action == CharacterMenuAction.CommunityChat);
        }

        [Theory]
        [InlineData(CharacterMenuAction.FriendManage,   121, 1)]
        [InlineData(CharacterMenuAction.Mail,           121, 13)]
        [InlineData(CharacterMenuAction.Wardrobe,       GopetCmd.PET_SERVICE, 62)]
        [InlineData(CharacterMenuAction.SelectPet,      GopetCmd.PET_SERVICE, 5)]
        [InlineData(CharacterMenuAction.Inventory,      GopetCmd.PET_SERVICE, GopetCmd.NORMAL_INVENTORY)]
        [InlineData(CharacterMenuAction.GemInventory,   GopetCmd.PET_SERVICE, GopetCmd.SHOW_GEM_INVENTORY)]
        [InlineData(CharacterMenuAction.WingInventory,  GopetCmd.PET_SERVICE, GopetCmd.WING)]
        [InlineData(CharacterMenuAction.Tasks,          GopetCmd.PET_SERVICE, GopetCmd.SHOW_LIST_TASK)]
        // Bank: opcode 44 top-level, KHÔNG body → chỉ verify opcode; sub-check bỏ qua
        // bằng way khác dưới đây.
        public void ServerAction_GuiDungOpcodeVaSub(CharacterMenuAction action, int expectedOpcode, int expectedSub)
        {
            Assert.True(CharacterMenu.TryBuildServerMessage(action, out var msg));
            Assert.Equal((sbyte)expectedOpcode, msg.Id);

            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal((sbyte)expectedSub, round.Reader.ReadSByte());
        }

        [Theory]
        [InlineData(CharacterMenuAction.PlaceChat)]
        [InlineData(CharacterMenuAction.CommunityChat)]
        [InlineData(CharacterMenuAction.GuildChat)]
        [InlineData(CharacterMenuAction.ChangePassword)]
        [InlineData(CharacterMenuAction.Channels)]
        [InlineData(CharacterMenuAction.Teleport)]
        [InlineData(CharacterMenuAction.Settings)]
        [InlineData(CharacterMenuAction.AutoAttack)]
        [InlineData(CharacterMenuAction.Logout)]
        [InlineData(CharacterMenuAction.Exit)]
        public void ClientAction_KhongDungMessage(CharacterMenuAction action)
        {
            Assert.False(CharacterMenu.TryBuildServerMessage(action, out var msg));
            Assert.Null(msg);
        }

        [Fact]
        public void BankAction_Wire_44_KhongBody()
        {
            Assert.True(CharacterMenu.TryBuildServerMessage(CharacterMenuAction.Bank, out var msg));
            Assert.Equal(GopetCmd.CHARGE_MONEY_INFO, msg.Id);
            var wire = msg.ToWire();
            Assert.Single(wire);   // chỉ 1 byte opcode, không body
        }

        [Fact]
        public void WingInventory_UsesNestedInventoryType()
        {
            Assert.True(CharacterMenu.TryBuildServerMessage(CharacterMenuAction.WingInventory, out var msg));
            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal(GopetCmd.WING, round.Reader.ReadSByte());
            Assert.Equal(GopetCmd.WING_TYPE_INVENTORY, round.Reader.ReadSByte());
        }

        [Fact]
        public void LabelKhongTrong()
        {
            foreach (var entry in CharacterMenu.Entries)
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Label), $"{entry.Action} thiếu label");
            }
        }

        [Fact]
        public void ClientActionKind_KhopVoiTryBuild()
        {
            foreach (var entry in CharacterMenu.Entries)
            {
                var built = CharacterMenu.TryBuildServerMessage(entry.Action, out _);
                var expected = entry.Kind == CharacterMenuActionKind.Server;
                Assert.Equal(expected, built);
            }
        }

        [Fact]
        public void MenuPhu_ChiConTacVuGameplay()
        {
            var nodes = CharacterMenu.GetNodes(CharacterMenuPage.Main);

            Assert.Equal(3, nodes.Count);
            Assert.Equal("Nhiệm vụ", nodes[0].Label);
            Assert.Equal("Bản đồ", nodes[1].Label);
            Assert.Equal("Hộp thư", nodes[2].Label);
        }

        [Fact]
        public void DichVu_ChiChuaTienIchLauDai()
        {
            var nodes = CharacterMenu.GetNodes(CharacterMenuPage.Services);

            Assert.Collection(nodes,
                n => Assert.Equal(CharacterMenuAction.GemInventory, n.Action),
                n => Assert.Equal(CharacterMenuAction.Bank, n.Action));
        }

        [Fact]
        public void SuKien_RongKhiBackendChuaCongBoSuKien()
        {
            Assert.Empty(CharacterMenu.GetNodes(CharacterMenuPage.Events));
        }
    }
}
