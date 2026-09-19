using System;
using Gopet.Net.Battle;
using Gopet.Net.Player;
using Gopet.Runtime.Audio;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using Gopet.Runtime.World.Battle;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed class BattleView : MonoBehaviour
    {
        private BattleHandler _handler;
        private BattleStart _start;
        private BattlePetCard _left, _right;
        private BattleHudPanel _hudLeft, _hudRight;
        private BattleSkillPanel _skillLeft, _skillRight;
        private BattleActionBar _actionBar;
        private BattleTopBar _topBar;
        private BattleVsIndicator _vsIndicator;
        private GameObject _result;
        private readonly SkillCooldownTracker _cooldowns = new SkillCooldownTracker();
        private bool _surrendered;

        public int BattleId => _start.BattleId;
        public bool IsParticipant => _start.IsParticipant;
        public int OpponentActorId => _start.Opponent.ActorId;
        public int TurnDurationMs => _start.TurnDurationMs;
        public event Action Closed;
        public event Action Ticked;

        public static BattleView Create(Transform parent, BattleStart start, BattleHandler handler,
            RemoteAssetCache assets, PlayerStats playerStats = null)
        {
            var go = new GameObject("Pet Battle", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 40;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f); scaler.matchWidthOrHeight = 1f;
            var view = go.AddComponent<BattleView>();
            view._handler = handler; view._start = start;
            view.Build(assets, UiBuilder.BuiltinFont(), playerStats);
            handler.BuffStateReceived += view.OnBuff;
            handler.StatsReceived += view.OnStats;
            return view;
        }

        private void OnDestroy()
        {
            if (_handler == null) return;
            _handler.BuffStateReceived -= OnBuff;
            _handler.StatsReceived -= OnStats;
        }

        public void Apply(BattleTurn turn)
        {
            if (turn.BattleId != BattleId) return;
            _vsIndicator?.SetTurn(turn.ActorId == _start.LocalPet.ActorId);
            _cooldowns.OnTurnAdvanced(turn.ActorId, _start.LocalPet.ActorId);
            Card(turn.ActorId)?.Apply(0, turn.MainMpDelta);
            foreach (var effect in turn.Effects)
            {
                var target = Card(effect.ActorId);
                if (target == null) continue;
                target.Apply(effect.HpDelta, effect.MpDelta);
                BattleEffectView.Play(transform, target.EffectAnchor, effect.SkillId);
                PlayHitSound(effect, target.transform);
            }
            RefreshHuds();
            _actionBar?.Unlock();
            _skillLeft?.RefreshState(_left.Mp);
        }

        public void ShowResult(BattleResult result)
        {
            if (result.BattleId != BattleId || _result != null) return;
            if (_actionBar != null) _actionBar.gameObject.SetActive(false);
            _result = BattleResultPanel.Create(transform, result,
                _start.LocalPet.ActorId, IsParticipant, () => Closed?.Invoke());
        }

        private void Build(RemoteAssetCache assets, Font font, PlayerStats playerStats)
        {
            var bg = new GameObject("Nền", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(transform, false);
            UiBuilder.Stretch((RectTransform)bg.transform);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = BattleSkin.Load("Battle/bg-forest");
            bgImg.color = bgImg.sprite == null ? new Color(0f, 0.08f, 0.13f, 0.92f) : Color.white;
            bgImg.type = Image.Type.Simple;
            bgImg.raycastTarget = false;

            _topBar = BattleTopBar.Create(transform, font, _start.Kind, playerStats);
            _topBar.BackClicked += OnBackClicked;
            _hudLeft = BattleHudPanel.Create(transform, _start.LocalPet, true, font, assets);
            _hudRight = BattleHudPanel.Create(transform, _start.Opponent, false, font, assets);
            _left = BattlePetCard.Create(transform, _start.LocalPet, true, assets);
            _right = BattlePetCard.Create(transform, _start.Opponent, false, assets);

            _vsIndicator = BattleVsIndicator.Create(transform, font);
            _vsIndicator.SetTurn(_start.LocalStarts);

            _skillLeft = BattleSkillPanel.Create(transform, _start.LocalPet.Skills, true, font, _cooldowns);
            _skillLeft.SkillUsed += OnSkillUsed;
            _skillLeft.RefreshState(_start.LocalPet.Mp);
            _skillRight = BattleSkillPanel.Create(transform, _start.Opponent.Skills, false, font);

            _actionBar = BattleActionBar.Create(transform, font, _start.IsParticipant);
            _actionBar.AttackClicked += OnAttack;
            _actionBar.PotionClicked += OnPotion;
            _actionBar.SurrenderClicked += OnSurrenderClicked;
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
                Hud(a.ActorId)?.UpdateStats(a);
                if (a.ActorId == _hudRight.ActorId) _skillRight?.UpdateMpCosts(a.Skills);
            }
        }

        private BattleHudPanel Hud(int actorId) =>
            actorId == _hudLeft.ActorId ? _hudLeft : actorId == _hudRight.ActorId ? _hudRight : null;

        private void RefreshHuds()
        {
            _hudLeft.UpdateVitals(_left.Hp, _left.Mp, _left.MaxHp, _left.MaxMp);
            _hudRight.UpdateVitals(_right.Hp, _right.Mp, _right.MaxHp, _right.MaxMp);
        }

        private void OnAttack() { _handler.SendNormalAttack(); _actionBar.Lock(); }
        private void OnPotion() { _handler.SendUseItem(); _actionBar.Lock(); }

        private void OnSkillUsed(int skillId)
        {
            SoundManager.Instance?.PlayEffect("s_button_ingame");
            _handler.SendSkill(skillId); _cooldowns.MarkUsed(skillId);
            _actionBar.Lock(); _skillLeft?.RefreshState(_left.Mp);
        }

        private void OnSurrenderClicked()
        {
            if (_surrendered) return;
            var d = YesNoDialog.Create(transform, "Bạn chắc chắn muốn xin thua?", "Xin thua", "Huỷ");
            d.Confirmed += () => { _surrendered = true; _handler.SendSurrender(); _actionBar.LockSurrender(); Destroy(d.gameObject); };
            d.Cancelled += () => Destroy(d.gameObject);
        }

        private void OnBackClicked()
        {
            if (_result != null) { Closed?.Invoke(); return; }
            OnSurrenderClicked();
        }

        private BattlePetCard Card(int actorId) =>
            _left.ActorId == actorId ? _left : _right.ActorId == actorId ? _right : null;

        private static void PlayHitSound(BattleEffect effect, Transform target)
        {
            if (effect.SkillId == 1)
            {
                BattleFloatText.CreateMiss(target);
                SoundManager.Instance?.PlayEffect("s_attack_miss");
            }
            else if (effect.SkillId == 2) SoundManager.Instance?.PlayEffect("s_attack_crit");
            else if (effect.HpDelta < 0) SoundManager.Instance?.PlayEffect("s_hit");
        }

        private void Update() => Ticked?.Invoke();
    }
}
