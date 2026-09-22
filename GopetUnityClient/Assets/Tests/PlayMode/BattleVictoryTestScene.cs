using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gopet.Net;
using Gopet.Net.Battle;
using Gopet.Runtime;
using Gopet.Runtime.Assets;
using Gopet.Runtime.World;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>Gói thử đi qua handler và coordinator thật, không mở socket/server.</summary>
    internal sealed class BattleVictoryTestScene : IDisposable
    {
        public readonly GameObject Root = new GameObject("Victory integration");
        public readonly List<bool> BattleModes = new List<bool>();
        public readonly List<Message> Sent = new List<Message>();
        public readonly BattleCoordinator Coordinator;
        private readonly MessageRouter _router;
        private readonly RemoteAssetCache _cache;

        public BattleVictoryTestScene()
        {
            var client = Root.AddComponent<GopetClient>();
            _router = client.Router;
            _cache = new RemoteAssetCache(client, _router,
                Path.Combine(Application.temporaryCachePath, "gopet-victory-tests"));
            var handler = new BattleHandler(m => Sent.Add(m), 7);
            handler.RegisterOn(_router);
            Coordinator = new BattleCoordinator(Root.transform, _cache, handler, BattleModes.Add);
        }

        public void Start(bool pvp = false)
        {
            var m = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(pvp ? GopetCmd.PLAYER_BATTLE : GopetCmd.ATTACK_MOB)
                .PutInt(5000).PutInt(15000).PutInt(7);
            if (pvp) m.PutSByte(1);
            Visual(m, "Mèo");
            for (var i = 0; i < 5; i++) m.PutInt(1);
            Vitals(m);
            m.PutSByte(1).PutInt(103).PutUtf("Cuồng nộ").PutUtf("Tăng sức mạnh").PutInt(5);
            m.PutInt(-99);
            Visual(m, "Sói");
            Vitals(m);
            m.PutSByte(0);
            Dispatch(m);
        }

        public void SkillTurn()
        {
            var m = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.PET_BATTLE)
                .PutInt(7).PutInt(7).PutInt(1000).PutInt(15000).PutSByte(BattleTurn.Wait)
                .PutInt(0).PutUtf("").PutInt(-5).PutInt(1)
                .PutInt(7).PutInt(103).PutUtf("");
            for (var i = 0; i < 7; i++) m.PutInt(0);
            Dispatch(m);
        }

        public void Result(int winner = 7, string reward = "Bùa x1") => Dispatch(
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.PET_BATTLE_STATE)
                .PutInt(7).PutInt(winner).PutSByte(0).PutInt(120).PutInt(350)
                .PutSByte(1).PutUtf(reward).PutUtf("2"));

        public void Remove() => Dispatch(Message.Create(GopetCmd.PET_SERVICE)
            .PutSByte(GopetCmd.FAST_REMOVE_MOB).PutInt(7));

        public void ExpireTimers()
        {
            SetField(Coordinator, "_lastPacketAt", Time.unscaledTime - 1000f);
            SetField(Coordinator.View, "_resultShownAt", Time.unscaledTime - 1000f);
        }

        private void Dispatch(Message message)
        {
            using (message)
            using (var incoming = Message.FromWire(message.ToWire(), false)) _router.Dispatch(incoming);
        }

        private static void Visual(Message m, string name) => m.PutInt(1).PutUtf("")
            .PutSByte(1).PutShort(0).PutUtf(name).PutInt(1);

        private static void Vitals(Message m) => m.PutInt(100).PutInt(20).PutInt(100).PutInt(20);

        internal static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);

        public void Dispose()
        {
            Coordinator.OnPlaceChanged();
            _cache.Dispose();
            foreach (var m in Sent) m.Dispose();
            UnityEngine.Object.DestroyImmediate(Root);
        }
    }
}
