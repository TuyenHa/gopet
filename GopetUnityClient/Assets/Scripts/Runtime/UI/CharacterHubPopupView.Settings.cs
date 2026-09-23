using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private void BuildSettingsTab()
        {
            var left = MakePane("Âm thanh", 0f, 0.5f);
            var right = MakePane("Tài khoản", 0.5f, 1f);
            MakeTitle(left, "Cài đặt game");
            MakeTitle(right, "Tài khoản");

            MakeSettingToggle(left, 44f, "Âm thanh", 0,
                () => _sound == null || _sound.Enabled,
                () => { if (_sound != null) _sound.SetEnabled(!_sound.Enabled); });
            MakeSettingToggle(left, 86f, "Nhạc nền", 1,
                () => _sound == null || _sound.MusicEnabled,
                () => { if (_sound != null) _sound.SetMusicEnabled(!_sound.MusicEnabled); });
            MakeSettingToggle(left, 128f, "Hiệu ứng", 2,
                () => _sound == null || _sound.EffectsEnabled,
                () => { if (_sound != null) _sound.SetEffectsEnabled(!_sound.EffectsEnabled); });
            MakeSettingValue(left, 170f, "Ngôn ngữ", 3,
                () => JarStrings.Current == Language.Vi ? "Tiếng Việt" : "English",
                JarStrings.ToggleLanguage);
            MakeSettingToggle(left, 212f, "Tự đánh quái", 4,
                () => _autoAttack, () =>
            {
                _autoAttack = !_autoAttack;
                AutoAttackChanged?.Invoke(_autoAttack);
            });

            var password = MakeAction(right, "Đổi mật khẩu", 44f, SettingsIcon(5));
            NormalizeActionLayout(password);
            password.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.ChangePassword));
            var logout = MakeAction(right, "Đăng xuất", 86f, SettingsIcon(6));
            NormalizeActionLayout(logout);
            logout.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.Logout));
            var exit = MakeAction(right, "Thoát game", 128f, SettingsIcon(7));
            NormalizeActionLayout(exit);
            exit.onClick.AddListener(() => CloseThenRequest(CharacterMenuAction.Exit));
        }

        private void MakeSettingToggle(Transform parent, float top, string label, int iconIndex,
            System.Func<bool> value, UnityEngine.Events.UnityAction toggle)
        {
            var button = MakeSettingValue(parent, top, label, iconIndex, () => OnOff(value()), toggle);
            var state = button.transform.Find("State")?.GetComponent<Text>();
            button.onClick.AddListener(() =>
            {
                toggle();
                if (state != null) state.text = OnOff(value());
                UpdateSwitch(button.transform, value());
            });
            var switchButton = button.transform.Find("Switch")?.GetComponent<Button>();
            switchButton?.onClick.AddListener(() =>
            {
                if (state != null) state.text = OnOff(value());
                UpdateSwitch(button.transform, value());
            });
        }

        private Button MakeSettingValue(Transform parent, float top, string label, int iconIndex,
            System.Func<string> value, UnityEngine.Events.UnityAction toggle)
        {
            var button = MakeAction(parent, label, top, SettingsIcon(iconIndex));
            NormalizeActionLayout(button);
            var labelText = button.GetComponentInChildren<Text>();
            var labelRect = labelText.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(48f, 0f);
            labelRect.offsetMax = new Vector2(-145f, 0f);
            labelRect.sizeDelta = Vector2.zero;
            labelText.alignment = TextAnchor.MiddleLeft;

            var state = UiBuilder.MakeText(button.transform, UiBuilder.DefaultFont(), "State", 13, false);
            state.text = value();
            UiBuilder.SetFontStyle(state, FontStyle.Bold);
            state.alignment = TextAnchor.MiddleRight;
            state.color = UiBuilder.TextMuted;
            var stateRect = state.rectTransform;
            stateRect.anchorMin = stateRect.anchorMax = new Vector2(1f, 0.5f);
            stateRect.pivot = new Vector2(1f, 0.5f);
            stateRect.sizeDelta = new Vector2(48f, 32f);
            stateRect.anchoredPosition = new Vector2(-78f, 0f);

            if (label != "Ngôn ngữ")
            {
                MakeSwitch(button.transform, value() == "Bật", toggle);
            }
            else
            {
                var arrow = UiBuilder.MakeText(button.transform, UiBuilder.DefaultFont(), "Arrow", 26, true);
                arrow.text = "›";
                arrow.alignment = TextAnchor.MiddleCenter;
                var arrowRect = arrow.rectTransform;
                arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(1f, 0.5f);
                arrowRect.pivot = new Vector2(1f, 0.5f);
                arrowRect.sizeDelta = new Vector2(28f, 34f);
                arrowRect.anchoredPosition = new Vector2(-8f, 0f);
            }
            return button;
        }

    }
}
