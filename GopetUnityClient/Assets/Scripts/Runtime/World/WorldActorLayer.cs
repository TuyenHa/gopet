using System;
using System.Collections.Generic;
using Gopet.Net.Map;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>NPC + quái của map. Tách khỏi <see cref="MapScene"/> để mỗi lớp một việc.</summary>
    public sealed partial class WorldActorLayer : MonoBehaviour
    {
        private const float ScanInterval = 0.1f; // 10 lần/giây đủ mượt, không tốn CPU trên 5-10 NPC
        private const float NpcBodyCenterOffset = 32f; // đẩy điểm proximity từ chân NPC lên tâm thân

        private readonly Dictionary<int, WorldActorView> _npcs = new Dictionary<int, WorldActorView>();
        private readonly Dictionary<int, WorldActorView> _mobs = new Dictionary<int, WorldActorView>();
        private readonly List<NpcPoint> _points = new List<NpcPoint>();
        private MapScene _scene;
        private RemoteAssetCache _assets;
        private Action<int> _talkToNpc;
        private Action<int> _attackMob;
        private int _promptNpcId = NpcProximity.None;
        private float _nextScan;
        private float _nextDebugLog; // TODO(debug): xoá khối log này sau khi xác định xong lý do nút không hiện

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
            _promptNpcId = NpcProximity.None;
            ClearMobTarget();
        }

        private int MapHeight => _scene.Map.Map.HeightPixels;

        private void OnNpcsReceived(NpcSpawn[] npcs)
        {
            ClearActors(_npcs);
            _promptNpcId = NpcProximity.None; // NPC cũ đã huỷ, nút cũ (nếu có) không còn đối tượng
            if (_scene.Map == null || _assets == null) return;
            foreach (var npc in npcs)
                _npcs[npc.Id] = WorldActorView.CreateNpc(_scene.transform, npc, MapHeight, _assets, _talkToNpc);
        }

        /// <summary>Quét khoảng cách người chơi ↔ NPC map hiện tại, hiện nút "Nói chuyện" trên
        /// NPC gần nhất trong tầm (throttle <see cref="ScanInterval"/> — không cần mỗi frame).</summary>
        private void Update()
        {
            if (Time.time < _nextScan) return;
            _nextScan = Time.time + ScanInterval;

            var self = _scene != null ? _scene.Self : null;
            if (self == null)
            {
                SetPrompt(NpcProximity.None);
                ClearMobTarget();
                return;
            }

            _points.Clear();
            foreach (var pair in _npcs)
            {
                if (pair.Value == null) continue;
                var pos = pair.Value.transform.localPosition;
                // NPC dựng với pivot ở CHÂN (0.5, 0), nên transform.localPosition = vị trí chân.
                // NPC cao ~64px, nếu tính khoảng cách chân-chân thì đứng "trên đầu" NPC vẫn xa
                // 64px so với chân → phải bước quá sát mới trigger. Đẩy điểm tham chiếu lên
                // tâm thân (chân + 32) để đứng cạnh bất kỳ hướng nào đều cho cùng cảm giác gần.
                _points.Add(new NpcPoint(pair.Key, pos.x, pos.y + NpcBodyCenterOffset));
            }

            var selfPos = self.transform.localPosition;
            // Đẩy Y người chơi lên tâm thân — đối xứng với NPC (xem NpcBodyCenterOffset).
            // Nếu chỉ đẩy 1 phía, khi 2 người đứng NGANG nhau khoảng cách vẫn đúng (dx thôi),
            // nhưng khi 1 người đứng "trên đầu" thì lệch. Đẩy cả 2 phía = thân-vs-thân chuẩn.
            var pickerY = selfPos.y + NpcBodyCenterOffset;
            var picked = NpcProximity.Pick(selfPos.x, pickerY, _points, _promptNpcId);
            if (Time.time >= _nextDebugLog)
            {
                _nextDebugLog = Time.time + 1f;
                var sb = new System.Text.StringBuilder();
                sb.Append($"[TalkPromptDebug] player=({selfPos.x:F0},{pickerY:F0}) npcCount={_points.Count} picked={picked} current={_promptNpcId} show={NpcProximity.ShowRadius}");
                foreach (var p in _points)
                {
                    var dx = selfPos.x - p.X;
                    var dy = pickerY - p.Y;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    sb.Append($" | npc{p.Id}=({p.X:F0},{p.Y:F0}) d={d:F0}");
                }
                Debug.Log(sb.ToString());
            }
            SetPrompt(picked);
            ScanMobs(selfPos);
        }

        private void SetPrompt(int id)
        {
            if (id == _promptNpcId) return;
            Debug.Log($"[TalkPromptDebug] SetPrompt {_promptNpcId} -> {id}");
            if (_promptNpcId != NpcProximity.None && _npcs.TryGetValue(_promptNpcId, out var previous)
                && previous != null)
                previous.SetTalkPromptVisible(false);
            if (id != NpcProximity.None && _npcs.TryGetValue(id, out var next) && next != null)
                next.SetTalkPromptVisible(true);
            else if (id != NpcProximity.None)
                Debug.LogWarning($"[TalkPromptDebug] picked id {id} khong co trong _npcs (khong tim thay view)");
            _promptNpcId = id;
        }

        private void OnMobsReceived(MobSpawn[] mobs)
        {
            if (_scene.Map == null || _assets == null) return;
            foreach (var mob in mobs)
            {
                OnMobRemoved(mob.Id);
                var view = WorldActorView.CreateMob(_scene.transform, mob, MapHeight, _assets, _attackMob);
                _mobs[mob.Id] = view;
                // Quái đứng chôn chân nhìn như tượng; server không gửi gói di chuyển nào
                // nên client tự cho nó đi lảng vảng quanh chỗ sinh.
                MobWanderer.Attach(view, _scene.Map?.Map, mob.Id, mob.X, mob.Y + mob.VerticalOffset);
            }
        }

        private void OnMobRemoved(int id)
        {
            if (!_mobs.TryGetValue(id, out var view)) return;
            if (view != null) Destroy(view.gameObject);
            _mobs.Remove(id);
        }

        /// <summary>Transform của một con quái, hoặc null nếu nó đã bị gỡ khỏi map.</summary>
        public Transform MobTransform(int mobId) =>
            _mobs.TryGetValue(mobId, out var view) && view != null ? view.transform : null;

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
