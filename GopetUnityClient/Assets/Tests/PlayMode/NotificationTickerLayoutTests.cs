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

        /// <summary>Băng dài 3/4 so với bản cũ (348px ở khung chuẩn 960) — 261px.</summary>
        [Test]
        public void BangThongBao_DaiBangBaPhanTuBanCu()
        {
            var pill = (RectTransform)NotificationTicker.Create(_root.transform).transform;

            var width = 960f + pill.offsetMax.x - pill.offsetMin.x;
            Assert.AreEqual(348f * 0.75f, width, 1f, "Băng thông báo sai độ dài.");
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
