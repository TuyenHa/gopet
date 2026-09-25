using System;
using System.Collections.Generic;
using Gopet.Net.Battle;

namespace Gopet.UiLogic
{
    public sealed class SpectatorBattles
    {
        public const int Capacity = 5;
        public sealed class Entry
        {
            public int BattleId;
            public BattlePet Left, Right;
            public double CreatedAt, ExpiresAt;
            public bool Ended;
            public int WinnerId;
            public BattlePet Pet(int actor) => Left.ActorId == actor ? Left : Right.ActorId == actor ? Right : null;
        }

        private readonly Dictionary<int, Entry> _entries = new Dictionary<int, Entry>();
        public int Count => _entries.Count;
        public IEnumerable<Entry> Entries => _entries.Values;
        public Entry Find(int id) => _entries.TryGetValue(id, out var value) ? value : null;

        public bool Start(BattleStart start, double now)
        {
            if (start == null || start.IsParticipant || start.Kind != BattleKind.Player ||
                start.LocalPet == null || start.Opponent == null) return false;
            if (!_entries.ContainsKey(start.BattleId) && Count >= Capacity) return false;
            _entries[start.BattleId] = new Entry
            {
                BattleId = start.BattleId, Left = Copy(start.LocalPet), Right = Copy(start.Opponent),
                CreatedAt = now, ExpiresAt = now + Timeout(start.TurnDurationMs)
            };
            return true;
        }

        public Entry Apply(BattleTurn turn, double now)
        {
            var entry = Find(turn.BattleId);
            if (entry == null || entry.Ended) return null;
            entry.ExpiresAt = now + Timeout(turn.TurnDurationMs);
            Change(entry.Pet(turn.ActorId), 0, turn.MainMpDelta);
            foreach (var effect in turn.Effects) Change(entry.Pet(effect.ActorId), effect.HpDelta, effect.MpDelta);
            return entry;
        }

        public void End(BattleResult result, double now)
        {
            var entry = Find(result.BattleId);
            if (entry == null || entry.Ended) return;
            entry.Ended = true; entry.WinnerId = result.WinnerId; entry.ExpiresAt = now + 2;
        }

        public void Remove(int actorOrBattleId)
        {
            var ids = new List<int>();
            foreach (var entry in _entries.Values)
                if (entry.BattleId == actorOrBattleId || entry.Pet(actorOrBattleId) != null)
                    ids.Add(entry.BattleId);
            foreach (var id in ids) _entries.Remove(id);
        }

        public void Expire(double now)
        {
            var ids = new List<int>();
            foreach (var entry in _entries.Values)
                if (now >= entry.ExpiresAt) ids.Add(entry.BattleId);
            foreach (var id in ids) _entries.Remove(id);
        }
        public void Clear() => _entries.Clear();
        private static double Timeout(int duration) => (duration > 0 ? duration / 1000.0 : 25) * 3;
        private static void Change(BattlePet pet, int hp, int mp)
        {
            if (pet == null) return;
            pet.Hp = (int)Math.Max(0, Math.Min((long)pet.Hp + hp, pet.MaxHp));
            pet.Mp = (int)Math.Max(0, Math.Min((long)pet.Mp + mp, pet.MaxMp));
        }
        private static BattlePet Copy(BattlePet p) => new BattlePet
        {
            ActorId = p.ActorId, Hp = p.Hp, MaxHp = p.MaxHp, Mp = p.Mp, MaxMp = p.MaxMp,
            Name = p.Name, TemplateId = p.TemplateId, ImagePath = p.ImagePath,
            FrameCount = p.FrameCount, VerticalOffset = p.VerticalOffset, Level = p.Level
        };
    }
}
