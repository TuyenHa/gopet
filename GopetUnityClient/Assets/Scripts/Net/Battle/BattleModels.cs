using System;

namespace Gopet.Net.Battle
{
    public enum BattleKind { Mob, Player }

    public sealed class BattleSkill
    {
        public int Id, MpCost;
        public string Name, Description;
    }

    public sealed class BattlePet
    {
        public int ActorId, TemplateId, Level, Hp, Mp, MaxHp, MaxMp;
        public int FrameCount, VerticalOffset;
        public string ImagePath, Name;
        public BattleSkill[] Skills = Array.Empty<BattleSkill>();
    }

    public sealed class BattleStart
    {
        public int BattleId, RemainingMs, TurnDurationMs;
        public bool LocalStarts, IsParticipant;
        public BattleKind Kind;
        public BattlePet LocalPet, Opponent;
    }

    public sealed class BattleEffect
    {
        public int ActorId, SkillId, HpDelta, MpDelta;
    }

    public sealed class BattleTurn
    {
        public const sbyte Normal = 1;
        public const sbyte Wait = 4;

        public int BattleId, ActorId, RemainingMs, TurnDurationMs, MainMpDelta;
        public sbyte Type;
        public BattleEffect[] Effects = Array.Empty<BattleEffect>();
    }

    /// <summary>EXP nhỏ giọt cho một đòn trúng (opcode 81/33, gate 1.5.0).</summary>
    public sealed class BattleExpGain
    {
        public int BattleId, ActorId, Amount;
    }

    public sealed class BattleResult
    {
        public int BattleId, WinnerId, Coin, Experience;
        public string[] Messages = Array.Empty<string>();
    }

    /// <summary>Một buff/debuff đang hiệu lực trên 1 pet.</summary>
    public sealed class BattleBuffEntry
    {
        public int TypeId;
        public int Value;
        public int TurnsLeft;
    }

    public sealed class BattleActorBuffs
    {
        public int ActorId;
        public BattleBuffEntry[] Entries = Array.Empty<BattleBuffEntry>();
    }

    /// <summary>Payload PET_BATTLE_BUFF — server chỉ gửi cho client >= 1.5.0.</summary>
    public sealed class BattleBuffState
    {
        public int BattleId;
        public BattleActorBuffs[] Actors = Array.Empty<BattleActorBuffs>();
    }

    public sealed class BattleSkillCost
    {
        public int SkillId;
        public int MpCost;
    }

    public sealed class BattleActorStats
    {
        public int ActorId, Level, Atk, Def;
        public short CritPermille;
        public BattleSkillCost[] Skills = Array.Empty<BattleSkillCost>();
    }

    /// <summary>Payload PET_BATTLE_STATS — HUD chỉ số trận (level, ATK, DEF, crit, skill MP).</summary>
    public sealed class BattleStatsState
    {
        public int BattleId;
        public BattleActorStats[] Actors = Array.Empty<BattleActorStats>();
    }
}
