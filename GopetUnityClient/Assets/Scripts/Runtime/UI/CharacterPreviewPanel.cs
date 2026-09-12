using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class CharacterPreviewPanel : MonoBehaviour
    {
        private static readonly Color Blue = new Color(0.16f, 0.53f, 0.91f, 1f);
        private RectTransform _highlightRect;
        private CharacterPreviewSlot[] _slots;

        public sbyte SelectedGender { get; private set; }
        public CharacterPreviewSlot MaleSlot => _slots == null ? null : _slots[0];
        public CharacterPreviewSlot FemaleSlot => _slots == null ? null : _slots[1];
        public event Action<sbyte> GenderChanged;

        public static CharacterPreviewPanel Create(Transform parent, Vector2 size, Font font = null)
        {
            var go = new GameObject("CharacterPreviewPanel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var root = (RectTransform)go.transform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = size;
            var panel = go.AddComponent<CharacterPreviewPanel>();
            panel.Build(font);
            return panel;
        }

        public void SelectGender(sbyte gender)
        {
            if (gender < 0 || gender > 1 || gender == SelectedGender) return;
            SelectedGender = gender;
            MoveHighlight(gender);
            GenderChanged?.Invoke(gender);
        }

        public CharacterPreviewSlot GetSlot(sbyte gender)
        {
            return gender >= 0 && gender < 2 && _slots != null ? _slots[gender] : null;
        }

        private void Build(Font font)
        {
            _slots = new CharacterPreviewSlot[2];
            for (sbyte gender = 0; gender < 2; gender++)
            {
                var slot = MakeSlot(gender);
                _slots[gender] = slot;
                CharacterPreviewAvatar.Create(slot.transform, gender);
                MakeLabel(slot.transform, font, gender);
            }
            MakeHighlight(font);
            SelectedGender = 0;
        }

        private CharacterPreviewSlot MakeSlot(sbyte gender)
        {
            var go = new GameObject(gender == 0 ? "Male" : "Female",
                typeof(RectTransform), typeof(Image), typeof(Shadow), typeof(CharacterPreviewSlot));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(gender == 0 ? 0f : 0.51f, 0f);
            rect.anchorMax = new Vector2(gender == 0 ? 0.49f : 1f, 1f);
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(-5f, -5f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(1f, 1f, 1f, 0.90f);
            image.raycastTarget = true;
            var shadow = go.GetComponent<Shadow>();
            shadow.effectColor = new Color(0.10f, 0.20f, 0.32f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -4f);

            var slot = go.GetComponent<CharacterPreviewSlot>();
            slot.Initialize(this, gender);
            return slot;
        }

        private static void MakeLabel(Transform parent, Font font, sbyte gender)
        {
            var label = UiBuilder.MakeText(parent, font, "GenderLabel", 18, false);
            label.text = gender == 0 ? "♂   Nam" : "♀   Nữ";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.color = gender == 0 ? Blue : new Color(0.93f, 0.26f, 0.52f, 1f);
            var rect = (RectTransform)label.transform;
            rect.anchorMin = new Vector2(0f, 0.03f);
            rect.anchorMax = new Vector2(1f, 0.25f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void MakeHighlight(Font font)
        {
            var frame = new GameObject("SelectedFrame", typeof(RectTransform), typeof(Image), typeof(Outline));
            frame.transform.SetParent(transform, false);
            _highlightRect = (RectTransform)frame.transform;
            var image = frame.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.16f, 0.53f, 0.91f, 0.02f);
            image.raycastTarget = false;
            var outline = frame.GetComponent<Outline>();
            outline.effectColor = Blue;
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            var badge = new GameObject("CheckBadge", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(frame.transform, false);
            var badgeRect = (RectTransform)badge.transform;
            badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(0.90f, 0.87f);
            badgeRect.sizeDelta = new Vector2(34f, 34f);
            RoundedUiSprite.Apply(badge.GetComponent<Image>());
            badge.GetComponent<Image>().color = Blue;

            var check = UiBuilder.MakeText(badge.transform, font, "Check", 24, true);
            check.text = "✓";
            check.color = Color.white;
            check.fontStyle = FontStyle.Bold;
            check.alignment = TextAnchor.MiddleCenter;
            check.raycastTarget = false;
            MoveHighlight(0);
            frame.transform.SetAsLastSibling();
        }

        private void MoveHighlight(sbyte gender)
        {
            if (_highlightRect == null) return;
            _highlightRect.anchorMin = new Vector2(gender == 0 ? 0f : 0.51f, 0f);
            _highlightRect.anchorMax = new Vector2(gender == 0 ? 0.49f : 1f, 1f);
            _highlightRect.offsetMin = new Vector2(1f, 1f);
            _highlightRect.offsetMax = new Vector2(-1f, -1f);
        }
    }

    public sealed class CharacterPreviewSlot : MonoBehaviour, IPointerClickHandler
    {
        private CharacterPreviewPanel _owner;
        public sbyte Gender { get; private set; }

        internal void Initialize(CharacterPreviewPanel owner, sbyte gender)
        {
            _owner = owner;
            Gender = gender;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _owner?.SelectGender(Gender);
        }
    }
}
