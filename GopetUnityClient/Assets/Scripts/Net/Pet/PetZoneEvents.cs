namespace Gopet.Net.Pet
{
    /// <summary>Một pet đang follow player khác trong zone.</summary>
    public sealed class PetZoneEntry
    {
        public int OwnerUserId;
        public int PetIdTemplate;
        public string FrameImagePath;
        public string DisplayName;
        public int Level;
        public sbyte FrameNum;
        public short VerticalOffset;
    }

    /// <summary>Danh sách pet trong zone — server bơm khi player vào map (GopetPlace.sendListPet).</summary>
    public sealed class PetZoneUpdate
    {
        public PetZoneEntry[] Entries;
    }

    /// <summary>Pet của <c>OwnerUserId</c> không còn follow (chết, unfollow, exit).</summary>
    public sealed class PetUnfollow
    {
        public int OwnerUserId;
    }

    /// <summary>HP/MP snapshot của pet self — server bơm qua MY_PET_INFO.</summary>
    public sealed class MyPetInfo
    {
        public int Hp;
        public int MaxHp;
        public int Mp;
        public int MaxMp;
    }
}
