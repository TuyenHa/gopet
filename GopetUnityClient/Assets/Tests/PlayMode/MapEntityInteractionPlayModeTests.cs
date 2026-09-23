using System.Collections;
using System.Linq;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Phase 2/3 (map-portal-warp-shop-interaction): map 11 phải dựng đúng 4 portal +
    /// 7 building bấm được, khớp <c>plans/.../reports/decode-map11-entities.md</c>.
    /// Đóng phần "structural" của success criteria — không thay được nghiệm thu bằng
    /// mắt trong Editor, nhưng bắt được hồi quy nếu parse/spawn lệch.
    /// </summary>
    public sealed class MapEntityInteractionPlayModeTests
    {
        [UnityTest]
        public IEnumerator Map11_Dung4Portal_TenVaMapDichDungBangDecode()
        {
            var renderer = MapRenderer.Create(null, 11);
            var portals = renderer.GetComponentsInChildren<MapPortalView>();

            Assert.AreEqual(4, portals.Length, "Map 11 phải có đúng 4 portal (decode-map11-entities.md).");

            // Đọc entity qua CHÍNH đường GameSession dùng để warp (sự kiện Selected) —
            var clicked = new System.Collections.Generic.List<JarMapEntity>();
            foreach (var portal in portals)
            {
                portal.Selected += e => clicked.Add(e);
                portal.OnPointerClick(null);
            }

            Assert.AreEqual(4, clicked.Count);
            var got = clicked.Select(e => (e.Name, e.ExtraA, e.ExtraB)).OrderBy(t => t.ExtraA).ToArray();
            var expected = new[]
            {
                ("Đấu trường", 19, 0),
                ("Linh Lâm", 13, 0),
                ("Đại Linh Cảnh", 15, 0),
                ("Đường lên núi", 16, 0),
            }.OrderBy(t => t.Item2).ToArray();

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].Item1, got[i].Name, $"portal mapId={expected[i].Item2}");
                Assert.AreEqual(expected[i].Item2, got[i].ExtraA);
                Assert.AreEqual(expected[i].Item3, got[i].ExtraB);
            }

            Object.Destroy(renderer.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Map11_Portal_NhanCanGiuaMuiTen_CaXVaY()
        {
            // TrueType labels prioritize staying within 26 world units of their
            // arrow over keeping every glyph within the map edge.
            var renderer = MapRenderer.Create(null, 11);
            var portals = renderer.GetComponentsInChildren<MapPortalView>();
            var mapH = renderer.Map.HeightPixels;
            yield return null; // Let TextMesh build its rendered geometry.

            foreach (var portal in portals)
            {
                var pRect = portal.transform;
                var label = pRect.GetComponentInChildren<JarNameLabel>();
                Assert.IsNotNull(label, $"portal {pRect.name} phải có JarNameLabel");
                var labelWorldX = label.transform.position.x;
                var labelWorldY = label.transform.position.y;

                JarMapObject nearest = null;
                var bestD = 80 * 80;
                var entity = FindEntityByPosition(renderer.Map, pRect);
                foreach (var obj in renderer.Map.Objects)
                {
                    if (obj.ResourceIndex < 0 || obj.ResourceIndex >= renderer.Map.ResourceTypes.Length) continue;
                    if (renderer.Map.ResourceTypes[obj.ResourceIndex] != JarMapLayout.TypeAnimation) continue;
                    if (obj.Bounds == null || obj.Bounds.Length < 4) continue;
                    if (obj.Bounds[2] > 20 || obj.Bounds[3] > 30) continue;
                    var dx = obj.X - entity.X;
                    var dy = obj.Y - entity.Y;
                    var d = dx * dx + dy * dy;
                    if (d < bestD) { bestD = d; nearest = obj; }
                }
                Assert.IsNotNull(nearest, $"portal '{entity.Name}' phải có arrow anim gần trong r=80");

                var arrowCenterWorldX = nearest.X;
                var arrowCenterWorldY = mapH - (nearest.Y - nearest.YOffset - nearest.Bounds[3] * 0.5f);
                var distX = Mathf.Abs(labelWorldX - arrowCenterWorldX);
                var distY = Mathf.Abs(labelWorldY - arrowCenterWorldY);
                Assert.LessOrEqual(distY, 8f,
                    $"portal '{entity.Name}': nhãn Y={labelWorldY} cách tâm arrow Y={arrowCenterWorldY} {distY} px");

                Assert.LessOrEqual(distX, 26f,
                    $"portal '{entity.Name}': label must stay near its arrow");
                var mesh = label.GetComponentInChildren<TextMesh>();
                Assert.IsNotNull(mesh);
                Assert.AreEqual(entity.Name, mesh.text);
                Assert.AreEqual(TextAnchor.LowerCenter, mesh.anchor);
                Assert.AreEqual(TextAlignment.Center, mesh.alignment);
                Assert.IsNotNull(mesh.font);
                var rendered = mesh.GetComponent<MeshRenderer>();
                Assert.IsNotNull(rendered);
                Assert.Greater(rendered.bounds.size.x, 0f, "Label must render visible text");
                Assert.Greater(rendered.bounds.size.y, 0f, "Label must render visible text");

            }

            Object.Destroy(renderer.gameObject);
            yield return null;
        }

        private static JarMapEntity FindEntityByPosition(JarMapLayout map, Transform portalTf)
        {
            var mapH = map.HeightPixels;
            var jarX = Mathf.RoundToInt(portalTf.position.x);
            var jarY = mapH - Mathf.RoundToInt(portalTf.position.y);
            foreach (var e in map.Entities)
            {
                if (Mathf.Abs(e.X - jarX) <= 1 && Mathf.Abs(e.Y - jarY) <= 1) return e;
            }
            return null;
        }

        [UnityTest]
        public IEnumerator Map11_Portal_BamVaoChuTenMapCungSangMap()
        {
            // Trước fix: collider chỉ bao vùng arrow (Raw5 bounds), còn chữ tên map bị
            // đẩy ra xa arrow (dx tới 34px) nên nằm NGOÀI collider — bấm đúng chữ không
            // sang map được. Fix: collider gộp cả vùng arrow lẫn vùng chữ.
            var renderer = MapRenderer.Create(null, 11);
            var portals = renderer.GetComponentsInChildren<MapPortalView>();

            foreach (var portal in portals)
            {
                var label = portal.GetComponentInChildren<JarNameLabel>();
                var labelWorldPos = (Vector2)label.transform.position;
                var collider = portal.GetComponent<BoxCollider2D>();
                Assert.IsTrue(collider.OverlapPoint(labelWorldPos),
                    $"portal '{portal.name}': collider phải bao trùm vị trí chữ tên map ({labelWorldPos}) để bấm vào chữ cũng sang map được.");
            }

            Object.Destroy(renderer.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Map11_Dung7Building_BamPhatDungBuildingType()
        {
            var renderer = MapRenderer.Create(null, 11);
            var buildings = renderer.GetComponentsInChildren<MapBuildingView>();

            Assert.AreEqual(7, buildings.Length, "Map 11 phải có đúng 7 building bấm được.");

            var types = buildings.Select(b => b.Entity.BuildingType).OrderBy(t => t).ToArray();
            CollectionAssert.AreEquivalent(new[] { 9, 27, 28, 29, 30, 31, 32 }, types,
                "BuildingType phải khớp bảng decode: 4 shop (27-30) + gym (31) + heal (32) + atm (9).");

            JarMapEntity clicked = null;
            buildings[0].Selected += e => clicked = e;
            buildings[0].OnPointerClick(null);
            Assert.IsNotNull(clicked, "Bấm building phải phát sự kiện Selected.");

            Object.Destroy(renderer.gameObject);
            yield return null;
        }
    }
}
