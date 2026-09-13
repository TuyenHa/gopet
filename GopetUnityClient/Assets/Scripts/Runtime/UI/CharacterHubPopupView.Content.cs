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
            MakeTitle(right, "Rương đồ");

            var wardrobe = MakeAction(left, "Tủ quần áo", 42f);
            wardrobe.onClick.AddListener(() => Request(CharacterMenuAction.Wardrobe));
            var wings = MakeAction(left, "Cánh", 82f);
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

            var select = MakeAction(left, "Chọn pet", 42f);
            select.onClick.AddListener(() => Request(CharacterMenuAction.SelectPet));
            var equipment = MakeAction(left, "Trang bị pet", 82f);
            equipment.onClick.AddListener(() => Request(CharacterMenuAction.PetEquipment));
            _leftListHost = MakeListHost(left, 124f);

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

        private void BuildFriendsTab()
        {
            var pane = MakePane("Bạn bè", 0f, 1f);
            MakeTitle(pane, "Danh sách bạn bè");
            var guild = MakeAction(pane, "Bang hội", 38f);
            guild.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.GuildChat));
            _rightListHost = MakeListHost(pane, 80f);
            Request(CharacterMenuAction.FriendManage);
        }

        private void BuildSettingsTab()
        {
            var left = MakePane("Âm thanh", 0f, 0.5f);
            var right = MakePane("Tài khoản", 0.5f, 1f);
            MakeTitle(left, "Cài đặt game");
            MakeTitle(right, "Tài khoản");

            MakeSettingToggle(left, 44f, () => $"Âm thanh: {OnOff(_sound == null || _sound.Enabled)}",
                () => { if (_sound != null) _sound.SetEnabled(!_sound.Enabled); });
            MakeSettingToggle(left, 86f, () => $"Nhạc nền: {OnOff(_sound == null || _sound.MusicEnabled)}",
                () => { if (_sound != null) _sound.SetMusicEnabled(!_sound.MusicEnabled); });
            MakeSettingToggle(left, 128f, () => $"Hiệu ứng: {OnOff(_sound == null || _sound.EffectsEnabled)}",
                () => { if (_sound != null) _sound.SetEffectsEnabled(!_sound.EffectsEnabled); });
            MakeSettingToggle(left, 170f,
                () => $"Ngôn ngữ: {(JarStrings.Current == Language.Vi ? "Tiếng Việt" : "English")}",
                JarStrings.ToggleLanguage);
            MakeSettingToggle(left, 212f, () => $"Tự đánh quái: {OnOff(_autoAttack)}", () =>
            {
                _autoAttack = !_autoAttack;
                AutoAttackChanged?.Invoke(_autoAttack);
            });

            var password = MakeAction(right, "Đổi mật khẩu", 44f);
            password.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.ChangePassword));
            var logout = MakeAction(right, "Đăng xuất", 86f);
            logout.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.Logout));
            var exit = MakeAction(right, "Thoát game", 128f);
            exit.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.Exit));
        }

        private void MakeSettingToggle(Transform parent, float top,
            System.Func<string> label, UnityEngine.Events.UnityAction toggle)
        {
            var button = MakeAction(parent, label(), top);
            var text = button.GetComponentInChildren<Text>();
            button.onClick.AddListener(() =>
            {
                toggle();
                text.text = label();
            });
        }

        private void CloseThenRequest(CharacterMenuAction action)
        {
            Closed?.Invoke();
            ActionRequested?.Invoke(action);
        }

        private static string OnOff(bool value) => value ? "Bật" : "Tắt";
    }
}
