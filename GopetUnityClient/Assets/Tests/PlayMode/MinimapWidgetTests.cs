using System.Collections;
using Gopet.Runtime.UI;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Minimap góc HUD: gắn ảnh map do <see cref="MinimapCamera"/> chụp và chấm vị trí
    /// bám theo nhân vật. Phần chọn map đã chuyển sang <see cref="WorldMapViewTests"/>.
    /// </summary>
    public sealed class MinimapWidgetTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp() => _root = new GameObject("Minimap Test Root", typeof(RectTransform), typeof(Canvas));

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator Minimap_HienAnhMapThat_VaDotTheoNhanVat()
        {
            var widget = MinimapWidget.Create(_root.transform, null);
            var map = JarMaps.Load(11);
            var camera = MinimapCamera.Attach(_root.transform);
            camera.Frame(map.WidthPixels, map.HeightPixels);

            widget.SetLiveMap(camera.Target, map.WidthPixels, map.HeightPixels, 11);
            var image = widget.GetComponentInChildren<RawImage>(true);
            Assert.AreSame(camera.Target, image.texture, "Minimap không gắn ảnh camera chụp.");
            Assert.AreEqual(Color.white, image.color, "Ruột phải trắng để hiện đúng màu ảnh map.");

            var avatar = PlayerAvatar.Spawn(_root.transform, 1, "Self", 0, 144, 120, map.HeightPixels);
            widget.BindPlayer(avatar);
            yield return null;

            var dot = widget.transform.Find("Map/PlayerDot") as RectTransform;
            Assert.IsTrue(dot.gameObject.activeSelf);
            Assert.AreEqual(144f / map.WidthPixels, dot.anchorMin.x, 0.002f);
            Assert.AreEqual(1f - 120f / map.HeightPixels, dot.anchorMin.y, 0.002f);
        }

        /// <summary>Chưa dựng xong map thì rơi về nhãn "Map N", không treo ảnh rác.</summary>
        [Test]
        public void Minimap_ChuaCoAnh_HienNhanMap()
        {
            var widget = MinimapWidget.Create(_root.transform, null);

            widget.SetLiveMap(null, 0, 0, 11);

            var image = widget.GetComponentInChildren<RawImage>(true);
            Assert.IsNull(image.texture);
            Assert.Less(image.color.grayscale, 0.3f, "Ruột minimap đang trắng.");
        }

        /// <summary>Tên map nằm ở dải trên đỉnh minimap, ảnh map chỉ nằm bên dưới dải đó.</summary>
        [Test]
        public void Minimap_HienTenMapTrenDinh_KhongDeLenAnhMap()
        {
            var widget = MinimapWidget.Create(_root.transform, null);
            var texture = new RenderTexture(64, 32, 0);
            try
            {
                widget.SetLiveMap(texture, 640, 320, 11);

                Assert.AreEqual(MapDisplayNames.Get(11), widget.MapName);
                var title = (RectTransform)widget.transform.Find("Map Title");
                var map = (RectTransform)widget.transform.Find("Map");
                Assert.IsNotNull(title, "Thiếu dải tên map trên minimap.");
                Assert.AreEqual(1f, title.anchorMax.y, 0.001f, "Tên map phải nằm trên đỉnh.");
                Assert.LessOrEqual(map.anchorMax.y, title.anchorMin.y + 0.001f, "Ảnh map đè lên tên map.");
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
