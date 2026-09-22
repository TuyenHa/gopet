using System;
using Gopet.Net;
using Gopet.Net.Battle;
using Gopet.Net.Guider;
using Gopet.Net.Map;
using Gopet.Net.Pet;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Trận PvE thật end-to-end (P7 Pet Battle "Nghiệm thu còn lại"): nhận pet miễn phí
    /// tại NPC -1 (TRAN CHAN, có trên map 11) → chọn pet theo → warp sang map 13 (Linh
    /// Lâm, có quái — xác nhận qua <c>gopet_mob_location</c>) → bắt quái thật → đánh 1
    /// đòn → đọc turn.
    ///
    /// <para>Đây là kiểm tra WIRE FORMAT với dữ liệu THẬT lấy trực tiếp từ
    /// <c>PetBattle.sendStartFightMob</c>/<c>onTurn</c> của server — mạnh hơn 479 unit
    /// test hiện có (chạy trên fixture tự dựng) vì mọi field <c>Reader.ExpectFullyConsumed</c>
    /// đều phải khớp byte thật, không phải giả lập.</para>
    ///
    /// <para><b>Đăng ký SỚM, trigger SAU</b> — cùng bài học với <c>MapMovementChecks</c>:
    /// <c>SEND_LIST_MOB_ZONE</c> tới ngay khi <c>GopetPlace.add()</c> chạy (lúc warp xong),
    /// trước khi code này kịp gọi <c>RegisterOn</c> nếu làm muộn → gói bị router bỏ.</para>
    /// </summary>
    internal static class BattleChecks
    {
        // NPC "TRAN CHAN" trên map 11 — option "Nhận pet miễn phí" (OP_LIST_PET_FREE=1).
        // internal: PvpBattleChecks dùng lại cho cả 2 tài khoản (giống hệt logic, khác account).
        internal const int FreePetNpcId = -1;
        internal const int FreePetOptionId = 1;

        // MENU_LIST_PET_FREE / MENU_PET_INVENTORY — MenuController.cs:26,31.
        internal const int MenuListPetFree = 0;
        internal const int MenuPetInventory = 5;

        // Map 13 (Linh Lâm) — đích portal có sẵn từ map 11, xác nhận có 12 quái qua
        // gopet_mob_location. Map 11 (hub) không có quái nào.
        private const int MapWithMobs = 13;

        /// <returns>
        /// Handler thế giới đã đăng ký, cho <see cref="PetInteractChecks"/> dùng lại —
        /// <c>MessageRouter</c> cấm hai handler cùng một sub-command.
        /// </returns>
        public static WorldObjectHandler Run(GopetSocket socket, MessageRouter router, GuiderHandler guider,
            MapHandler mapHandler, int userId)
        {
            // Đăng ký TRƯỚC mọi send — quái/battle push không đợi client xin. Đăng ký SỚM hơn
            // nữa (ở check chạy trước) lại hỏng: SEND_LIST_MOB_ZONE tới trong lúc check đó đang
            // bơm, handler chưa có ai nghe MobsReceived nên danh sách quái rơi mất.
            MobSpawn[] mobs = null;
            var world = new WorldObjectHandler();
            world.MobsReceived += m => mobs = m;
            world.RegisterOn(router);

            var battle = new BattleHandler(socket.Send, userId);
            BattleStart started = null;
            BattleTurn turn = null;
            battle.BattleStarted += b => started = b;
            battle.TurnReceived += t => turn = t;
            battle.RegisterOn(router);

            MenuScreen freePetMenu = null;
            MenuScreen petInventory = null;
            guider.MenuShown += m =>
            {
                if (m.ListId == MenuListPetFree) freePetMenu = m;
                else if (m.ListId == MenuPetInventory) petInventory = m;
            };

            if (!AcquireFreePet(socket, router, guider, () => freePetMenu)) return world;
            if (!SelectPetToFollow(socket, router, () => petInventory)) return world;
            RemainingParityChecks.Run(socket, router);
            PetRecoveryChecks.Run(socket, router);
            if (!WarpToMobMap(socket, router, mapHandler)) return world;
            if (!WaitForMobs(socket, router, () => mobs)) return world;

            RunOneTurn(socket, router, battle, () => started, () => turn, mobs[0].Id);
            return world;
        }

        /// <summary>Nói chuyện NPC free-pet, chọn dòng đầu. Dùng chung cho PvE lẫn PvP (2 tài khoản).</summary>
        internal static bool AcquireFreePet(GopetSocket socket, MessageRouter router, GuiderHandler guider,
            Func<MenuScreen> menu, string tag = "")
        {
            NpcOptions options = null;
            guider.NpcOptionsShown += o => options = o;
            guider.TalkToNpc(FreePetNpcId);
            MessagePump.Until(socket, router, () => options != null, TimeSpan.FromSeconds(5));

            Report.Check($"X{tag}. Nói chuyện NPC free-pet (map 11) — nhận được danh sách lựa chọn",
                options != null && options.NpcId == FreePetNpcId,
                options == null ? "không nhận được NPC_OPTION trong 5s" : $"npcId={options.NpcId}");
            if (options == null) return false;

            guider.SelectNpcOption(FreePetNpcId, FreePetOptionId);
            MessagePump.Until(socket, router, () => menu() != null, TimeSpan.FromSeconds(5));

            var freePetMenu = menu();
            Report.Check($"Y{tag}. Server trả danh sách pet miễn phí (MENU_LIST_PET_FREE)",
                freePetMenu != null && freePetMenu.Items.Length > 0,
                freePetMenu == null ? "không nhận SHOW_MENU_ITEM trong 5s" : "danh sách rỗng — đã nhận pet free từ lần chạy trước?");
            if (freePetMenu == null || freePetMenu.Items.Length == 0) return false;

            Console.WriteLine($"       -> {freePetMenu.Items.Length} pet miễn phí, chọn dòng đầu \"{freePetMenu.Items[0].Title}\"");
            guider.Select(freePetMenu, 0);
            return true;
        }

        /// <summary>Mở túi pet, chọn dòng đầu làm pet theo. Dùng chung cho PvE lẫn PvP.</summary>
        internal static bool SelectPetToFollow(GopetSocket socket, MessageRouter router, Func<MenuScreen> menu,
            string tag = "")
        {
            socket.Send(PetEquipPackets.RequestPetInventory());
            MessagePump.Until(socket, router, () => menu() != null, TimeSpan.FromSeconds(5));

            var inventory = menu();
            Report.Check($"Z{tag}. Túi pet (PET_INVENTORY) có pet vừa nhận",
                inventory != null && inventory.Items.Length > 0,
                inventory == null ? "không nhận SHOW_MENU_ITEM trong 5s" : "túi rỗng — addPet không chạy?");
            if (inventory == null || inventory.Items.Length == 0) return false;

            // Chọn dòng đầu → server set petSelected = pet (MenuController.selectMenu.cs:450).
            socket.Send(GuiderPackets.SelectMenuElement(inventory.ListId, inventory.Items[0].ItemId));
            return true;
        }

        private static bool WarpToMobMap(GopetSocket socket, MessageRouter router, MapHandler mapHandler)
        {
            MapUpdate update = null;
            mapHandler.MapUpdated += e => update = e;
            mapHandler.SendWarp(MapWithMobs, 0, 1);
            MessagePump.Until(socket, router, () => update != null && update.MapId == MapWithMobs,
                TimeSpan.FromSeconds(5));

            Report.Check($"AA. Warp sang map {MapWithMobs} (Linh Lâm, có quái)",
                update != null && update.MapId == MapWithMobs,
                update == null ? "không nhận MAP_UPDATE trong 5s" : $"vào map {update.MapId}, kỳ vọng {MapWithMobs}");
            return update != null && update.MapId == MapWithMobs;
        }

        private static bool WaitForMobs(GopetSocket socket, MessageRouter router, Func<MobSpawn[]> mobs)
        {
            // Server tự đẩy SEND_LIST_MOB_ZONE ngay khi vào place (GopetPlace.add → sendMob) —
            // không gửi gì, chỉ bơm hàng đợi (gói có thể đã tới trong lúc WarpToMobMap chờ).
            MessagePump.Until(socket, router, () => mobs() != null, TimeSpan.FromSeconds(5));

            var list = mobs();
            Report.Check("BB. Nhận danh sách quái map 13 (SEND_LIST_MOB_ZONE)",
                list != null && list.Length > 0,
                list == null ? "không nhận trong 5s" : "danh sách rỗng");
            if (list == null || list.Length == 0) return false;

            Console.WriteLine($"       -> {list.Length} quái, bắt \"{list[0].Name}\" lv{list[0].Level} (id={list[0].Id})");
            return true;
        }

        private static void RunOneTurn(GopetSocket socket, MessageRouter router, BattleHandler battle,
            Func<BattleStart> started, Func<BattleTurn> turn, int mobId)
        {
            battle.SendAttackMob(mobId);
            MessagePump.Until(socket, router, () => started() != null, TimeSpan.FromSeconds(5));

            var start = started();
            Report.Check("CC. ATTACK_MOB bắt đầu trận — parse LocalPet/Opponent sạch (byte thật)",
                start != null && start.IsParticipant,
                start == null ? "không nhận PET_SERVICE/ATTACK_MOB trong 5s" : "IsParticipant=false — không phải trận của mình");
            if (start == null) return;

            Console.WriteLine($"       -> trận #{start.BattleId}: {start.LocalPet.Name} lv{start.LocalPet.Level} " +
                              $"vs {start.Opponent.Name} lv{start.Opponent.Level}");

            battle.SendNormalAttack();
            MessagePump.Until(socket, router, () => turn() != null, TimeSpan.FromSeconds(5));

            var t = turn();
            Report.Check("DD. PET_BATTLE turn nhận được — parse effects sạch (byte thật)",
                t != null,
                "không nhận PET_SERVICE/PET_BATTLE trong 5s sau khi đánh thường");

            if (t != null)
            {
                Console.WriteLine($"       -> turn actor=#{t.ActorId} type={t.Type} {t.Effects.Length} effect");
            }
        }
    }
}
