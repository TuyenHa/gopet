using System;
using Gopet.Runtime.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Hai preset avatar world-space với lớp chọn uGUI để dùng được trên touch.</summary>
    public sealed class CharacterPreviewPanel : MonoBehaviour
    {
        private Image _highlight;
        private RectTransform _highlightRect;
        private CharacterPreviewSlot[] _slots;

        public sbyte SelectedGender { get; private set; }
        public CharacterPreviewSlot MaleSlot => _slots == null ? null : _slots[0];
        public CharacterPreviewSlot FemaleSlot => _slots == null ? null : _slots[1];
        public event Action<sbyte> GenderChanged;

        public static CharacterPreviewPanel Create(Transform parent, Vector2 size)
        {
            var go = new GameObject("CharacterPreviewPanel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var root = (RectTransform)go.transform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = size;

            var panel = go.AddComponent<CharacterPreviewPanel>();
            panel.Build(size);
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

        private void Build(Vector2 size)
        {
            _slots = new CharacterPreviewSlot[2];
            for (sbyte gender = 0; gender < 2; gender++)
            {
                var go = new GameObject(gender == 0 ? "Male" : "Female",
                    typeof(RectTransform), typeof(Image), typeof(BoxCollider2D), typeof(CharacterPreviewSlot));
                go.transform.SetParent(transform, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(gender == 0 ? 0f : 0.5f, 0f);
                rect.anchorMax = new Vector2(gender == 0 ? 0.5f : 1f, 1f);
                rect.offsetMin = new Vector2(8f, 8f);
                rect.offsetMax = new Vector2(-8f, -8f);

                var clickImage = go.GetComponent<Image>();
                clickImage.color = new Color(1f, 1f, 1f, 0.015f);
                clickImage.raycastTarget = true;
                var collider = go.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(48f, 64f);
                collider.offset = Vector2.zero;

                var slot = go.GetComponent<CharacterPreviewSlot>();
                slot.Initialize(this, gender);
                _slots[gender] = slot;

                try
                {
                    var avatar = AvatarAppearance.Create(go.transform, gender);
                    avatar.transform.localPosition = new Vector3(0f, -8f, 0f);
                    avatar.SetSortingOrder(20);
                }
                catch (Exception ex)
                {
                    // Keep the selectable slot usable in an editor/test scene without unpacked jar art.
                    Debug.LogWarning($"[Gopet] Không dựng được avatar giới tính {gender}: {ex.Message}");
                }

                try
                {
                    var label = Gopet.Runtime.World.JarNameLabel.Create(go.transform,
                        new Vector3(0f, -42f, 0f), 1f, gender == 0 ? "Nam" : "Nữ");
                    label.SetSortingOrder(40);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Gopet] Không dựng được nhãn giới tính {gender}: {ex.Message}");
                }
            }

            var frame = new GameObject("Highlight", typeof(RectTransform), typeof(Image), typeof(Outline));
            frame.transform.SetParent(transform, false);
            _highlightRect = (RectTransform)frame.transform;
            _highlightRect.anchorMin = new Vector2(0f, 0f);
            _highlightRect.anchorMax = new Vector2(0.5f, 1f);
            _highlightRect.offsetMin = new Vector2(4f, 4f);
            _highlightRect.offsetMax = new Vector2(-4f, -4f);
            _highlight = frame.GetComponent<Image>();
            RoundedUiSprite.Apply(_highlight);
            _highlight.color = new Color(1f, 0.8f, 0.2f, 0.06f);
            _highlight.raycastTarget = false;
            var outline = frame.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 0.8f, 0.2f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);
            frame.transform.SetAsLastSibling();
            SelectedGender = 0;
        }

        private void MoveHighlight(sbyte gender)
        {
            if (_highlightRect == null) return;
            _highlightRect.anchorMin = new Vector2(gender == 0 ? 0f : 0.5f, 0f);
            _highlightRect.anchorMax = new Vector2(gender == 0 ? 0.5f : 1f, 1f);
            _highlightRect.offsetMin = new Vector2(4f, 4f);
            _highlightRect.offsetMax = new Vector2(-4f, -4f);
        }
    }

    /// <summary>Lớp click trên một ô avatar; giữ cả IPointerClickHandler và collider 2D cho các scene khác nhau.</summary>
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
