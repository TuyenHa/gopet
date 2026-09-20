using System.Collections;
using System.Linq;
using Gopet.Net.Map;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Bản đồ thế giới là một MÀN RIÊNG: tranh phủ kín màn (không dải lót hai bên như
    /// một popup), mọi nhãn nằm trong tầm nhìn, và vùng bấm đủ rộng cho ngón tay.
    /// </summary>
    public sealed class WorldMapScreenTests
    {
        private static readonly Vector2 Screen16By9 = new Vector2(960f, 540f);

        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("World Map Screen Root", typeof(RectTransform), typeof(Canvas));
            ((RectTransform)_root.transform).sizeDelta = Screen16By9;
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        private static MapTeleportOption Option(int mapId, string name, bool locked = false) =>
            new MapTeleportOption
            {
                MapId = mapId,
                Name = name,
                Description = name,
                WaypointIndex = 0,
                Locked = locked,
                LockReason = locked ? "khoá" : ""
            };

        private static MapTeleportOption[] AllMaps() =>
            Enumerable.Range(11, 24).Select(id => Option(id, $"Bản đồ số {id}", id >= 26)).ToArray();

        /// <summary>
        /// Tranh vừa khít trong màn để lại hai dải nền lót — nhìn y như một popup dán
        /// giữa cảnh chơi, đúng thứ màn này không được phép trông giống.
        /// </summary>
        [UnityTest]
        public IEnumerator BanDo_TranhPhuKinMan_KhongConDaiLot()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(AllMaps(), 11);
            yield return null;
            yield return null;

            var picture = (RectTransform)view.transform.Find("Picture");
            Assert.GreaterOrEqual(picture.rect.width, Screen16By9.x - 0.5f,
                "Tranh hụt bề ngang màn — lộ dải lót hai bên.");
            Assert.GreaterOrEqual(picture.rect.height, Screen16By9.y - 0.5f,
                "Tranh hụt chiều cao màn — lộ dải lót trên dưới.");
        }

        /// <summary>Tranh ghim mép trên nên phần bị cắt rơi vào biển ở đáy, không cắt mất đỉnh.</summary>
        [UnityTest]
        public IEnumerator BanDo_CatPhanThua_ODayChuKhongODinh()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(AllMaps(), 11);
            yield return null;
            yield return null;

            var picture = (RectTransform)view.transform.Find("Picture");
            Assert.AreEqual(WorldRect((RectTransform)view.transform).yMax, WorldRect(picture).yMax, 0.5f,
                "Mép trên tranh phải trùng mép trên màn.");
        }

        /// <summary>
        /// Tranh tràn ra ngoài màn, nên nhãn rơi vào phần bị cắt là mất hút — người chơi
        /// không thấy và cũng không bấm tới được map đó.
        /// </summary>
        [UnityTest]
        public IEnumerator BanDo_MoiNhan_DeuNamTrongTamNhin()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(AllMaps(), 11);
            yield return null;
            yield return null;

            var screen = WorldRect((RectTransform)view.transform);
            foreach (var label in Labels(view))
            {
                var rect = WorldRect(label);
                Assert.IsTrue(screen.xMin <= rect.xMin + 0.5f && rect.xMax <= screen.xMax + 0.5f
                    && screen.yMin <= rect.yMin + 0.5f && rect.yMax <= screen.yMax + 0.5f,
                    $"'{label.name}' nằm ngoài tầm nhìn: {rect} so với màn {screen}.");
            }
        }

        /// <summary>Thẻ tên chỉ cao 20px — bấm bằng ngón tay là trượt, nên vùng bấm rộng hơn.</summary>
        [Test]
        public void BanDo_VungBam_DuRongDeChamBangNgonTay()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(new[] { Option(11, "Làng"), Option(12, "Ải") }, 11);
            var node = view.transform.Find("Picture/Nodes/Map12");
            var hit = (RectTransform)node.Find("Hit");
            var label = (RectTransform)node.Find("Label");
            var size = ((RectTransform)node).rect.size;

            Assert.GreaterOrEqual(size.x, 64f, "Vùng bấm quá hẹp cho tên ngắn như 'Ải'.");
            Assert.GreaterOrEqual(size.y, 53f, "Vùng bấm phải trùm cả pin, khe 5px lẫn thẻ tên.");
            Assert.GreaterOrEqual(label.anchoredPosition.y - label.sizeDelta.y, -size.y * 0.5f,
                "Thẻ tên thò xuống dưới vùng bấm — phần thò ra không bấm được.");
            Assert.IsTrue(hit.GetComponent<Image>().raycastTarget, "Vùng bấm phải nhận raycast.");
            Assert.AreEqual(node.childCount - 1, hit.GetSiblingIndex(),
                "Vùng bấm phải là con cuối — con sau sẽ nuốt mất cú chạm.");
        }

        /// <summary>Map đang đứng dùng pin xanh có sao để phân biệt với hai chục pin vàng.</summary>
        [Test]
        public void BanDo_MapDangDung_DungPinXanh()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(new[] { Option(11, "Làng"), Option(13, "Linh Lâm") }, 11);
            var current = view.transform.Find("Picture/Nodes/Map11");
            var other = view.transform.Find("Picture/Nodes/Map13");

            var pin = (RectTransform)other.Find("Pin");
            var label = (RectTransform)other.Find("Label");
            Assert.AreEqual(5f, pin.anchoredPosition.y - label.anchoredPosition.y, 0.01f,
                "Mũi pin phải cách mép trên thẻ tên 5px — dính nhau thì nhìn như một cục.");

            var openPin = Resources.Load<Sprite>(WorldMapView.PinResource);
            Assert.IsNotNull(openPin, $"Thiếu icon Resources/{WorldMapView.PinResource}.");
            Assert.AreEqual(openPin, other.Find("Pin").GetComponent<Image>().sprite,
                "Map mở phải dùng pin vàng.");
            Assert.AreEqual(Resources.Load<Sprite>(WorldMapView.CurrentPinResource),
                current.Find("Pin").GetComponent<Image>().sprite, "Map đang đứng phải dùng pin xanh.");
        }

        /// <summary>Mây trôi ngang qua tên map thì không đọc được — mây phải nằm dưới lớp pin.</summary>
        [Test]
        public void BanDo_LopMay_NamDuoiLopPin()
        {
            var view = WorldMapView.Create(_root.transform, null);
            var clouds = view.transform.Find("Picture/Clouds");
            var nodes = view.transform.Find("Picture/Nodes");

            Assert.IsNotNull(clouds, "Lớp mây phải là con của tranh, không phải của root.");
            Assert.Less(clouds.GetSiblingIndex(), nodes.GetSiblingIndex(),
                "Mây vẽ sau lớp pin sẽ che mất tên map.");
        }

        private static RectTransform[] Labels(WorldMapView view) =>
            view.transform.Find("Picture/Nodes")
                .GetComponentsInChildren<RectTransform>(true)
                .Where(r => r.name.StartsWith("Map") || r.name.StartsWith("Region"))
                .ToArray();

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y,
                corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }
    }
}
