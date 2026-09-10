using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>Tranh nền màn đăng nhập: ĐÚNG MỘT lớp, phủ kín màn hình.</summary>
    public sealed class LoginBackgroundTests
    {
        private GameObject _root;
        private LoginBackground _background;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _background = LoginBackground.Create(_root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        /// <summary>
        /// Ảnh phủ KÍN khung theo đúng tỉ lệ. Bản trước có thêm một lớp lót phía sau
        /// và trên màn rộng nó thò ra hai mép, trông như hai tấm ảnh chồng nhau.
        /// </summary>
        [Test]
        public void ChiCoMotLopAnh_PhuKinKhung()
        {
            var layers = _background.GetComponentsInChildren<AspectRatioFitter>(true);

            Assert.AreEqual(1, layers.Length, "Nền phải là MỘT lớp duy nhất.");
            Assert.AreEqual(AspectRatioFitter.AspectMode.EnvelopeParent, layers[0].aspectMode);
        }

        [Test]
        public void MoiLopDeuGiuDungTiLeAnhGoc()
        {
            foreach (var fitter in _background.GetComponentsInChildren<AspectRatioFitter>(true))
            {
                var image = fitter.GetComponent<Image>();
                if (image.sprite == null) continue;

                Assert.AreEqual(image.sprite.rect.width / image.sprite.rect.height, fitter.aspectRatio, 0.01f);
            }
        }

        /// <summary>Nền phủ kín màn và nằm dưới ô nhập — bật raycast là nuốt sạch cú chạm vào biểu mẫu.</summary>
        [Test]
        public void Nen_KhongChanCuCham()
        {
            foreach (var image in _background.GetComponentsInChildren<Image>(true))
            {
                Assert.IsFalse(image.raycastTarget, $"{image.name} đang chặn raycast.");
            }
        }

    }
}
