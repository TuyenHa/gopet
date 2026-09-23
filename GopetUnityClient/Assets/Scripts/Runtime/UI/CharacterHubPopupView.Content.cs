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

        private void BuildFriendsTab()
        {
            var friends = MakePane("Bạn bè", 0f, 0.42f);
            var tools = MakePane("Kết nối bạn bè", 0.42f, 1f);
            TightenFriendsTitle(MakeTitle(friends, "Danh sách bạn bè", FriendsIcon(0)));
            _leftListHost = MakeListHost(friends, 38f);
            _friendsEmptyState = BuildFriendsEmptyState(_leftListHost);

            TightenFriendsTitle(MakeTitle(tools, "Tìm kiếm bạn bè", FriendsIcon(2)));
            BuildFriendSearch(tools);

            var invitesTitle = MakeTitle(tools, "Lời mời kết bạn", FriendsIcon(1));
            TightenFriendsTitle(invitesTitle);
            var invitesTitleRect = invitesTitle.rectTransform;
            invitesTitleRect.offsetMin = new Vector2(48f, invitesTitleRect.offsetMin.y);
            invitesTitleRect.anchoredPosition = new Vector2(0f, -104f);
            _rightListHost = MakeListHost(tools, 136f);
            Request(CharacterMenuAction.FriendManage);
        }

        private static Transform BuildFriendsEmptyState(Transform parent)
        {
            var go = new GameObject("Empty friends", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0f, 120f);
            rect.anchoredPosition = new Vector2(0f, 10f);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(64f, 64f);
            iconRect.anchoredPosition = new Vector2(0f, 0f);
            var iconImage = icon.GetComponent<Image>();
            iconImage.sprite = FriendsIcon(1);
            iconImage.preserveAspect = true;

            var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Empty label", 16, true);
            label.text = "Danh sách bạn bè";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.48f, 0.60f, 0.82f, 1f);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(8f, 4f);
            labelRect.offsetMax = new Vector2(-8f, -68f);
            return go.transform;
        }

        private static void TightenFriendsTitle(Text title)
        {
            if (title == null) return;
            var titleRect = title.rectTransform;
            titleRect.offsetMin = new Vector2(36f, titleRect.offsetMin.y);
            var siblingIndex = title.transform.GetSiblingIndex() + 1;
            if (siblingIndex >= title.transform.parent.childCount) return;
            var icon = title.transform.parent.GetChild(siblingIndex) as RectTransform;
            if (icon == null || icon.name != "Title icon") return;
            icon.sizeDelta = new Vector2(24f, 24f);
            icon.anchoredPosition = new Vector2(20f, -20f);
        }

        private void BuildFriendSearch(Transform parent)
        {
            var inputGo = new GameObject("Friend name input", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(parent, false);
            var inputRect = (RectTransform)inputGo.transform;
            inputRect.anchorMin = new Vector2(0f, 1f);
            inputRect.anchorMax = new Vector2(1f, 1f);
            inputRect.pivot = new Vector2(0f, 1f);
            inputRect.offsetMin = new Vector2(8f, -78f);
            inputRect.offsetMax = new Vector2(-122f, -42f);
            var inputImage = inputGo.GetComponent<Image>();
            RoundedUiSprite.Apply(inputImage);
            inputImage.color = Color.white;
            var inputOutline = inputGo.AddComponent<Outline>();
            inputOutline.effectColor = new Color(0.68f, 0.72f, 0.80f, 1f);
            inputOutline.effectDistance = new Vector2(1f, -1f);

            var input = inputGo.GetComponent<InputField>();
            input.contentType = InputField.ContentType.Standard;
            input.lineType = InputField.LineType.SingleLine;
            input.textComponent = UiBuilder.MakeText(inputGo.transform, UiBuilder.DefaultFont(), "Text", 13, false);
            input.textComponent.color = new Color(0.14f, 0.2f, 0.3f, 1f);
            input.textComponent.alignment = TextAnchor.MiddleLeft;
            UiBuilder.Stretch(input.textComponent.rectTransform);
            input.textComponent.rectTransform.offsetMin = new Vector2(12f, 0f);
            input.textComponent.rectTransform.offsetMax = new Vector2(-44f, 0f);
            var placeholder = UiBuilder.MakeText(inputGo.transform, UiBuilder.DefaultFont(), "Placeholder", 13, false);
            placeholder.text = "Nhập tên bạn bè...";
            placeholder.color = UiBuilder.TextMuted;
            placeholder.alignment = TextAnchor.MiddleLeft;
            UiBuilder.Stretch(placeholder.rectTransform);
            placeholder.rectTransform.offsetMin = new Vector2(12f, 0f);
            placeholder.rectTransform.offsetMax = new Vector2(-44f, 0f);
            input.placeholder = placeholder;

            var searchIcon = new GameObject("Search icon", typeof(RectTransform), typeof(Image));
            searchIcon.transform.SetParent(inputGo.transform, false);
            var searchRect = (RectTransform)searchIcon.transform;
            searchRect.anchorMin = searchRect.anchorMax = new Vector2(1f, 0.5f);
            searchRect.pivot = new Vector2(1f, 0.5f);
            searchRect.sizeDelta = new Vector2(28f, 28f);
            searchRect.anchoredPosition = new Vector2(-10f, 0f);
            var searchImage = searchIcon.GetComponent<Image>();
            searchImage.sprite = FriendsIcon(2);
            searchImage.preserveAspect = true;

            var send = MakeAction(parent, "Gửi lời mời", 44f);
            var sendRect = send.GetComponent<RectTransform>();
            sendRect.anchorMin = new Vector2(1f, 1f);
            sendRect.anchorMax = new Vector2(1f, 1f);
            sendRect.pivot = new Vector2(1f, 1f);
            sendRect.sizeDelta = new Vector2(108f, 36f);
            sendRect.anchoredPosition = new Vector2(-8f, -42f);
            var sendText = send.GetComponentInChildren<Text>();
            sendText.alignment = TextAnchor.MiddleCenter;
            send.onClick.AddListener(() =>
            {
                var name = input.text == null ? string.Empty : input.text.Trim();
                if (name.Length == 0) return;
                FriendAddRequested?.Invoke(name);
                input.text = string.Empty;
            });
        }

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

        private static void NormalizeActionLayout(Button button)
        {
            if (button == null) return;
            var icon = button.transform.Find("Action icon") as RectTransform;
            if (icon != null)
            {
                icon.anchorMin = icon.anchorMax = new Vector2(0f, 0.5f);
                icon.pivot = new Vector2(0.5f, 0.5f);
                icon.sizeDelta = new Vector2(30f, 30f);
                icon.anchoredPosition = new Vector2(28f, 0f);
            }

            var text = button.GetComponentInChildren<Text>();
            if (text == null) return;
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
                textRect.offsetMin = new Vector2(54f, 0f);
            textRect.offsetMax = new Vector2(-12f, 0f);
            textRect.sizeDelta = Vector2.zero;
            text.alignment = TextAnchor.MiddleLeft;
        }

        private static void MakeSwitch(Transform parent, bool enabled, UnityEngine.Events.UnityAction toggle)
        {
            var go = new GameObject("Switch", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(38f, 20f);
            rect.anchoredPosition = new Vector2(-8f, 0f);
            var track = go.GetComponent<Image>();
            RoundedUiSprite.Apply(track);
            track.color = enabled
                ? new Color(0.96f, 0.25f, 0.19f, 1f)
                : new Color(0.66f, 0.70f, 0.76f, 1f);
            var trackOutline = go.AddComponent<Outline>();
            trackOutline.effectColor = enabled
                ? new Color(0.72f, 0.18f, 0.13f, 0.55f)
                : new Color(0.43f, 0.47f, 0.53f, 0.55f);
            trackOutline.effectDistance = new Vector2(0.5f, -0.5f);
            var knob = new GameObject("Knob", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            knob.SetParent(go.transform, false);
            knob.anchorMin = knob.anchorMax = new Vector2(enabled ? 1f : 0f, 0.5f);
            knob.pivot = new Vector2(0.5f, 0.5f);
            knob.sizeDelta = new Vector2(16f, 16f);
            knob.anchoredPosition = new Vector2(enabled ? -10f : 10f, 0f);
            var knobImage = knob.GetComponent<Image>();
            knobImage.sprite = SwitchKnobSprite();
            knobImage.type = Image.Type.Simple;
            knobImage.color = new Color(0.97f, 0.98f, 1f, 1f);
            var knobShadow = knob.gameObject.AddComponent<Shadow>();
            knobShadow.effectColor = new Color(0f, 0f, 0f, 0.22f);
            knobShadow.effectDistance = new Vector2(1f, -1f);
            go.GetComponent<Button>().onClick.AddListener(toggle);
        }

        private static void UpdateSwitch(Transform parent, bool enabled)
        {
            var knob = parent.Find("Switch/Knob") as RectTransform;
            if (knob == null) return;
            var track = knob.parent.GetComponent<Image>();
            if (track != null)
            {
                track.color = enabled
                    ? new Color(0.96f, 0.25f, 0.19f, 1f)
                    : new Color(0.66f, 0.70f, 0.76f, 1f);
                var outline = track.GetComponent<Outline>();
                if (outline != null)
                    outline.effectColor = enabled
                        ? new Color(0.72f, 0.18f, 0.13f, 0.55f)
                        : new Color(0.43f, 0.47f, 0.53f, 0.55f);
            }
            knob.anchorMin = knob.anchorMax = new Vector2(enabled ? 1f : 0f, 0.5f);
            knob.anchoredPosition = new Vector2(enabled ? -10f : 10f, 0f);
        }

        private static Sprite _switchKnobSprite;

        private static Sprite SwitchKnobSprite()
        {
            if (_switchKnobSprite != null) return _switchKnobSprite;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Settings switch knob";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                var alpha = Mathf.Clamp01(center + 0.5f - distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            _switchKnobSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            _switchKnobSprite.name = "Settings switch knob";
            _switchKnobSprite.hideFlags = HideFlags.HideAndDontSave;
            return _switchKnobSprite;
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
