using System;
using Gopet.Net.Battle;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Giữ vòng đời battle local và trả quyền điều khiển về map khi xong.</summary>
    public sealed class BattleCoordinator
    {
        private readonly Transform _parent;
        private readonly RemoteAssetCache _assets;
        private readonly Action<bool> _setBattleMode;
        private BattleView _view;

        public BattleCoordinator(Transform parent, RemoteAssetCache assets, BattleHandler handler,
            Action<bool> setBattleMode)
        {
            _parent = parent; _assets = assets; _setBattleMode = setBattleMode;
            handler.BattleStarted += start => OnStarted(start, handler);
            handler.TurnReceived += OnTurn;
            handler.BattleEnded += OnEnded;
            handler.BattleRemoved += OnRemoved;
        }

        public BattleView View => _view;

        private void OnStarted(BattleStart start, BattleHandler handler)
        {
            // Server broadcast mọi trận trong zone. JAR vẽ trận người khác ngay trong world;
            // overlay toàn màn hình chỉ dành cho trận có người chơi hiện tại tham gia.
            if (!start.IsParticipant) return;
            Close();
            _view = BattleView.Create(_parent, start, handler, _assets);
            _view.Closed += Close;
            _setBattleMode?.Invoke(true);
        }

        private void OnTurn(BattleTurn turn)
        {
            if (_view != null && _view.BattleId == turn.BattleId) _view.Apply(turn);
        }

        private void OnEnded(BattleResult result)
        {
            if (_view != null && _view.BattleId == result.BattleId) _view.ShowResult(result);
        }

        private void OnRemoved(int battleId)
        {
            if (_view != null && _view.BattleId == battleId) Close();
        }

        private void Close()
        {
            if (_view != null)
            {
                var go = _view.gameObject;
                _view = null;
                MapScene.DestroyWorldObject(go);
            }
            _setBattleMode?.Invoke(false);
        }
    }
}
