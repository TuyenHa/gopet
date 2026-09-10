namespace Gopet.Net.Map
{
    public sealed class NpcSpawn
    {
        public int Id, X, Y, FrameCount, VerticalOffset, NpcType;
        public int[] Bounds;
        public string ImagePath, Name;
        public string[] Chat;
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
