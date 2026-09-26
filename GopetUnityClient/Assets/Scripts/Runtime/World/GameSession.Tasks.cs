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
            if (TryConsumeBattleItemMenu(screen)) return true;
            if (TryConsumeCharacterHubMenu(screen)) return true;
            // Chỉ danh sách nhiệm vụ ĐANG NHẬN (1034) mới lên dòng nhiệm vụ HUD. Danh sách
            // NPC mời nhận (1033) phải hiện popup để chọn — trước đây HUD nuốt nó và đưa
            // nhiệm vụ NPC mời lên dòng HUD, bấm vào lại mở 1034 trống.
            if (screen == null || screen.ListId != TaskListPopupView.MyTaskMenuId) return false;

            _hud.TaskTracker.SetFirstTask(screen);
            if (_taskPopupRequested)
            {
                _taskPopupRequested = false;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Xin lại danh sách nhiệm vụ (im lặng) sau khi popup nhiệm vụ đóng — vừa nhận,
        /// trả hay huỷ nhiệm vụ thì dòng HUD phải đổi theo. Server xử lý gói theo thứ tự
        /// nên gói chọn gửi trước đã được áp dụng khi yêu cầu này tới.
        /// </summary>
        public void RefreshTaskTracker() => RequestTasks(false);

        /// <summary>
        /// Thắng trận của mình thì xin lại danh sách nhiệm vụ im lặng để dòng HUD cập nhật
        /// số quái đã đánh — không cần mở popup. Server cộng tiến độ
        /// (<c>TaskCalculator.onKillMob</c>) TRƯỚC khi gửi PET_BATTLE_STATE, nên gói xin gửi
        /// sau gói kết quả luôn thấy số mới. Chỉ trận có mình (battleId = userId) và mình thắng.
        /// </summary>
        private void OnBattleEndedRefreshTasks(Net.Battle.BattleResult result)
        {
            if (result == null || result.BattleId != _login.UserId || result.WinnerId != _login.UserId) return;
            RefreshTaskTracker();
        }

        private void LoadTaskTracker()
        {
            if (_taskLoaded) return;
            _taskLoaded = true;
            RequestTasks(false);
        }
    }
}
