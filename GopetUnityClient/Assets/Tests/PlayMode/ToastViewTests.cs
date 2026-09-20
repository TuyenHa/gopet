using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Toast phải cao thêm khi câu dài. Lý do khoá map server gửi
    /// ("Hãy chăm chỉ làm nhiệm vụ để mở map này") dài hơn một dòng toast cũ.
    /// </summary>
    public sealed class ToastViewTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp() => _root = new GameObject("Toast Test Root", typeof(RectTransform), typeof(Canvas));

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test]
        public void Toast_CauNgan_GiuMotDong()
        {
            var toast = ToastView.Create(_root.transform, null, "Đã lưu");
            Assert.AreEqual(44f, ((RectTransform)toast.transform).sizeDelta.y, 0.01f);
        }

        [Test]
        public void Toast_CauDai_CaoHonMotDong()
        {
            var toast = ToastView.Create(_root.transform, null,
                "Hãy chăm chỉ làm nhiệm vụ để mở map này, phần thưởng đang chờ bạn phía trước");
            Assert.Greater(((RectTransform)toast.transform).sizeDelta.y, 44f,
                "Câu dài phải nới chiều cao toast thay vì tràn ra ngoài nền.");
        }
    }
}
