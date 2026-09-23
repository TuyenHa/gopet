using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Menu popup khi tap 1 slot đầy: Tháo / Gắn ngọc / Cường hoá / Tiến hoá / Huỷ.
    /// Slot rỗng thì hiện menu chọn item từ túi (chưa thêm — cần PET_INVENTORY handler,
    /// noted in phase-07 doc).
    /// </summary>
    public sealed class PetSlotActionsView : MonoBehaviour
    {
        public enum Action { Unequip, MountGem, Enchant, UpTier, Destroy }

        public event System.Action<Action> Chosen;
        public event System.Action CloseRequested;

        public static PetSlotActionsView Create(Transform parent)
        {
            var backdrop = new GameObject("Slot Actions Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

            var view = backdrop.AddComponent<PetSlotActionsView>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(220f, 260f);
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            view.Row(panel.transform, "Tháo",       Action.Unequip,  0, UiBuilder.ButtonFace);
            view.Row(panel.transform, "Gắn ngọc",   Action.MountGem, 1, new Color(0.32f, 0.5f, 0.72f, 1f));
            view.Row(panel.transform, "Cường hoá",  Action.Enchant,  2, new Color(0.7f, 0.55f, 0.15f, 1f));
            view.Row(panel.transform, "Tiến hoá",   Action.UpTier,   3, new Color(0.55f, 0.35f, 0.7f, 1f));
            view.Row(panel.transform, "Huỷ",        Action.Destroy,  4, new Color(0.75f, 0.25f, 0.25f, 1f));
            return view;
        }

        private void Row(Transform panel, string label, Action action, int index, Color color)
        {
            const float h = 44f, pad = 8f;
            var go = new GameObject($"Row:{action}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var top = pad + (h + 4f) * index;
            rect.offsetMin = new Vector2(pad, -(top + h));
            rect.offsetMax = new Vector2(-pad, -top);

            var img = go.GetComponent<Image>();
            img.color = color;
            RoundedUiSprite.Apply(img);

            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.color = Color.white;

            go.GetComponent<Button>().onClick.AddListener(() => Chosen?.Invoke(action));
        }
    }
}
