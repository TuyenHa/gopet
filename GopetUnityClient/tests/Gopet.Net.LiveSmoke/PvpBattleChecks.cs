using System;
using System.Diagnostics;
using System.Threading;
using Gopet.Net;
using Gopet.Net.Battle;
using Gopet.Net.Guider;
using Gopet.Net.Social;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Trận PvP thật, 2 tài khoản riêng — phần "Nghiệm thu còn lại" cuối của P7.
    ///
    /// <para><b>Vì sao 2 tài khoản riêng, không tái dùng account PvE ở trên</b>: PvE để
    /// lại trận đang dở (<c>player.controller.getPetBattle() != null</c>), khiến
    /// <c>PLAYER_CHALLENGE</c> bị chặn ("PlayerHasABatte"). <c>gopetpvp1</c>/<c>gopetpvp2</c>
    /// được tạo riêng thẳng vào DB web (<c>gopettae_gopet_web.user</c>, bcrypt cost 12
    /// khớp <c>GopetHashHelper.ComputeHash</c>) vì <c>REGISTER</c> bị khoá cứng phía server
    /// (<c>Player.cs</c>, xem <see cref="RegisterCheck"/>) — không có đường nào khác để có
    /// tài khoản thứ hai trên môi trường test cục bộ này.</para>
    ///
    /// <para><b>Luồng thật</b> (đối chiếu <c>GameController.cs:3570-3752</c> +
    /// <c>MenuController.answerYesNo.cs:144</c>):
    /// A gửi <c>PLAYER_CHALLENGE</c>(B) → nhận <c>MENU_INTIVE_CHALLENGE</c> (1032, chọn mức
    /// cược) → server gửi B hộp thoại Có/Không (<c>DIALOG_INVITE_CHALLENGE</c>=6) → B trả
    /// lời Có → server <c>startChallenge()</c> → <c>PLAYER_BATTLE</c> (59) — HAI gói RIÊNG,
    /// một cho A một cho B, không phải broadcast chung (<c>PetBattle.sendStartFightPlayer</c>).</para>
    /// </summary>
    internal static class PvpBattleChecks
    {
        private const string UserA = "gopetpvp1";
        private const string UserB = "gopetpvp2";
        private const string Pass = "abc12345";

        private const int MenuInviteChallenge = 1032; // MenuController.MENU_INTIVE_CHALLENGE
        private const int DialogInviteChallenge = 6;  // MenuController.DIALOG_INVITE_CHALLENGE

        public static void Run(string host, int port)
        {
            var a = SmokeLogin.Connect(host, port, UserA, Pass);
            Report.Check("EE. Đăng nhập tài khoản PvP #1 (gopetpvp1)", a != null, "không đăng nhập được trong thời gian chờ");
            if (a == null) return;

            var b = SmokeLogin.Connect(host, port, UserB, Pass);
            Report.Check("FF. Đăng nhập tài khoản PvP #2 (gopetpvp2)", b != null, "không đăng nhập được trong thời gian chờ");
            if (b == null) { a.Dispose(); return; }

            try
            {
                var guiderA = new GuiderHandler(a.Socket.Send);
                guiderA.RegisterOn(a.Router);
                var guiderB = new GuiderHandler(b.Socket.Send);
                guiderB.RegisterOn(b.Router);

                MenuScreen freePetA = null, freePetB = null, invA = null, invB = null;
                guiderA.MenuShown += m =>
                {
                    if (m.ListId == BattleChecks.MenuListPetFree) freePetA = m;
                    else if (m.ListId == BattleChecks.MenuPetInventory) invA = m;
                };
                guiderB.MenuShown += m =>
                {
                    if (m.ListId == BattleChecks.MenuListPetFree) freePetB = m;
                    else if (m.ListId == BattleChecks.MenuPetInventory) invB = m;
                };

                if (!BattleChecks.AcquireFreePet(a.Socket, a.Router, guiderA, () => freePetA, "-A")) return;
                if (!BattleChecks.AcquireFreePet(b.Socket, b.Router, guiderB, () => freePetB, "-B")) return;
                if (!BattleChecks.SelectPetToFollow(a.Socket, a.Router, () => invA, "-A")) return;
                if (!BattleChecks.SelectPetToFollow(b.Socket, b.Router, () => invB, "-B")) return;

                RunChallenge(a, guiderA, b, guiderB);
            }
            finally
            {
                a.Dispose();
                b.Dispose();
            }
        }

        private static void RunChallenge(SmokeLogin.Session a, GuiderHandler guiderA,
            SmokeLogin.Session b, GuiderHandler guiderB)
        {
            var battleA = new BattleHandler(a.Socket.Send, a.Success.UserId);
            var battleB = new BattleHandler(b.Socket.Send, b.Success.UserId);
            BattleStart startedA = null, startedB = null;
            battleA.BattleStarted += s => startedA = s;
            battleB.BattleStarted += s => startedB = s;
            battleA.RegisterOn(a.Router);
            battleB.RegisterOn(b.Router);

            // MENU_INTIVE_CHALLENGE chỉ 3 dòng giá tiền (không icon) → server dùng
            // GUIDER_LIST_OPTION (sub 3), KHÔNG PHẢI SHOW_MENU_ITEM — giống màn ATM đổi
            // vàng/ngọc ở check Q. Bắt sai loại thì im lặng chờ hết 5s không có gì.
            ListOptionScreen inviteMenu = null;
            guiderA.ListOptionShown += m => { if (m.ListId == MenuInviteChallenge) inviteMenu = m; };

            a.Socket.Send(TargetPlayerPackets.Challenge(b.Success.UserId));
            Pump(a, () => inviteMenu != null, 5);

            Report.Check("GG. PLAYER_CHALLENGE mở menu mức cược (MENU_INTIVE_CHALLENGE)",
                inviteMenu != null && inviteMenu.Options.Length > 0,
                inviteMenu == null ? "không nhận GUIDER_LIST_OPTION trong 5s" : "danh sách rỗng");
            if (inviteMenu == null || inviteMenu.Options.Length == 0) return;

            Console.WriteLine($"       -> mức cược: {string.Join(" | ", Array.ConvertAll(inviteMenu.Options, o => o.Text))}");

            // Mức cược thấp nhất (2000 ngọc) — tài khoản mới mặc định 50000, đủ trả.
            guiderA.Select(inviteMenu, 0);

            YesNoRequest ask = null;
            guiderB.YesNoAsked += y => { if (y.DialogId == DialogInviteChallenge) ask = y; };
            Pump(b, () => ask != null, 5);

            Report.Check("HH. Đối phương nhận hộp thoại xác nhận thách đấu",
                ask != null, "không nhận SERVER_MESSAGE/SEND_YES_NO trong 5s");
            if (ask == null) return;

            Console.WriteLine($"       -> B nhận: \"{ask.Text}\"");
            guiderB.AnswerYesNo(ask.DialogId, true);

            // Cả hai đầu đều phải nhận PLAYER_BATTLE — hai gói RIÊNG (khác LocalStarts),
            // không phải broadcast chung, nên bơm CẢ HAI socket song song.
            var clock = Stopwatch.StartNew();
            while ((startedA == null || startedB == null) && clock.Elapsed < TimeSpan.FromSeconds(5))
            {
                DrainOnce(a);
                DrainOnce(b);
                Thread.Sleep(20);
            }

            Report.Check("II. PLAYER_BATTLE tới CẢ HAI đầu, IsParticipant đúng — parse byte thật",
                startedA != null && startedA.IsParticipant && startedB != null && startedB.IsParticipant,
                $"A={(startedA == null ? "không nhận" : $"IsParticipant={startedA.IsParticipant}")}; " +
                $"B={(startedB == null ? "không nhận" : $"IsParticipant={startedB.IsParticipant}")}");

            if (startedA != null)
            {
                Console.WriteLine($"       -> A thấy: {startedA.LocalPet.Name} (mình) vs {startedA.Opponent.Name} " +
                                  $"(LocalStarts={startedA.LocalStarts})");
            }
            if (startedB != null)
            {
                Console.WriteLine($"       -> B thấy: {startedB.LocalPet.Name} (mình) vs {startedB.Opponent.Name} " +
                                  $"(LocalStarts={startedB.LocalStarts})");
            }
        }

        private static void Pump(SmokeLogin.Session s, Func<bool> until, int seconds) =>
            MessagePump.Until(s.Socket, s.Router, until, TimeSpan.FromSeconds(seconds), s.Auth.Tick);

        private static void DrainOnce(SmokeLogin.Session s)
        {
            while (s.Socket.Incoming.TryDequeue(out var message)) s.Router.Dispatch(message);
        }
    }
}
