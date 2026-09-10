using System;

namespace Gopet.Net.Pet
{
    /// <summary>Nhận toàn bộ phản hồi của màn tiến hoá pet từ PET_SERVICE.</summary>
    public sealed class PetUpgradeHandler
    {
        private readonly Action<Message> _send;

        public PetUpgradeHandler(Action<Message> send)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public event Action ShowRequested;
        public event Action<PetUpgradeSelection> PetSelected;
        public event Action<PetUpgradePrice> PriceReceived;
        public event Action<PetUpgradePreview> PreviewReceived;
        public event Action Completed;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SHOW_UPGRADE_PET, OnShow);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_UPGRADE_PET_INFO, OnPetInfo);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PRICE_UPGRADE_PET, OnPrice);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.INFO_UP_TIER_PET, OnPreview);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_UP_TIER, OnCompleted);
        }

        public void SelectPet(sbyte role) => _send(PetUpgradePackets.SelectPet(role));
        public void RequestPrice() => _send(PetUpgradePackets.RequestPrice());
        public void RequestPreview(int activeId, int materialId) =>
            _send(PetUpgradePackets.RequestPreview(activeId, materialId));
        public void Confirm(int activeId, int materialId, string name) =>
            _send(PetUpgradePackets.Confirm(activeId, materialId, name));

        private void OnShow(Message message)
        {
            message.Reader.ExpectFullyConsumed("SHOW_UPGRADE_PET");
            ShowRequested?.Invoke();
        }

        private void OnPetInfo(Message message)
        {
            var value = new PetUpgradeSelection
            {
                Role = message.Reader.ReadSByte(),
                PetId = message.Reader.ReadInt(),
                FrameImagePath = message.Reader.ReadUtf(),
                FrameIndex = message.Reader.ReadSByte()
            };
            message.Reader.ExpectFullyConsumed("PET_UPGRADE_PET_INFO");
            PetSelected?.Invoke(value);
        }

        private void OnPrice(Message message)
        {
            var value = new PetUpgradePrice
            {
                Gold = message.Reader.ReadInt(),
                Coin = message.Reader.ReadInt()
            };
            message.Reader.ExpectFullyConsumed("PRICE_UPGRADE_PET");
            PriceReceived?.Invoke(value);
        }

        private void OnPreview(Message message)
        {
            var title = message.Reader.ReadUtf();
            var count = message.Reader.ReadSByte();
            if (count < 0 || count > 64)
                throw new ProtocolException($"INFO_UP_TIER_PET có {count} dòng — ngoài khoảng hợp lệ.");
            var lines = new string[count];
            for (var i = 0; i < count; i++) lines[i] = message.Reader.ReadUtf();
            message.Reader.ExpectFullyConsumed("INFO_UP_TIER_PET");
            PreviewReceived?.Invoke(new PetUpgradePreview { Title = title, Lines = lines });
        }

        private void OnCompleted(Message message)
        {
            message.Reader.ExpectFullyConsumed("PET_UP_TIER");
            Completed?.Invoke();
        }
    }
}
