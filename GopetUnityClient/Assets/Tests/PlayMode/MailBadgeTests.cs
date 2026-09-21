using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Huy hiệu số thư chưa đọc trên icon hộp thư của HUD: đúng số, đúng góc, và KHÔNG
    /// văng ra giữa khe giữa hai nút — khung nút neo theo tỉ lệ nên nó rộng gần gấp đôi
    /// ô vuông mà ảnh icon thật sự chiếm (xem <see cref="UnreadCountBadge"/>).
    /// </summary>
    public sealed class MailBadgeTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Hud Root", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler));
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 1f;
            ((RectTransform)_root.transform).sizeDelta = new Vector2(960f, 540f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        private static UnreadCountBadge Badge(ShopServiceEventHud hud) =>
            hud.GetComponentInChildren<UnreadCountBadge>(true);

        [UnityTest]
        public IEnumerator KhongCoThu_AnHuyHieu()
        {
            var hud = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            hud.SetMailCount(0);
            Assert.IsFalse(Badge(hud).gameObject.activeSelf, "Không thư mà vẫn hiện huy hiệu.");
        }

        [UnityTest]
        public IEnumerator CoThu_HienDungSo()
        {
            var hud = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            hud.SetMailCount(2);
            var badge = Badge(hud);
            Assert.IsTrue(badge.gameObject.activeSelf, "Có thư mà không hiện huy hiệu.");
            Assert.AreEqual("2", badge.GetComponentInChildren<Text>(true).text);
        }

        /// <summary>Số ba chữ số không đọc nổi ở cỡ huy hiệu nên chặn ở "99+".</summary>
        [UnityTest]
        public IEnumerator QuaMotTram_Hien99Cong()
        {
            var hud = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            hud.SetMailCount(128);
            Assert.AreEqual("99+", Badge(hud).GetComponentInChildren<Text>(true).text);
        }

        /// <summary>
        /// Huy hiệu phải bám góc trên-phải của Ô VUÔNG ảnh icon, không phải góc khung nút.
        /// Neo nhầm vào khung là nó trôi sang khe bên cạnh, chờm lên minimap.
        /// </summary>
        [UnityTest]
        public IEnumerator HuyHieu_BamGocIcon_KhongTroiSangMinimap()
        {
            var minimap = MinimapWidget.Create(_root.transform, null);
            var hud = ShopServiceEventHud.Create(_root.transform, null);
            yield return null;

            hud.SetMailCount(3);
            var mailRect = (RectTransform)hud.transform.Find($"Hud_{HudSkin.Mail}");
            var badgeRect = (RectTransform)Badge(hud).transform;
            var mapRect = (RectTransform)minimap.transform;

            var mail = WorldRect(mailRect);
            var badge = WorldRect(badgeRect);
            var map = WorldRect(mapRect);

            // Ô vuông ảnh icon = cạnh bằng CHIỀU CAO khung (khung rộng hơn cao ở 16:9).
            var iconRight = mail.center.x + mail.height * 0.5f;
            Assert.Less(badge.center.x, iconRight + badge.width,
                "Huy hiệu trôi ra ngoài ô vuông icon — chắc đang neo vào khung nút.");
            Assert.Greater(badge.center.y, mail.center.y, "Huy hiệu phải ở NỬA TRÊN icon.");
            Assert.Greater(badge.center.x, mail.center.x, "Huy hiệu phải ở NỬA PHẢI icon.");
            Assert.IsFalse(map.Overlaps(badge), "Huy hiệu đè lên minimap.");
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
