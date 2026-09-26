using System;
using System.Collections.Generic;
using Gopet.Net.Battle;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed class SpectatorBattleLayer : MonoBehaviour
    {
        private readonly SpectatorBattles _state = new SpectatorBattles();
        private readonly Dictionary<int, SpectatorBattleView> _views = new Dictionary<int, SpectatorBattleView>();
        private readonly List<int> _remove = new List<int>();
        private Func<int, Transform> _resolve;
        public int Count => _state.Count;
        public static SpectatorBattleLayer Create(Transform parent, Func<int, Transform> resolve)
        {
            var go = new GameObject("Spectator battles"); go.transform.SetParent(parent, false);
            var layer = go.AddComponent<SpectatorBattleLayer>(); layer._resolve = resolve; return layer;
        }
        public void StartBattle(BattleStart start)
        {
            if (!_state.Start(start, Time.unscaledTime)) return;
            DropView(start.BattleId);
            Tick();
        }
        public void Apply(BattleTurn turn)
        {
            if (_state.Apply(turn, Time.unscaledTime) == null) return;
            if (_views.TryGetValue(turn.BattleId, out var view) && view != null) view.Apply(turn);
        }
        public void End(BattleResult result) => _state.End(result, Time.unscaledTime);
        public void Remove(int id) { _state.Remove(id); Tick(); }
        public void Clear()
        {
            _state.Clear();
            foreach (var view in _views.Values) if (view != null) MapScene.DestroyWorldObject(view.gameObject);
            _views.Clear();
        }
        private void LateUpdate() => Tick();
        private void Tick()
        {
            var now = Time.unscaledTime;
            _state.Expire(now); _remove.Clear();
            foreach (var entry in _state.Entries)
            {
                var left = _resolve?.Invoke(entry.Left.ActorId);
                var right = _resolve?.Invoke(entry.Right.ActorId);
                if (left == null || right == null)
                {
                    if (_views.ContainsKey(entry.BattleId) || now - entry.CreatedAt >= 2)
                        _remove.Add(entry.BattleId);
                    continue;
                }
                if (!_views.TryGetValue(entry.BattleId, out var view) || view == null)
                    _views[entry.BattleId] = view = SpectatorBattleView.Create(transform, entry);
                view.Follow(left, right);
            }
            foreach (var id in _remove) _state.Remove(id);
            _remove.Clear();
            foreach (var pair in _views) if (_state.Find(pair.Key) == null) _remove.Add(pair.Key);
            foreach (var id in _remove) DropView(id);
        }
        private void DropView(int id)
        {
            if (!_views.TryGetValue(id, out var view)) return;
            if (view != null) MapScene.DestroyWorldObject(view.gameObject);
            _views.Remove(id);
        }
        private void OnDestroy() => Clear();
    }
}
