using System;
using System.Collections.Generic;
using System.Linq;

namespace Gopet.UiLogic
{
    /// <summary>Bản tổng kết đã chốt; không giữ tham chiếu tới mảng của gói tin.</summary>
    public sealed class BattleSummarySnapshot
    {
        public string OpponentName { get; }
        public string DurationText { get; }
        public int Coin { get; }
        public int Experience { get; }
        public IReadOnlyList<string> RewardLines { get; }
        public IReadOnlyList<string> SkillNames { get; }

        internal BattleSummarySnapshot(string opponentName, string durationText, int coin,
            int experience, IEnumerable<string> rewards, IEnumerable<string> skills)
        {
            OpponentName = opponentName;
            DurationText = durationText;
            Coin = coin;
            Experience = experience;
            RewardLines = Array.AsReadOnly(rewards.ToArray());
            SkillNames = Array.AsReadOnly(skills.ToArray());
        }
    }
}
