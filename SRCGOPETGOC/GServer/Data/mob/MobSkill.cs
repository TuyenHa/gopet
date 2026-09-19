namespace Gopet.Data.Mob
{
    public class MobSkill
    {
        public int petId { get; set; }
        public int skillID { get; set; }
        public int skillLv { get; set; } = 1;
        public byte useRate { get; set; } = 30;
    }
}
