using System;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public enum CharacterHubTab { Character, Pet, Settings, Friends }

    /// <summary>Popup hồ sơ mở từ avatar/HP/MP, dùng chung phong cách tab với Cửa hàng.</summary>
    public sealed partial class CharacterHubPopupView : MonoBehaviour
    {
        private const float PopupWidth = 780f;
        private const float PopupHeight = 440f;
        private const float HeaderHeight = 58f;
        private const float TabHeight = 38f;
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.88f, 0.93f, 1f, 1f);
        private static readonly Color TabText = new Color(0.14f, 0.24f, 0.44f, 1f);

        private readonly string[] _tabNames = { "Nhân vật", "Pet", "Bạn bè", "Cài đặt" };
        private readonly CharacterHubTab[] _tabOrder =
        {
            CharacterHubTab.Character,
            CharacterHubTab.Pet,
            CharacterHubTab.Friends,
            CharacterHubTab.Settings
        };
        private Image[] _tabBackgrounds;
        private Transform _body;
        private Transform _leftListHost;
        private Transform _rightListHost;
        private Transform _friendsEmptyState;
        private GenericMenuView _leftMenu;
        private GenericMenuView _rightMenu;
        private InventoryGridView _inventoryGrid;
        private PetEquipView _embeddedEquip;
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private SoundManager _sound;
        private WingHandler _wings;
        private YesNoDialog _confirmation;
        private bool _equipRequestPending;
        private bool _autoAttack;
        private CharacterHubTab _activeTab;
        private bool _hasActiveTab;

        public event Action Closed;
        public event Action<CharacterMenuAction> ActionRequested;
        public event Action<string> FriendAddRequested;
        public event Action<bool> AutoAttackChanged;
        public event Action<PetEquipItem, PetSlotActionsView.Action> PetEquipActionChosen;
        public event Action PetHiddenStatsRequested;
        public event Action<EquipSlot> EmptyPetSlotTapped;

        public CharacterHubTab ActiveTab => _activeTab;

        public static CharacterHubPopupView Create(Transform parent, GuiderHandler guider,
            RemoteAssetCache assets, SoundManager sound, bool autoAttack, WingHandler wings = null)
        {
            var frame = new GameObject("CharacterHubPopup", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(parent, false);
            var rect = (RectTransform)frame.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(PopupWidth, PopupHeight);

            var bg = frame.GetComponent<Image>();
            RoundedUiSprite.Apply(bg);
            bg.color = new Color(0.96f, 0.98f, 1f, 1f);
            var outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            var view = frame.AddComponent<CharacterHubPopupView>();
            view._guider = guider;
            view._assets = assets;
            view._sound = sound;
            view._autoAttack = autoAttack;
            view._wings = wings;
            view.BuildHeader();
            view.BuildTabs();
            view.BuildBody();
            view.BuildClose();
            return view;
        }

        public void OpenInitial() => SelectTab(CharacterHubTab.Character);

        public void SelectTab(CharacterHubTab tab)
        {
            if (_hasActiveTab && _activeTab == tab) return;
            _hasActiveTab = true;
            _activeTab = tab;
            for (var i = 0; i < _tabBackgrounds.Length; i++)
                _tabBackgrounds[i].color = _tabOrder[i] == tab ? TabActive : TabInactive;
            ClearBody();
            switch (tab)
            {
                case CharacterHubTab.Character: BuildCharacterTab(); break;
                case CharacterHubTab.Pet: BuildPetTab(); break;
                case CharacterHubTab.Settings: BuildSettingsTab(); break;
                case CharacterHubTab.Friends: BuildFriendsTab(); break;
            }
        }

        public bool TryConsumeMenu(MenuScreen screen)
        {
            if (screen == null || !_hasActiveTab) return false;
            Transform host = null;
            if (_activeTab == CharacterHubTab.Character)
            {
                if (screen.ListId == 81004)
                {
                    host = _rightListHost;
                    if (_inventoryGrid == null)
                        _inventoryGrid = InventoryGridView.Create(host, transform, _assets, _guider);
                    _inventoryGrid.Bind(screen);
                    return true;
                }
                else if (screen.ListId == 803 || screen.ListId == 81040) host = _leftListHost;
            }
            else if (_activeTab == CharacterHubTab.Pet && screen.ListId == 5)
                host = _leftListHost;
            else if (_activeTab == CharacterHubTab.Friends && screen.ListId >= 1060 && screen.ListId <= 1065)
            {
                host = screen.ListId == 1060 ? _leftListHost : _rightListHost;
                if (screen.ListId == 1060 && _friendsEmptyState != null)
                {
                    Destroy(_friendsEmptyState.gameObject);
                    _friendsEmptyState = null;
                }
            }

            if (host == null) return false;
            BindMenu(host, screen, host == _leftListHost);
            return true;
        }

        public bool TryApplyPetEquip(PetEquipInfo info)
        {
            if (_activeTab != CharacterHubTab.Pet || _embeddedEquip == null) return false;
            _embeddedEquip.ApplyInfo(info);
            return true;
        }

        private void Request(CharacterMenuAction action) => ActionRequested?.Invoke(action);

        private void BindMenu(Transform host, MenuScreen screen, bool left)
        {
            var current = left ? _leftMenu : _rightMenu;
            if (current == null)
            {
                current = GenericMenuView.Create(host, UiBuilder.BuiltinFont());
                current.CloseRequested += _ => ClearMenu(left);
                current.ConfirmRequested += ShowEmbeddedConfirm;
                current.EnableEmbeddedScroll(Mathf.Max(80f, ((RectTransform)host).rect.height));
                if (left) _leftMenu = current; else _rightMenu = current;
            }
            current.SelectionOverride = _activeTab == CharacterHubTab.Character &&
                screen.ListId == 81040 ? index => TryUseWing(screen, index) : null;
            current.SetCompactCards(_activeTab == CharacterHubTab.Character &&
                (screen.ListId == 803 || screen.ListId == 81040));
            // Pane Tủ quần áo có thể thấp hơn menu khác; đồng bộ viewport thực tế
            // để thẻ ở dưới luôn kéo/cuộn được thay vì bị cắt mất.
            current.SetViewport(Mathf.Max(80f, ((RectTransform)host).rect.height));
            current.Bind(screen, _assets, _guider);
        }

        private void ClearMenu(bool left)
        {
            var menu = left ? _leftMenu : _rightMenu;
            if (menu != null) Destroy(menu.gameObject);
            if (left) _leftMenu = null; else _rightMenu = null;
        }

        private void ClearBody()
        {
            DismissConfirmation();
            for (var i = _body.childCount - 1; i >= 0; i--) Destroy(_body.GetChild(i).gameObject);
            _leftListHost = _rightListHost = null;
            _friendsEmptyState = null;
            _leftMenu = _rightMenu = null;
            _inventoryGrid = null;
            _embeddedEquip = null;
        }

        private void ShowEmbeddedConfirm(MenuSelection.ConfirmPrompt prompt, Action onYes)
        {
            if (_equipRequestPending || prompt == null || onYes == null) return;
            var message = string.IsNullOrWhiteSpace(prompt.Text)
                ? "Bạn có muốn sử dụng vật phẩm này cho nhân vật không?"
                : prompt.Text;
            ShowConfirmation(message, prompt.ConfirmLabel, prompt.CancelLabel, () =>
            {
                _equipRequestPending = true;
                try { onYes(); }
                finally { _equipRequestPending = false; }
            });
        }

        /// <summary>
        /// Kho cánh của JAR là một protocol chuyên biệt: xác nhận xong phải gửi
        /// PET_SERVICE / WING / WING_TYPE_USE / inventoryIndex, không phải guider menu.
        /// </summary>
        private bool TryUseWing(MenuScreen screen, int index)
        {
            if (_equipRequestPending || _wings == null || screen == null ||
                index < 0 || index >= screen.Items.Length) return true;

            var item = screen.Items[index];
            if (!item.CanSelect) return true;

            var equipped = item.ItemId == -1;
            var action = equipped ? (Action)_wings.Unequip : () => _wings.Use(item.ItemId);
            var message = equipped
                ? $"B\u1ea1n c\u00f3 mu\u1ed1n th\u00e1o {item.Title} kh\u00f4ng?"
                : string.IsNullOrWhiteSpace(item.DialogText)
                    ? $"B\u1ea1n c\u00f3 mu\u1ed1n s\u1eed d\u1ee5ng {item.Title} kh\u00f4ng?"
                    : item.DialogText;

            ShowConfirmation(message, item.LeftCommandText, item.RightCommandText, () =>
            {
                _equipRequestPending = true;
                try
                {
                    action();
                    // JAR Ä‘Ã³ng danh sÃ¡ch sau khi gá»­i; server sáº½ tráº£ kho CÃ¡nh má»›i.
                    ClearMenu(true);
                }
                finally { _equipRequestPending = false; }
            });
            return true;
        }

        private void ShowConfirmation(string message, string yesLabel, string noLabel, Action onYes)
        {
            if (_confirmation != null || _equipRequestPending) return;

            _confirmation = YesNoDialog.Create(transform,
                string.IsNullOrWhiteSpace(message) ? "B\u1ea1n c\u00f3 mu\u1ed1n s\u1eed d\u1ee5ng v\u1eadt ph\u1ea9m n\u00e0y kh\u00f4ng?" : message,
                string.IsNullOrWhiteSpace(yesLabel) ? "\u0110\u1ed3ng \u00fd" : yesLabel,
                string.IsNullOrWhiteSpace(noLabel) ? "Kh\u00f4ng" : noLabel);
            _confirmation.Confirmed += () =>
            {
                DismissConfirmation();
                onYes?.Invoke();
            };
            _confirmation.Cancelled += DismissConfirmation;
        }

        private void DismissConfirmation()
        {
            if (_confirmation == null) return;
            var dialog = _confirmation;
            _confirmation = null;
            if (dialog != null) Destroy(dialog.gameObject);
        }
    }
}
