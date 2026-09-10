using System;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime
{
    /// <summary>Kiểm tra máy chủ trong lúc splash còn hiện và chặn chuyển màn khi lỗi.</summary>
    public sealed partial class GopetBootstrap
    {
        private void StartSplashConnection(Transform canvasParent, Font font, SoundToggleButton soundToggle)
        {
            _login.SetPresentationEnabled(false);

            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, waitForSignal: true);
            var popup = ConnectionPopupView.Create(canvasParent, font);
            popup.Hide();
            popup.RetryRequested += _flow.Retry;

            Action<LoginStage> stageChanged = null;
            stageChanged = stage =>
            {
                if (stage == LoginStage.Disconnected)
                {
                    popup.Show(_flow.Notice);
                    return;
                }

                popup.Hide();
                if (CanLeaveSplash(stage)) splash.AllowFinish();
            };
            _flow.StageChanged += stageChanged;

            splash.Finished += () =>
            {
                _flow.StageChanged -= stageChanged;
                popup.Hide();
                _login.SetPresentationEnabled(true);
                soundToggle.gameObject.SetActive(true);
            };

            _flow.Start(host, port);
        }

        private static bool CanLeaveSplash(LoginStage stage)
        {
            return stage == LoginStage.ChoosingServer
                   || stage == LoginStage.EnteringCredentials
                   || stage == LoginStage.CreatingCharacter
                   || stage == LoginStage.Ready;
        }
    }
}
