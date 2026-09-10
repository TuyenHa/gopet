using System;

namespace Gopet.Net.Pet
{
    /// <summary>Nhận danh sách, cập nhật và lựa chọn nguyên liệu gem.</summary>
    public sealed class GemHandler
    {
        private readonly Action<Message> _send;

        public GemHandler(Action<Message> send)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public event Action<GemInventory> InventoryReceived;
        public event Action<GemItemInfo> GemUpdated;
        public event Action<int> GemRemoved;
        public event Action<GemMaterialSelection> EnchantMaterialSelected;
        public event Action<GemMaterialSelection> TierMaterialSelected;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SHOW_GEM_INVENTORY, OnInventory);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SEND_GEM_INFo, OnGemUpdated);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.REMOVE_GEM_ITEM, OnGemRemoved);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SELECT_GEM_ENCHANT, OnEnchantMaterial);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SELECT_GEM_UP_TIER, OnTierMaterial);
        }

        public void RequestInventory() => _send(PetEquipPackets.RequestGemInventory());
        public void BeginEnchant(int gemId) => _send(PetEquipPackets.SelectGemEnchant(gemId));
        public void ConfirmEnchant(int gemId, int material1, int material2) =>
            _send(PetEquipPackets.ConfirmEnchantGem(gemId, material1, material2));
        public void BeginTier() => _send(PetEquipPackets.RequestGemUpTierMaterial());
        public void ConfirmTier(int gemId, int materialId) =>
            _send(PetEquipPackets.UpTierGem(gemId, materialId));
        public void Remove(int gemId) => _send(PetEquipPackets.RemoveGem(gemId));

        private void OnInventory(Message message)
        {
            var count = message.Reader.ReadInt();
            if (count < 0 || count > 512)
                throw new ProtocolException($"SHOW_GEM_INVENTORY có {count} gem — ngoài khoảng hợp lệ.");
            var items = new GemItemInfo[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = new GemItemInfo
                {
                    ItemId = message.Reader.ReadInt(), IconId = message.Reader.ReadInt(),
                    Name = message.Reader.ReadUtf(), Level = message.Reader.ReadInt()
                };
            }
            message.Reader.ExpectFullyConsumed("SHOW_GEM_INVENTORY");
            InventoryReceived?.Invoke(new GemInventory { Items = items });
        }

        private void OnGemUpdated(Message message)
        {
            var item = new GemItemInfo
            {
                ItemId = message.Reader.ReadInt(), IconPath = message.Reader.ReadUtf(),
                Name = message.Reader.ReadUtf(), Level = message.Reader.ReadSByte()
            };
            message.Reader.ExpectFullyConsumed("SEND_GEM_INFO");
            GemUpdated?.Invoke(item);
        }

        private void OnGemRemoved(Message message)
        {
            var id = message.Reader.ReadInt();
            message.Reader.ExpectFullyConsumed("REMOVE_GEM_ITEM");
            GemRemoved?.Invoke(id);
        }

        private void OnEnchantMaterial(Message message)
        {
            var value = ReadMaterial(message, false);
            message.Reader.ExpectFullyConsumed("SELECT_GEM_ENCHANT");
            EnchantMaterialSelected?.Invoke(value);
        }

        private void OnTierMaterial(Message message)
        {
            var value = ReadMaterial(message, true);
            message.Reader.ExpectFullyConsumed("SELECT_GEM_UP_TIER");
            TierMaterialSelected?.Invoke(value);
        }

        private static GemMaterialSelection ReadMaterial(Message message, bool hasLevel) =>
            new GemMaterialSelection
            {
                ItemOrTemplateId = message.Reader.ReadInt(),
                IconPath = message.Reader.ReadUtf(),
                Name = message.Reader.ReadUtf(),
                Slot = message.Reader.ReadInt(),
                Level = hasLevel ? message.Reader.ReadInt() : 0
            };
    }
}
