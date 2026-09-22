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
        /// <summary>Nhịp mở trận: khoá mọi nút chừng này giây sau khi màn đấu dựng xong, để
        /// ảnh pet kịp tải và người chơi kịp nhìn thấy đối thủ trước khi đòn đầu nổ.
        ///
        /// <para>PHẢI khớp <c>PetBattle.OpeningDelayMs</c> bên server (1500ms). Server mới là
        /// bên thật sự chặn; khoá ở client chỉ để nút không bấm được trong lúc chờ, tránh bấm
        /// rồi tưởng treo.</para></summary>
        private const float OpeningSeconds = 1.5f;

        private float _openUntil;
        private bool _openingDone;

        private bool InOpening => !_openingDone;

        private void BeginOpening() => _openUntil = Time.unscaledTime + OpeningSeconds;

        /// <returns>true đúng MỘT lần, ở frame nhịp mở trận vừa hết — để người gọi mở khoá nút.</returns>
        private bool TickOpening()
        {
            if (_openingDone || Time.unscaledTime < _openUntil) return false;
            _openingDone = true;
            return true;
        }

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

        /// <summary>Nhánh kết quả dùng banner vẫn giữ phần thưởng bay trên đầu pet —
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
    }
}
