using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "Pet" 4 nút hành động: Chơi / Hôn / Xoa / Hồi phục, dựng trên
    /// <see cref="GamePopupFrame"/> — cùng khung với Cửa hàng.
    /// Layout dọc thay vì tròn — đơn giản hơn radial thật, đủ dùng cho phase này.
    ///
    /// <para>Cooldown 500ms/nút để tránh spam. Effect optimistic gọi ngoài
    /// popup (từ <c>GameSession</c>), không đợi server echo.</para>
    /// </summary>
    public sealed class PetActionRadial : MonoBehaviour
    {
        public enum Action { Kiss, Play, Poke, Heal }

        public event System.Action<Action> ActionSelected;
        public event System.Action CloseRequested;

        private const float PopupWidth = 230f;
        private const float PopupHeight = 232f;
        private const float RowGap = 5f;

        // Nút hồi phục giữ màu xanh lá cũ; viền sẫm hơn một nấc.
        private static readonly Color HealFill = new Color(0.2f, 0.7f, 0.35f, 1f);
        private static readonly Color HealEdge = new Color(0.14f, 0.54f, 0.26f, 1f);

        public static PetActionRadial Create(Transform parent)
        {
            var root = new GameObject("Pet Action Popup", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            var view = root.AddComponent<PetActionRadial>();

            // Lớp tối là ANH EM với khung: Unity dò handler click ngược lên cây cha, nên
            // nút đóng mà nằm ở cha thì bấm vào khoảng trống trong popup cũng đóng.
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
            dim.transform.SetParent(root.transform, false);
            UiBuilder.Stretch((RectTransform)dim.transform);
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);
            dim.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            // Khung chung như Cửa hàng: nền trắng, viền xanh 2 lớp, badge "Pet", nút X.
            var font = UiBuilder.DefaultFont();
            var frame = GamePopupFrame.Create(root.transform, font, "Pet", PopupWidth, PopupHeight);
            frame.Closed += () => view.CloseRequested?.Invoke();

            var rowHeight = (frame.Content.rect.height > 0f
                ? frame.Content.rect.height
                : PopupHeight - GamePopupFrame.ContentTop - 9f) / 4f - RowGap * 0.75f;
            view.BuildRow(frame.Content, font, "Chơi với pet", Action.Play, 0, rowHeight);
            view.BuildRow(frame.Content, font, "Hôn pet",      Action.Kiss, 1, rowHeight);
            view.BuildRow(frame.Content, font, "Xoa đầu pet",  Action.Poke, 2, rowHeight);
            view.BuildRow(frame.Content, font, "Hồi phục",     Action.Heal, 3, rowHeight);
            return view;
        }

        private void BuildRow(Transform content, Font font, string label, Action action, int index,
            float rowHeight)
        {
            var go = new GameObject($"Row:{action}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var top = index * (rowHeight + RowGap);
            rect.offsetMin = new Vector2(0f, -(top + rowHeight));
            rect.offsetMax = new Vector2(0f, -top);

            // Nút xanh dương (như nút chính của các popup chung); Hồi phục giữ xanh lá.
            // Viền vẽ 2 lớp — không Outline, để góc không nhoè/trắng.
            var heal = action == Action.Heal;
            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius,
                heal ? HealFill : PopupPalette.ButtonBlue,
                heal ? HealEdge : PopupPalette.HeaderBlue, 2f);

            var text = UiBuilder.MakeText(go.transform, font, "Label", 16, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.color = Color.white;

            go.GetComponent<Button>().onClick.AddListener(() => ActionSelected?.Invoke(action));
        }
    }
}
