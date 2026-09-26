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
        private readonly BattleSceneSettings _scenes;
        private readonly SpectatorBattleLayer _spectators;
        private BattleView _view;
        /// <summary>Trận của mình mở trong lúc bảng thắng trận trước còn chờ OK — dựng ngay
        /// sau khi OK, KHÔNG bỏ: server đã mở trận thật, bỏ là thành trận ẩn không đánh được.</summary>
        private BattleStart _pendingStart;
        private BattleHandler _pendingHandler;
        private float _lastPacketAt;
        private float _timeoutSeconds;

        public BattleCoordinator(Transform parent, RemoteAssetCache assets, BattleHandler handler,
            Action<bool> setBattleMode, Action<string> toast = null, PlayerStatsHandler playerStats = null,
            BattleSceneSettings scenes = null, SpectatorBattleLayer spectators = null)
        {
            _parent = parent; _assets = assets; _setBattleMode = setBattleMode;
            _toast = toast; _playerStats = playerStats; _scenes = scenes;
            _spectators = spectators;
            // Thanh tiền trong trận trước đây chỉ đọc snapshot lúc mở trận; mua khung cảnh
            // giữa trận phải thấy vàng giảm ngay.
            if (_playerStats != null) _playerStats.StatsUpdated += stats => _view?.ApplyPlayerStats(stats);
            handler.BattleStarted += start => OnStarted(start, handler);
            handler.TurnReceived += OnTurn;
            handler.BattleEnded += OnEnded;
            handler.BattleRemoved += OnRemoved;
        }

        public BattleView View => _view;

        /// <summary>Map cập nhật thì dọn trận đang diễn. Kết quả thắng đã nhận phải
        /// giữ đến OK; map vẫn cập nhật bên dưới lớp chiến đấu.</summary>
        public void OnPlaceChanged()
        {
            if (_spectators != null) _spectators.Clear();
            DropPending(null);
            if (_view != null) Close();
        }

        private void OnStarted(BattleStart start, BattleHandler handler)
        {
            // Server broadcast mọi trận trong zone. JAR vẽ trận người khác ngay trong world;
            // overlay toàn màn hình chỉ dành cho trận có người chơi hiện tại tham gia.
            if (!start.IsParticipant)
            {
                if (_spectators != null) _spectators.StartBattle(start);
                return;
            }
            if (_spectators != null) _spectators.Remove(start.BattleId);
            // Không thay thế popup hoặc trả điều khiển map khi chưa xác nhận OK — hoãn tới OK.
            if (_view != null && _view.AwaitingVictoryConfirmation)
            {
                _pendingStart = start;
                _pendingHandler = handler;
                return;
            }
            Close();
            Open(start, handler);
        }

        private void Open(BattleStart start, BattleHandler handler)
        {
            _view = BattleView.Create(_parent, start, handler, _assets, _playerStats?.Snapshot, _scenes);
            _view.Closed += Close;
            _view.Ticked += CheckStalled;
            _setBattleMode?.Invoke(true);
            RefreshTimeout(start.TurnDurationMs);
        }

        private void OnTurn(BattleTurn turn)
        {
            if (_spectators != null) _spectators.Apply(turn);
            if (_view == null || _view.BattleId != turn.BattleId) return;
            _view.Apply(turn);
            RefreshTimeout(turn.TurnDurationMs);
        }

        private void OnEnded(BattleResult result)
        {
            if (_spectators != null) _spectators.End(result);
            DropPending(result.BattleId);
            if (_view != null && _view.BattleId == result.BattleId) _view.ShowResult(result);
        }

        private void OnRemoved(int battleId)
        {
            if (_spectators != null) _spectators.Remove(battleId);
            DropPending(battleId);
            if (_view == null) return;
            // Khi THẮNG, server gửi PET_BATTLE_STATE rồi sendFastRemove() ngay sau đó
            // (PetBattle.cs:951-955) vì quái đã chết. Đóng ngay ở đây sẽ giết panel kết quả
            // trong cùng frame — thắng thì panel loé rồi biến mất, thua thì panel ở lại.
            // Kết quả tự quản lý đóng: thắng PvE phải chờ người chơi bấm OK.
            if (_view.HasResult) return;
            // PvP có 2 gói FAST_REMOVE với battleId khác nhau (một cho mỗi bên) từ khi
            // phase-01 sửa server. Client vẫn phải khớp cả OpponentActorId để bền với
            // server cũ chưa có bản sửa.
            if (_view.BattleId == battleId || _view.OpponentActorId == battleId) Close();
        }

        private void Close()
        {
            // Chốt chung cho mọi đường đóng, kể cả MapUpdated từ server.
            // RequestClose chỉ bỏ cờ chờ xác nhận khi người chơi bấm OK.
            if (_view != null && _view.AwaitingVictoryConfirmation) return;
            if (_view != null)
            {
                var go = _view.gameObject;
                _view = null;
                MapScene.DestroyWorldObject(go);
            }
            _setBattleMode?.Invoke(false);
            _timeoutSeconds = 0f;
            OpenPendingStart();
        }

        /// <summary>Trận hoãn đã kết thúc (hoặc đổi map) trước khi người chơi bấm OK thì
        /// không dựng nữa. <paramref name="battleId"/> null = bỏ bất kể trận nào.</summary>
        private void DropPending(int? battleId)
        {
            if (_pendingStart == null) return;
            if (battleId.HasValue && _pendingStart.BattleId != battleId.Value) return;
            _pendingStart = null;
            _pendingHandler = null;
        }

        private void OpenPendingStart()
        {
            var start = _pendingStart;
            var handler = _pendingHandler;
            _pendingStart = null;
            _pendingHandler = null;
            if (start != null && handler != null) Open(start, handler);
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
            if (_view == null || _view.HasResult || _timeoutSeconds <= 0f) return;
            if (Time.unscaledTime - _lastPacketAt < _timeoutSeconds) return;
            _toast?.Invoke("Trận đấu đã kết thúc.");
            Close();
        }
    }
}
