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
    /// Màn bản đồ thế giới: dựng đủ node theo danh sách server, map khoá có ổ khoá và
    /// KHÔNG gửi warp, map đang đứng không tự warp lại.
    /// </summary>
    public sealed class WorldMapViewTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            // Kích thước thật của canvas HUD (ref 960×540) — thuật toán giãn nhãn tính
            // bằng pixel nên canvas không kích thước thì không kiểm được gì.
            _root = new GameObject("World Map Test Root", typeof(RectTransform), typeof(Canvas));
            ((RectTransform)_root.transform).sizeDelta = new Vector2(960f, 540f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        private static MapTeleportOption Option(int mapId, string name, bool locked = false,
            string reason = "", int waypoint = 0) =>
            new MapTeleportOption
            {
                MapId = mapId,
                Name = name,
                Description = name,
                WaypointIndex = waypoint,
                Locked = locked,
                LockReason = reason
            };

        [Test]
        public void WorldMap_DungDuNodeTheoDanhSachServer()
        {
            var view = WorldMapView.Create(_root.transform, null);
            var options = Enumerable.Range(11, 24).Select(id => Option(id, $"Map {id}")).ToArray();

            view.Bind(options, 11);

            var nodes = view.transform.Find("Picture/Nodes");
            var built = options.Count(o => nodes.Find($"Map{o.MapId}") != null);
            Assert.AreEqual(options.Length, built, "Thiếu node map — có map bị rơi mất khi bind.");
        }

        [Test]
        public void WorldMap_MapKhoa_HienOKhoaVaKhongGuiWarp()
        {
            var view = WorldMapView.Create(_root.transform, null);
            var reason = "Hãy chăm chỉ làm nhiệm vụ để mở map này";
            view.Bind(new[] { Option(11, "Làng"), Option(26, "Thiên Đình", true, reason) }, 11);
            var chosen = -1;
            string lockedReason = null;
            view.Chosen += mapId => chosen = mapId;
            view.LockedChosen += value => lockedReason = value;

            NodeButton(view, 26).onClick.Invoke();

            Assert.AreEqual(-1, chosen, "Map khoá không được gửi warp.");
            Assert.AreEqual(reason, lockedReason, "Phải báo đúng lý do server gửi.");
            Assert.AreEqual(JarSkin.Raw("lock"),
                Node(view, 26).Find("Pin").GetComponent<Image>().sprite,
                "Map khoá phải hiện icon ổ khoá thay cho chấm pin.");
        }

        [Test]
        public void WorldMap_ChonMapMo_TraVeMapId()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(new[] { Option(11, "Làng"), Option(13, "Linh Lâm") }, 11);
            var chosen = -1;
            view.Chosen += mapId => chosen = mapId;

            NodeButton(view, 13).onClick.Invoke();

            Assert.AreEqual(13, chosen);
            Assert.AreNotEqual(JarSkin.Raw("lock"),
                Node(view, 13).Find("Pin").GetComponent<Image>().sprite,
                "Map mở dùng chấm pin, không phải ổ khoá.");
        }

        [Test]
        public void WorldMap_MapDangDung_KhongWarpLai()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(new[] { Option(11, "Làng"), Option(13, "Linh Lâm") }, 11);
            var chosen = -1;
            view.Chosen += mapId => chosen = mapId;

            NodeButton(view, 11).onClick.Invoke();

            Assert.AreEqual(-1, chosen, "Đang đứng ở map này thì không gửi warp nữa.");
        }

        [Test]
        public void WorldMap_MapLa_VanHienChuKhongRoiMat()
        {
            var view = WorldMapView.Create(_root.transform, null);
            // Map 99 không có trong bảng bố cục — phải rơi vào cụm "Khác", không biến mất.
            view.Bind(new[] { Option(11, "Làng"), Option(99, "Map lạ") }, 11);

            Assert.IsNotNull(Node(view, 99), "Map ngoài bảng bố cục vẫn phải hiện.");
        }

        [Test]
        public void WorldMap_NutDong_PhatSuKienDong()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(new MapTeleportOption[0], 11);
            var closed = false;
            view.CloseRequested += () => closed = true;

            view.transform.Find("Close").GetComponent<Button>().onClick.Invoke();

            Assert.IsTrue(closed);
        }

        /// <summary>
        /// Tên map server gửi dài ngắn rất khác nhau; xếp lưới cứng là chúng chồng thành
        /// một đống chữ không đọc được (đã xảy ra một lần với cụm Thượng giới).
        /// </summary>
        [UnityTest]
        public IEnumerator WorldMap_NhanKhongChongLenNhau()
        {
            var view = WorldMapView.Create(_root.transform, null);
            view.Bind(new[]
            {
                Option(11, "Thành Phố Linh Thú"), Option(22, "Chợ trời"), Option(19, "Đấu trường"),
                Option(20, "Lôi đài"), Option(12, "Ải"), Option(13, "Linh Lâm"),
                Option(14, "Linh Mộc"), Option(15, "Đại Linh Cảnh"), Option(16, "Đường lên đỉnh núi"),
                Option(17, "Thung lũng Hoàng Nham"), Option(18, "Núi Phục Quang"), Option(21, "Thạch Động"),
                Option(23, "Băng động 1"), Option(24, "Sông băng"), Option(25, "Băng động 2"),
                Option(26, "Vùng đất phong ấn", true, "khoá"), Option(27, "Đài tưởng niệm", true, "khoá"),
                Option(28, "Quảng trường chính", true, "khoá"), Option(29, "Khu vực bang hội", true, "khoá"),
                Option(30, "Ải thượng giới", true, "khoá"), Option(31, "Chốt chặn cuối cùng", true, "khoá"),
                Option(32, "Những cây cầu", true, "khoá"), Option(33, "Vùng chiến sự", true, "khoá"),
                Option(34, "Rừng Linh cuối", true, "khoá")
            }, 11);
            // Giãn nhãn chạy ở Update đầu tiên có kích thước thật.
            yield return null;
            yield return null;

            var labels = view.transform.Find("Picture/Nodes")
                .GetComponentsInChildren<RectTransform>(true)
                .Where(r => r.name.StartsWith("Map") || r.name.StartsWith("Region"))
                .ToArray();
            for (var i = 0; i < labels.Length; i++)
            for (var j = i + 1; j < labels.Length; j++)
                Assert.IsFalse(WorldRect(labels[i]).Overlaps(WorldRect(labels[j])),
                    $"'{labels[i].name}' đè lên '{labels[j].name}'.");
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y,
                corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }

        private static Button NodeButton(WorldMapView view, int mapId) =>
            Node(view, mapId).Find("Hit").GetComponent<Button>();

        private static Transform Node(WorldMapView view, int mapId)
        {
            var node = view.transform.Find($"Picture/Nodes/Map{mapId}");
            Assert.IsNotNull(node, $"Không tìm thấy node của map {mapId}.");
            return node;
        }
    }
}
