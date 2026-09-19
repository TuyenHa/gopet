namespace Gopet.UiLogic
{
    public static class BattleSkillIconKey
    {
        public static string For(int skillId) => skillId <= 0
            ? "Battle/skills/attack"
            : $"Battle/skills/{skillId}";
    }
}
