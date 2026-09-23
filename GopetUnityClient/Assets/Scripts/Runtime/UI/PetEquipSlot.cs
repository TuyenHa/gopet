using System;
using Gopet.Net.Images;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một ô trang bị 64×64 (icon + label + level badge). Rỗng thì hiện outline mờ với
    /// tên slot; đầy thì hiện icon từ <see cref="RemoteAssetCache"/> + level ở góc.
    /// </summary>
    public sealed class PetEquipSlot : MonoBehaviour
    {
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

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
            RoundedUiSprite.Apply(bg);

            var comp = go.AddComponent<PetEquipSlot>();
            comp.Slot = slot;
            comp._assets = assets;
            comp.BuildInner(label);
            go.GetComponent<Button>().onClick.AddListener(() => comp.Clicked?.Invoke());
            comp.SetItem(null);
            return comp;
        }

        private RemoteAssetCache _assets;

        private void BuildInner(string label)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(transform, false);
            _icon = iconGo.GetComponent<Image>();
            _icon.raycastTarget = false;
            UiBuilder.Stretch(_icon.rectTransform);
            _icon.rectTransform.offsetMin = new Vector2(4f, 4f);
            _icon.rectTransform.offsetMax = new Vector2(-4f, -4f);

            var font = UiBuilder.DefaultFont();
            _slotLabel = UiBuilder.MakeText(transform, font, "SlotLabel", 10, true);
            _slotLabel.text = label;
            _slotLabel.alignment = TextAnchor.MiddleCenter;
            _slotLabel.color = UiBuilder.TextMuted;

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
    }
}
