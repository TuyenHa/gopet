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
        public event Action<PetEquipDelta> EquipChanged;
        public event Action<PetEquipItem> EquipItemRefreshed;
        public event Action<PetEquipMaterialSelection> EnchantMaterialSelected;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.EQUIP_INFO, OnEquipInfo);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.USE_EQUIP_ITEM,
                message => OnSimpleDelta(message, true, "USE_EQUIP_ITEM"));
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.UNEQUIP_ITEM,
                message => OnSimpleDelta(message, false, "UNEQUIP_ITEM"));
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.REMOVE_ITEM_EQUIP,
                OnRemoved);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.ON_UNQUIP_GEM,
                OnItemRefreshed);
            router.RegisterSub(GopetCmd.PET_SERVICE,
                GopetCmd.SELECT_METERIAL_ENCHANT_PET_INFO, OnEnchantMaterial);
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

        private void OnSimpleDelta(Message message, bool equipped, string context)
        {
            var accepted = message.Reader.ReadSByte() == 1;
            var itemId = message.Reader.ReadInt();
            message.Reader.ExpectFullyConsumed(context);
            EquipChanged?.Invoke(new PetEquipDelta
            {
                ItemId = itemId, Equipped = equipped, Removed = false, Accepted = accepted
            });
        }

        private void OnRemoved(Message message)
        {
            var itemId = message.Reader.ReadInt();
            message.Reader.ExpectFullyConsumed("REMOVE_ITEM_EQUIP");
            EquipChanged?.Invoke(new PetEquipDelta
            {
                ItemId = itemId, Equipped = false, Removed = true, Accepted = true
            });
        }

        private void OnItemRefreshed(Message message)
        {
            var item = ReadItem(message.Reader, isResend: true);
            message.Reader.ExpectFullyConsumed("ON_UNQUIP_GEM");
            EquipItemRefreshed?.Invoke(item);
        }

        private void OnEnchantMaterial(Message message)
        {
            var value = new PetEquipMaterialSelection
            {
                ItemOrTemplateId = message.Reader.ReadInt(),
                IconPath = message.Reader.ReadUtf(),
                Name = message.Reader.ReadUtf(),
                Slot = message.Reader.ReadInt()
            };
            message.Reader.ExpectFullyConsumed("SELECT_METERIAL_ENCHANT_PET_INFO");
            EnchantMaterialSelected?.Invoke(value);
        }

        /// <param name="isResend">
        /// Gói làm mới một món (<c>resendPetEquipInfo</c> → <c>writeItemEquip(isReSend: true)</c>):
        /// server LUÔN ghi long + int sau cờ ngọc, không có ngọc thì ghi -1, -1. Gói danh
        /// sách (EQUIP_INFO) chỉ ghi hai trường đó khi có ngọc.
        /// </param>
        private static PetEquipItem ReadItem(JavaBinaryReader r, bool isResend = false)
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
            else if (isResend)
            {
                r.ReadLong(); // -1
                r.ReadInt();  // -1
            }
            return item;
        }
    }
}
