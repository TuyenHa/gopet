using System;
using System.Collections.Generic;

namespace Gopet.Net.Map
{
    /// <summary>NPC, quái và tương tác pet được server gửi khi vào khu vực.</summary>
    public sealed class WorldObjectHandler
    {
        public event Action<NpcSpawn[]> NpcsReceived;
        public event Action<MobSpawn[]> MobsReceived;
        public event Action<int> MobRemoved;
        public event Action<PetInteraction> PetInteractionReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.Register(GopetCmd.GAME_OBJECT, OnGameObjects);
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SEND_LIST_MOB_ZONE, OnMobs);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.ON_PET_INTERACT, OnPetInteraction);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.REMOVE_BATTLE_BY_MOB_ID, OnMobRemoved);
        }

        private void OnGameObjects(Message message)
        {
            var r = message.Reader;
            var result = new List<NpcSpawn>();
            while (r.Remaining > 0)
            {
                var kind = r.ReadSByte();
                if (kind != 0) throw new ProtocolException($"GAME_OBJECT kind chưa hỗ trợ: {kind}.");
                var npc = new NpcSpawn { Bounds = new int[4] };
                for (var i = 0; i < npc.Bounds.Length; i++) npc.Bounds[i] = r.ReadInt();
                npc.Id = r.ReadInt();
                npc.ImagePath = r.ReadUtf();
                npc.FrameCount = r.ReadInt();
                npc.X = r.ReadInt();
                npc.Y = r.ReadInt();
                npc.VerticalOffset = r.ReadInt();
                var chatCount = Count(r.ReadInt(), 128, "NPC chat");
                npc.Chat = new string[chatCount];
                for (var i = 0; i < chatCount; i++) npc.Chat[i] = r.ReadUtf();
                npc.Name = r.ReadUtf();
                npc.NpcType = r.ReadSByte();
                result.Add(npc);
            }
            NpcsReceived?.Invoke(result.ToArray());
        }

        private void OnMobs(Message message)
        {
            var r = message.Reader;
            var count = Count(r.ReadInt(), 1024, "mob");
            var result = new MobSpawn[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = new MobSpawn
                {
                    Id = r.ReadInt(), ImagePath = r.ReadUtf(), Name = r.ReadUtf(),
                    Level = r.ReadInt(), X = r.ReadInt(), Y = r.ReadInt()
                };
                r.ReadSByte(); // trạng thái, server hiện luôn 0
                result[i].FrameCount = r.ReadSByte();
                result[i].VerticalOffset = r.ReadShort();
                result[i].IsBoss = r.ReadBool();
            }
            r.ExpectFullyConsumed("SEND_LIST_MOB_ZONE");
            MobsReceived?.Invoke(result);
        }

        private void OnPetInteraction(Message message)
        {
            var evt = new PetInteraction
            {
                UserId = message.Reader.ReadInt(),
                Type = message.Reader.ReadSByte()
            };
            message.Reader.ExpectFullyConsumed("ON_PET_INTERACT");
            PetInteractionReceived?.Invoke(evt);
        }

        private void OnMobRemoved(Message message)
        {
            var id = message.Reader.ReadInt();
            message.Reader.ExpectFullyConsumed("REMOVE_BATTLE_BY_MOB_ID");
            MobRemoved?.Invoke(id);
        }

        private static int Count(int value, int max, string label)
        {
            if (value < 0 || value > max)
                throw new ProtocolException($"Số lượng {label} không hợp lệ: {value}.");
            return value;
        }
    }
}
