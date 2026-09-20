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
    /// Trạng thái chờ server của màn đăng nhập / đăng ký: vòng xoay giữa màn hình và
    /// khoá thao tác để không gửi gói hai lần.
    /// </summary>
    public sealed class LoginFormBusyTests
    {
        private GameObject _root;
        private GameObject _eventSystem;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
        }

        private static LoadingSpinner SpinnerOf(LoginFormView view) =>
            view.GetComponentInChildren<LoadingSpinner>(true);

        [Test]
        public void SetBusy_BatVongXoayVaKhoaThaoTac()
        {
            var view = LoginFormView.Create(_root.transform, null);

            view.SetBusy(true);

            Assert.IsTrue(view.IsBusy, "SetBusy(true) phải đặt IsBusy.");
            var spinner = SpinnerOf(view);
            Assert.IsNotNull(spinner, "Bấm đăng nhập xong phải có vòng xoay báo đang chờ.");
            Assert.IsFalse(view.transform.Find("Panel/Content/Button_DangNhap")
                .GetComponent<Button>().interactable, "Đang chờ server thì nút phải khoá.");
        }

        /// <summary>Vòng phải nằm GIỮA MÀN HÌNH: con trực tiếp của gốc view (đã stretch
        /// kín màn), neo và pivot đều ở tâm, không lệch khỏi tâm.</summary>
        [Test]
        public void VongXoay_NamGiuaManHinh()
        {
            var view = LoginFormView.Create(_root.transform, null);

            view.SetBusy(true);

            var spinner = (RectTransform)SpinnerOf(view).transform;
            Assert.AreSame(view.transform, spinner.parent,
                "Gắn vào panel hay vào nút thì không còn là giữa màn hình.");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), spinner.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), spinner.anchorMax);
            Assert.AreEqual(Vector2.zero, spinner.anchoredPosition);
        }

        /// <summary>Vòng là con THÊM SAU CÙNG nên vẽ đè lên panel, không bị panel che.</summary>
        [Test]
        public void VongXoay_VeDeLenPanel()
        {
            var view = LoginFormView.Create(_root.transform, null);

            view.SetBusy(true);

            var spinner = SpinnerOf(view).transform;
            Assert.AreEqual(view.transform.childCount - 1, spinner.GetSiblingIndex());
        }

        [Test]
        public void SetBusy_TatDiThiGoVongVaMoKhoa()
        {
            var view = LoginFormView.Create(_root.transform, null);
            view.SetBusy(true);

            view.SetBusy(false);

            Assert.IsFalse(view.IsBusy);
            Assert.IsTrue(view.transform.Find("Panel/Content/Button_DangNhap")
                .GetComponent<Button>().interactable, "Server trả lời xong phải mở khoá nút.");
        }

        /// <summary>Gọi hai lần liên tiếp không được đẻ ra hai cái vòng chồng nhau.</summary>
        [Test]
        public void SetBusy_GoiLaiKhongTaoThemVong()
        {
            var view = LoginFormView.Create(_root.transform, null);

            view.SetBusy(true);
            view.SetBusy(true);

            Assert.AreEqual(1, view.GetComponentsInChildren<LoadingSpinner>(true).Length);
        }

        /// <summary>Lý do chính của cả tính năng: chặn bấm gửi gói lần hai khi đang chờ.</summary>
        [Test]
        public void DangCho_BamTiepKhongBanThemSuKien()
        {
            var view = LoginFormView.Create(_root.transform, null);
            var fired = 0;
            view.SubmitRequested += (_, __) => fired++;

            view.SetBusy(true);
            view.SubmitDefault();

            Assert.AreEqual(0, fired, "Đang chờ server mà vẫn gửi thêm LOGIN.");
        }

        /// <summary>Form đăng ký cũng phải có vòng, cũng ở giữa màn.</summary>
        [Test]
        public void FormDangKy_CungCoVongXoay()
        {
            var view = LoginFormView.CreateRegistration(_root.transform, null);

            view.SetBusy(true);

            Assert.IsNotNull(SpinnerOf(view));
            Assert.AreSame(view.transform, SpinnerOf(view).transform.parent);
            Assert.IsFalse(view.transform.Find("Panel/Content/Button_DangKy")
                .GetComponent<Button>().interactable, "Đang chờ server thì nút phải khoá.");
        }

        [UnityTest]
        public IEnumerator VongXoay_ThucSuQuay()
        {
            var view = LoginFormView.Create(_root.transform, null);
            view.SetBusy(true);
            var spinner = (RectTransform)SpinnerOf(view).transform;
            var before = spinner.localRotation;

            yield return null;
            yield return null;

            Assert.AreNotEqual(before.eulerAngles.z, spinner.localRotation.eulerAngles.z,
                "Vòng đứng im thì không phải hiệu ứng loading.");
        }
    }
}
