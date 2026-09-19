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
        private const string AutoRecoveryPrefKey = "gopet.auto_recovery";
        private bool _autoRecovery;

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

        /// <summary>Bật/tắt PET_RECOVERY_HP (opcode 45). Lưu PlayerPrefs như các toggle khác
        /// để nhớ giữa các phiên; server không tự nhớ trạng thái này.</summary>
        internal void SetAutoRecovery(bool enabled)
        {
            _autoRecovery = enabled;
            try { PlayerPrefs.SetInt(AutoRecoveryPrefKey, enabled ? 1 : 0); PlayerPrefs.Save(); }
            catch { /* PlayerPrefs có thể fail trong build đặc biệt — không chặn UX */ }
            _battleHandler?.SetAutoRecovery(enabled);
            ShowToast($"Tự hồi HP: {(enabled ? "Bật" : "Tắt")}");
        }

        /// <summary>Gửi trạng thái tự hồi HP sau khi đăng nhập. Server khởi tạo
        /// <c>isPetRecovery = false</c> (<c>Player.cs:43</c>) và KHÔNG nhớ giữa các phiên,
        /// nên phải gửi lại mỗi lần vào — kể cả khi tắt, để trạng thái hai bên khớp nhau.
        ///
        /// <para>Mặc định BẬT: thua quái xong mà pet không bao giờ hồi máu là bế tắc, người
        /// chơi không tự đoán được là phải vào Cài đặt bật lên. Server vẫn phạt chờ 30 giây
        /// sau khi thua (<c>TIME_DELAY_HEAL_WHEN_MOB_KILL_PET</c>) rồi mới hồi 20%/3 giây.</para></summary>
        internal void RestoreAutoRecoveryOnLogin()
        {
            try { _autoRecovery = PlayerPrefs.GetInt(AutoRecoveryPrefKey, 1) == 1; } catch { _autoRecovery = true; }
            _battleHandler?.SetAutoRecovery(_autoRecovery);
        }

        private void OpenSettings()
        {
            if (_settingsView != null) return;
            _settingsView = SettingsView.Create(_hudParent, SoundManager.Instance,
                _autoAttack.Enabled, _autoRecovery);
            _settingsView.CloseRequested += CloseSettings;
            _settingsView.AutoAttackChanged += SetAutoAttack;
            _settingsView.AutoRecoveryChanged += SetAutoRecovery;
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
