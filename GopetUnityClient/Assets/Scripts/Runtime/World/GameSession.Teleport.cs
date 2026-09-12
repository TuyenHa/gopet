using Gopet.Net.Map;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private void OnTeleportOptionsReceived(MapTeleportOption[] options)
        {
            CloseMapPicker();
            if (options == null || options.Length == 0)
            {
                ShowToast("Không có bản đồ nào khả dụng.");
                return;
            }

            _mapPickerView = MapPickerView.Create(_hudParent, UiBuilder.BuiltinFont());
            var labels = new string[options.Length];
            for (var i = 0; i < options.Length; i++)
                labels[i] = string.IsNullOrEmpty(options[i].Description) || options[i].Description == options[i].Name
                    ? options[i].Name : $"{options[i].Name}\n{options[i].Description}";
            _mapPickerView.Bind("Chọn bản đồ", labels);
            _mapPickerView.Chosen += index => OnMapPickerChosen(options, index);
            _mapPickerView.CloseRequested += CloseMapPicker;
        }

        private void OnMapPickerChosen(MapTeleportOption[] options, int index)
        {
            if (index < 0 || index >= options.Length) return;
            var option = options[index];
            SoundManager.Instance?.PlayEffect("s_outMap_1");
            _mapHandler.SendWarp(option.MapId, option.WaypointIndex, 1);
            _warpFade?.FadeOut();
            CloseMapPicker();
        }

        private void CloseMapPicker()
        {
            if (_mapPickerView == null) return;
            Object.Destroy(_mapPickerView.gameObject);
            _mapPickerView = null;
        }
    }
}
