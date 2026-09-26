using Gopet.Net.Guider;
using Gopet.Net.Npc;
using Gopet.Net.Pet;
using Gopet.Net.Social;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private CharacterHubPopupView _characterHub;
        private bool _hubPetEquipRequestPending;

        public bool AutoAttackEnabled => _autoAttack.Enabled;

        private void OpenCharacterHub()
        {
            if (_characterHub != null) return;
            CloseCharacterMenu();
            _characterHub = CharacterHubPopupView.Create(_hudParent, _guider, _assets,
                SoundManager.Instance, _autoAttack.Enabled, _wingHandler);
            _characterHub.ActionRequested += ExecuteCharacterHubAction;
            _characterHub.FriendAddRequested += name => _client.Send(FriendPackets.AddFriendByName(name));
            _characterHub.AutoAttackChanged += SetAutoAttack;
            _characterHub.PetEquipActionChosen += OnPetEquipAction;
            _characterHub.PetHiddenStatsRequested += () =>
                _client.Send(PetEquipPackets.RequestHiddenStats());
            _characterHub.Closed += CloseCharacterHub;
            _characterHub.OpenInitial();
        }

        private void CloseCharacterHub()
        {
            if (_characterHub == null) return;
            Object.Destroy(_characterHub.gameObject);
            _characterHub = null;
            _hubPetEquipRequestPending = false;
        }

        /// <summary>Chỗ nhúng Kho ngọc trong tab Pet của Hành lý — nối vào
        /// <c>UiRoot.GemInventoryHost</c>; null khi Hành lý không mở trang Kho ngọc.</summary>
        public Transform GemInventoryHost() =>
            _characterHub != null ? _characterHub.PetContentHost(CharacterMenuAction.GemInventory) : null;

        public bool TryConsumeCharacterHubMenu(MenuScreen screen) =>
            _characterHub != null && _characterHub.TryConsumeMenu(screen);

        private void ExecuteCharacterHubAction(CharacterMenuAction action)
        {
            if (CharacterMenu.TryBuildServerMessage(action, out var message))
            {
                _client.Send(message);
                return;
            }

            switch (action)
            {
                case CharacterMenuAction.PetEquipment:
                    _hubPetEquipRequestPending = true;
                    _client.Send(PetEquipPackets.RequestEquipInfo(_login.UserId));
                    break;
                case CharacterMenuAction.PetPotential:
                    _client.Send(PetProfilePackets.RequestGym());
                    break;
                case CharacterMenuAction.PetTattoo:
                    _client.Send(TattooPackets.RequestScreen());
                    break;
                case CharacterMenuAction.PetGymReset:
                    _guider.SelectNpcOption(-7, LinhThuCityNpcOptions.BacSiTayGym);
                    break;
                case CharacterMenuAction.ChangePassword:
                    OpenChangePassword();
                    break;
                case CharacterMenuAction.GuildChat:
                    OpenGuildView();
                    break;
                case CharacterMenuAction.Logout:
                    ConfirmLogout();
                    break;
                case CharacterMenuAction.Exit:
                    ConfirmExit();
                    break;
            }
        }
    }
}
