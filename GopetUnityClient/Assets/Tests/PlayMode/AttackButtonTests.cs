using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nút đánh quái: có icon hai kiếm, mờ đi khi không có quái trong tầm, và vẫn bấm
    /// được ở trạng thái mờ để còn báo cho người chơi biết vì sao chưa đánh được.
    /// </summary>
    public sealed class AttackButtonTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Attack Button Root", typeof(RectTransform), typeof(Canvas));
            ((RectTransform)_root.transform).sizeDelta = new Vector2(960f, 540f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test]
        public void NutDanh_DungIconHaiKiem()
        {
            var button = AttackButton.Create(_root.transform);

            var icon = Resources.Load<Sprite>(AttackButton.IconResource);
            Assert.IsNotNull(icon, $"Thiếu icon Resources/{AttackButton.IconResource}.");
            Assert.AreEqual(icon, button.GetComponent<Image>().sprite);
        }

        /// <summary>Không có quái mà nút vẫn sáng thì bấm hoài không thấy gì xảy ra.</summary>
        [Test]
        public void NutDanh_KhongCoQuaiTrongTam_MoDi()
        {
            var button = AttackButton.Create(_root.transform);
            var image = button.GetComponent<Image>();

            Assert.Less(image.color.a, 0.5f, "Mặc định chưa có mục tiêu thì nút phải mờ.");

            button.SetTargetInRange(true);
            Assert.AreEqual(1f, image.color.a, 0.001f, "Có quái trong tầm thì nút phải sáng.");

            button.SetTargetInRange(false);
            Assert.Less(image.color.a, 0.5f, "Quái đi khỏi tầm thì nút mờ lại.");
        }

        /// <summary>Nút mờ vẫn phải bắn sự kiện — session dùng nó để báo "không có quái nào ở gần".</summary>
        [Test]
        public void NutDanh_DangMo_VanBamDuoc()
        {
            var button = AttackButton.Create(_root.transform);
            var clicks = 0;
            button.Clicked += () => clicks++;

            button.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, clicks);
        }

        /// <summary>Nút nằm góc dưới-phải, cách lề phải đúng 10px.</summary>
        [Test]
        public void NutDanh_SatLePhai10px()
        {
            var rect = (RectTransform)AttackButton.Create(_root.transform).transform;

            Assert.AreEqual(new Vector2(1f, 0f), rect.anchorMin, "Phải neo góc dưới-phải.");
            var rightEdge = -rect.anchoredPosition.x - rect.sizeDelta.x * 0.5f;
            Assert.AreEqual(10f, rightEdge, 0.001f, "Nút đánh phải cách lề phải 10px.");
        }

        /// <summary>Nút đánh xấp xỉ cần điều khiển — hai nút ngón cái hai bên màn.</summary>
        [Test]
        public void NutDanh_ToBangCanDieuKhien()
        {
            var rect = (RectTransform)AttackButton.Create(_root.transform).transform;

            Assert.AreEqual(new Vector2(120f, 120f), rect.sizeDelta);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), rect.pivot, "Pivot lệch thì cú nảy nở lệch.");

            // Đáy nút phải ngang đáy cần điều khiển, nếu không hai nút ngón cái lệch hàng.
            var bottomEdge = rect.anchoredPosition.y - rect.sizeDelta.y * 0.5f;
            Assert.AreEqual(Gopet.Runtime.World.VirtualJoystick.ScreenMargin, bottomEdge, 0.01f);
        }

        /// <summary>Giữ nút là nó nổi lên, nhả ra thì xẹp về cỡ cũ.</summary>
        [UnityTest]
        public IEnumerator NutDanh_AnVao_NoiLenRoiXep()
        {
            var button = AttackButton.Create(_root.transform);
            var pointer = new PointerEventData(EventSystem.current);

            button.OnPointerDown(pointer);
            yield return null;
            yield return null;
            Assert.Greater(button.transform.localScale.x, 1f, "Giữ nút thì nút phải nổi lên.");

            button.OnPointerUp(pointer);
            for (var i = 0; i < 60 && button.transform.localScale.x > 1.001f; i++) yield return null;
            Assert.AreEqual(1f, button.transform.localScale.x, 0.01f, "Nhả nút thì phải xẹp về cỡ cũ.");
        }
    }
}
