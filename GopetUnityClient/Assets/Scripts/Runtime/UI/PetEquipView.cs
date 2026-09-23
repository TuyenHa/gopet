using System;
using System.Collections.Generic;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Panel trang bị pet: 5 slot dọc bên trái, cột phải hiện tên/level/stat.
    /// Tap slot đầy → <see cref="PetSlotActionsView"/>; tap slot rỗng chỉ log (cần
    /// PET_INVENTORY handler ở phase sau).
    /// </summary>
    public sealed partial class PetEquipView : MonoBehaviour
    {
        private const float PanelWidth = 380f;
        private const float PanelHeight = 380f;
        private const float SlotColX = 12f;
        private const float SlotStartY = 46f;
        private const float SlotStride = 68f;

        public event Action CloseRequested;
        public event Action<PetEquipItem, PetSlotActionsView.Action> ActionChosen;
        public event Action HiddenStatsRequested;
        /// <summary>Tap slot RỖNG — GameSession sẽ gửi RequestNormalInventory qua đường này.</summary>
        public event Action<EquipSlot> EmptySlotTapped;

        private RemoteAssetCache _assets;
        private readonly Dictionary<EquipSlot, PetEquipSlot> _slots = new Dictionary<EquipSlot, PetEquipSlot>();
        private Text _headerName;
        private Text _headerStats;
        private PetSlotActionsView _actions;

        public static PetEquipView Create(Transform parent, RemoteAssetCache assets)
        {
            var backdrop = new GameObject("Pet Equip Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var view = backdrop.AddComponent<PetEquipView>();
            view._assets = assets;
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            view.BuildContent(panel.transform);
            return view;
        }

        public void ApplyInfo(PetEquipInfo info)
        {
            if (info == null) return;
            LoadEmbeddedPortrait(info);
            var name = Gopet.UiLogic.JarIconTokens.Strip(info.PetName ?? string.Empty);
            _headerName.text = string.IsNullOrEmpty(name) ? "Pet" : name;
            _headerStats.text = $"Lv {info.Level}   STR {info.Str}  AGI {info.Agi}  INT {info.Int}";

            foreach (var slot in _slots.Values) slot.SetItem(null);
            foreach (var item in info.Items)
            {
                if (item.PetEquipId < 0) continue;   // đang ở túi, không mặc
                var slot = item.Slot;
                if (slot.HasValue && _slots.TryGetValue(slot.Value, out var view))
                    view.SetItem(item);
            }
        }

        private void BuildContent(Transform panel)
        {
            var font = UiBuilder.DefaultFont();

            _headerName = UiBuilder.MakeText(panel, font, "Header", 16, false);
            UiBuilder.SetFontStyle(_headerName, FontStyle.Bold);
            _headerName.alignment = TextAnchor.MiddleLeft;
            _headerName.color = UiBuilder.TextMain;
            HeaderRect(_headerName.rectTransform, 8f, 22f);

            _headerStats = UiBuilder.MakeText(panel, font, "HeaderStats", 12, false);
            _headerStats.alignment = TextAnchor.MiddleLeft;
            _headerStats.color = UiBuilder.TextMuted;
            HeaderRect(_headerStats.rectTransform, 26f, 16f);

            var hiddenStats = new GameObject("Kích ẩn", typeof(RectTransform), typeof(Image), typeof(Button));
            hiddenStats.transform.SetParent(panel, false);
            var hiddenRect = (RectTransform)hiddenStats.transform;
            hiddenRect.anchorMin = hiddenRect.anchorMax = new Vector2(1f, 1f);
            hiddenRect.pivot = new Vector2(1f, 1f);
            hiddenRect.anchoredPosition = new Vector2(-12f, -10f);
            hiddenRect.sizeDelta = new Vector2(92f, 26f);
            hiddenStats.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var hiddenLabel = UiBuilder.MakeText(hiddenStats.transform, font, "Nhãn", 11, false);
            UiBuilder.Stretch(hiddenLabel.rectTransform);
            hiddenLabel.alignment = TextAnchor.MiddleCenter;
            hiddenLabel.text = "Kích ẩn";
            hiddenStats.GetComponent<Button>().onClick.AddListener(() => HiddenStatsRequested?.Invoke());

            AddSlot(panel, EquipSlot.Hat,    "Nón",    0);
            AddSlot(panel, EquipSlot.Weapon, "Vũ khí", 1);
            AddSlot(panel, EquipSlot.Armor,  "Giáp",   2);
            AddSlot(panel, EquipSlot.Boot,   "Giày",   3);
            AddSlot(panel, EquipSlot.Glove,  "Bao tay",4);
        }

        private void AddSlot(Transform panel, EquipSlot slot, string label, int row)
        {
            var y = SlotStartY + SlotStride * row;
            var s = PetEquipSlot.Create(panel, slot, label, new Vector2(SlotColX, -y), _assets);
            s.Clicked += () => OnSlotTap(s);
            _slots[slot] = s;
        }

        private void OnSlotTap(PetEquipSlot slot)
        {
            if (slot.Item == null)
            {
                EmptySlotTapped?.Invoke(slot.Slot);
                return;
            }
            if (_actions != null) Destroy(_actions.gameObject);
            _actions = PetSlotActionsView.Create(transform);
            _actions.CloseRequested += CloseActions;
            _actions.Chosen += action =>
            {
                ActionChosen?.Invoke(slot.Item, action);
                CloseActions();
            };
        }

        private void CloseActions()
        {
            if (_actions == null) return;
            Destroy(_actions.gameObject);
            _actions = null;
        }

        private static void HeaderRect(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(12f, -(top + height));
            rect.offsetMax = new Vector2(-12f, -top);
        }
    }
}
