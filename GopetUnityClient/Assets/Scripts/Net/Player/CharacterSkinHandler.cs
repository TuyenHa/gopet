using System;

namespace Gopet.Net.Player
{
    public sealed class CharacterSkinUpdate
    {
        public int UserId;
        public string FrameImagePath;
    }

    /// <summary>Parses PET_SERVICE/SEND_SKIN for initial place state and live equip changes.</summary>
    public sealed class CharacterSkinHandler
    {
        public event Action<CharacterSkinUpdate[]> SkinsReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SEND_SKIN, OnSkins);
        }

        private void OnSkins(Message message)
        {
            var count = message.Reader.ReadInt();
            if (count < 0 || count > 256)
                throw new ProtocolException($"SEND_SKIN có {count} bản ghi — ngoài khoảng hợp lệ.");
            var updates = new CharacterSkinUpdate[count];
            for (var i = 0; i < count; i++)
            {
                updates[i] = new CharacterSkinUpdate
                {
                    UserId = message.Reader.ReadInt(),
                    FrameImagePath = message.Reader.ReadUtf()
                };
            }
            message.Reader.ExpectFullyConsumed("SEND_SKIN");
            SkinsReceived?.Invoke(updates);
        }
    }
}
