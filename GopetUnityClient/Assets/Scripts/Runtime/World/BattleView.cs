using System;
using System.Collections.Generic;
using Gopet.Net.Battle;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Sân đấu phủ lên map giống JAR: map vẫn thấy, điều khiển battle nằm phía dưới.</summary>
    public sealed class BattleView : MonoBehaviour
    {
        private BattleHandler _handler;
        private BattleStart _start;
        private BattlePetCard _left, _right;
        private Text _timer;
        private GameObject _actions, _skillMenu, _result;
        private readonly List<Button> _buttons = new List<Button>();
        private readonly List<Button> _skillButtons = new List<Button>();
        private float _turnEndsAt, _unlockAt;

        public int BattleId => _start.BattleId;
        public bool IsParticipant => _start.IsParticipant;
        public event Action Closed;

        public static BattleView Create(Transform parent, BattleStart start, BattleHandler handler,
            RemoteAssetCache assets)
        {
            var go = new GameObject("Pet Battle", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f); scaler.matchWidthOrHeight = 1f;
            var view = go.AddComponent<BattleView>();
            view._handler = handler; view._start = start;
            view.Build(assets, UiBuilder.BuiltinFont());
            return view;
        }

        public void Apply(BattleTurn turn)
        {
            if (turn.BattleId != BattleId) return;
            SetTimer(turn.RemainingMs);
            Card(turn.ActorId)?.Apply(0, turn.MainMpDelta);
            foreach (var effect in turn.Effects)
            {
                var target = Card(effect.ActorId);
                if (target == null) continue;
                target.Apply(effect.HpDelta, effect.MpDelta);
                BattleEffectView.Play(transform, target.EffectAnchor, effect.SkillId);
                if (effect.SkillId == 1) BattleFloatText.CreateMiss(target.transform);
            }
            UnlockActions();
        }

        public void ShowResult(BattleResult result)
        {
            if (result.BattleId != BattleId || _result != null) return;
            _actions.SetActive(false); _skillMenu.SetActive(false);
            _result = new GameObject("Kết quả", typeof(RectTransform), typeof(Image));
            _result.transform.SetParent(transform, false);
            var rect = (RectTransform)_result.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(460f, 230f);
            _result.GetComponent<Image>().color = new Color(0.02f, 0.14f, 0.23f, 0.97f);
            var title = UiBuilder.MakeText(_result.transform, UiBuilder.BuiltinFont(), "Tiêu đề", 28, false);
            UiBuilder.PlaceRow(title.rectTransform, 15f, 45f, 15f); title.alignment = TextAnchor.MiddleCenter;
            title.text = result.WinnerId == _start.LocalPet.ActorId ? "THẮNG CUỘC" :
                IsParticipant ? "THUA CUỘC" : "KẾT THÚC";
            var body = UiBuilder.MakeText(_result.transform, UiBuilder.BuiltinFont(), "Thưởng", 18, false);
            UiBuilder.PlaceRow(body.rectTransform, 70f, 90f, 20f); body.alignment = TextAnchor.UpperCenter;
            body.text = $"Ngọc: {result.Coin}    EXP: {result.Experience}\n{string.Join("\n", result.Messages)}";
            MakeButton(_result.transform, "Tiếp tục", null, new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(170f, 48f)).onClick.AddListener(() => Closed?.Invoke());
        }

        private void Build(RemoteAssetCache assets, Font font)
        {
            var shade = new GameObject("Nền mờ", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(transform, false); UiBuilder.Stretch((RectTransform)shade.transform);
            shade.GetComponent<Image>().color = new Color(0f, 0.08f, 0.13f, 0.28f);
            shade.GetComponent<Image>().raycastTarget = false;
            _left = BattlePetCard.Create(transform, _start.LocalPet, true, assets, font);
            _right = BattlePetCard.Create(transform, _start.Opponent, false, assets, font);
            _timer = UiBuilder.MakeText(transform, font, "Thời gian lượt", 20, false);
            UiBuilder.PlaceRow(_timer.rectTransform, 8f, 36f, 350f); _timer.alignment = TextAnchor.MiddleCenter;
            SetTimer(_start.RemainingMs);
            BuildActions(font);
            BuildSkills(font);
            if (!_start.IsParticipant) _actions.SetActive(false);
        }

        private void BuildActions(Font font)
        {
            _actions = new GameObject("Hành động", typeof(RectTransform), typeof(Image));
            _actions.transform.SetParent(transform, false);
            var rect = (RectTransform)_actions.transform;
            rect.anchorMin = new Vector2(0.5f, 0f); rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f); rect.anchoredPosition = new Vector2(0f, 12f);
            rect.sizeDelta = new Vector2(510f, 72f);
            _actions.GetComponent<Image>().color = new Color(0.02f, 0.14f, 0.23f, 0.93f);
            AddAction("Đánh", "attack", -170f, () => { _handler.SendNormalAttack(); LockActions(); });
            AddAction("Kỹ năng", "skill", 0f, () => _skillMenu.SetActive(!_skillMenu.activeSelf));
            AddAction("Vật phẩm", "potion", 170f, () => { _handler.SendUseItem(); LockActions(); });
        }

        private void AddAction(string label, string icon, float x, UnityEngine.Events.UnityAction action)
        {
            var button = MakeButton(_actions.transform, label, icon, new Vector2(0.5f, 0.5f),
                new Vector2(x, 0f), new Vector2(150f, 52f));
            button.onClick.AddListener(action); _buttons.Add(button);
        }

        private void BuildSkills(Font font)
        {
            _skillMenu = new GameObject("Danh sách kỹ năng", typeof(RectTransform), typeof(Image));
            _skillMenu.transform.SetParent(transform, false);
            var rect = (RectTransform)_skillMenu.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f); rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 90f);
            rect.sizeDelta = new Vector2(520f, Mathf.Max(58f, _start.LocalPet.Skills.Length * 48f + 12f));
            _skillMenu.GetComponent<Image>().color = new Color(0.03f, 0.12f, 0.2f, 0.97f);
            for (var i = 0; i < _start.LocalPet.Skills.Length; i++) AddSkill(_start.LocalPet.Skills[i], i);
            _skillMenu.SetActive(false);
        }

        private void AddSkill(BattleSkill skill, int index)
        {
            var button = MakeButton(_skillMenu.transform, $"{skill.Name}   MP {skill.MpCost}", null,
                new Vector2(0.5f, 1f), new Vector2(0f, -8f - index * 48f), new Vector2(490f, 42f));
            button.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            button.interactable = skill.MpCost <= _start.LocalPet.Mp;
            _skillButtons.Add(button);
            button.onClick.AddListener(() => { _handler.SendSkill(skill.Id); _skillMenu.SetActive(false); LockActions(); });
        }

        private static Button MakeButton(Transform parent, string label, string icon, Vector2 anchor,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.color = UiBuilder.ButtonFace;
            if (icon != null) image.sprite = JarSkin.Raw($"pet/battle/{icon}");
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Nhãn", 17, true);
            text.text = label; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white; text.fontStyle = FontStyle.Bold;
            return go.GetComponent<Button>();
        }

        private BattlePetCard Card(int actorId) => _left.ActorId == actorId ? _left :
            _right.ActorId == actorId ? _right : null;
        private void SetTimer(int ms) { _turnEndsAt = Time.unscaledTime + Mathf.Max(0, ms) / 1000f; }
        private void LockActions() { foreach (var b in _buttons) b.interactable = false; _unlockAt = Time.unscaledTime + 3.5f; }
        private void UnlockActions()
        {
            foreach (var b in _buttons) b.interactable = true;
            for (var i = 0; i < _skillButtons.Count; i++)
                _skillButtons[i].interactable = _start.LocalPet.Skills[i].MpCost <= _left.Mp;
            _unlockAt = 0f;
        }

        private void Update()
        {
            if (_timer != null) _timer.text = $"Lượt: {Mathf.CeilToInt(Mathf.Max(0f, _turnEndsAt - Time.unscaledTime))}s";
            if (_unlockAt > 0f && Time.unscaledTime >= _unlockAt) UnlockActions();
        }
    }
}
