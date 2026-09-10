using System;

namespace Gopet.Net.Pet
{
    /// <summary>
    /// Nhận 3 sub-command PET_SERVICE về pet-follow lifecycle:
    /// <list type="bullet">
    ///   <item><c>SEND_LIST_PET_ZONE (8)</c> — danh sách pet đi cùng player khác trong zone.</item>
    ///   <item><c>PET_UNFOLLOW (3)</c> — pet của userId đã unfollow.</item>
    ///   <item><c>MY_PET_INFO (40)</c> — HP/MP pet self (bơm sau LIST_PET_ZONE + mỗi thay đổi).</item>
    /// </list>
    /// Wire khớp <c>GopetPlace.sendListPet</c> (line 208-246) và <c>GameController.sendMyPetInfo</c> (1362).
    ///
    /// <para>Version note: server bơm 2 format khác nhau tùy VERSION_133. Client Unity mới —
    /// dùng format &gt; 133 với <c>frameNum</c> + <c>vY</c>. Server phân versioned message
    /// (<c>ListWriterMessage(2, PET_SERVICE)</c>) → 1 trong 2 message được gửi tuỳ version.</para>
    /// </summary>
    public sealed class PetZoneHandler
    {
        public event Action<PetZoneUpdate> PetZoneReceived;
        public event Action<PetUnfollow> PetUnfollowed;
        public event Action<MyPetInfo> MyPetInfoReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SEND_LIST_PET_ZONE, OnZone);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_UNFOLLOW, OnUnfollow);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.MY_PET_INFO, OnMyPetInfo);
        }

        private void OnZone(Message message)
        {
            var r = message.Reader;
            var count = r.ReadSByte();
            if (count < 0 || count > 64)
                throw new ProtocolException($"SEND_LIST_PET_ZONE count không hợp lệ: {count}.");
            var entries = new PetZoneEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = new PetZoneEntry
                {
                    OwnerUserId = r.ReadInt(),
                    PetIdTemplate = r.ReadInt(),
                    FrameImagePath = r.ReadUtf(),
                    DisplayName = r.ReadUtf(),
                    Level = r.ReadInt(),
                    FrameNum = r.ReadSByte(),
                    VerticalOffset = r.ReadShort(),
                };
            }
            r.ExpectFullyConsumed("SEND_LIST_PET_ZONE");
            PetZoneReceived?.Invoke(new PetZoneUpdate { Entries = entries });
        }

        private void OnUnfollow(Message message)
        {
            var r = message.Reader;
            var evt = new PetUnfollow { OwnerUserId = r.ReadInt() };
            r.ExpectFullyConsumed("PET_UNFOLLOW");
            PetUnfollowed?.Invoke(evt);
        }

        private void OnMyPetInfo(Message message)
        {
            var r = message.Reader;
            var evt = new MyPetInfo
            {
                Hp = r.ReadInt(),
                MaxHp = r.ReadInt(),
                Mp = r.ReadInt(),
                MaxMp = r.ReadInt(),
            };
            r.ExpectFullyConsumed("MY_PET_INFO");
            MyPetInfoReceived?.Invoke(evt);
        }
    }
}
