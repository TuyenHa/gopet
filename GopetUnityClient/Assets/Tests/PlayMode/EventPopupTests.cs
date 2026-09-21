using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Popup Sự kiện có hai tab: Điểm danh và Quà tặng. Đổi tab phải đổi trang, và gửi
    /// mã quà tặng phải ra đúng gói input của server.
    /// </summary>
    public sealed class EventPopupTests
    {
        private GameObject _host;
        private List<Message> _sent;
        private DailyCheckinView _view;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("UiHost", typeof(RectTransform));
            _sent = new List<Message>();
            var guider = new GuiderHandler(_sent.Add);
            _view = DailyCheckinView.Create(_host.transform, UiBuilder.BuiltinFont(), guider, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        private Transform Page(string name) => _view.transform.Find("Content/" + name);
        private PopupTabRail Rail => _view.GetComponentInChildren<PopupTabRail>();
        private Text FooterLabel => _view.transform.Find("Footer/Label").GetComponent<Text>();

        [Test]
        public void MoRa_NamODiemDanh()
        {
            Assert.AreEqual(2, Rail.TabCount);
            Assert.AreEqual(0, Rail.ActiveIndex);
            Assert.AreEqual("Điểm danh mỗi ngày để nhận quà", FooterLabel.text);
            Assert.IsTrue(Page("Page_Checkin").gameObject.activeSelf);
            Assert.IsFalse(Page("Page_Gift").gameObject.activeSelf);
        }

        [Test]
        public void ChonQuaTang_DoiSangTrangNhapMa()
        {
            Rail.Select(1);

            Assert.IsFalse(Page("Page_Checkin").gameObject.activeSelf,
                "lưới điểm danh phải ẩn, không nằm chồng dưới form");
            Assert.IsTrue(Page("Page_Gift").gameObject.activeSelf);
            Assert.IsNotNull(_view.GetComponentInChildren<PopupInputForm>(true));
            Assert.AreEqual("Có GiftCode là có quà!", FooterLabel.text,
                "băng chân phải nói về tab đang xem");
        }

        [Test]
        public void GuiMaRong_KhongGuiGoiNao()
        {
            Rail.Select(1);
            var form = _view.GetComponentInChildren<PopupInputForm>(true);

            var sent = 0;
            form.Submitted += _ => sent++;
            form.GetComponentInChildren<Button>().onClick.Invoke();

            Assert.AreEqual(0, sent, "mã rỗng thì báo tại chỗ chứ không gửi lên server");
            Assert.AreEqual(0, _sent.Count);
        }

        [Test]
        public void HuyBoQuayVeTabDiemDanh()
        {
            Rail.Select(1);
            var form = _view.GetComponentInChildren<PopupInputForm>(true);

            form.GetComponentsInChildren<Button>()[1].onClick.Invoke();

            Assert.AreEqual(0, Rail.ActiveIndex, "Huỷ chỉ quay lại tab điểm danh, không đóng popup");
            Assert.IsTrue(Page("Page_Checkin").gameObject.activeSelf);
        }
    }
}
