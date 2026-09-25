using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed class SpectatorBattleView : MonoBehaviour
    {
        private sealed class Vitals
        {
            public RectTransform Root;
            public Image Hp, Mp;
            public Text Label;
        }
        private SpectatorBattles.Entry _state;
        private Vitals _left, _right;
        private RectTransform _effects;

        public static SpectatorBattleView Create(Transform parent, SpectatorBattles.Entry state)
        {
            var go = new GameObject("Spectator " + state.BattleId, typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true; canvas.sortingOrder = 20000;
            ((RectTransform)go.transform).sizeDelta = new Vector2(160, 100);
            var view = go.AddComponent<SpectatorBattleView>(); view._state = state;
            view._effects = new GameObject("Effects", typeof(RectTransform)).GetComponent<RectTransform>();
            view._effects.SetParent(go.transform, false);
            // Overlay effects assume a pixel-sized canvas. Keep their local bounds explicit
            // and bring them back to the scale of the existing world pet sprites.
            view._effects.sizeDelta = new Vector2(320, 240);
            view._effects.localScale = Vector3.one / 3f;
            view._left = view.CreateVitals("Left"); view._right = view.CreateVitals("Right");
            view.Refresh(); return view;
        }

        public void Follow(Transform left, Transform right)
        {
            _left.Root.position = left.position;
            _right.Root.position = right.position;
            _effects.position = (left.position + right.position) * 0.5f;
            Refresh();
        }

        public void Apply(BattleTurn turn)
        {
            var source = Anchor(turn.ActorId);
            foreach (var effect in turn.Effects)
            {
                var anchor = Anchor(effect.ActorId);
                if (anchor == null) continue;
                if (effect.HpDelta != 0) BattleFloatText.Create(anchor, effect.HpDelta, false);
                if (effect.MpDelta != 0) BattleFloatText.Create(anchor, effect.MpDelta, true, 0.5f);
                if (effect.SkillId == 1) BattleFloatText.CreateMiss(anchor);
                else
                    BattleEffectView.Play(_effects, anchor, effect.SkillId,
                        source != null ? (Vector3?)source.position : null);
            }
            if (source != null && turn.MainMpDelta != 0)
                BattleFloatText.Create(source, turn.MainMpDelta, true, 0.5f);
            Refresh();
        }

        private RectTransform Anchor(int id) => id == _state.Left.ActorId ? _left.Root :
            id == _state.Right.ActorId ? _right.Root : null;

        private void Refresh()
        {
            UpdateVitals(_left, _state.Left); UpdateVitals(_right, _state.Right);
        }
        private void UpdateVitals(Vitals view, BattlePet pet)
        {
            view.Hp.rectTransform.anchorMax = new Vector2(pet.MaxHp > 0 ? Mathf.Clamp01((float)pet.Hp / pet.MaxHp) : 0, 1);
            view.Mp.rectTransform.anchorMax = new Vector2(pet.MaxMp > 0 ? Mathf.Clamp01((float)pet.Mp / pet.MaxMp) : 0, 1);
            view.Label.text = _state.Ended ? (pet.ActorId == _state.WinnerId ? "WIN" : "LOSE") :
                pet.Hp + "/" + pet.MaxHp + " HP";
        }
        private Vitals CreateVitals(string name)
        {
            var root = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(transform, false); root.sizeDelta = new Vector2(70, 60);
            var label = new GameObject("HP value", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            label.transform.SetParent(root, false); label.font = UiBuilder.DefaultFont();
            label.fontSize = 11; label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white; label.raycastTarget = false;
            label.rectTransform.sizeDelta = new Vector2(100, 18);
            label.rectTransform.anchoredPosition = new Vector2(0, 75);
            return new Vitals { Root = root, Label = label,
                Hp = Bar(root, "HP", 64, new Color(0.25f, 0.85f, 0.35f)),
                Mp = Bar(root, "MP", 58, new Color(0.25f, 0.65f, 1f)) };
        }
        private static Image Bar(Transform parent, string name, float y, Color color)
        {
            var bg = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            bg.transform.SetParent(parent, false); bg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            bg.raycastTarget = false; bg.rectTransform.sizeDelta = new Vector2(64, 5);
            bg.rectTransform.anchoredPosition = new Vector2(0, y);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            fill.transform.SetParent(bg.transform, false); fill.color = color; fill.raycastTarget = false;
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.sizeDelta = Vector2.zero;
            return fill;
        }
    }
}
