using System;
using System.Collections.Generic;
using System.Linq;
using Gopet.Net.Battle;

namespace Gopet.UiLogic
{
    /// <summary>Thống kê một trận từ gói server, không từ thao tác bấm kỹ năng.</summary>
    public sealed class BattleSummaryTracker
    {
        private readonly int _battleId, _localActorId;
        private readonly string _opponentName;
        private readonly double _startedAt;
        private readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        private readonly HashSet<int> _seen = new HashSet<int>();
        private readonly List<string> _skills = new List<string>();
        private BattleSummarySnapshot _snapshot;

        public BattleSummaryTracker(BattleStart start, double startedAtSeconds)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            _battleId = start.BattleId;
            _localActorId = start.LocalPet.ActorId;
            _opponentName = start.Opponent.Name;
            _startedAt = startedAtSeconds;
            foreach (var skill in start.LocalPet.Skills ?? Array.Empty<BattleSkill>())
                if (skill != null && !string.IsNullOrWhiteSpace(skill.Name))
                    _names[skill.Id] = skill.Name;
        }

        public void RecordTurn(BattleTurn turn)
        {
            if (_snapshot != null || turn == null || turn.BattleId != _battleId
                || turn.ActorId != _localActorId) return;
            foreach (var effect in turn.Effects ?? Array.Empty<BattleEffect>())
            {
                if (effect == null || effect.SkillId < BattleEffectNames.FirstSkillId
                    || !_seen.Add(effect.SkillId)) continue;
                _skills.Add(_names.TryGetValue(effect.SkillId, out var name)
                    ? name : $"Kỹ năng #{effect.SkillId}");
            }
        }

        public BattleSummarySnapshot Complete(BattleResult result, double endedAtSeconds)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.BattleId != _battleId)
                throw new ArgumentException("Kết quả thuộc trận khác.", nameof(result));
            if (_snapshot != null) return _snapshot;
            var seconds = (long)Math.Max(0d, Math.Floor(endedAtSeconds - _startedAt));
            _snapshot = new BattleSummarySnapshot(_opponentName,
                $"{seconds / 60} phút {seconds % 60:00} giây", result.Coin, result.Experience,
                (result.Messages ?? Array.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s)),
                _skills);
            return _snapshot;
        }
    }
}
