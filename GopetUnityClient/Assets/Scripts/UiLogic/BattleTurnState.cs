using Gopet.Net.Battle;

namespace Gopet.UiLogic
{
    /// <summary>Trạng thái lượt phía client cho một trận đang mở.
    ///
    /// <para><b>Điểm dễ hiểu sai nhất:</b> trường <c>ActorId</c> của gói
    /// <c>PET_BATTLE</c> (37) là <b>người VỪA ra đòn</b>, không phải người sắp đánh.
    /// Server gửi gói rồi mới lật lượt (<c>PetBattle.cs:307-308</c> cho đánh thường,
    /// <c>:1109-1110</c> cho kỹ năng). Nên lượt kế tiếp thuộc về bên còn lại.</para>
    ///
    /// <para>Thay cho cách khoá nút bằng đồng hồ 3.5s trước đây: jar gốc khoá theo
    /// <b>trạng thái lượt</b> (<c>fr.b</c>/<c>fr.c</c> giữ sub-command đang chờ, reset
    /// về -1 khi gói 37 về — <c>fr.java:240-294</c>), không theo thời gian.</para>
    ///
    /// <para>Thuần C# — không phụ thuộc Unity, test được ở <c>Gopet.Net.Tests</c>.</para>
    /// </summary>
    public sealed class BattleTurnState
    {
        /// <summary>ActorId của gói hệ thống (độc, phản đòn). Server gửi đúng hằng -1.
        /// Id quái là số âm khác (từ -2 trở xuống) và id người chơi là số dương.</summary>
        public const int SystemActorId = -1;

        private readonly int _localActorId;

        public BattleTurnState(int localActorId, bool localStarts)
        {
            _localActorId = localActorId;
            IsLocalTurn = localStarts;
        }

        /// <summary>Đang là lượt của pet người chơi cục bộ.</summary>
        public bool IsLocalTurn { get; private set; }

        /// <summary>Đã gửi hành động cho lượt này, đang chờ server phản hồi.</summary>
        public bool ActionPending { get; private set; }

        /// <summary>Được phép bấm Tấn công / Thuốc / kỹ năng.</summary>
        public bool CanAct => IsLocalTurn && !ActionPending;

        /// <summary>Vừa gửi một gói hành động — khoá cho tới khi gói lượt về.</summary>
        public void MarkActionSent() => ActionPending = true;

        /// <summary>Mở khoá khi watchdog phát hiện chờ quá lâu.</summary>
        public void ClearPending() => ActionPending = false;

        /// <summary>Cập nhật theo một gói <c>PET_BATTLE</c> (37).</summary>
        public void ApplyTurnPacket(int actorId, sbyte type, int effectCount)
        {
            // Gói hệ thống của độc / phản đòn gửi petId = -1 (PetBattle.cs:1547, :1617).
            // Chúng chạy bên trong nextTurn() nên tới ngay sau gói hành động thật — nếu
            // xử lý như gói hành động thì nhãn lượt sẽ nhấp nháy tắt giữa lượt mình.
            //
            // PHẢI so sánh == -1, KHÔNG được dùng <= 0: id quái cũng ÂM
            // (`GopetPlace.cs:90`: `mobId = -Utilities.nextInt(2, int.MaxValue - 12)`),
            // nên `<= 0` sẽ nuốt mọi gói lượt của quái ⇒ lượt không bao giờ lật về người
            // chơi, nhãn không hiện và nút khoá vĩnh viễn. Dải id quái bắt đầu từ -2 nên
            // không bao giờ đụng -1.
            if (actorId == SystemActorId) return;

            // Dùng vật phẩm KHÔNG đổi lượt: PetBattle.cs:1643 gửi gói 37 với turnDatas rỗng
            // và KHÔNG gọi nextTurn() sau đó. Đảo lượt ở đây sẽ khoá chết nút cho tới khi
            // quái đánh xong. Đường kỹ năng cũng dùng Wait (:1109) nhưng LUÔN kèm ≥1 effect
            // (:1102 hoặc :1106 đều add), nên effectCount phân biệt được 2 ca.
            if (type == BattleTurn.Wait && effectCount == 0)
            {
                ActionPending = false;
                return;
            }

            IsLocalTurn = actorId != _localActorId;
            ActionPending = false;
        }
    }
}
