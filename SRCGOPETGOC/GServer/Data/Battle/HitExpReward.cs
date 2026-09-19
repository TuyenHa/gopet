using Gopet.Data.Mob;
using Gopet.IO;
using Gopet.Manager;
using Gopet.Util;
using System;

namespace Gopet.Battle
{
    /// <summary>EXP nhỏ giọt mỗi đòn pet TRÚNG quái, để client hiện số vàng bay trên đầu.
    ///
    /// <para><b>Đây là thưởng THÊM, không phải ứng trước.</b> Quyết định của user ngày
    /// 2026-09-19: đánh nhiều đòn thì pet thật sự ăn nhiều EXP hơn, <c>PetBattle.win()</c>
    /// KHÔNG trừ lại phần đã nhỏ giọt. Đừng "sửa cho nhất quán" thành ứng trước.</para>
    ///
    /// <para>Hệ quả: một con quái cho tối đa <c>100% + BattleCapPercent</c> EXP, nên
    /// <see cref="BattleCapPercent"/> là chốt chặn chống cày DUY NHẤT — nó chặn vòng lặp
    /// "đánh một đòn → xin thua → đánh lại" ăn EXP mà không cần giết quái.</para>
    ///
    /// <para>Vòng đời theo TRẬN: <c>PetBattle</c> được tạo mới mỗi lần
    /// <c>GopetPlace.startFightMob</c> nên trần không rò sang trận sau.</para></summary>
    public sealed class HitExpReward
    {
        /// <summary>Phần trăm EXP giết quái được cộng cho mỗi đòn trúng.</summary>
        public const float PerHitPercent = 5f;

        /// <summary>Trần cộng dồn trong một trận.</summary>
        public const float BattleCapPercent = 30f;

        private int _killExp = -1;
        private int _paid;

        /// <summary>Tổng EXP đã nhỏ giọt trong trận này.</summary>
        public int Paid => _paid;

        /// <summary>EXP được cộng cho đòn này, 0 nghĩa là không cộng (chạm trần hoặc quái
        /// không có bảng cấp). Chỉ gọi khi đòn đã TRÚNG và là PvE với quái thường.</summary>
        public int Grant(Player player, Pet pet, Mob mob)
        {
            if (mob == null || mob.getMobLvInfo() == null) return 0;
            if (_killExp < 0)
            {
                // genExpWhenMobDie hiện tất định (nhánh chênh cấp đã comment-out) nên cache được.
                // Nếu sau này nó thành ngẫu nhiên, phải truyền giá trị từ win() xuống thay vì cache.
                _killExp = PetBattle.genExpWhenMobDie(player, pet, mob, mob.getMobLvInfo().exp);
            }
            if (_killExp <= 0) return 0;

            int cap = Utilities.round(Utilities.GetValueFromPercent(_killExp, BattleCapPercent));
            int left = cap - _paid;
            if (left <= 0) return 0;

            int perHit = Math.Max(1, Utilities.round(Utilities.GetValueFromPercent(_killExp, PerHitPercent)));
            perHit = Math.Min(perHit, left);
            _paid += perHit;
            return perHit;
        }

        /// <summary>Gửi gói <c>PET_BATTLE_EXP</c>. Client &lt; 1.5.0 bị bỏ qua — jar cũ không
        /// hiểu opcode này và sẽ vỡ khung gói nếu nhận.</summary>
        public static void Send(Player target, int battleId, int actorId, int expDelta)
        {
            if (target?.ApplicationVersion == null || target.ApplicationVersion < GopetManager.VERSION_150) return;
            if (target.session == null) return;
            Message m = new Message(GopetCMD.PET_SERVICE);
            m.putsbyte(GopetCMD.PET_BATTLE_EXP);
            m.putInt(battleId);
            m.putInt(actorId);
            m.putInt(expDelta);
            m.cleanup();
            target.session.sendMessage(m);
        }
    }
}
