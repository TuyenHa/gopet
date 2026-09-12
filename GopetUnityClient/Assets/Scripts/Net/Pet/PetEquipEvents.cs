namespace Gopet.Net.Pet
{
    /// <summary>
    /// Subtype code mà server đặt trong <c>Item.type</c> để client biết slot.
    /// Hardcoded từ jar <c>fu.java:53-57</c> (báo cáo cũ analysis-260908-2209 §3).
    /// </summary>
    public enum EquipSlot
    {
        Weapon = 1,
        Armor = 2,
        Hat = 3,
        Boot = 104,
        Glove = 105,
    }

    /// <summary>
    /// Một item trang bị pet (hoặc trong túi). Bao gồm cả trạng thái ngọc gắn.
    /// Field <c>PetEquipId</c> ≥ 0 nghĩa là item đang mặc lên pet có id đó (petId).
    /// </summary>
    public sealed class PetEquipItem
    {
        public int ItemId;
        public string FrameImagePath;
        public string RawName;    // server luôn gửi "???" — không dùng
        public string DisplayName;
        public int Type;
        public int PetEquipId;
        public sbyte Reserved;    // luôn 0
        public sbyte Level;
        public bool HasGem;
        public long GemTimeUnequip;
        public int GemSecondsRemaining;

        public EquipSlot? Slot =>
            System.Enum.IsDefined(typeof(EquipSlot), Type) ? (EquipSlot)Type : (EquipSlot?)null;
    }

    /// <summary>Gói EQUIP_INFO server bơm khi client mở màn trang bị pet.</summary>
    public sealed class PetEquipInfo
    {
        public int UserId;
        public int PetId;
        public string FrameImage;
        public string PetName;
        public int Level;
        public int Str;
        public int Agi;
        public int Int;
        public sbyte FrameNumber;
        public PetEquipItem[] Items;
    }

    public sealed class PetEquipDelta
    {
        public int ItemId;
        public bool Equipped;
        public bool Removed;
        public bool Accepted;
    }
}
