using System;
using System.Collections.Generic;

namespace Gopet.Net.Player
{
    /// <summary>Achievement/decoration animations attached to players in the current place.</summary>
    public sealed class CharacterAnimationHandler
    {
        public event Action<CharacterAnimationUpdate> PlayerUpdated;
        public event Action<CharacterAnimationUpdate[]> PlayersListed;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SEND_ANIMATION_CHARACTER, OnPlayer);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SEND_LIST_ANIMATION_CHARACTER, OnList);
        }

        private void OnPlayer(Message message)
        {
            var update = ReadPlayer(message);
            message.Reader.ExpectFullyConsumed("SEND_ANIMATION_CHARACTER");
            PlayerUpdated?.Invoke(update);
        }

        private void OnList(Message message)
        {
            // Legacy server omits a player count; records continue until packet EOF.
            var players = new List<CharacterAnimationUpdate>();
            while (message.Reader.Remaining > 0)
            {
                if (players.Count >= 256)
                    throw new ProtocolException("SEND_LIST_ANIMATION_CHARACTER vượt quá 256 người chơi.");
                players.Add(ReadPlayer(message));
            }
            PlayersListed?.Invoke(players.ToArray());
        }

        private static CharacterAnimationUpdate ReadPlayer(Message message)
        {
            var result = new CharacterAnimationUpdate { UserId = message.Reader.ReadInt() };
            var count = message.Reader.ReadInt();
            if (count < 0 || count > 32)
                throw new ProtocolException($"Player #{result.UserId} có {count} animation — ngoài khoảng hợp lệ.");
            result.Animations = new CharacterAnimation[count];
            for (var i = 0; i < count; i++)
            {
                var frameCount = message.Reader.ReadSByte();
                if (frameCount <= 0 || frameCount > 64)
                    throw new ProtocolException($"Animation có frameCount không hợp lệ: {frameCount}.");
                result.Animations[i] = new CharacterAnimation
                {
                    FrameCount = frameCount,
                    FrameImagePath = message.Reader.ReadUtf(),
                    OffsetX = message.Reader.ReadShort(),
                    OffsetY = message.Reader.ReadShort(),
                    DrawAtEnd = message.Reader.ReadBool(),
                    MirrorWithCharacter = message.Reader.ReadBool(),
                    Type = message.Reader.ReadSByte()
                };
            }
            return result;
        }
    }
}
