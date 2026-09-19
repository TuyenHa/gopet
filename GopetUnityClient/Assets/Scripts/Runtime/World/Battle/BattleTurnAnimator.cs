using System;
using System.Collections;
using System.Collections.Generic;
using Gopet.Net.Battle;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Phát kết quả mỗi lượt theo hàng đợi, một bước mỗi lần — port <c>di.java:82-89</c>
    /// + <c>e.java</c> của jar. Trước đây áp toàn bộ effect trong một frame nên nổ cùng lúc,
    /// không có nhịp. Dùng thời gian unscaled cho khớp phần còn lại của battle UI.</summary>
    public sealed partial class BattleTurnAnimator : MonoBehaviour
    {
        private const float LungeSeconds = 0.22f;

        /// <summary>Khoảng chừa để 2 sprite không đè nhau. Quãng lao tính từ anchor thật mỗi
        /// lần chạy, không hằng số hoá: khoảng cách 2 card đổi theo tỉ lệ màn hình.</summary>
        private const float StandoffPixels = 90f;

        /// <summary>Độ cao thả hiệu ứng kỹ năng, theo tỉ lệ chiều cao canvas.</summary>
        private const float FallHeightRatio = 0.55f;

        /// <summary>Độ lệch ngang của điểm thả, theo tỉ lệ bề ngang canvas — tạo đường rơi chéo.</summary>
        private const float FallSideRatio = 0.22f;

        private const float BetweenEffects = 0.12f;
        private const float AfterEffects = 0.25f;

        private readonly Queue<BattleTurn> _queue = new Queue<BattleTurn>();
        private Func<int, BattlePetCard> _card;
        private Action _refreshHuds;
        private Action<BattleEffect, Transform> _playHitSound;
        private Action _onDrained;
        private Coroutine _running;
        private BattleTurn _inFlight;
        private int _appliedEffects;
        private bool _mainApplied;

        public static BattleTurnAnimator Attach(GameObject host, Func<int, BattlePetCard> card,
            Action refreshHuds, Action<BattleEffect, Transform> playHitSound, Action onDrained)
        {
            var animator = host.AddComponent<BattleTurnAnimator>();
            animator._card = card;
            animator._refreshHuds = refreshHuds;
            animator._playHitSound = playHitSound;
            animator._onDrained = onDrained;
            return animator;
        }

        public bool Idle => _running == null && _queue.Count == 0;

        public void Enqueue(BattleTurn turn)
        {
            _queue.Enqueue(turn);
            if (_running == null) _running = StartCoroutine(Drain());
        }

        /// <summary>Áp nốt thay đổi còn treo mà không diễn hoạt — trận kết thúc giữa chừng
        /// thì số liệu cuối vẫn phải đúng.</summary>
        public void FlushImmediate()
        {
            StopAllCoroutines();
            _running = null;
            if (_inFlight != null)
            {
                ApplyRemaining(_inFlight, _appliedEffects, _mainApplied);
                _inFlight = null;
            }
            while (_queue.Count > 0) ApplyRemaining(_queue.Dequeue(), 0, false);
            SnapAll();
            _refreshHuds?.Invoke();
            _onDrained?.Invoke();
        }

        /// <param name="mainApplied">MP chính đã áp trong PlayTurn chưa. Thiếu cờ này thì mọi
        /// trận kết thúc bằng kỹ năng đều bị trừ MP hai lần.</param>
        private void ApplyRemaining(BattleTurn turn, int fromIndex, bool mainApplied)
        {
            if (!mainApplied && turn.MainMpDelta != 0)
            {
                Card(turn.ActorId)?.Apply(0, turn.MainMpDelta);
            }
            for (var i = fromIndex; i < turn.Effects.Length; i++)
            {
                var effect = turn.Effects[i];
                Card(effect.ActorId)?.Apply(effect.HpDelta, effect.MpDelta);
            }
        }

        private IEnumerator Drain()
        {
            // try/finally (C# cấm try/catch quanh yield) để `_running` luôn được xoá, kể cả khi
            // coroutine bị dừng hoặc chết vì exception — thiếu nó thì `Idle` kẹt false cả trận.
            try
            {
                while (_queue.Count > 0)
                {
                    _inFlight = _queue.Dequeue();
                    _appliedEffects = 0;
                    _mainApplied = false;
                    yield return PlayTurn(_inFlight);
                    _inFlight = null;
                }
            }
            finally
            {
                _running = null;
            }
            _onDrained?.Invoke();
        }

        private IEnumerator PlayTurn(BattleTurn turn)
        {
            var actor = Card(turn.ActorId);
            if (turn.MainMpDelta != 0) actor?.Apply(0, turn.MainMpDelta);

            _mainApplied = true;

            // Đòn thường mới lao sang; kỹ năng thì pet ĐỨNG IM, hiệu ứng tự bay.
            // Server: đòn thường type=Normal, kỹ năng type=Wait (PetBattle.sendPetAttack).
            var melee = turn.Type == BattleTurn.Normal;
            var attacker = melee && HitsOpponent(turn, actor) ? actor : null;
            var lunged = false;
            if (attacker != null)
            {
                var travel = LungeTravel(attacker);
                if (travel != 0f)
                {
                    lunged = true;
                    yield return attacker.PlayLunge(attacker.HomeX + travel, LungeSeconds);
                }
            }

            foreach (var effect in turn.Effects)
            {
                // Diễn hiệu ứng TRƯỚC, trừ máu SAU: Apply() gọi FaintIfDown() nên nếu trừ
                // ngay thì pet đổ vật ra trước khi ngọn lửa kịp rơi tới.
                var impact = StartEffect(effect, melee ? null : actor);
                if (impact > 0f) yield return new WaitForSecondsRealtime(impact);
                ApplyImpact(effect);
                _appliedEffects++;
                yield return new WaitForSecondsRealtime(BetweenEffects);
            }

            yield return new WaitForSecondsRealtime(AfterEffects);
            if (lunged) yield return attacker.ReturnHome(LungeSeconds);
            _refreshHuds?.Invoke();
        }

        /// <summary>Dựng phần NHÌN của một effect. Hiệu ứng KHÔNG được làm chết hàng đợi:
        /// JarSkin ném khi thiếu sprite, mà Unity dừng hẳn coroutine khi có exception —
        /// nút sẽ khoá cả trận.</summary>
        /// <returns>Số giây chờ tới lúc chạm đích, 0 nếu nổ ngay tại chỗ.</returns>
        private float StartEffect(BattleEffect effect, BattlePetCard caster)
        {
            var hit = Card(effect.ActorId);
            if (hit == null) return 0f;
            try
            {
                // Buff lên chính mình thì nổ tại chỗ: không tia, không rơi, không vòng phép.
                if (caster == null || caster == hit)
                {
                    BattleEffectView.Play(transform, hit.EffectAnchor, effect.SkillId);
                    return 0f;
                }

                // Sét KHÔNG có vòng phép dưới chân: sét giáng thẳng từ trời, vẽ thêm vòng
                // triệu hồi dưới chân là sai nguồn gốc đòn đánh. Lửa thì vẫn giữ.
                if (!BattleEffectNames.UsesBoltStrike(effect.SkillId))
                {
                    BattleGroundSigil.Play(transform, hit.EffectAnchor);
                }
                // Chỉ một luồng: dội từ trên xuống. Tia ngang từ tay pet ra đòn đã bỏ theo
                // yêu cầu — cần lại thì phát thêm một lượt xuất phát từ caster.EffectAnchor.
                return BattleEffectView.Play(transform, hit.EffectAnchor, effect.SkillId,
                    FallOrigin(hit.EffectAnchor, caster));
            }
            catch (Exception e)
            {
                // Nêu rõ skillId: exception trần không cho biết kỹ năng nào hỏng, mà đây là
                // đường duy nhất báo lỗi vì hàng đợi cố tình nuốt để không chết cả trận.
                Debug.LogError($"[BattleEffect] skillId={effect.SkillId} không dựng được: {e}");
                return 0f;
            }
        }

        /// <summary>Trừ máu/mana, số bay và tiếng va chạm — chạy khi hiệu ứng đã tới nơi.</summary>
        private void ApplyImpact(BattleEffect effect)
        {
            var hit = Card(effect.ActorId);
            if (hit == null) return;
            hit.Apply(effect.HpDelta, effect.MpDelta);
            _playHitSound?.Invoke(effect, hit.transform);
            _refreshHuds?.Invoke();
        }

        private BattlePetCard Card(int actorId) => _card?.Invoke(actorId);
    }
}
