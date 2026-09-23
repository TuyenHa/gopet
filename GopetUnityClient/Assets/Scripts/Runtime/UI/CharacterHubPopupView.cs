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

    }
}
