using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private void BuildCharacterTab()
        {
            var left = MakePane("Ngoại hình", 0f, 0.36f);
            var right = MakePane("Rương đồ", 0.36f, 1f);
            MakeTitle(left, "Thông tin · Ngoại hình");
            MakeTitle(right, "Rương đồ", HubActionIcon(2));

            var wardrobe = MakeAction(left, "Tủ quần áo", 42f, HubActionIcon(0));
            wardrobe.onClick.AddListener(() => Request(CharacterMenuAction.Wardrobe));
            var wings = MakeAction(left, "Cánh", 82f, HubActionIcon(1));
            wings.onClick.AddListener(() => Request(CharacterMenuAction.WingInventory));

            _leftListHost = MakeListHost(left, 124f);
            _rightListHost = MakeListHost(right, 38f);
            Request(CharacterMenuAction.Inventory);
        }

        private void BuildPetTab()
        {
            var left = MakePane("Chọn pet", 0f, 0.40f);
            var right = MakePane("Trang bị pet", 0.40f, 1f);
            MakeTitle(left, "Pet của bạn");
            MakeTitle(right, "Trang bị pet");

            // Sáu thao tác Pet đặt gọn theo 2 cột để vẫn còn chỗ cho danh sách pet.
            var select = MakePetAction(left, "Chọn pet", 0, 42f);
            select.onClick.AddListener(() => Request(CharacterMenuAction.SelectPet));
            var equipment = MakePetAction(left, "Trang bị pet", 1, 42f);
            equipment.onClick.AddListener(() => Request(CharacterMenuAction.PetEquipment));
            var gems = MakePetAction(left, "Kho ngọc", 0, 82f);
            gems.onClick.AddListener(() => Request(CharacterMenuAction.GemInventory));
            var potential = MakePetAction(left, "Cộng tiềm năng", 1, 82f);
            potential.onClick.AddListener(() => Request(CharacterMenuAction.PetPotential));
            var tattoo = MakePetAction(left, "Hình xăm / Tẩy xăm", 0, 122f);
            tattoo.onClick.AddListener(() => Request(CharacterMenuAction.PetTattoo));
            var resetGym = MakePetAction(left, "Tẩy gym", 1, 122f);
            resetGym.onClick.AddListener(() => Request(CharacterMenuAction.PetGymReset));
            _leftListHost = MakeListHost(left, 164f);

            var equipHost = new GameObject("Pet equipment", typeof(RectTransform)).transform;
            equipHost.SetParent(right, false);
            var equipRect = (RectTransform)equipHost;
            equipRect.anchorMin = Vector2.zero;
            equipRect.anchorMax = Vector2.one;
            equipRect.offsetMin = new Vector2(4f, 4f);
            equipRect.offsetMax = new Vector2(-4f, -38f);
            _embeddedEquip = PetEquipView.CreateEmbedded(equipHost, _assets);
            _embeddedEquip.ActionChosen += (item, action) => PetEquipActionChosen?.Invoke(item, action);
            _embeddedEquip.HiddenStatsRequested += () => PetHiddenStatsRequested?.Invoke();
            _embeddedEquip.EmptySlotTapped += slot => EmptyPetSlotTapped?.Invoke(slot);
        }

        private static Button MakePetAction(Transform parent, string label, int column, float top)
        {
            var go = new GameObject("PetAction_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(column == 0 ? 0f : 0.5f, 1f);
            rect.anchorMax = new Vector2(column == 0 ? 0.5f : 1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(column == 0 ? 8f : 2f, -(top + 32f));
            rect.offsetMax = new Vector2(column == 0 ? -2f : -8f, -top);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 11, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UiBuilder.TextMain;
            return go.GetComponent<Button>();
        }

        private void CloseThenRequest(CharacterMenuAction action)
        {
            Closed?.Invoke();
            ActionRequested?.Invoke(action);
        }

        private static string OnOff(bool value) => value ? "Bật" : "Tắt";
    }
}
