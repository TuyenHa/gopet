using System;
using Gopet.Net.Map;
using Gopet.Net.Pet;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Hôn / chơi / xoa đầu pet đi trọn vòng: client gửi <c>PET_SERVICE 17 / type</c>
    /// (đúng như jar <c>dc.a(int)</c>), server phát lại <c>ON_PET_INTERACT</c> cho cả
    /// khu vực kèm <c>user_id</c> + <c>type</c>, client đọc sạch gói.
    ///
    /// <para>Có bài này vì server từng KHÔNG có nhánh cho sub-command 17: gói gửi đi rơi
    /// vào default, không ai báo lỗi, và trong game bấm nút thì tịt. Bắt bằng mắt rất khó
    /// nên chốt bằng smoke.</para>
    ///
    /// <para>Dùng lại <see cref="WorldObjectHandler"/> do <see cref="BattleChecks"/> dựng
    /// (nên phải chạy SAU nó): <c>MessageRouter</c> cấm hai handler cùng một sub-command.</para>
    /// </summary>
    internal static class PetInteractChecks
    {
        public static void Run(GopetSocket socket, MessageRouter router, int userId,
            WorldObjectHandler world)
        {
            PetInteraction last = null;
            world.PetInteractionReceived += e => last = e;

            CheckOne(socket, router, userId, PetActionPackets.Kiss, "hôn",
                "MM. ON_PET_INTERACT vọng lại khi hôn pet", () => last, v => last = v);
            CheckOne(socket, router, userId, PetActionPackets.Play, "chơi với",
                "SS. ON_PET_INTERACT vọng lại khi chơi với pet", () => last, v => last = v);
            CheckOne(socket, router, userId, PetActionPackets.Poke, "xoa đầu",
                "TT. ON_PET_INTERACT vọng lại khi xoa đầu pet", () => last, v => last = v);
        }

        private static void CheckOne(GopetSocket socket, MessageRouter router,
            int userId, sbyte type, string verb, string title,
            Func<PetInteraction> read, Action<PetInteraction> reset)
        {
            reset(null);
            // Server chặn spam 500ms giữa hai lần tương tác (GameController.petInteract),
            // nên ba lần gọi liên tiếp phải chờ qua cửa sổ đó, không thì lần sau bị nuốt.
            System.Threading.Thread.Sleep(600);
            socket.Send(PetActionPackets.Interact(type));

            MessagePump.Until(socket, router, () => read() != null, TimeSpan.FromSeconds(5));

            var got = read();
            Report.Check(title,
                got != null && got.UserId == userId && got.Type == type,
                got == null
                    ? $"không nhận ON_PET_INTERACT trong 5s sau khi {verb} pet"
                    : $"nhận user_id={got.UserId} type={got.Type}, kỳ vọng {userId}/{type}");
        }
    }
}
