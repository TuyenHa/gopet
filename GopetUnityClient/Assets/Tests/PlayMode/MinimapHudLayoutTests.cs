using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Minimap là widget MỚI trên HUD nên dễ đè lên thứ đã có. Test này chặn lỗi đó tái
    /// diễn: minimap không được giao với hàng icon Cửa hàng/Dịch vụ/Sự kiện (góc
    /// phải-trên).
    /// </summary>
    public sealed class MinimapHudLayoutTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            // Canvas HUD thật: ref 960×540, match theo CHIỀU CAO (GameSession.CreateHudOverlayCanvas).
            _root = new GameObject("Hud Layout Root", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler));
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 1f;
            var rect = (RectTransform)_root.transform;
            rect.sizeDelta = new Vector2(960f, 540f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator Minimap_KhongDeLenIconHudKhac()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var topRow = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            var minimapRect = WorldRect((RectTransform)minimap.transform);
            foreach (var icon in topRow.GetComponentsInChildren<Button>(true))
            {
                Assert.IsFalse(minimapRect.Overlaps(WorldRect((RectTransform)icon.transform)),
                    $"Minimap đè lên icon HUD '{icon.name}'.");
            }
        }

        /// <summary>
        /// Minimap là ô ngoài cùng bên phải của hàng HUD: cùng đáy, nhô cao hơn hàng nút
        /// (sát lề trên), nằm bên phải mọi nút. Mốc canh hàng vẫn đo theo "Sự kiện" vì nhãn của nó là
        /// mốc chiều cao chung của cả hàng.
        /// </summary>
        [UnityTest]
        public IEnumerator Minimap_CungHangVaBenPhaiSuKien()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var topRow = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            var eventIcon = topRow.transform.Find($"Hud_{HudSkin.Event}");
            var map = WorldRect((RectTransform)minimap.transform);
            var events = WorldRect((RectTransform)eventIcon);
            var label = WorldRect((RectTransform)eventIcon.Find("Label"));

            Assert.Greater(map.yMax, events.yMax, "Minimap phải nhô sát lề trên hơn icon HUD.");
            Assert.AreEqual(label.yMin, map.yMin, 0.5f,
                "Đáy minimap phải ngang đáy chữ 'Sự kiện'.");
            Assert.GreaterOrEqual(map.xMin, events.xMax,
                "Minimap phải nằm BÊN PHẢI 'Sự kiện'.");
        }

        /// <summary>
        /// Hộp thư chèn vào GIỮA "Sự kiện" và minimap — không đè lên cái nào, không rơi
        /// ra ngoài mép phải màn hình.
        /// </summary>
        [UnityTest]
        public IEnumerator HopThu_NamGiuaSuKienVaMinimap()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var topRow = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            var mail = WorldRect((RectTransform)topRow.transform.Find($"Hud_{HudSkin.Mail}"));
            var events = WorldRect((RectTransform)topRow.transform.Find($"Hud_{HudSkin.Event}"));
            var map = WorldRect((RectTransform)minimap.transform);

            Assert.GreaterOrEqual(mail.xMin, events.xMax, "Hộp thư phải nằm BÊN PHẢI 'Sự kiện'.");
            Assert.LessOrEqual(mail.xMax, map.xMin, "Hộp thư phải nằm BÊN TRÁI minimap.");
            Assert.AreEqual(events.yMax, mail.yMax, 0.5f, "Hộp thư lệch hàng so với 'Sự kiện'.");
            Assert.AreEqual(events.height, mail.height, 0.5f, "Hộp thư khác cỡ các nút còn lại.");
        }

        /// <summary>
        /// Chưa có map thì ruột phải TỐI, không phải mảng trắng: RawImage không texture
        /// được Unity vẽ trắng tinh, nhìn y như widget hỏng (đã xảy ra một lần).
        /// </summary>
        [Test]
        public void Minimap_ChuaCoMap_RuotToiChuKhongTrang()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var map = minimap.transform.Find("Map").GetComponent<RawImage>();

            Assert.IsNull(map.texture, "Chưa nạp map thì không có texture.");
            Assert.Less(map.color.grayscale, 0.3f, "Ruột minimap đang trắng.");
        }

        /// <summary>Viền mảnh và xanh dương — dày quá thì ăn mất phần map vốn đã bé.</summary>
        [Test]
        public void Minimap_VienManhMauXanhDuong()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var border = minimap.GetComponent<Image>().color;
            var viewport = (RectTransform)minimap.transform.Find("Map");

            Assert.Greater(border.b, border.r + 0.3f, "Viền phải là xanh dương.");
            Assert.Greater(border.b, border.g + 0.3f, "Viền phải là xanh dương.");
            Assert.AreEqual(2f, viewport.offsetMin.x, 0.01f, "Viền dày hơn 2px.");

            // Ảnh map không bao giờ phủ kín ô (khác tỉ lệ), phần thừa phải là ruột tối
            // chứ không phải màu viền — không thì trông như viền trên/dưới dày cộp.
            var inner = (RectTransform)minimap.transform.Find("Inner");
            Assert.IsNotNull(inner, "Thiếu lớp ruột tối lót trong viền.");
            Assert.AreEqual(2f, inner.offsetMin.y, 0.01f, "Ruột tối không lót sát viền.");
            Assert.Less(inner.GetComponent<Image>().color.grayscale, 0.3f, "Ruột phải tối.");
            Assert.Less(inner.GetSiblingIndex(), minimap.transform.Find("Map").GetSiblingIndex(),
                "Ruột phải nằm DƯỚI ảnh map.");
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y,
                corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }
    }
}
