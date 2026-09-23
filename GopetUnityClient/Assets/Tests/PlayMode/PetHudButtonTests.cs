using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nút Pet: icon đầu cún (HUD skin) nằm NGAY DƯỚI minimap, thẳng cột với nó; bấm vẫn
    /// bắn <see cref="PetActionButton.Clicked"/> để mở popup pet như cũ.
    /// </summary>
    public sealed class PetHudButtonTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Pet HUD Root", typeof(RectTransform), typeof(Canvas));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [UnityTest]
        public IEnumerator NutPet_NamDuoiMinimap_CungCot()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var pet = PetActionButton.Create(_root.transform);
            yield return null;

            var map = WorldRect((RectTransform)minimap.transform);
            var button = WorldRect((RectTransform)pet.transform);
            Assert.LessOrEqual(button.yMax, map.yMin, "Nút Pet phải nằm DƯỚI minimap, không đè lên.");
            Assert.AreEqual(map.xMax, button.xMax, 0.5f, "Nút Pet phải thẳng mép phải với minimap.");
            Assert.AreEqual(map.width, button.width, 0.5f, "Nút Pet phải rộng bằng minimap.");
        }

        [Test]
        public void NutPet_DungIconPet_BamMoPopup()
        {
            var pet = PetActionButton.Create(_root.transform);
            var clicks = 0;
            pet.Clicked += () => clicks++;

            Assert.AreSame(HudSkin.Get(HudSkin.Pet), pet.GetComponent<Image>().sprite, "Thiếu Resources/Ui/Hud/pet.png.");
            pet.GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(1, clicks);
        }

        private static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }
    }
}
