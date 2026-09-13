using System;
using Gopet.Net;
using Gopet.Net.Guider;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Bản đồ <c>buildingType</c> (0-32, từ <c>eg.java</c>) sang hành động client — mở shop
    /// server, opcode top-level, hoặc menu local (mini-game / real-estate). Bảng này là
    /// bản sao 1-1 của <c>eg.java a(Object)</c> case 0-32.
    ///
    /// <para><b>KHÔNG</b> chứa logic UI. Trả <see cref="BuildingAction"/> để caller (UI layer)
    /// tự dịch sang <see cref="Message"/> hoặc mở view local.</para>
    ///
    /// <para><b>Shop opcode</b>: server dùng <c>REQUEST_SHOP</c> (opcode 2 top-level) + 1 sbyte
    /// shopId khớp <c>MenuController.cs:423-441</c>. <c>REQUEST_SHOP_SKIN</c> (60) là opcode
    /// riêng cho thời trang char (đi qua <c>PET_SERVICE</c>).</para>
    /// </summary>
    public static class BuildingDispatcher
    {
        // Shop id — khớp SRCGOPETGOC/GServer/Server/MenuController.cs:423-441.
        private const sbyte ShopWeapon = 1;
        private const sbyte ShopArmour = 2;
        private const sbyte ShopHat = 3;
        private const sbyte ShopFood = 4;
        private const sbyte ShopSkin = 7;
        private const sbyte ShopPet = 8;
        private const sbyte ShopEnergy = 11;

        // PET_SERVICE sub — arena PVP/PVE (jar eg.java case 26: en(81).a(58).a(0)).
        private const sbyte PetServiceArena = GopetCmd.ARENA_MENU;

        /// <summary>Nhãn tiếng Việt hiện trên map (jar eg.java:55-155). Rỗng nghĩa là không đặt tên (server-driven).</summary>
        public static string LabelOf(int buildingType)
        {
            switch (buildingType)
            {
                case 0: return "Nhà hẻm";
                case 1: return "Nhà mặt tiền";
                case 2: return "Biệt thự";
                case 3: return "Dinh thự";
                case 4: return "Nhà";
                case 6: return "Thú cưng";
                case 7: return "Vườn";
                case 8: return "Phòng vé";
                case 10: return "Cà phê";
                case 11: return "Khu";
                case 12: return "Hộp thư";
                case 13: return "Caro";
                case 14: return "Cờ tướng";
                case 15: return "Tiến lên";
                case 16: return "Phỏm";
                case 17: return "Thời trang";
                case 18: return "Nón";
                case 19: return "Giày";
                case 20: return "Mỹ viện";
                case 21: return "Tóc";
                case 22: return "Vật phẩm";
                case 23: return "Gara";
                case 24: return "Trò chơi trong nhà";
                case 25: return "Pet shop";
                case 26: return "Đấu trường";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Sinh <see cref="BuildingAction"/> khi bấm building. <paramref name="hasPet"/>
        /// chỉ có nghĩa với type 32 (cần pet mới gọi được recovery/follow).
        /// </summary>
        public static BuildingAction Dispatch(int buildingType, bool hasPet = true)
        {
            switch (buildingType)
            {
                // JAR tạo các command local tương ứng, nhưng listener dv.java không xử lý chúng.
                // Đây là label/menu chết của client cũ, không phải flow cần migrate.
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 7:
                case 10:
                    return BuildingAction.Noop;

                case 8:  return BuildingAction.LocalMenu(LocalMenu.TicketRoom);
                case 11: return BuildingAction.LocalMenu(LocalMenu.ChangeZone);
                case 12: return BuildingAction.LocalMenu(LocalMenu.Mailbox);

                // JAR tạo command local 602/603/606/607 nhưng dv.java không xử lý chúng;
                // 24 map cũng không đặt type 13-16 và server không có engine tương ứng.
                case 13:
                case 14:
                case 15:
                case 16:
                    return BuildingAction.Noop;

                // case 17 — Thời trang char: (new en(81)).a(60) → PET_SERVICE 60 = REQUEST_SHOP_SKIN.
                case 17: return BuildingAction.Send(Message.Create(GopetCmd.PET_SERVICE)
                    .PutSByte(GopetCmd.REQUEST_SHOP_SKIN));

                // 18/22 là tiện ích Unity mở shop thật; JAR command 1003/1009 là inert.
                // Giữ như extension có chủ đích, không dùng làm bằng chứng parity.
                case 18: return BuildingAction.Send(GuiderPackets.RequestShop(ShopHat));
                case 22: return BuildingAction.Send(GuiderPackets.RequestShop(ShopFood));

                // Các command JAR 1004/1005/1006/1008 cũng không có case trong dv.java.
                case 19:
                case 20:
                case 21:
                case 23:
                    return BuildingAction.Noop;

                case 25: return BuildingAction.Send(GuiderPackets.RequestShop(ShopPet));      // Pet shop

                // case 26 — Đấu trường: (new en(81)).a(58).a(0).
                case 26: return BuildingAction.Send(Message.Create(GopetCmd.PET_SERVICE)
                    .PutSByte(PetServiceArena).PutSByte(0));

                // case 27-30 — dc.d(1..4): en(81).a(2=REQUEST_SHOP).a(shopId). ĐÃ xác nhận
                // bằng dc.java:45-51 + GameController.cs processPet (951-1029): REQUEST_SHOP
                // là sub-command của PET_SERVICE, KHÔNG PHẢI mini-game (bản trước đọc nhầm).
                // shopId khớp MenuController.cs:423-441.
                case 27: return BuildingAction.Send(GuiderPackets.RequestShop(ShopWeapon));  // Vũ khí
                case 28: return BuildingAction.Send(GuiderPackets.RequestShop(ShopArmour));  // Giáp
                case 29: return BuildingAction.Send(GuiderPackets.RequestShop(ShopHat));     // Mũ
                case 30: return BuildingAction.Send(GuiderPackets.RequestShop(ShopFood));    // Thức ăn

                // case 31 — (new en(81)).a(21) — PET_SERVICE 21 = GYM.
                case 31: return BuildingAction.Send(Message.Create(GopetCmd.PET_SERVICE)
                    .PutSByte(GopetCmd.GYM));

                // case 32 — JAR gọi dc.b(petTemplateId), tức PET_SERVICE/MAGIC (11).
                // Server bỏ qua int cũ nên Unity chỉ cần mở profile pet hiện đang theo.
                case 32:
                    return hasPet
                        ? BuildingAction.LocalMenu(LocalMenu.PetProfile)
                        : BuildingAction.Toast("Bạn không dẫn theo pet");

                // case 5/6/9/24/default — jar không làm gì (return sớm).
                default: return BuildingAction.Noop;
            }
        }

        public enum LocalMenu
        {
            None,
            RealEstateNhaHem,
            RealEstateMatTien,
            RealEstateBietThu,
            RealEstateDinhThu,
            RealEstateNha,
            Garden,
            TicketRoom,
            Cafe,
            ChangeZone,
            Mailbox,
            PetShoesKiosk,
            BeautySalon,
            HairSalon,
            Garage,
            PetProfile,
        }
    }

    /// <summary>Kết quả của <see cref="BuildingDispatcher.Dispatch"/>. Immutable.</summary>
    public readonly struct BuildingAction
    {
        public enum Kind { Noop, Send, LocalMenu, Toast }

        public readonly Kind Type;
        public readonly Message Packet;
        public readonly BuildingDispatcher.LocalMenu Menu;
        public readonly string ToastText;

        private BuildingAction(Kind t, Message p, BuildingDispatcher.LocalMenu m, string toast)
        {
            Type = t; Packet = p; Menu = m; ToastText = toast;
        }

        public static readonly BuildingAction Noop =
            new BuildingAction(Kind.Noop, null, BuildingDispatcher.LocalMenu.None, null);

        public static BuildingAction Send(Message m) =>
            new BuildingAction(Kind.Send, m ?? throw new ArgumentNullException(nameof(m)),
                BuildingDispatcher.LocalMenu.None, null);

        public static BuildingAction LocalMenu(BuildingDispatcher.LocalMenu menu) =>
            new BuildingAction(Kind.LocalMenu, null, menu, null);

        public static BuildingAction Toast(string text) =>
            new BuildingAction(Kind.Toast, null, BuildingDispatcher.LocalMenu.None,
                text ?? string.Empty);
    }
}
