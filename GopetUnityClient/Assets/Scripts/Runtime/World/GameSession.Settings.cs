using System;
using Gopet.Net;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private readonly AutoAttackLoop _autoAttack = new AutoAttackLoop();
        private SettingsView _settingsView;
        private YesNoDialog _sessionConfirm;

        public event Action LogoutRequested;

        private void TickAutoAttack()
        {
            if (!_autoAttack.ShouldRequest() || !_client.IsConnected) return;
            _client.Send(Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.AUTO_ATTACK_SUPPORT));
        }

        private void SetAutoAttack(bool enabled)
        {
            _autoAttack.SetEnabled(enabled);
            ShowToast($"Tự đánh quái: {(enabled ? "Bật" : "Tắt")}");
        }

        private void OpenSettings()
        {
            if (_settingsView != null) return;
            _settingsView = SettingsView.Create(_hudParent, SoundManager.Instance,
                _autoAttack.Enabled);
            _settingsView.CloseRequested += CloseSettings;
            _settingsView.AutoAttackChanged += SetAutoAttack;
        }

        private void CloseSettings()
        {
            if (_settingsView == null) return;
            UnityEngine.Object.Destroy(_settingsView.gameObject);
            _settingsView = null;
        }

        private void ConfirmLogout()
        {
            ShowSessionConfirm("Bạn có chắc muốn đăng xuất?", "Đăng xuất",
                () => LogoutRequested?.Invoke());
        }

        private void ConfirmExit()
        {
            ShowSessionConfirm("Bạn có chắc muốn thoát game?", "Thoát game",
                Application.Quit);
        }

        private void ShowSessionConfirm(string message, string confirmLabel, Action confirmed)
        {
            if (_sessionConfirm != null) UnityEngine.Object.Destroy(_sessionConfirm.gameObject);
            _sessionConfirm = YesNoDialog.Create(_hudParent, message, confirmLabel, "Ở lại");
            _sessionConfirm.Confirmed += () =>
            {
                CloseSessionConfirm();
                confirmed?.Invoke();
            };
            _sessionConfirm.Cancelled += CloseSessionConfirm;
        }

        private void CloseSessionConfirm()
        {
            if (_sessionConfirm == null) return;
            UnityEngine.Object.Destroy(_sessionConfirm.gameObject);
            _sessionConfirm = null;
        }
    }
}
