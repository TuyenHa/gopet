using System;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
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
        private const float PopupWidth = 700f;
        private const float PopupHeight = 390f;
        private const float TabHeight = 38f;
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.35f, 0.65f, 1f, 1f);
        private static readonly Color TabText = new Color(0.14f, 0.24f, 0.44f, 1f);

        private readonly string[] _tabNames = { "Nhân vật", "Pet", "Cài đặt", "Bạn bè" };
        private Image[] _tabBackgrounds;
        private Transform _body;
        private Transform _leftListHost;
        private Transform _rightListHost;
        private GenericMenuView _leftMenu;
        private GenericMenuView _rightMenu;
        private PetEquipView _embeddedEquip;
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private SoundManager _sound;
        private bool _autoAttack;
        private CharacterHubTab _activeTab;
        private bool _hasActiveTab;

        public event Action Closed;
        public event Action<CharacterMenuAction> ActionRequested;
        public event Action<bool> AutoAttackChanged;
        public event Action<PetEquipItem, PetSlotActionsView.Action> PetEquipActionChosen;
        public event Action PetHiddenStatsRequested;
        public event Action<EquipSlot> EmptyPetSlotTapped;

        public CharacterHubTab ActiveTab => _activeTab;

        public static CharacterHubPopupView Create(Transform parent, GuiderHandler guider,
            RemoteAssetCache assets, SoundManager sound, bool autoAttack)
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
                _tabBackgrounds[i].color = i == (int)tab ? TabActive : TabInactive;
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
                if (screen.ListId == 81004) host = _rightListHost;
                else if (screen.ListId == 803 || screen.ListId == 81040) host = _leftListHost;
            }
            else if (_activeTab == CharacterHubTab.Pet && screen.ListId == 5)
                host = _leftListHost;
            else if (_activeTab == CharacterHubTab.Friends && screen.ListId >= 1060 && screen.ListId <= 1065)
                host = _rightListHost;

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
                current.EnableEmbeddedScroll(260f);
                if (left) _leftMenu = current; else _rightMenu = current;
            }
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
            for (var i = _body.childCount - 1; i >= 0; i--) Destroy(_body.GetChild(i).gameObject);
            _leftListHost = _rightListHost = null;
            _leftMenu = _rightMenu = null;
            _embeddedEquip = null;
        }
    }
}
