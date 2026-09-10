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

    public sealed class BattleResult
    {
        public int BattleId, WinnerId, Coin, Experience;
        public string[] Messages = Array.Empty<string>();
    }
}
