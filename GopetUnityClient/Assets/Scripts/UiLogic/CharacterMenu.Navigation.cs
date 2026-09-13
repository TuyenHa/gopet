using System.Collections.Generic;

namespace Gopet.UiLogic
{
    public enum CharacterMenuPage
    {
        Main,
        Character,
        Appearance,
        Pet,
        Adventure,
        Community,
        System,
        Services,
        Events,
    }

    public sealed class CharacterMenuNode
    {
        public string Label { get; }
        public CharacterMenuAction? Action { get; }
        public CharacterMenuPage? TargetPage { get; }

        private CharacterMenuNode(string label, CharacterMenuAction? action, CharacterMenuPage? targetPage)
        {
            Label = label;
            Action = action;
            TargetPage = targetPage;
        }

        public static CharacterMenuNode Run(string label, CharacterMenuAction action) =>
            new CharacterMenuNode(label, action, null);

        public static CharacterMenuNode Open(string label, CharacterMenuPage page) =>
            new CharacterMenuNode(label, null, page);
    }

    public static partial class CharacterMenu
    {
        private static readonly IReadOnlyList<CharacterMenuNode> MainNodes = new[]
        {
            CharacterMenuNode.Run("Nhiệm vụ", CharacterMenuAction.Tasks),
            CharacterMenuNode.Open("Bản đồ", CharacterMenuPage.Adventure),
            CharacterMenuNode.Run("Hộp thư", CharacterMenuAction.Mail),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> CharacterNodes = new[]
        {
            CharacterMenuNode.Run("Rương đồ", CharacterMenuAction.Inventory),
            CharacterMenuNode.Open("Ngoại hình", CharacterMenuPage.Appearance),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> AppearanceNodes = new[]
        {
            CharacterMenuNode.Run("Tủ quần áo", CharacterMenuAction.Wardrobe),
            CharacterMenuNode.Run("Cánh", CharacterMenuAction.WingInventory),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> PetNodes = new[]
        {
            CharacterMenuNode.Run("Chọn pet", CharacterMenuAction.SelectPet),
            CharacterMenuNode.Run("Trang bị pet", CharacterMenuAction.PetEquipment),
            CharacterMenuNode.Run("Kho ngọc", CharacterMenuAction.GemInventory),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> AdventureNodes = new[]
        {
            CharacterMenuNode.Run("Bản đồ thế giới", CharacterMenuAction.Teleport),
            CharacterMenuNode.Run("Đổi khu vực", CharacterMenuAction.Channels),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> CommunityNodes = new[]
        {
            CharacterMenuNode.Run("Bạn bè", CharacterMenuAction.FriendManage),
            CharacterMenuNode.Run("Chat bang hội", CharacterMenuAction.GuildChat),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> SystemNodes = new[]
        {
            CharacterMenuNode.Run("Cài đặt game", CharacterMenuAction.Settings),
            CharacterMenuNode.Run("Đổi mật khẩu", CharacterMenuAction.ChangePassword),
            CharacterMenuNode.Run("Đăng xuất", CharacterMenuAction.Logout),
            CharacterMenuNode.Run("Thoát game", CharacterMenuAction.Exit),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> ServiceNodes = new[]
        {
            CharacterMenuNode.Run("ATM", CharacterMenuAction.Bank),
        };

        private static readonly IReadOnlyList<CharacterMenuNode> EmptyNodes =
            new CharacterMenuNode[0];

        public static IReadOnlyList<CharacterMenuNode> GetNodes(CharacterMenuPage page)
        {
            switch (page)
            {
                case CharacterMenuPage.Main: return MainNodes;
                case CharacterMenuPage.Character: return CharacterNodes;
                case CharacterMenuPage.Appearance: return AppearanceNodes;
                case CharacterMenuPage.Pet: return PetNodes;
                case CharacterMenuPage.Adventure: return AdventureNodes;
                case CharacterMenuPage.Community: return CommunityNodes;
                case CharacterMenuPage.System: return SystemNodes;
                case CharacterMenuPage.Services: return ServiceNodes;
                default: return EmptyNodes;
            }
        }

        public static string GetTitle(CharacterMenuPage page)
        {
            switch (page)
            {
                case CharacterMenuPage.Main: return "Menu";
                case CharacterMenuPage.Character: return "Nhân vật";
                case CharacterMenuPage.Appearance: return "Ngoại hình";
                case CharacterMenuPage.Pet: return "Pet";
                case CharacterMenuPage.Adventure: return "Bản đồ";
                case CharacterMenuPage.Community: return "Cộng đồng";
                case CharacterMenuPage.System: return "Cài đặt";
                case CharacterMenuPage.Services: return "Dịch vụ";
                case CharacterMenuPage.Events: return "Sự kiện";
                default: return "Menu";
            }
        }
    }
}
