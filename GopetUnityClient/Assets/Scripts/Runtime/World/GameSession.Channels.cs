using Gopet.Net.Map;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private void OnChannelsReceived(ChannelEntry[] channels)
        {
            if (_channelDialog != null) Object.Destroy(_channelDialog.gameObject);
            var labels = new string[channels.Length];
            for (var i = 0; i < channels.Length; i++)
            {
                var state = channels[i].Locked ? "Khoá" : $"{channels[i].PlayerCount} người";
                labels[i] = $"Khu {channels[i].ZoneId + 1} — {state}";
            }
            if (labels.Length == 0)
            {
                ShowToast("Map hiện tại không có khu vực khả dụng.");
                return;
            }

            var dialog = ChoiceDialogView.Create(_hudParent, UiBuilder.BuiltinFont());
            _channelDialog = dialog;
            dialog.Bind("Chọn khu vực", labels);
            dialog.Chosen += index =>
            {
                if (index >= 0 && index < channels.Length && !channels[index].Locked)
                    _channelHandler.ChangeChannel(_scene.MapId, channels[index].ZoneId);
                if (_channelDialog != null) Object.Destroy(_channelDialog.gameObject);
                _channelDialog = null;
            };
        }
    }
}
