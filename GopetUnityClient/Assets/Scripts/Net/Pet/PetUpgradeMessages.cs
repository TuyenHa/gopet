namespace Gopet.Net.Pet
{
    public sealed class PetUpgradeSelection
    {
        public sbyte Role;
        public int PetId;
        public string FrameImagePath;
        public sbyte FrameIndex;
    }

    public sealed class PetUpgradePrice
    {
        public int Gold;
        public int Coin;
    }

    public sealed class PetUpgradePreview
    {
        public string Title;
        public string[] Lines;
    }
}
