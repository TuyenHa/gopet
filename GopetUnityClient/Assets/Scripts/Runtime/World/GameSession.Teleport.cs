using Gopet.Net.Map;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private void OnTeleportOptionsReceived(MapTeleportOption[] options)
        {
            if (_teleportDialog != null) Object.Destroy(_teleportDialog.gameObject);
            var labels = new string[options.Length];
            for (var i = 0; i < options.Length; i++)
                labels[i] = string.IsNullOrEmpty(options[i].Description) || options[i].Description == options[i].Name
                    ? options[i].Name
                    : $"{options[i].Name}\n{options[i].Description}";

            _teleportDialog = ChoiceDialogView.Create(_hudParent, null);
            _teleportDialog.Bind("Bản đồ dịch chuyển", labels);
            _teleportDialog.Chosen += index =>
            {
                if (index >= 0 && index < options.Length)
                {
                    var option = options[index];
                    // Embedded map data currently matches server map set; the legacy client
                    // likewise sends its local map version as the third ON_PLAYER_WARPING int.
                    _mapHandler.SendWarp(option.MapId, option.WaypointIndex, 1);
                    _warpFade?.FadeOut();
                }
                if (_teleportDialog != null) Object.Destroy(_teleportDialog.gameObject);
                _teleportDialog = null;
            };
        }
    }
}
