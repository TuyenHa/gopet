using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Gopet.Runtime;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Gopet.PlayModeTests
{
    public sealed class SplashConnectionTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            foreach (var canvas in Object.FindObjectsByType<Canvas>())
            {
                if (canvas != null) Object.DestroyImmediate(canvas.gameObject);
            }
            foreach (var events in Object.FindObjectsByType<EventSystem>())
            {
                if (events != null) Object.DestroyImmediate(events.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator MatKetNoiTrongSplash_HienPopupVaThuLai()
        {
            _host = new GameObject("Bootstrap");
            var bootstrap = _host.AddComponent<GopetBootstrap>();
            SetPort(bootstrap, 1);
            LogAssert.Expect(LogType.Error, new Regex("Kết nối thất bại"));

            yield return WaitUntil(() => bootstrap.Flow != null && bootstrap.Flow.Stage == LoginStage.Disconnected, 2f);

            var splash = Object.FindAnyObjectByType<JarSplashScreen>();
            var popup = Object.FindAnyObjectByType<ConnectionPopupView>(FindObjectsInactive.Include);
            var login = Object.FindAnyObjectByType<LoginScreens>();
            Assert.IsNotNull(splash, "Splash đóng trước khi báo lỗi kết nối.");
            Assert.IsNotNull(popup, "Không dựng popup mất kết nối.");
            Assert.IsNull(login.Form, "Màn đăng nhập che splash trước khi kiểm tra xong.");
            Assert.IsTrue(popup.gameObject.activeSelf);
            Assert.IsNotEmpty(popup.Detail);

            popup.RetryButton.onClick.Invoke();
            Assert.AreEqual(LoginStage.Connecting, bootstrap.Flow.Stage);
            Assert.IsFalse(popup.gameObject.activeSelf);
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float seconds)
        {
            var deadline = Time.unscaledTime + seconds;
            while (!condition() && Time.unscaledTime < deadline) yield return null;
            Assert.IsTrue(condition(), "Quá thời gian chờ trạng thái kết nối.");
        }

        private static void SetPort(GopetBootstrap bootstrap, int port)
        {
            var field = typeof(GopetBootstrap).GetField("port", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(bootstrap, port);
        }
    }
}
