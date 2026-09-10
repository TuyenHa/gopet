using Gopet.Net.Pet;

namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        private PetUpgradeHandler _petUpgradeHandler;
        private PetUpgradeView _petUpgradeView;

        public void InitializePetUpgrade(PetUpgradeHandler handler)
        {
            _petUpgradeHandler = handler;
            handler.ShowRequested += ShowPetUpgrade;
            handler.PetSelected += OnPetUpgradeSelection;
            handler.PriceReceived += OnPetUpgradePrice;
            handler.PreviewReceived += OnPetUpgradePreview;
            handler.Completed += OnPetUpgradeCompleted;
        }

        private void ShowPetUpgrade()
        {
            if (_petUpgradeView != null) Close(_petUpgradeView);
            var view = PetUpgradeView.Create(transform, _font);
            _petUpgradeView = view;
            view.CloseRequested += () => ClosePetUpgrade(view);
            view.SelectRequested += _petUpgradeHandler.SelectPet;
            view.PreviewRequested += _petUpgradeHandler.RequestPreview;
            view.ConfirmRequested += (activeId, materialId, name) =>
                ShowConfirm(
                    new Gopet.UiLogic.MenuSelection.ConfirmPrompt
                    {
                        Text = "Tiến hoá sẽ tiêu thụ cả hai pet và không thể hoàn tác.",
                        ConfirmLabel = "Tiến hoá",
                        CancelLabel = "Huỷ"
                    },
                    () => _petUpgradeHandler.Confirm(activeId, materialId, name));
            Push(view, view.gameObject);
            _petUpgradeHandler.RequestPrice();
        }

        private void ClosePetUpgrade(PetUpgradeView view)
        {
            Close(view);
            if (ReferenceEquals(_petUpgradeView, view)) _petUpgradeView = null;
        }

        private void OnPetUpgradeSelection(PetUpgradeSelection value) =>
            _petUpgradeView?.ApplySelection(value);

        private void OnPetUpgradePrice(PetUpgradePrice value) =>
            _petUpgradeView?.ApplyPrice(value);

        private void OnPetUpgradePreview(PetUpgradePreview value) =>
            _petUpgradeView?.ApplyPreview(value);

        private void OnPetUpgradeCompleted()
        {
            if (_petUpgradeView != null) ClosePetUpgrade(_petUpgradeView);
            ShowToast("Tiến hoá pet thành công.");
        }

        private void UnbindPetUpgrade()
        {
            if (_petUpgradeHandler == null) return;
            _petUpgradeHandler.ShowRequested -= ShowPetUpgrade;
            _petUpgradeHandler.PetSelected -= OnPetUpgradeSelection;
            _petUpgradeHandler.PriceReceived -= OnPetUpgradePrice;
            _petUpgradeHandler.PreviewReceived -= OnPetUpgradePreview;
            _petUpgradeHandler.Completed -= OnPetUpgradeCompleted;
        }
    }
}
