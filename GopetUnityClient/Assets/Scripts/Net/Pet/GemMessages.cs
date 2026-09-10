namespace Gopet.Net.Pet
{
    public sealed class GemItemInfo
    {
        public int ItemId;
        public int IconId;
        public string IconPath;
        public string Name;
        public int Level;
    }

    public sealed class GemInventory
    {
        public GemItemInfo[] Items;
    }

    public sealed class GemMaterialSelection
    {
        public int ItemOrTemplateId;
        public string IconPath;
        public string Name;
        public int Slot;
        public int Level;
    }
}
