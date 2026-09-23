using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup 4 nút hành động pet: Chơi / Hôn / Xoa / Hồi phục.
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

        public static PetActionRadial Create(Transform parent)
        {
            var backdrop = new GameObject("Pet Action Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

            var view = backdrop.AddComponent<PetActionRadial>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200f, 232f);
            var panelImg = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(panelImg);
            panelImg.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            view.BuildRow(panel.transform, "Chơi với pet", Action.Play, 0);
            view.BuildRow(panel.transform, "Hôn pet",      Action.Kiss, 1);
            view.BuildRow(panel.transform, "Xoa đầu pet",  Action.Poke, 2);
            view.BuildRow(panel.transform, "Hồi phục",     Action.Heal, 3);
            return view;
        }

        private void BuildRow(Transform panel, string label, Action action, int index)
        {
            const float rowHeight = 48f;
            const float padding = 8f;
            var go = new GameObject($"Row:{action}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var top = padding + rowHeight * index + index * 4f;
            rect.offsetMin = new Vector2(padding, -(top + rowHeight));
            rect.offsetMax = new Vector2(-padding, -top);

            var img = go.GetComponent<Image>();
            img.color = action == Action.Heal
                ? new Color(0.2f, 0.7f, 0.35f, 1f)
                : UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(img);

            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 16, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.color = Color.white;

            go.GetComponent<Button>().onClick.AddListener(() => ActionSelected?.Invoke(action));
        }
    }
}
