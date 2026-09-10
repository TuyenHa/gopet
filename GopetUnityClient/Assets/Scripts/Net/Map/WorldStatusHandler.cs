using System;

namespace Gopet.Net.Map
{
    public sealed class BossHpUpdate
    {
        public int MobId;
        public int Hp;
    }

    public sealed class ExpBuffStatus
    {
        public string IconPath;
        /// <summary>Unix timestamp (seconds) when the server-side buff expires.</summary>
        public int ExpiresAtUnixSeconds;
        public int ReservedInt;
        public long ReservedLong;
    }

    /// <summary>Các cập nhật HUD/world ngắn nằm trong bao PET_SERVICE.</summary>
    public sealed class WorldStatusHandler
    {
        public event Action<BossHpUpdate> BossHpUpdated;
        public event Action<int> PlaceTimeUpdated;
        public event Action<string> BigTextShown;
        public event Action<ExpBuffStatus> ExpBuffUpdated;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.UPDATE_HP_BOSS, OnBossHp);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.TIME_PLACE, OnPlaceTime);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SHOW_BIG_TEXT_EFF, OnBigText);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.SHOW_EXP, OnExpBuff);
        }

        private void OnBossHp(Message message)
        {
            var update = new BossHpUpdate
            {
                MobId = message.Reader.ReadInt(),
                Hp = message.Reader.ReadInt()
            };
            message.Reader.ExpectFullyConsumed("UPDATE_HP_BOSS");
            BossHpUpdated?.Invoke(update);
        }

        private void OnPlaceTime(Message message)
        {
            var seconds = message.Reader.ReadInt();
            message.Reader.ExpectFullyConsumed("TIME_PLACE");
            PlaceTimeUpdated?.Invoke(seconds);
        }

        private void OnBigText(Message message)
        {
            var text = message.Reader.ReadUtf();
            message.Reader.ExpectFullyConsumed("SHOW_BIG_TEXT_EFF");
            BigTextShown?.Invoke(text);
        }

        private void OnExpBuff(Message message)
        {
            var status = new ExpBuffStatus
            {
                IconPath = message.Reader.ReadUtf(),
                ExpiresAtUnixSeconds = message.Reader.ReadInt(),
                // Server currently writes zero for both legacy fields. Consume and retain them
                // so a future server rollout can add meaning without shifting the wire parser.
                ReservedInt = message.Reader.ReadInt(),
                ReservedLong = message.Reader.ReadLong()
            };
            message.Reader.ExpectFullyConsumed("SHOW_EXP");
            ExpBuffUpdated?.Invoke(status);
        }
    }
}
