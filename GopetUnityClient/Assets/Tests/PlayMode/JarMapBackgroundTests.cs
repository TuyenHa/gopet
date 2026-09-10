using System.Collections;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nền map của màn đăng nhập, dựng trên ASSET THẬT đã giải từ jar
    /// (<c>maps/11.bytes</c> + <c>newMapData/*.png</c>) — không dựng dữ liệu giả.
    /// </summary>
    public sealed class JarMapBackgroundTests
    {
        private const float ViewWidth = 320f;
        private const float ViewHeight = 240f;

        private GameObject _root;
        private JarMapBackground _background;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _background = JarMapBackground.Create(_root.transform, JarMapBackground.LoginMapId, ViewWidth, ViewHeight);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test]
        public void Create_DungDuNenVaVatThe()
        {
            var map = JarMaps.Load(JarMapBackground.LoginMapId);
            var images = _background.GetComponentsInChildren<Image>(true);

            // 84 vật thể + gần 480 ô nền: chỉ cần chắc là đã dựng hơn hẳn số vật thể,
            // tức lớp ô nền không bị bỏ sót.
            Assert.Greater(images.Length, map.Objects.Length,
                "Chỉ dựng được vật thể mà thiếu lớp ô nền.");

            foreach (var image in images)
            {
                Assert.IsNotNull(image.sprite, "Có ô/vật thể dựng ra mà không gắn sprite nào.");
            }
        }

        /// <summary>Map 11 rộng 576, khung nhìn 320 → cuộn được đúng 256 pixel.</summary>
        [Test]
        public void QuangCuon_BangBeNgangMapTruKhungNhin()
        {
            var map = JarMaps.Load(JarMapBackground.LoginMapId);

            Assert.AreEqual(map.WidthPixels - ViewWidth, _background.MaxScrollX, 0.01f);
        }

        /// <summary>
        /// Nền phải trong suốt với cú chạm: nó phủ kín khung và nằm dưới ô nhập —
        /// bật raycast là mọi cú chạm vào ô tài khoản đều bị nền nuốt mất.
        /// </summary>
        [Test]
        public void Nen_KhongChanCuCham()
        {
            foreach (var image in _background.GetComponentsInChildren<Image>(true))
            {
                Assert.IsFalse(image.raycastTarget, $"{image.name} đang chặn raycast.");
            }
        }

        [UnityTest]
        public IEnumerator SauVaiFrame_CanhTroiSangPhai()
        {
            var start = _background.ScrollX;

            yield return null;
            yield return null;
            yield return null;

            Assert.Greater(_background.ScrollX, start, "Cảnh nền đứng im — không cuộn.");
        }

        [Test]
        public void CatDungOTuAnhDai_KhongPhaiCaAnh()
        {
            var map = JarMaps.Load(JarMapBackground.LoginMapId);
            var strip = JarSkin.Raw($"newMapData/{map.ResourceIds[0]}");

            // Ảnh dải rộng hàng trăm pixel; mỗi ô cắt ra phải đúng 24×24, nếu không
            // thì cả lớp nền bị kéo giãn.
            Assert.Greater(strip.rect.width, JarMapLayout.TileSize);

            var tile = FindTileImage(_background);
            Assert.IsNotNull(tile, "Không tìm thấy ô nền nào.");
            Assert.AreEqual(JarMapLayout.TileSize, tile.sprite.rect.width, 0.01f);
            Assert.AreEqual(JarMapLayout.TileSize, tile.sprite.rect.height, 0.01f);
        }

        private static Image FindTileImage(JarMapBackground background)
        {
            foreach (var image in background.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite != null && Mathf.Approximately(image.sprite.rect.width, JarMapLayout.TileSize)) return image;
            }

            return null;
        }
    }
}
