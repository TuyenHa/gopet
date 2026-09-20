using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Watchdog của màn fade warp. Không có nó thì một nhánh server im lặng (pet đang
    /// đánh nhau, pet chết vì PK, ngoại lệ bị nuốt) để lại màn đen chặn raycast vĩnh
    /// viễn — người chơi phải thoát game.
    /// </summary>
    public sealed class WarpFadeOverlayTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp() => _root = new GameObject("Warp Fade Test Root", typeof(RectTransform));

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator HetHanChoMapMoi_TuMoLaiManHinh()
        {
            var overlay = WarpFadeOverlay.Create(_root.transform);
            overlay.WatchdogSeconds = 0.2f;
            var timedOut = false;
            overlay.TimedOut += () => timedOut = true;

            overlay.FadeOut();
            yield return new WaitForSeconds(0.2f + 0.35f + 0.2f); // hạn chờ + thời gian fade + biên

            var group = overlay.GetComponent<CanvasGroup>();
            Assert.IsTrue(timedOut, "Hết hạn chờ phải bắn sự kiện để session báo người chơi.");
            Assert.AreEqual(0f, group.alpha, 0.01f, "Màn đen phải tự mờ đi.");
            Assert.IsFalse(group.blocksRaycasts, "Màn hình phải nhận lại thao tác.");
        }

        [UnityTest]
        public IEnumerator MapNapKipThoi_KhongBanHetHan()
        {
            var overlay = WarpFadeOverlay.Create(_root.transform);
            overlay.WatchdogSeconds = 0.5f;
            var timedOut = false;
            overlay.TimedOut += () => timedOut = true;

            overlay.FadeOut();
            yield return new WaitForSeconds(0.1f);
            overlay.FadeIn();
            yield return new WaitForSeconds(0.7f); // vượt qua mốc hạn chờ cũ

            Assert.IsFalse(timedOut, "FadeIn phải huỷ hẹn giờ, không được báo lỗi oan.");
            Assert.AreEqual(0f, overlay.GetComponent<CanvasGroup>().alpha, 0.01f);
        }
    }
}
