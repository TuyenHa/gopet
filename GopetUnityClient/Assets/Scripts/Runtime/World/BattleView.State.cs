using Gopet.Net.Battle;
using Gopet.Runtime.World.Battle;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Nhóm đồng bộ "gói tin → widget" của <see cref="BattleView"/>:
    /// buff, chỉ số, thanh HP/MP và tra cứu card/hud theo actorId.
    /// Tách khỏi phần dựng UI để giữ mỗi file dưới ngưỡng 200 dòng.</summary>
    public sealed partial class BattleView
    {
        private void OnBuff(BattleBuffState state)
        {
            if (state == null || state.BattleId != BattleId) return;
            foreach (var a in state.Actors) Hud(a.ActorId)?.UpdateBuffs(a);
        }

        private void OnStats(BattleStatsState state)
        {
            if (state == null || state.BattleId != BattleId) return;
            foreach (var a in state.Actors)
            {
                // Bỏ qua danh sách kỹ năng của quái: không hiển thị nữa, quái tự dùng.
                Hud(a.ActorId)?.UpdateStats(a);
            }
        }

        /// <summary>EXP nhỏ giọt mỗi đòn trúng → số vàng bay trên đầu pet.</summary>
        private void ApplyPendingVitals()
        {
            var v = _pendingVitals;
            _pendingVitals = null;
            var healed = v[0] - _left.Hp;
            _left.SetVitals(v[0], v[1], v[2], v[3]);
            if (healed > 0) BattleFloatText.Create(_left.transform, healed, false);
            RefreshHuds();
            RefreshLocks();
        }

        private void OnAnimatorDrained()
        {
            ShowPendingResult();
            RefreshLocks();
        }

        /// <summary>Hàng đợi vừa cạn — giờ mới dựng băng kết quả đang chờ.</summary>
        private void ShowPendingResult()
        {
            var result = _pendingResult;
            _pendingResult = null;
            if (result != null) ShowResult(result);
        }

        private void OnHitExp(BattleExpGain gain)
        {
            if (gain == null || gain.BattleId != BattleId || gain.Amount <= 0) return;
            var card = Card(gain.ActorId) ?? _left;
            if (card != null) BattleFloatText.CreateExp(card.transform, gain.Amount);
        }

        private BattleHudPanel Hud(int actorId) =>
            actorId == _hudLeft.ActorId ? _hudLeft : actorId == _hudRight.ActorId ? _hudRight : null;

        private void RefreshHuds()
        {
            _hudLeft.UpdateVitals(_left.Hp, _left.Mp, _left.MaxHp, _left.MaxMp);
            _hudRight.UpdateVitals(_right.Hp, _right.Mp, _right.MaxHp, _right.MaxMp);
        }

        private BattlePetCard Card(int actorId) =>
            _left.ActorId == actorId ? _left : _right.ActorId == actorId ? _right : null;

        /// <summary>Gói kết thúc trận tới gần như cùng lúc với gói lượt cuối. Nếu dựng băng
        /// chữ ngay thì phải FlushImmediate, tức áp sát thương lập tức và pet gục TRƯỚC khi
        /// hiệu ứng kịp rơi tới. Nên chờ hàng đợi diễn xong rồi mới hiện kết quả.</summary>
        public void ShowResult(BattleResult result)
        {
            if (result.BattleId != BattleId || _result != null) return;
            if (_animator != null && !_animator.Idle)
            {
                _pendingResult = result;
                return;
            }
            if (_actionBar != null) _actionBar.gameObject.SetActive(false);
            // Panel kết quả chỉ che phần giữa màn hình; panel kỹ năng nằm sát mép trái vẫn
            // lộ ra nên phải khoá tay, nếu không người chơi bấm được kỹ năng sau khi trận xong.
            _skillPopup?.SetOpen(false);
            _skillPopup?.RefreshState(_left.Mp, true);
            _result = BattleResultBanner.Create(transform, result,
                _start.LocalPet.ActorId, IsParticipant);
            ShowRewards(result);
            _resultShownAt = Time.unscaledTime;
        }

        /// <summary>Phần thưởng bay thành số trên đầu pet thay vì nằm trong popup —
        /// đúng cách jar gốc làm (<c>e.java:57-63</c>: "N (ngoc)" rồi "N EXP" so le 1s),
        /// và chỉ hiện khi &gt; 0 y như bản gốc.</summary>
        private void ShowRewards(BattleResult result)
        {
            if (_left == null) return;
            if (result.Coin > 0) BattleFloatText.CreateCoin(_left.transform, result.Coin);
            if (result.Experience > 0)
            {
                BattleFloatText.CreateExp(_left.transform, result.Experience, 1f, withUnit: true);
            }
        }

        /// <summary>Trận xong thì tự trả người chơi về map, thắng hay thua đều vậy.
        /// Nút "Tiếp tục" chỉ để đóng sớm hơn.</summary>
        private void RequestClose()
        {
            if (_closeRequested) return;
            _closeRequested = true;
            Closed?.Invoke();
        }
    }
}
