using System;
using Gopet.Net.Pet;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// "Hồi phục" là CÔNG TẮC (<c>PET_SERVICE 45 / 1|0</c>, jar <c>dc.a(boolean)</c>), không
    /// phải lệnh một nhát: bật thì server cộng 20% HP/MP pet mỗi 3 giây và bơm
    /// <c>MY_PET_INFO</c> theo từng nhịp, tới khi nhận gói tắt (<c>Player.cs:377</c>).
    ///
    /// <para>Bài này chốt cả hai đầu công tắc — bật thấy nhịp, tắt thì hết nhịp. Thiếu vế
    /// tắt là cờ <c>isPetRecovery</c> kẹt bật cả phiên mà không ai biết.</para>
    ///
    /// <para>Chạy trước khi đánh quái còn vì lý do thực dụng: pet của tài khoản smoke
    /// thường còn máu âm từ trận của lần chạy trước, mà <c>GopetPlace.startFightMob</c>
    /// đòi <c>hp &gt; 0</c> — không hồi trước thì ATTACK_MOB bị nuốt im lặng.</para>
    /// </summary>
    internal static class PetRecoveryChecks
    {
        public static void Run(GopetSocket socket, MessageRouter router)
        {
            var pets = new PetZoneHandler();
            MyPetInfo latest = null;
            pets.MyPetInfoReceived += info => latest = info;
            pets.RegisterOn(router);

            socket.Send(PetActionPackets.Heal(true));
            // Nhịp hồi là 3s; pet máu âm cần vài nhịp mới dương lại (mỗi nhịp +20% maxHp).
            var alive = MessagePump.Until(socket, router, () => latest != null && latest.Hp > 0,
                                          TimeSpan.FromSeconds(20));
            Report.Check("UU. PET_RECOVERY_HP bật — server bơm MY_PET_INFO và HP pet dương lại",
                alive,
                latest == null ? "không nhận MY_PET_INFO nào trong 20s"
                               : $"hp={latest.Hp}/{latest.MaxHp} sau 20s");
            if (latest != null) Console.WriteLine($"       -> hp={latest.Hp}/{latest.MaxHp} mp={latest.Mp}/{latest.MaxMp}");

            socket.Send(PetActionPackets.Heal(false));
            // Rút sạch nhịp đang trên đường rồi mới canh im lặng, không thì bắt nhầm gói cũ.
            MessagePump.Until(socket, router, () => false, TimeSpan.FromSeconds(1));
            latest = null;
            var ticked = MessagePump.Until(socket, router, () => latest != null, TimeSpan.FromSeconds(7));
            Report.Check("VV. PET_RECOVERY_HP tắt — nhịp hồi dừng hẳn",
                !ticked,
                "vẫn nhận MY_PET_INFO sau khi gửi gói tắt — cờ isPetRecovery còn bật");
        }
    }
}
