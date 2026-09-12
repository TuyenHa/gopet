using Gopet.Net;
using Gopet.Net.Pet;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private PetProfileHandler _petProfileHandler;
        private PetProfileView _petProfileView;
        private PetGymView _petGymView;
        private TattooView _tattooView;
        private int _tattooEnchantId, _tattooMaterial1;
        private sbyte _tattooMaterialSlot;

        private void InitializeRemainingParityHandlers()
        {
            _client.Ticked += TickAutoAttack;
            InitializeChatHistory();
            _chatHandler.GlobalChatReceived += value =>
                _hud?.Ticker?.Show($"[Cộng đồng] {value.Sender}: {value.Text}");

            _petProfileHandler = new PetProfileHandler();
            _petProfileHandler.RegisterOn(_client.Router);
            _petProfileHandler.ProfileReceived += ShowPetProfile;
            _petProfileHandler.GymReceived += ShowPetGym;
            _petProfileHandler.PotentialUpdated += value => _petGymView?.Apply(value);
            _petProfileHandler.TattooMaterialSelected += ApplyTattooMaterial;
            _petProfileHandler.TattooScreenReceived += ShowTattooScreen;
            _petEquipHandler.EquipChanged += _ => RefreshOwnPetEquipment();
            _petEquipHandler.EquipItemRefreshed += _ => RefreshOwnPetEquipment();
        }

        private void ShowPetProfile(PetProfile value)
        {
            ClosePetProfile();
            _petProfileView = PetProfileView.Create(_hudParent, value, _assets,
                value.UserId == _login.UserId);
            _petProfileView.CloseRequested += ClosePetProfile;
            _petProfileView.GymRequested += () => _client.Send(PetProfilePackets.RequestGym());
            _petProfileView.TattooRequested += () => _client.Send(TattooPackets.RequestScreen());
        }

        private void ClosePetProfile()
        {
            if (_petProfileView == null) return;
            Object.Destroy(_petProfileView.gameObject);
            _petProfileView = null;
        }

        private void ShowPetGym(PetGymState value)
        {
            if (_petGymView != null) Object.Destroy(_petGymView.gameObject);
            _petGymView = PetGymView.Create(_hudParent, value);
            _petGymView.CloseRequested += ClosePetGym;
            _petGymView.StatSelected += index => _client.Send(PetProfilePackets.AddPotential(1, index));
        }

        private void ClosePetGym()
        {
            if (_petGymView == null) return;
            Object.Destroy(_petGymView.gameObject);
            _petGymView = null;
        }

        private void RefreshOwnPetEquipment()
        {
            if (_petEquipView != null) _client.Send(PetEquipPackets.RequestEquipInfo(_login.UserId));
        }

        private void ShowTattooScreen(TattooScreen value)
        {
            CloseTattoo();
            _tattooView = TattooView.Create(_hudParent, value);
            _tattooView.CloseRequested += CloseTattoo;
            _tattooView.GenerateRequested += () => _client.Send(TattooPackets.SelectGenerationItem());
            _tattooView.RemoveRequested += id => _client.Send(TattooPackets.SelectRemoveItem(id));
            _tattooView.EnchantRequested += BeginTattooEnchant;
        }

        private void BeginTattooEnchant(int tattooId)
        {
            _tattooEnchantId = tattooId;
            _tattooMaterial1 = 0;
            _tattooMaterialSlot = GopetCmd.TATTOO_ENCHANT_SELECT_MATERIAL1;
            _client.Send(TattooPackets.SelectEnchantMaterial(_tattooMaterialSlot));
        }

        private void ApplyTattooMaterial(TattooMaterialSelection value)
        {
            if (_tattooMaterialSlot == GopetCmd.TATTOO_ENCHANT_SELECT_MATERIAL1)
            {
                _tattooMaterial1 = value.ItemId;
                _tattooMaterialSlot = GopetCmd.TATTOO_ENCHANT_SELECT_MATERIAL2;
                _client.Send(TattooPackets.SelectEnchantMaterial(_tattooMaterialSlot));
                return;
            }
            if (_tattooMaterialSlot != GopetCmd.TATTOO_ENCHANT_SELECT_MATERIAL2) return;
            _client.Send(TattooPackets.ConfirmEnchant(_tattooEnchantId, _tattooMaterial1, value.ItemId));
            _tattooMaterialSlot = 0;
        }

        private void CloseTattoo()
        {
            if (_tattooView != null) Object.Destroy(_tattooView.gameObject);
            _tattooView = null;
        }
    }
}
