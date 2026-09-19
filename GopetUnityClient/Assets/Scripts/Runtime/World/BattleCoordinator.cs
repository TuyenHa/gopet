using System;
using Gopet.Net.Battle;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Giữ vòng đời battle local và trả quyền điều khiển về map khi xong.
    ///
    /// <para>Có 3 lối thoát để overlay không bao giờ treo:</para>
    /// <list type="number">
    ///   <item>Server đẩy <c>PET_BATTLE_STATE</c> hoặc <c>FAST_REMOVE_MOB</c> — nhánh chuẩn.</item>
    ///   <item><see cref="OnPlaceChanged"/> gọi từ <c>MapHandler.MapUpdated</c> khi player
    ///         bị teleport về map khác (ví dụ hết trận đấu trường server đá về map 19).</item>
    ///   <item>Timeout <c>TurnDurationMs × 3</c> không nhận gói lượt/kết thúc nào.</item>
    /// </list>
    /// </summary>
    public sealed class BattleCoordinator
    {
        private const int TimeoutMultiplier = 3;
        private const float FallbackTurnSeconds = 25f; // khớp GopetManager.TimeNextTurn

        private readonly Transform _parent;
        private readonly RemoteAssetCache _assets;
        private readonly Action<bool> _setBattleMode;
        private readonly Action<string> _toast;
        private readonly PlayerStatsHandler _playerStats;
        private BattleView _view;
        private float _lastPacketAt;
        private float _timeoutSeconds;

        public BattleCoordinator(Transform parent, RemoteAssetCache assets, BattleHandler handler,
            Action<bool> setBattleMode, Action<string> toast = null, PlayerStatsHandler playerStats = null)
        {
            _parent = parent; _assets = assets; _setBattleMode = setBattleMode;
            _toast = toast; _playerStats = playerStats;
            handler.BattleStarted += start => OnStarted(start, handler);
            handler.TurnReceived += OnTurn;
            handler.BattleEnded += OnEnded;
            handler.BattleRemoved += OnRemoved;
        }

        public BattleView View => _view;

        /// <summary>Gọi khi <c>MapHandler.MapUpdated</c> báo player đã sang place mới.
        /// Server LUÔN gọi <c>petBattle.Close(player)</c> trước khi chuyển place
        /// (<c>GopetPlace.cs:48,76</c>) nên đây là tín hiệu "trận cũ hết đời" đáng tin.</summary>
        public void OnPlaceChanged()
        {
            if (_view != null) Close();
        }

        private void OnStarted(BattleStart start, BattleHandler handler)
        {
            // Server broadcast mọi trận trong zone. JAR vẽ trận người khác ngay trong world;
            // overlay toàn màn hình chỉ dành cho trận có người chơi hiện tại tham gia.
            if (!start.IsParticipant) return;
            Close();
            _view = BattleView.Create(_parent, start, handler, _assets, _playerStats?.Snapshot);
            _view.Closed += Close;
            _view.Ticked += CheckStalled;
            _setBattleMode?.Invoke(true);
            RefreshTimeout(start.TurnDurationMs);
        }

        private void OnTurn(BattleTurn turn)
        {
            if (_view == null || _view.BattleId != turn.BattleId) return;
            _view.Apply(turn);
            RefreshTimeout(turn.TurnDurationMs);
        }

        private void OnEnded(BattleResult result)
        {
            if (_view != null && _view.BattleId == result.BattleId) _view.ShowResult(result);
        }

        private void OnRemoved(int battleId)
        {
            if (_view == null) return;
            // PvP có 2 gói FAST_REMOVE với battleId khác nhau (một cho mỗi bên) từ khi
            // phase-01 sửa server. Client vẫn phải khớp cả OpponentActorId để bền với
            // server cũ chưa có bản sửa.
            if (_view.BattleId == battleId || _view.OpponentActorId == battleId) Close();
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
            _timeoutSeconds = 0f;
        }

        private void RefreshTimeout(int turnDurationMs)
        {
            var seconds = turnDurationMs > 0 ? turnDurationMs / 1000f : FallbackTurnSeconds;
            _timeoutSeconds = seconds * TimeoutMultiplier;
            _lastPacketAt = Time.unscaledTime;
        }

        /// <summary>Ticker gọi mỗi frame bởi <see cref="BattleView.Update"/> — tránh thêm
        /// MonoBehaviour riêng chỉ để đo thời gian.</summary>
        private void CheckStalled()
        {
            if (_view == null || _timeoutSeconds <= 0f) return;
            if (Time.unscaledTime - _lastPacketAt < _timeoutSeconds) return;
            _toast?.Invoke("Trận đấu đã kết thúc.");
            Close();
        }
    }
}
