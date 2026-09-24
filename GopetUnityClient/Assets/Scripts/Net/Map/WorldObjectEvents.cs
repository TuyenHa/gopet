namespace Gopet.Net.Map
{
    public sealed class NpcSpawn
    {
        public int Id, X, Y, FrameCount, VerticalOffset, NpcType;
        public int[] Bounds;
        public string ImagePath, Name;
        public string[] Chat;

        /// <summary>
        /// Cột <c>npc.type</c> = 9: NPC chỉ đứng làm cảnh — không tên, không nút "Nói chuyện",
        /// bấm không có gì. 0/1 đã có trong dữ liệu; 2/3 là NPC đi lại bên jar, nên dùng số khác hẳn.
        /// </summary>
        public const int DecorationType = 9;

        public bool IsDecoration => NpcType == DecorationType;
    }

    public sealed class MobSpawn
    {
        public int Id, Level, X, Y, FrameCount, VerticalOffset;
        public string ImagePath, Name;
        public bool IsBoss;
    }

    public sealed class PetInteraction
    {
        public int UserId;
        public int Type;
    }
}
