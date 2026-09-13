using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using Gopet.UiLogic;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private bool _taskPopupRequested;
        private bool _taskLoaded;

        private void RequestTasks(bool showPopup)
        {
            _taskPopupRequested = showPopup;
            if (CharacterMenu.TryBuildServerMessage(CharacterMenuAction.Tasks, out var message))
                _client.Send(message);
        }

        public bool TryConsumeHudMenu(MenuScreen screen)
        {
            if (TryConsumeCharacterHubMenu(screen)) return true;
            if (screen == null || (screen.ListId != 1033 && screen.ListId != 1034)) return false;

            _hud.TaskTracker.SetFirstTask(screen);
            if (_taskPopupRequested)
            {
                _taskPopupRequested = false;
                return false;
            }
            return true;
        }

        private void LoadTaskTracker()
        {
            if (_taskLoaded) return;
            _taskLoaded = true;
            RequestTasks(false);
        }
    }
}
