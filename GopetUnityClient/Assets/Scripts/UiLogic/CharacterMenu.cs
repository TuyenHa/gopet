using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Bank;

namespace Gopet.UiLogic
{
    /// <summary>
    /// 12 mục menu chính của nhân vật, đối chiếu <c>fr.java:129-141</c> (jar J2ME).
    ///
    /// <para><b>Cân nhắc đã chốt:</b> "Chat" (jar cd 3286) trùng ý với "Chat khu vực"
    /// (cd 331) — jar giữ cả hai để tương thích cũ, Unity gộp làm một dòng.</para>
    /// </summary>
    public enum CharacterMenuAction
    {
        FriendManage,
        Mail,
        CommunityChat,
        GuildChat,
        PlaceChat,
        Wardrobe,
        SelectPet,
        PetEquipment,
        Inventory,
        GemInventory,
        WingInventory,
        Channels,
        Teleport,
        Tasks,
        AutoAttack,
        Bank,
        ChangePassword,
        Settings,
        Logout,
        Exit,
    }

    /// <summary>Kiểu xử lý cho một action: gửi opcode lên server, hay xử lý client-side.</summary>
    public enum CharacterMenuActionKind
    {
        /// <summary>Gửi opcode. UI kết quả tới sau qua các handler khác.</summary>
        Server,
        /// <summary>Xử lý ngay ở client (Cài đặt/Đăng xuất/Thoát/mở view riêng).</summary>
        Client,
    }

    public sealed class CharacterMenuEntry
    {
        public CharacterMenuAction Action { get; }
        public string Label { get; }
        public CharacterMenuActionKind Kind { get; }

        internal CharacterMenuEntry(CharacterMenuAction action, string label, CharacterMenuActionKind kind)
        {
            Action = action;
            Label = label;
            Kind = kind;
        }
    }

    /// <summary>
    /// Nguồn duy nhất về "menu char có gì" và "mỗi mục gửi opcode nào".
    /// Runtime layer nạp danh sách <see cref="Entries"/> vào view, và dùng
    /// <see cref="TryBuildServerMessage"/> để dựng gói khi bấm.
    /// </summary>
    public static class CharacterMenu
    {
        public static IReadOnlyList<CharacterMenuEntry> Entries { get; } = new[]
        {
            new CharacterMenuEntry(CharacterMenuAction.FriendManage,   "Bạn bè",         CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.Mail,           "Hộp thư",        CharacterMenuActionKind.Server),
            // Chat cộng đồng/bang: mở panel chat cần channel-picker riêng (Phase 5).
            // Không gửi opcode ở lần bấm menu — chỉ khi Submit text mới gửi CHAT_GLOBAL / GUILD_CHAT.
            new CharacterMenuEntry(CharacterMenuAction.CommunityChat,  "Chat cộng đồng", CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.GuildChat,      "Chat bang hội",  CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.PlaceChat,      "Chat khu vực",   CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Wardrobe,       "Tủ quần áo",     CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.SelectPet,      "Chọn pet",       CharacterMenuActionKind.Server),
            // Trang bị pet: gửi EQUIP_INFO với userId self → server bơm EQUIP_INFO xuống.
            // Client là kind=Client vì phải biết localUserId; GameSession dispatch riêng.
            new CharacterMenuEntry(CharacterMenuAction.PetEquipment,   "Trang bị pet",   CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Inventory,      "Rương đồ",        CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.GemInventory,   "Kho ngọc",        CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.WingInventory,  "Cánh",            CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.Channels,       "Đổi khu vực",    CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Teleport,       "Bản đồ",         CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Tasks,          "Nhiệm vụ",       CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.AutoAttack,     "Tự đánh quái",   CharacterMenuActionKind.Server),
            // Ngân hàng (ATM): trigger opcode 44 top-level → server bơm ListOption 3 dòng
            // qua Guider generic — không cần UI riêng.
            new CharacterMenuEntry(CharacterMenuAction.Bank,           "Ngân hàng",      CharacterMenuActionKind.Server),
            new CharacterMenuEntry(CharacterMenuAction.ChangePassword, "Đổi mật khẩu",   CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Settings,       "Cài đặt",        CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Logout,         "Đăng xuất",      CharacterMenuActionKind.Client),
            new CharacterMenuEntry(CharacterMenuAction.Exit,           "Thoát",          CharacterMenuActionKind.Client),
        };

        /// <summary>
        /// Dựng gói gửi lên server cho action loại <see cref="CharacterMenuActionKind.Server"/>.
        /// Client-side actions trả <c>false</c> — caller xử lý riêng.
        ///
        /// <para>Opcode/subcommand đối chiếu jar (<c>fr.java</c> + <c>dc.java</c>):</para>
        /// </summary>
        public static bool TryBuildServerMessage(CharacterMenuAction action, out Message message)
        {
            switch (action)
            {
                case CharacterMenuAction.FriendManage:
                    // 121/1 = mở danh sách bạn (COMMAND_FRIEND family). Phase 4 register response.
                    message = Message.Create(121).PutSByte(1);
                    return true;

                case CharacterMenuAction.Mail:
                    // 121/13 = LETTER_BOX = mở hộp thư (server showLetterBox).
                    // Server trả về LETTER_COMMAND với sub=INT (không phải sbyte!) — xem LetterHandler.
                    message = Message.Create(121).PutSByte(13);
                    return true;

                case CharacterMenuAction.Wardrobe:
                    // Jar fr.java cd 330 gửi `new en(81)).a(62)` — opcode 81 (PET_SERVICE),
                    // sub 62 (SKIN_INVENTORY). Server processPet đã dispatch case 62 →
                    // MenuController.sendMenu(MENU_SKIN_INVENTORY) trả về SHOW_MENU_ITEM
                    // qua GuiderHandler generic — hạ tầng có sẵn xử lý được.
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(62);
                    return true;

                case CharacterMenuAction.SelectPet:
                    // 81/5 = mở chọn pet (fr.java:238).
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(5);
                    return true;

                case CharacterMenuAction.Inventory:
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.NORMAL_INVENTORY);
                    return true;

                case CharacterMenuAction.GemInventory:
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.SHOW_GEM_INVENTORY);
                    return true;

                case CharacterMenuAction.WingInventory:
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.WING)
                        .PutSByte(GopetCmd.WING_TYPE_INVENTORY);
                    return true;

                case CharacterMenuAction.Bank:
                    // TOP-LEVEL opcode 44 — Player.cs:143 bắt trước controller, mở MENU_ATM
                    // qua Guider generic. Xem BankPackets.OpenBankMenu.
                    message = BankPackets.OpenBankMenu();
                    return true;

                case CharacterMenuAction.Tasks:
                    // PET_SERVICE/54 mở danh sách nhiệm vụ đang nhận; các màn tiếp theo
                    // đều dùng MenuScreen/ListOption server-driven.
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.SHOW_LIST_TASK);
                    return true;

                case CharacterMenuAction.AutoAttack:
                    // Server chọn quái rảnh đầu tiên trong khu và bắt đầu battle nếu nhân vật
                    // chưa chiến đấu. Đây là một lần kích hoạt, không phải vòng spam client.
                    message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.AUTO_ATTACK_SUPPORT);
                    return true;

                default:
                    message = null;
                    return false;
            }
        }
    }
}
