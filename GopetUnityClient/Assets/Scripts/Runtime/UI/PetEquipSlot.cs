using System;
using Gopet.Net.Images;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một ô trang bị 64×64 (icon + label + level badge) trong khung viền vàng có sao nhỏ ở
    /// góc. Đầy: nền xanh + icon từ <see cref="RemoteAssetCache"/> + level ở góc. Rỗng: cùng
    /// khung nhưng nền xám, hiện tên slot.
    /// </summary>
    public sealed class PetEquipSlot : MonoBehaviour
    {
        /// <summary>Khung sinh bằng tools/image-gen; bản rỗng là cùng khung, nền đổi xám.</summary>
        private const string FilledFramePath = "Ui/Generated/equip_slot_filled";
        private const string EmptyFramePath = "Ui/Generated/equip_slot_empty";
        /// <summary>Lề trong để icon nằm gọn trong viền vàng, không đè lên góc trang trí.</summary>
        private const float IconInset = 9f;

        private static Sprite _filledFrame, _emptyFrame;

        public event Action Clicked;

        public EquipSlot Slot { get; private set; }
        public PetEquipItem Item { get; private set; }

        private Image _icon;
        private Text _slotLabel;
        private Text _levelBadge;
        private Text _gemDot;

        public static PetEquipSlot Create(Transform parent, EquipSlot slot, string label,
            Vector2 topLeft, RemoteAssetCache assets)
        {
            var go = new GameObject($"Slot:{slot}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = topLeft;
            rect.sizeDelta = new Vector2(64f, 64f);

            var comp = go.AddComponent<PetEquipSlot>();
            comp._frame = go.GetComponent<Image>();
            comp.Slot = slot;
            comp._assets = assets;
            comp.BuildInner(label);
            go.GetComponent<Button>().onClick.AddListener(() => comp.Clicked?.Invoke());
            comp.SetItem(null);
            return comp;
        }

        private RemoteAssetCache _assets;
        private Image _frame;

        private void BuildInner(string label)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(transform, false);
            _icon = iconGo.GetComponent<Image>();
            _icon.raycastTarget = false;
            UiBuilder.Stretch(_icon.rectTransform);
            _icon.rectTransform.offsetMin = new Vector2(IconInset, IconInset);
            _icon.rectTransform.offsetMax = new Vector2(-IconInset, -IconInset);

            var font = UiBuilder.DefaultFont();
            _slotLabel = UiBuilder.MakeText(transform, font, "SlotLabel", 10, true);
            _slotLabel.text = label;
            _slotLabel.alignment = TextAnchor.MiddleCenter;
            _slotLabel.color = new Color(1f, 1f, 1f, 0.85f); // đọc được trên nền xám

            _levelBadge = UiBuilder.MakeText(transform, font, "Lvl", 10, false);
            UiBuilder.SetFontStyle(_levelBadge, FontStyle.Bold);
            _levelBadge.alignment = TextAnchor.LowerRight;
            _levelBadge.color = new Color(1f, 0.85f, 0.2f, 1f);
            var lRect = _levelBadge.rectTransform;
            lRect.anchorMin = new Vector2(0f, 0f);
            lRect.anchorMax = new Vector2(1f, 0f);
            lRect.pivot = new Vector2(1f, 0f);
            lRect.offsetMin = new Vector2(0f, 2f);
            lRect.offsetMax = new Vector2(-3f, 16f);

            _gemDot = UiBuilder.MakeText(transform, font, "Gem", 14, false);
            _gemDot.text = "◆";
            _gemDot.color = new Color(0.5f, 0.85f, 1f, 1f);
            _gemDot.alignment = TextAnchor.UpperRight;
            var gRect = _gemDot.rectTransform;
            gRect.anchorMin = new Vector2(1f, 1f);
            gRect.anchorMax = new Vector2(1f, 1f);
            gRect.pivot = new Vector2(1f, 1f);
            gRect.anchoredPosition = new Vector2(-3f, -3f);
            gRect.sizeDelta = new Vector2(14f, 14f);
        }

        public void SetItem(PetEquipItem item)
        {
            Item = item;
            ApplyFrame(item != null);
            if (item == null)
            {
                _icon.sprite = null;
                _icon.color = new Color(1f, 1f, 1f, 0.05f);
                _slotLabel.gameObject.SetActive(true);
                _levelBadge.text = string.Empty;
                _gemDot.gameObject.SetActive(false);
                return;
            }
            _slotLabel.gameObject.SetActive(false);
            _levelBadge.text = item.Level > 0 ? "+" + item.Level : string.Empty;
            _gemDot.gameObject.SetActive(item.HasGem);
            _icon.color = new Color(1f, 1f, 1f, 0.4f);
            _assets?.Get(item.FrameImagePath, ImagePackets.TypeIcon, _icon, tex =>
            {
                if (tex == null || _icon == null) return;
                _icon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                _icon.color = Color.white;
            });
        }

        private void ApplyFrame(bool filled)
        {
            var sprite = filled ? LoadFrame(ref _filledFrame, FilledFramePath)
                                : LoadFrame(ref _emptyFrame, EmptyFramePath);
            _frame.sprite = sprite;
            _frame.type = Image.Type.Simple;
            // Thiếu ảnh (build lỗi) thì rơi về ô bo góc màu cũ, không để trắng trơn.
            if (sprite == null) RoundedUiSprite.Apply(_frame);
            _frame.color = sprite != null ? Color.white
                : filled ? new Color(0.13f, 0.35f, 0.75f, 0.95f) : new Color(0.3f, 0.32f, 0.36f, 0.95f);
        }

        private static Sprite LoadFrame(ref Sprite cache, string path)
        {
            if (cache != null) return cache;
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null) return null;
            return cache = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
