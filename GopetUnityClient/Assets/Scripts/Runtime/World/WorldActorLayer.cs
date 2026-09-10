using System;
using System.Collections.Generic;
using Gopet.Net.Map;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>NPC + quái của map. Tách khỏi <see cref="MapScene"/> để mỗi lớp một việc.</summary>
    public sealed class WorldActorLayer : MonoBehaviour
    {
        private readonly Dictionary<int, WorldActorView> _npcs = new Dictionary<int, WorldActorView>();
        private readonly Dictionary<int, WorldActorView> _mobs = new Dictionary<int, WorldActorView>();
        private MapScene _scene;
        private RemoteAssetCache _assets;
        private Action<int> _talkToNpc;
        private Action<int> _attackMob;

        public static WorldActorLayer Attach(MapScene scene)
        {
            var layer = scene.gameObject.AddComponent<WorldActorLayer>();
            layer._scene = scene;
            return layer;
        }

        public void Subscribe(WorldObjectHandler handler, RemoteAssetCache assets, Action<int> talkToNpc,
            Action<int> attackMob)
        {
            _assets = assets;
            _talkToNpc = talkToNpc;
            _attackMob = attackMob;
            handler.NpcsReceived += OnNpcsReceived;
            handler.MobsReceived += OnMobsReceived;
            handler.MobRemoved += OnMobRemoved;
        }

        /// <summary>Xoá toàn bộ NPC/quái khi đổi map.</summary>
        public void Clear()
        {
            ClearActors(_npcs);
            ClearActors(_mobs);
        }

        private int MapHeight => _scene.Map.Map.HeightPixels;

        private void OnNpcsReceived(NpcSpawn[] npcs)
        {
            ClearActors(_npcs);
            if (_scene.Map == null || _assets == null) return;
            foreach (var npc in npcs)
                _npcs[npc.Id] = WorldActorView.CreateNpc(_scene.transform, npc, MapHeight, _assets, _talkToNpc);
        }

        private void OnMobsReceived(MobSpawn[] mobs)
        {
            if (_scene.Map == null || _assets == null) return;
            foreach (var mob in mobs)
            {
                OnMobRemoved(mob.Id);
                _mobs[mob.Id] = WorldActorView.CreateMob(_scene.transform, mob, MapHeight, _assets, _attackMob);
            }
        }

        private void OnMobRemoved(int id)
        {
            if (!_mobs.TryGetValue(id, out var view)) return;
            if (view != null) Destroy(view.gameObject);
            _mobs.Remove(id);
        }

        public void ApplyBossHp(BossHpUpdate update)
        {
            if (update == null) return;
            if (_mobs.TryGetValue(update.MobId, out var view) && view != null)
                view.SetBossHp(update.Hp);
        }

        private static void ClearActors(Dictionary<int, WorldActorView> actors)
        {
            foreach (var view in actors.Values)
                if (view != null) MapScene.DestroyWorldObject(view.gameObject);
            actors.Clear();
        }
    }
}
