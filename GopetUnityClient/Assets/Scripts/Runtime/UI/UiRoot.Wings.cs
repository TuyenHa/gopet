using Gopet.Net.Guider;
using Gopet.Net.Player;

namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        private const int WingInventoryMenuId = 81040;
        private WingHandler _wingHandler;

        public void InitializeWings(WingHandler handler) => _wingHandler = handler;

        private bool TryShowWingActions(GenericMenuView inventory, MenuScreen screen, int index)
        {
            if (_wingHandler == null || index < 0 || index >= screen.Items.Length) return false;
            var item = screen.Items[index];
            var equipped = item.ItemId == -1;
            var choices = equipped
                ? new[] { "Tháo cánh", "Đóng" }
                : new[] { "Sử dụng", "Cường hóa", "Đóng" };
            var dialog = ChoiceDialogView.Create(transform, _font);
            dialog.Bind(item.Title, choices);
            dialog.Chosen += choice =>
            {
                Close(dialog);
                if (choice >= choices.Length - 1) return;
                Close(inventory);
                if (equipped) _wingHandler.Unequip();
                else if (choice == 0) _wingHandler.Use(item.ItemId);
                else _wingHandler.Enchant(item.ItemId);
            };
            Push(dialog, dialog.gameObject);
            return true;
        }
    }
}
