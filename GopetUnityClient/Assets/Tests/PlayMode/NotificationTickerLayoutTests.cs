using Gopet.Runtime.UI;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Băng thông báo nằm ngay dưới thanh tài nguyên (đậu/lúa/vàng) nên phải thẳng cột
    /// với nó — hai thanh cùng một khối thông tin, lệch mép trái là thấy ngay.
    /// </summary>
    public sealed class NotificationTickerLayoutTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Ticker Root", typeof(RectTransform), typeof(Canvas));
            ((RectTransform)_root.transform).sizeDelta = new Vector2(960f, 540f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test]
        public void BangThongBao_ThangCotVoiThanhTaiNguyen()
        {
            var ticker = NotificationTicker.Create(_root.transform);
            var pill = (RectTransform)ticker.transform;
            var speaker = (RectTransform)ticker.transform.Find("Speaker");

            // Loa thò ra ngoài viên thuốc nên nó mới là mép trái mắt nhìn thấy.
            var visibleLeft = pill.offsetMin.x
                + speaker.anchoredPosition.x - speaker.sizeDelta.x * 0.5f;

            Assert.AreEqual(CurrencyBar.LeftMargin, visibleLeft, 0.01f,
                "Mép trái băng thông báo phải trùng mép trái thanh tài nguyên.");
        }

        /// <summary>
        /// Băng rộng cố định, KHÔNG co theo bề ngang màn: màn hẹp hơn 16:9 từng làm băng
        /// co tới mức vùng chữ rộng ~0px, thông báo chạy mà không thấy chữ.
        /// </summary>
        [Test]
        public void BangThongBao_RongCoDinh_VungChuLuonDuRong()
        {
            var ticker = NotificationTicker.Create(_root.transform);
            var pill = (RectTransform)ticker.transform;
            var viewport = (RectTransform)ticker.transform.Find("Text Viewport");

            Assert.AreEqual(NotificationTicker.Width, pill.rect.width, 0.01f, "Băng thông báo sai độ dài.");
            Assert.AreEqual(pill.anchorMin.x, pill.anchorMax.x, "Băng không được giãn theo bề ngang màn.");
            Assert.Greater(viewport.rect.width, 100f, "Vùng chữ quá hẹp, thông báo sẽ không thấy chữ.");
        }

        /// <summary>Băng phải nằm HẲN dưới thanh tài nguyên, không đè lên nó.</summary>
        [Test]
        public void BangThongBao_NamDuoiThanhTaiNguyen()
        {
            var ticker = NotificationTicker.Create(_root.transform);
            var pill = (RectTransform)ticker.transform;

            // offsetMax.y âm = khoảng cách từ mép trên màn xuống mép trên băng.
            Assert.Less(pill.offsetMax.y, -28f,
                "Băng thông báo chồm lên thanh tài nguyên (cao 28px tính cả lề).");
        }
    }
}
