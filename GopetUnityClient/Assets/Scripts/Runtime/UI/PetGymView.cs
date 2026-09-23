using System;
using Gopet.Net.Pet;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "Điểm tiềm năng" (Cộng tiềm năng của pet). Khung, badge tiêu đề và nút X
    /// lấy từ <see cref="GamePopupFrame"/> — cùng khung với Cửa hàng. Ba thẻ STR/AGI/INT
    /// dựng ở <c>PetGymView.Cards.cs</c>.
    /// </summary>
    public sealed partial class PetGymView : MonoBehaviour
    {
        private const string Title = "Điểm tiềm năng";
        private const float PopupWidth = 430f;
        private const float PopupHeight = 262f;
        private const float NameHeight = 24f;
        private const float PointsHeight = 26f;

        private Text _pointsLabel;
        private int _points;

        public event Action<sbyte> StatSelected;
        public event Action CloseRequested;

        public static PetGymView Create(Transform parent, PetGymState state)
        {
            var root = new GameObject("Pet Gym", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            var view = root.AddComponent<PetGymView>();

            // Lớp tối là ANH EM với khung chứ không phải cha: Unity dò handler click
            // ngược lên cây cha, nên nếu nút đóng nằm ở cha thì bấm vào bất kỳ chỗ nào
            // trong popup cũng đóng popup.
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
            dim.transform.SetParent(root.transform, false);
            UiBuilder.Stretch((RectTransform)dim.transform);
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            dim.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var font = UiBuilder.DefaultFont();
            var frame = GamePopupFrame.Create(root.transform, font, Title, PopupWidth, PopupHeight);
            frame.Closed += () => view.CloseRequested?.Invoke();
            view.Build(frame, font, state);
            return view;
        }

        public void Apply(PetPotentialUpdate value)
        {
            _points = Mathf.Max(0, _points - 1);
            Refresh(value.Str, value.Agi, value.Int);
        }

        private void Build(GamePopupFrame frame, Font font, PetGymState state)
        {
            _points = state.PotentialPoints;
            var content = frame.Content;

            var name = UiBuilder.MakeText(content, font, "Pet name", 15, false);
            // Tên server kèm token sao "(saoden)…" — bỏ đi, chỉ giữ "Tên - Lv.N".
            name.text = $"{JarIconTokens.Strip(state.Name ?? string.Empty)} - Lv.{state.Level}";
            name.alignment = TextAnchor.MiddleCenter;
            name.color = PopupPalette.TextDark;
            UiBuilder.SetFontStyle(name, FontStyle.Bold);
            PlaceTop(name.rectTransform, 4f, NameHeight);

            BuildCards(content, font, frame.ContentWidth, 4f + NameHeight + 6f);
            _pointsLabel = BuildPointsChip(content, font);
            Refresh(state.Str, state.Agi, state.Int);
        }

        /// <summary>Chip "Điểm còn lại" ở chân popup, bề ngang ôm theo chữ.</summary>
        private static Text BuildPointsChip(Transform parent, Font font)
        {
            var go = new GameObject("Points", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, PointsHeight);
            rect.anchoredPosition = new Vector2(0f, 2f);
            RoundedBorder.Apply(go, PointsHeight * 0.5f, PopupPalette.ListBg,
                PopupPalette.Hairline, 1f);
            go.GetComponent<Image>().raycastTarget = false;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            go.AddComponent<ContentSizeFitter>().horizontalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var label = UiBuilder.MakeText(go.transform, font, "Label", 14, false);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = PopupPalette.TextDark;
            label.raycastTarget = false;
            label.supportRichText = true;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            return label;
        }

        private void Refresh(int str, int agi, int intel)
        {
            SetValue(0, str);
            SetValue(1, agi);
            SetValue(2, intel);
            if (_pointsLabel != null)
                _pointsLabel.text = $"Điểm còn lại: <color=#E8900C>{_points}</color>";
            SetButtonsEnabled(_points > 0);
        }

        /// <summary>Neo rect theo mép trên của cha, kéo hết bề ngang.</summary>
        private static void PlaceTop(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, -top - height);
            rect.offsetMax = new Vector2(0f, -top);
        }
    }
}
