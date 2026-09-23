using System;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class TattooView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action GenerateRequested;
        public event Action<int> RemoveRequested;
        public event Action<int> EnchantRequested;

        public static TattooView Create(Transform parent, TattooScreen screen)
        {
            var root = LegacyOverlayUi.Overlay(parent, "Tattoo");
            var view = root.AddComponent<TattooView>();
            var panel = LegacyOverlayUi.Panel(root.transform, new Vector2(440f, 380f));
            var title = LegacyOverlayUi.Text(panel, "Title", "Hình xăm pet", 18, 10f, 34f);
            title.alignment = TextAnchor.MiddleCenter;
            for (var i = 0; i < screen.Slots.Length && i < 5; i++)
                view.Row(panel, screen.Slots[i], 52f + i * 48f);
            LegacyOverlayUi.Button(panel, "Tạo hình xăm", 18f, () => view.GenerateRequested?.Invoke());
            LegacyOverlayUi.Button(panel, "Đóng", 298f, () => view.CloseRequested?.Invoke());
            return view;
        }

        private void Row(Transform parent, TattooSlot slot, float top)
        {
            var go = new GameObject($"Tattoo:{slot.Position}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, top, 40f, 18f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 14, true);
            text.text = $"Ô {slot.Position}: {slot.Name}";
            text.alignment = TextAnchor.MiddleLeft;
            text.rectTransform.offsetMin = new Vector2(10f, 0f);
            text.rectTransform.offsetMax = new Vector2(-150f, 0f);
            if (slot.TattooId != 0)
            {
                RowButton(go.transform, "Nâng", -132f, () => EnchantRequested?.Invoke(slot.TattooId));
                RowButton(go.transform, "Xoá", -66f, () => RemoveRequested?.Invoke(slot.TattooId));
            }
        }

        private static void RowButton(Transform parent, string label, float right, Action action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, .5f);
            rect.anchoredPosition = new Vector2(right, 0f);
            rect.sizeDelta = new Vector2(60f, 30f);
            go.GetComponent<Image>().color = new Color(.25f, .45f, .65f, 1f);
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 12, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<Button>().onClick.AddListener(() => action());
        }
    }
}
