using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Camera chụp map cho minimap: phải ôm TRỌN map (thiếu một góc là người chơi mất
    /// phương hướng) và ảnh phải đúng tỉ lệ map, nếu không minimap bị méo.
    /// </summary>
    public sealed class MinimapCameraTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp() => _root = new GameObject("Minimap Camera Root");

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test]
        public void Camera_OmTronMapVaDungTiLe()
        {
            var view = MinimapCamera.Attach(_root.transform);
            const int width = 1200, height = 800;

            view.Frame(width, height);

            var cam = view.GetComponent<Camera>();
            Assert.IsTrue(cam.enabled, "Có map rồi thì camera phải bật.");
            Assert.AreEqual(new Vector3(width * 0.5f, height * 0.5f), (Vector2)cam.transform.localPosition,
                "Camera phải nhìn vào giữa map.");
            Assert.AreEqual(height * 0.5f, cam.orthographicSize, 0.01f,
                "Zoom phải vừa đúng chiều cao map.");
            Assert.AreEqual((float)width / height, (float)view.Target.width / view.Target.height, 0.02f,
                "Ảnh minimap méo so với tỉ lệ map.");
            Assert.LessOrEqual(Mathf.Max(view.Target.width, view.Target.height),
                MinimapCamera.MaxTextureSize, "Ảnh minimap vượt cạnh tối đa.");
            Assert.AreEqual(view.Target, cam.targetTexture,
                "Quên gán targetTexture là camera này vẽ đè lên màn hình chính.");
        }

        /// <summary>Chưa có map (vừa vào game, map chưa dựng) thì đừng render gì cả.</summary>
        [Test]
        public void Camera_ChuaCoMap_TatHan()
        {
            var view = MinimapCamera.Attach(_root.transform);

            view.Frame(0, 0);

            Assert.IsFalse(view.GetComponent<Camera>().enabled);
            Assert.IsNull(view.Target);
        }

        /// <summary>Đổi map → tạo ảnh mới đúng tỉ lệ map mới, ảnh cũ phải được thả.</summary>
        [Test]
        public void Camera_DoiMap_TaoLaiAnhTheoTiLeMoi()
        {
            var view = MinimapCamera.Attach(_root.transform);

            view.Frame(1200, 800);
            var first = view.Target;
            view.Frame(600, 900);

            Assert.AreNotSame(first, view.Target, "Map mới tỉ lệ khác mà vẫn xài ảnh cũ.");
            Assert.AreEqual(600f / 900f, (float)view.Target.width / view.Target.height, 0.02f);
        }
    }
}
