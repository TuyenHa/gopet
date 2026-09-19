using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>Đếm cooldown 3 lượt client-side. Server không gửi cooldown qua packet
    /// (<c>MAX_SKILL_COOLDOWN=3</c> là hằng số trong <c>GopetManager.cs</c>), nên client
    /// tự đếm. Nếu client đoán sai, server vẫn từ chối bằng dialog đỏ và
    /// <see cref="ResyncDenied"/> đưa cooldown về đúng 3 lượt để đồng bộ lại.
    ///
    /// <para>Thuần C# — không phụ thuộc Unity, test được ở <c>Gopet.Net.Tests</c>.</para>
    /// </summary>
    public sealed class SkillCooldownTracker
    {
        public const int DefaultTurns = 3;

        private readonly Dictionary<int, int> _remaining = new Dictionary<int, int>();
        private readonly int _initialTurns;
        private int _lastActorId = -1;
        private int _lastUsedSkillId = -1;

        public SkillCooldownTracker(int initialTurns = DefaultTurns) { _initialTurns = initialTurns; }

        /// <summary>Client vừa bấm skill thành công (server chưa từ chối).</summary>
        public void MarkUsed(int skillId)
        {
            _remaining[skillId] = _initialTurns;
            _lastUsedSkillId = skillId;
        }

        /// <summary>Huỷ lần <see cref="MarkUsed"/> gần nhất — dùng khi server báo kỹ năng
        /// TRƯỢT: server không trừ MP và không đặt cooldown cho đòn trượt, nên cooldown lạc
        /// quan của client sẽ xám nút oan 3 lượt nếu không gỡ.</summary>
        public void CancelLastUsed()
        {
            if (_lastUsedSkillId < 0) return;
            _remaining.Remove(_lastUsedSkillId);
            _lastUsedSkillId = -1;
        }

        /// <summary>Gọi khi nhận <c>BattleTurn</c>. Chỉ giảm khi <paramref name="actorId"/>
        /// là chính mình — tránh giảm nhầm lượt của đối thủ.</summary>
        public void OnTurnAdvanced(int actorId, int localActorId)
        {
            // Server tick cooldown ở nextTurn() sau khi setIsActiveTurn — mỗi lần trận
            // đổi lượt là mỗi bên -1. Client đơn giản hoá: khi nhận packet mà actorId
            // giống lượt trước (double) thì bỏ qua để tránh trừ 2 lần.
            if (actorId == _lastActorId) return;
            _lastActorId = actorId;
            if (actorId != localActorId) return;
            if (_remaining.Count == 0) return;
            var keys = new List<int>(_remaining.Keys);
            foreach (var id in keys)
            {
                var next = _remaining[id] - 1;
                if (next <= 0) _remaining.Remove(id); else _remaining[id] = next;
            }
        }

        /// <summary>Server báo skill còn cooldown → đồng bộ về đúng 3 lượt (bảo thủ hơn
        /// giá trị hiện tại nếu client đoán sai).</summary>
        public void ResyncDenied(int skillId) => _remaining[skillId] = _initialTurns;

        public bool IsReady(int skillId) => !_remaining.ContainsKey(skillId);

        public int TurnsLeft(int skillId) =>
            _remaining.TryGetValue(skillId, out var n) ? n : 0;

        public void Reset() { _remaining.Clear(); _lastActorId = -1; _lastUsedSkillId = -1; }
    }
}
