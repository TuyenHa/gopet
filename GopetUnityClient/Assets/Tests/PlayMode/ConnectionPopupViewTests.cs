using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class ConnectionPopupViewTests
    {
        private GameObject _canvas;
        private ConnectionPopupView _popup;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _popup = ConnectionPopupView.Create(_canvas.transform, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
        }

        [Test]
        public void Show_HienLyDoVaNutThuLai()
        {
            _popup.Show("Máy chủ không trả lời.");

            Assert.IsTrue(_popup.gameObject.activeSelf);
            Assert.AreEqual("Máy chủ không trả lời.", _popup.Detail);
            Assert.IsTrue(_popup.RetryButton.interactable);
            Assert.AreEqual(Color.white, _popup.RetryButton.GetComponentInChildren<UnityEngine.UI.Text>().color);
        }

        [Test]
        public void BamThuLai_ChiGuiMotLan()
        {
            _popup.Show(null);
            var calls = 0;
            _popup.RetryRequested += () => calls++;

            _popup.RetryButton.onClick.Invoke();
            _popup.RetryButton.onClick.Invoke();

            Assert.AreEqual(1, calls);
            Assert.IsFalse(_popup.RetryButton.interactable);
        }
    }
}
