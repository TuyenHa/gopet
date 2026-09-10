using System;

namespace Gopet.Net.Pet
{
    /// <summary>
    /// Nhận <c>PET_SERVICE (81) / EQUIP_INFO (28)</c> — thông tin trang bị pet.
    /// Wire khớp <c>GameController.equipInfo</c> + <c>writeItemEquip</c>
    /// (<c>GameController.cs:1825-1923</c>).
    ///
    /// <para><b>11 int placeholder</b> giữa mỗi item là stat slots server hardcoded
    /// (<c>for j 0..10 putInt(j+1)</c>) — chưa dùng, đọc-bỏ.</para>
    /// </summary>
    public sealed class PetEquipHandler
    {
        public event Action<PetEquipInfo> EquipInfoReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.EQUIP_INFO, OnEquipInfo);
        }

        private void OnEquipInfo(Message message)
        {
            var r = message.Reader;
            var info = new PetEquipInfo
            {
                UserId = r.ReadInt(),
                PetId = r.ReadInt(),
                FrameImage = r.ReadUtf(),
                PetName = r.ReadUtf(),
                Level = r.ReadInt(),
                Str = r.ReadInt(),
                Agi = r.ReadInt(),
                Int = r.ReadInt(),
            };

            var itemCount = r.ReadInt();
            if (itemCount < 0 || itemCount > 256)
                throw new ProtocolException($"EQUIP_INFO đếm {itemCount} item — ngoài khoảng hợp lệ.");

            var items = new PetEquipItem[itemCount];
            for (var i = 0; i < itemCount; i++) items[i] = ReadItem(r);

            info.Items = items;
            info.FrameNumber = r.ReadSByte();
            r.ExpectFullyConsumed("EQUIP_INFO");
            EquipInfoReceived?.Invoke(info);
        }

        private static PetEquipItem ReadItem(JavaBinaryReader r)
        {
            var item = new PetEquipItem
            {
                ItemId = r.ReadInt(),
                FrameImagePath = r.ReadUtf(),
                RawName = r.ReadUtf(),
                DisplayName = r.ReadUtf(),
                Type = r.ReadInt(),
                PetEquipId = r.ReadInt(),
            };
            // 11 int placeholder — server hardcode putInt(j+1) for j in 0..10
            for (var j = 0; j < 11; j++) r.ReadInt();
            item.Reserved = r.ReadSByte();
            item.Level = r.ReadSByte();
            item.HasGem = r.ReadBool();
            if (item.HasGem)
            {
                item.GemTimeUnequip = r.ReadLong();
                item.GemSecondsRemaining = r.ReadInt();
            }
            return item;
        }
    }
}
