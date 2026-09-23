using System;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class PetGymView : MonoBehaviour
    {
        private Text _stats;
        private int _points;

        public event Action<sbyte> StatSelected;
        public event Action CloseRequested;

        public static PetGymView Create(Transform parent, PetGymState state)
        {
            var root = new GameObject("Pet Gym", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var view = root.AddComponent<PetGymView>();
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view.Build(state);
            return view;
        }

        public void Apply(PetPotentialUpdate value)
        {
            _points = Mathf.Max(0, _points - 1);
            Refresh(value.Str, value.Agi, value.Int);
        }

        private void Build(PetGymState state)
        {
            _points = state.PotentialPoints;
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(430f, 260f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());

            var title = UiBuilder.MakeText(panel.transform, UiBuilder.DefaultFont(), "Title", 19, false);
            UiBuilder.PlaceRow(title.rectTransform, 12f, 34f, 14f);
            title.text = $"Gym — {state.Name}";
            title.alignment = TextAnchor.MiddleCenter;
            _stats = UiBuilder.MakeText(panel.transform, UiBuilder.DefaultFont(), "Stats", 16, false);
            UiBuilder.PlaceRow(_stats.rectTransform, 55f, 56f, 20f);
            _stats.alignment = TextAnchor.MiddleCenter;
            Refresh(state.Str, state.Agi, state.Int);

            MakeStatButton(panel.transform, "STR", 0, -130f);
            MakeStatButton(panel.transform, "AGI", 1, 0f);
            MakeStatButton(panel.transform, "INT", 2, 130f);
            var close = MakeButton(panel.transform, "Đóng", 0f, 18f, 150f);
            close.onClick.AddListener(() => CloseRequested?.Invoke());
        }

        private void MakeStatButton(Transform parent, string label, sbyte index, float x)
        {
            var button = MakeButton(parent, $"+ {label}", x, 86f, 110f);
            button.onClick.AddListener(() =>
            {
                if (_points > 0) StatSelected?.Invoke(index);
            });
        }

        private void Refresh(int str, int agi, int intel)
        {
            if (_stats != null)
                _stats.text = $"STR {str}     AGI {agi}     INT {intel}\nĐiểm còn lại: {_points}";
        }

        private static Button MakeButton(Transform parent, string label, float x, float y, float width)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, 42f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }
    }
}
