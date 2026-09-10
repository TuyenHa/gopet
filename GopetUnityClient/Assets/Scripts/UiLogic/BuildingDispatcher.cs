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
        private const sbyte PetServiceArena = 58;

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
                // Real-estate (case 0..4): jar cd(2/3/4/5/502) là MENU LOCAL — chưa có client dialog.
                case 0: return BuildingAction.LocalMenu(LocalMenu.RealEstateNhaHem);
                case 1: return BuildingAction.LocalMenu(LocalMenu.RealEstateMatTien);
                case 2: return BuildingAction.LocalMenu(LocalMenu.RealEstateBietThu);
                case 3: return BuildingAction.LocalMenu(LocalMenu.RealEstateDinhThu);
                case 4: return BuildingAction.LocalMenu(LocalMenu.RealEstateNha);

                case 7:  return BuildingAction.LocalMenu(LocalMenu.Garden);
                case 8:  return BuildingAction.LocalMenu(LocalMenu.TicketRoom);
                case 10: return BuildingAction.LocalMenu(LocalMenu.Cafe);
                case 11: return BuildingAction.LocalMenu(LocalMenu.ChangeZone);
                case 12: return BuildingAction.LocalMenu(LocalMenu.Mailbox);

                // Mini-game bàn cờ: jar cd(602/603/606/607) — scene game chưa có (Phase 8 plan).
                case 13: return BuildingAction.LocalMenu(LocalMenu.MiniGameCaro);
                case 14: return BuildingAction.LocalMenu(LocalMenu.MiniGameCoTuong);
                case 15: return BuildingAction.LocalMenu(LocalMenu.MiniGameTienLen);
                case 16: return BuildingAction.LocalMenu(LocalMenu.MiniGamePhom);

                // case 17 — Thời trang char: (new en(81)).a(60) → PET_SERVICE 60 = REQUEST_SHOP_SKIN.
                case 17: return BuildingAction.Send(Message.Create(GopetCmd.PET_SERVICE)
                    .PutSByte(GopetCmd.REQUEST_SHOP_SKIN));

                // case 18-23: jar cd(1003/1004/1005/1006/1009/1008) là MENU LOCAL client-side
                // (mở KIOSK dialog để chọn item bán/mua). Ở Unity chưa có KIOSK dialog; nhưng
                // đích cuối cùng đa số là mở shop server tương ứng. Ưu tiên gói REQUEST_SHOP
                // (PET_SERVICE sub-command — xem GuiderPackets.RequestShop) nơi server đã có
                // id shop; còn lại giữ LocalMenu.
                case 18: return BuildingAction.Send(GuiderPackets.RequestShop(ShopHat));      // Nón
                case 19: return BuildingAction.LocalMenu(LocalMenu.PetShoesKiosk);            // Giày pet — không có SHOP_SHOES; kiosk local
                case 20: return BuildingAction.LocalMenu(LocalMenu.BeautySalon);              // Mỹ viện — chưa có shopId
                case 21: return BuildingAction.LocalMenu(LocalMenu.HairSalon);                // Tóc char — chưa có shopId
                case 22: return BuildingAction.Send(GuiderPackets.RequestShop(ShopFood));     // Vật phẩm/thức ăn battle
                case 23: return BuildingAction.LocalMenu(LocalMenu.Garage);                   // Gara xe — chưa có shopId

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

                // case 32 — heal pet: yêu cầu có pet. Ở đây dùng LocalMenu để UI layer trigger
                // pet follow / recovery flow đang có sẵn.
                case 32:
                    return hasPet
                        ? BuildingAction.LocalMenu(LocalMenu.PetFollowTrigger)
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
            MiniGameCaro,
            MiniGameCoTuong,
            MiniGameTienLen,
            MiniGamePhom,
            PetShoesKiosk,
            BeautySalon,
            HairSalon,
            Garage,
            PetFollowTrigger,
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
