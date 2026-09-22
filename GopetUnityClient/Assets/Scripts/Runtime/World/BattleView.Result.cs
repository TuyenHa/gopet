using Gopet.Net.Battle;
using Gopet.Runtime.World.Battle;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class BattleView
    {
        private BattleSummaryTracker _summaryTracker;
        private BattleSummarySnapshot _summary;
        private bool _resultReceived;

        public bool AwaitingVictoryConfirmation { get; private set; }

        /// <summary>Chốt ở gói đầu tiên, không tính thời gian diễn đòn cuối/đọc popup.</summary>
        public void ShowResult(BattleResult result)
        {
            if (result == null || result.BattleId != BattleId || HasResult || _closeRequested) return;
            _summary = _summaryTracker.Complete(result, Time.realtimeSinceStartupAsDouble);
            _resultReceived = true;
            AwaitingVictoryConfirmation = _start.Kind == BattleKind.Mob && IsParticipant
                && result.WinnerId == _start.LocalPet.ActorId;
            if (_actionBar != null) _actionBar.gameObject.SetActive(false);
            _skillPopup?.SetOpen(false);
            RefreshLocks();
            if (_animator != null && !_animator.Idle)
            {
                _pendingResult = result;
                return;
            }
            PresentResult(result);
        }

        private void ShowPendingResult()
        {
            var result = _pendingResult;
            _pendingResult = null;
            if (result != null && !_closeRequested) PresentResult(result);
        }

        private void PresentResult(BattleResult result)
        {
            if (_result != null) return;
            if (AwaitingVictoryConfirmation)
                _result = BattleVictoryPopup.Create(transform, _summary, RequestClose).gameObject;
            else
            {
                _result = BattleResultBanner.Create(transform, result, _start.LocalPet.ActorId, IsParticipant);
                ShowRewards(result);
            }
            _resultShownAt = Time.unscaledTime;
        }

        /// <summary>OK chỉ đóng lớp chiến đấu, không dịch chuyển hay lĩnh thưởng lại.</summary>
        private void RequestClose()
        {
            if (_closeRequested) return;
            _closeRequested = true;
            AwaitingVictoryConfirmation = false;
            Closed?.Invoke();
        }
    }
}
