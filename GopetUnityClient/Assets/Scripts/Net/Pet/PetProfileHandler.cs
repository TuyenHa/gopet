using System;

namespace Gopet.Net.Pet
{
    /// <summary>Nhận màn thông tin pet, Gym và chọn nguyên liệu tattoo từ PET_SERVICE.</summary>
    public sealed class PetProfileHandler
    {
        private const int MaxSkills = 64;
        private const int MaxTattoos = 64;

        public event Action<PetProfile> ProfileReceived;
        public event Action<PetGymState> GymReceived;
        public event Action<PetPotentialUpdate> PotentialUpdated;
        public event Action<TattooMaterialSelection> TattooMaterialSelected;
        public event Action<TattooScreen> TattooScreenReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.MAGIC, OnProfile);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.GYM, OnGym);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.UP_TIEM_NANG, OnPotential);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.TATTOO, OnTattoo);
        }

        private void OnProfile(Message message)
        {
            var r = message.Reader;
            var value = new PetProfile
            {
                UserId = r.ReadInt(), TemplateId = r.ReadInt(), Element = r.ReadSByte(),
                FrameImage = r.ReadUtf(), Name = r.ReadUtf(), PetClass = r.ReadSByte(),
                Level = r.ReadInt(), Experience = r.ReadLong(),
                ExperienceToNextLevel = r.ReadLong(), ReservedExperience = r.ReadLong(),
                Str = r.ReadInt(), Agi = r.ReadInt(), Int = r.ReadInt(),
                Atk = r.ReadInt(), Def = r.ReadInt(), Hp = r.ReadInt(), Mp = r.ReadInt(),
                MaxHp = r.ReadInt(), MaxMp = r.ReadInt()
            };
            value.Skills = ReadSkills(r);
            value.PotentialPoints = r.ReadInt();
            value.Tattoos = ReadTattoos(r);
            value.FrameCount = r.ReadSByte();
            r.ExpectFullyConsumed("MAGIC");
            ProfileReceived?.Invoke(value);
        }

        private void OnGym(Message message)
        {
            var r = message.Reader;
            var value = new PetGymState
            {
                TemplateId = r.ReadInt(), FrameImage = r.ReadUtf(), Name = r.ReadUtf(),
                PetClass = r.ReadSByte(), Level = r.ReadInt(), Experience = r.ReadLong(),
                ExperienceToNextLevel = r.ReadLong(), ReservedExperience = r.ReadLong(),
                Str = r.ReadInt(), Agi = r.ReadInt(), Int = r.ReadInt(),
                PotentialPoints = r.ReadSByte(), Options = ReadGymOptions(r),
                FrameCount = r.ReadSByte()
            };
            r.ExpectFullyConsumed("GYM");
            GymReceived?.Invoke(value);
        }

        private void OnPotential(Message message)
        {
            var r = message.Reader;
            var value = new PetPotentialUpdate
            {
                TemplateId = r.ReadInt(), Str = r.ReadInt(), Agi = r.ReadInt(),
                Int = r.ReadInt(), Options = ReadGymOptions(r)
            };
            r.ExpectFullyConsumed("UP_TIEM_NANG");
            PotentialUpdated?.Invoke(value);
        }

        private void OnTattoo(Message message)
        {
            var subtype = message.Reader.ReadSByte();
            if (subtype == GopetCmd.TATTOO_INIT_SCREEN)
            {
                ReadTattooScreen(message);
                return;
            }
            if (subtype != 7)
                throw new ProtocolException($"TATTOO server sub chưa hỗ trợ: {subtype}.");
            var value = new TattooMaterialSelection
            {
                ItemId = message.Reader.ReadInt(), IconPath = message.Reader.ReadUtf(),
                Name = message.Reader.ReadUtf()
            };
            message.Reader.ExpectFullyConsumed("TATTOO/7");
            TattooMaterialSelected?.Invoke(value);
        }

        private void ReadTattooScreen(Message message)
        {
            var r = message.Reader;
            var count = CheckedCount(r.ReadInt(), MaxTattoos, "tattoo slot");
            var value = new TattooScreen { Slots = new TattooSlot[count] };
            for (var i = 0; i < count; i++) value.Slots[i] = new TattooSlot
            {
                TattooId = r.ReadInt(), Name = r.ReadUtf(),
                Position = r.ReadSByte(), IconPath = r.ReadUtf()
            };
            r.ExpectFullyConsumed("TATTOO/1");
            TattooScreenReceived?.Invoke(value);
        }

        private static PetSkillInfo[] ReadSkills(JavaBinaryReader r)
        {
            var count = CheckedCount(r.ReadSByte(), MaxSkills, "skill");
            var values = new PetSkillInfo[count];
            for (var i = 0; i < count; i++) values[i] = new PetSkillInfo
            {
                Id = r.ReadInt(), Name = r.ReadUtf(), Description = r.ReadUtf(), MpCost = r.ReadInt()
            };
            return values;
        }

        private static PetTattooInfo[] ReadTattoos(JavaBinaryReader r)
        {
            var count = CheckedCount(r.ReadInt(), MaxTattoos, "tattoo");
            var values = new PetTattooInfo[count];
            for (var i = 0; i < count; i++) values[i] = new PetTattooInfo
            {
                Type = r.ReadInt(), Name = r.ReadUtf(), Level = r.ReadSByte(),
                Description = r.ReadUtf(), State = r.ReadSByte()
            };
            return values;
        }

        private static GymOption[] ReadGymOptions(JavaBinaryReader r)
        {
            var values = new GymOption[3];
            for (var i = 0; i < values.Length; i++) values[i] = new GymOption
            {
                Value = r.ReadInt(), Cost = r.ReadInt(), State = r.ReadSByte(),
                Description = r.ReadUtf(), Step = r.ReadSByte()
            };
            return values;
        }

        private static int CheckedCount(int value, int max, string label)
        {
            if (value < 0 || value > max)
                throw new ProtocolException($"MAGIC có {value} {label} — ngoài khoảng hợp lệ.");
            return value;
        }
    }
}
