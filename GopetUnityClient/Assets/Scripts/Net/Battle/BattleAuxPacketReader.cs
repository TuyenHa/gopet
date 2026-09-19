using System;

namespace Gopet.Net.Battle
{
    /// <summary>Parse các gói phụ (buff, stats) để giữ BattleHandler gọn.</summary>
    public static class BattleAuxPacketReader
    {
        private const int MaxActors = 2;
        private const int MaxBuffEntries = 32;
        private const int MaxSkills = 16;

        public static BattleBuffState ReadBuffState(JavaBinaryReader r)
        {
            var state = new BattleBuffState { BattleId = r.ReadInt() };
            var actorCount = Clamp(r.ReadSByte(), MaxActors, "actor buff");
            state.Actors = new BattleActorBuffs[actorCount];
            for (var i = 0; i < actorCount; i++)
            {
                var actor = new BattleActorBuffs { ActorId = r.ReadInt() };
                var entryCount = Clamp(r.ReadSByte(), MaxBuffEntries, "buff");
                actor.Entries = new BattleBuffEntry[entryCount];
                for (var j = 0; j < entryCount; j++)
                {
                    actor.Entries[j] = new BattleBuffEntry
                    {
                        TypeId = r.ReadInt(),
                        Value = r.ReadInt(),
                        TurnsLeft = r.ReadSByte(),
                    };
                }
                state.Actors[i] = actor;
            }
            r.ExpectFullyConsumed("PET_BATTLE_BUFF");
            return state;
        }

        public static BattleStatsState ReadStatsState(JavaBinaryReader r)
        {
            var state = new BattleStatsState { BattleId = r.ReadInt() };
            var actorCount = Clamp(r.ReadSByte(), MaxActors, "actor stats");
            state.Actors = new BattleActorStats[actorCount];
            for (var i = 0; i < actorCount; i++)
            {
                var actor = new BattleActorStats
                {
                    ActorId = r.ReadInt(),
                    Level = r.ReadInt(),
                    Atk = r.ReadInt(),
                    Def = r.ReadInt(),
                    CritPermille = r.ReadShort(),
                };
                var skillCount = Clamp(r.ReadSByte(), MaxSkills, "skill stats");
                actor.Skills = new BattleSkillCost[skillCount];
                for (var j = 0; j < skillCount; j++)
                {
                    actor.Skills[j] = new BattleSkillCost
                    {
                        SkillId = r.ReadInt(),
                        MpCost = r.ReadInt(),
                    };
                }
                state.Actors[i] = actor;
            }
            r.ExpectFullyConsumed("PET_BATTLE_STATS");
            return state;
        }

        public static BattlePet ReadOwnedPet(JavaBinaryReader r, int actorId)
        {
            var pet = ReadVisual(r, actorId);
            pet.Level = r.ReadInt();
            for (var i = 0; i < 5; i++) r.ReadInt();
            ReadVitals(r, pet);
            pet.Skills = ReadSkills(r, true);
            return pet;
        }

        public static BattlePet ReadPassivePet(JavaBinaryReader r, int actorId)
        {
            var pet = ReadVisual(r, actorId);
            pet.Level = r.ReadInt();
            ReadVitals(r, pet);
            pet.Skills = ReadSkills(r, false);
            return pet;
        }

        private static BattlePet ReadVisual(JavaBinaryReader r, int actorId) => new BattlePet
        {
            ActorId = actorId, TemplateId = r.ReadInt(), ImagePath = r.ReadUtf(),
            FrameCount = r.ReadSByte(), VerticalOffset = r.ReadShort(), Name = r.ReadUtf()
        };

        private static void ReadVitals(JavaBinaryReader r, BattlePet pet)
        {
            pet.Hp = r.ReadInt(); pet.Mp = r.ReadInt();
            pet.MaxHp = r.ReadInt(); pet.MaxMp = r.ReadInt();
        }

        private static BattleSkill[] ReadSkills(JavaBinaryReader r, bool detailed)
        {
            var count = Clamp(r.ReadSByte(), MaxSkills, "kỹ năng");
            var result = new BattleSkill[count];
            for (var i = 0; i < count; i++)
            {
                var skill = new BattleSkill { Id = r.ReadInt(), Name = r.ReadUtf() };
                if (detailed) { skill.Description = r.ReadUtf(); skill.MpCost = r.ReadInt(); }
                result[i] = skill;
            }
            return result;
        }

        private static int Clamp(int value, int max, string label)
        {
            if (value < 0 || value > max)
                throw new ProtocolException($"Số {label} không hợp lệ: {value}.");
            return value;
        }
    }
}
