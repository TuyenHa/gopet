using System;

namespace Gopet.Net.Pet
{
    public sealed class PetSkillInfo
    {
        public int Id, MpCost;
        public string Name, Description;
    }

    public sealed class PetTattooInfo
    {
        public int Type;
        public string Name, Description;
        public sbyte Level, State;
    }

    public sealed class PetProfile
    {
        public int UserId, TemplateId, Level, Str, Agi, Int, Atk, Def;
        public int Hp, Mp, MaxHp, MaxMp, PotentialPoints, FrameCount;
        public sbyte Element, PetClass;
        public long Experience, ExperienceToNextLevel, ReservedExperience;
        public string FrameImage, Name;
        public PetSkillInfo[] Skills = Array.Empty<PetSkillInfo>();
        public PetTattooInfo[] Tattoos = Array.Empty<PetTattooInfo>();
    }

    public sealed class PetGymState
    {
        public int TemplateId, Level, Str, Agi, Int, PotentialPoints, FrameCount;
        public sbyte PetClass;
        public long Experience, ExperienceToNextLevel, ReservedExperience;
        public string FrameImage, Name;
        public GymOption[] Options = Array.Empty<GymOption>();
    }

    public sealed class GymOption
    {
        public int Value, Cost;
        public sbyte State, Step;
        public string Description;
    }

    public sealed class PetPotentialUpdate
    {
        public int TemplateId, Str, Agi, Int;
        public GymOption[] Options = Array.Empty<GymOption>();
    }

    public sealed class TattooMaterialSelection
    {
        public int ItemId;
        public string IconPath, Name;
    }

    public sealed class TattooScreen
    {
        public TattooSlot[] Slots = Array.Empty<TattooSlot>();
    }

    public sealed class TattooSlot
    {
        public int TattooId;
        public sbyte Position;
        public string Name, IconPath;
    }
}
