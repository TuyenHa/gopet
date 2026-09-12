using System.Collections;
using Gopet.Net.Map;
using Gopet.Runtime.UI;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class MinimapPickerTests
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
        public IEnumerator Minimap_BakeMapThat_VaDotTheoNhanVat()
        {
            var widget = MinimapWidget.Create(_root.transform, null);
            var map = JarMaps.Load(11);
            widget.SetMap(map, 11);
            var image = widget.GetComponentInChildren<RawImage>(true);
            Assert.IsNotNull(image.texture, "Minimap không bake được ảnh map thật.");

            var avatar = PlayerAvatar.Spawn(_root.transform, 1, "Self", 0, 144, 120, map.HeightPixels);
            widget.BindPlayer(avatar);
            yield return null;

            var dot = widget.transform.Find("Map/PlayerDot") as RectTransform;
            Assert.IsTrue(dot.gameObject.activeSelf);
            Assert.AreEqual(144f / map.WidthPixels, dot.anchorMin.x, 0.002f);
            Assert.AreEqual(1f - 120f / map.HeightPixels, dot.anchorMin.y, 0.002f);
        }

        [Test]
        public void Picker_ChonMotLan_DungOptionServer()
        {
            var picker = MapPickerView.Create(_root.transform, null);
            var first = new MapTeleportOption { MapId = 11, Name = "Làng", WaypointIndex = 2 };
            var second = new MapTeleportOption { MapId = 12, Name = "Rừng", WaypointIndex = 4 };
            picker.Bind("Chọn bản đồ", new[] { first.Name, second.Name });
            var chosen = -1;
            picker.Chosen += index => chosen = index;

            FindButton(picker, "Rừng").onClick.Invoke();
            FindButton(picker, "Làng").onClick.Invoke();

            Assert.AreEqual(1, chosen, "Picker đổi lựa chọn sau lần chạm đầu tiên.");
        }

        [Test]
        public void Picker_NutDong_PhatSuKienDong()
        {
            var picker = MapPickerView.Create(_root.transform, null);
            picker.Bind("Chọn bản đồ", new string[0]);
            var closed = false;
            picker.CloseRequested += () => closed = true;

            FindButton(picker, "Đóng").onClick.Invoke();

            Assert.IsTrue(closed);
        }

        private static Button FindButton(MapPickerView picker, string label)
        {
            foreach (var button in picker.GetComponentsInChildren<Button>(true))
            {
                var text = button.GetComponentInChildren<Text>(true);
                if (text != null && text.text == label) return button;
            }
            Assert.Fail($"Không tìm thấy nút '{label}'.");
            return null;
        }
    }
}
