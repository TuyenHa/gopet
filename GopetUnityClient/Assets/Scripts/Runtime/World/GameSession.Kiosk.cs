using Gopet.Net.Kiosk;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private void OnKioskListingReceived(KioskListing listing)
        {
            if (_kioskDialog != null) Object.Destroy(_kioskDialog.gameObject);

            var dialog = KioskListingView.Create(_hudParent, UiBuilder.DefaultFont());
            _kioskDialog = dialog;
            dialog.Bind(listing, _assets);
            dialog.ActionChosen += hasListing =>
            {
                if (hasListing) _client.Send(KioskPackets.RemoveListing(listing.ItemId));
                else _client.Send(KioskPackets.SelectItem(listing.Type));
                CloseKioskDialog();
            };
            dialog.CloseRequested += CloseKioskDialog;
        }

        private void CloseKioskDialog()
        {
            if (_kioskDialog == null) return;
            Object.Destroy(_kioskDialog.gameObject);
            _kioskDialog = null;
        }
    }
}
