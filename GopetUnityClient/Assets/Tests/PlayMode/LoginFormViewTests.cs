using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Màn đăng nhập theo bộ art của dự án. Chạy trên sprite THẬT trong
    /// <c>Resources/Ui/Login</c> — thiếu file thì view vẫn phải dựng được (lùi về màu
    /// phẳng), nên test không phụ thuộc việc art đã có hay chưa.
    /// </summary>
    public sealed class LoginFormViewTests
    {
        private GameObject _root;
        private GameObject _eventSystem;
        private SoundManager _sound;
        private LoginFormView _view;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));

            _sound = SoundManager.Create(_root.transform);
            _view = LoginFormView.Create(_root.transform, null, _sound);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
            PlayerPrefs.DeleteKey("Gopet.SoundEnabled");
        }

        [Test]
        public void SetCredentials_DienDungVaoOTuongUng()
        {
            _view.SetCredentials("gopettest", "abc12345");

            Assert.AreEqual("gopettest", _view.Username);
            Assert.AreEqual("abc12345", _view.Password);
        }

        [Test]
        public void PanelGon_LoGoKhongVuotKhoiDinhManHinh()
        {
            var panel = (RectTransform)_view.transform.Find("Panel");
            var logo = (RectTransform)_view.transform.Find("Panel/Logo");
            var panelHeight = panel.anchorMax.y - panel.anchorMin.y;
            var logoTopOnScreen = panel.anchorMin.y + panelHeight * logo.anchorMax.y;

            Assert.LessOrEqual(panelHeight, 0.65f, "Panel login vẫn còn quá cao.");
            Assert.LessOrEqual(logoTopOnScreen, 1f,
                "Logo nhô khỏi mép trên màn hình và sẽ bị cắt mất.");
        }

        /// <summary>Ô mật khẩu phải bị che sẵn — bẫy đã trả giá ở P1/P5 nếu quên.</summary>
        [Test]
        public void OMatKhau_MacDinhBiChe()
        {
            var fields = _view.GetComponentsInChildren<InputField>(true);

            Assert.AreEqual(2, fields.Length);
            Assert.AreEqual(InputField.ContentType.Password, fields[1].contentType);
        }

        /// <summary>
        /// Nút con mắt mở/che lại được. Có ích thật: bộ gõ Telex nuốt phím trong ô
        /// nhập, mà ô bị che thì không nhìn ra mình gõ hỏng chỗ nào.
        /// </summary>
        [Test]
        public void BamConMat_HienRoiCheLaiMatKhau()
        {
            var password = _view.GetComponentsInChildren<InputField>(true)[1];
            var eye = FindByName<Button>("ToggleReveal");
            Assert.IsNotNull(eye, "Không tìm thấy nút con mắt.");

            eye.onClick.Invoke();
            Assert.AreEqual(InputField.ContentType.Standard, password.contentType);

            eye.onClick.Invoke();
            Assert.AreEqual(InputField.ContentType.Password, password.contentType);
        }

        /// <summary>
        /// Nền ô nhập phải là sprite 9-slice CÓ VIỀN. Viền bằng 0 thì uGUI vẽ nó như
        /// ảnh thường: sprite rộng ~28px bị kéo ra hơn 300px, bo góc dẹt theo chiều
        /// ngang tới mức nhìn hệt như góc vuông.
        /// </summary>
        [Test]
        public void ONhap_DungSpriteBoGoc9Slice()
        {
            foreach (var field in _view.GetComponentsInChildren<InputField>(true))
            {
                var background = field.GetComponent<Image>();
                if (background.sprite == null) continue; // chưa có bộ art, lùi về màu phẳng

                Assert.AreEqual(Image.Type.Sliced, background.type, $"{field.name}: nền không ở chế độ 9-slice.");
                Assert.AreNotEqual(Vector4.zero, background.sprite.border,
                    $"{field.name}: sprite không có viền 9-slice — bo góc sẽ bị kéo dẹt thành góc vuông.");
            }
        }

        [Test]
        public void MacDinh_GhiNhoBat()
        {
            Assert.IsTrue(_view.Remember, "Trước khi có ô này hành vi là LUÔN nhớ — mặc định phải giữ nguyên.");
        }

        [Test]
        public void TatGhiNho_RememberBaoFalse()
        {
            _view.GetComponentInChildren<Toggle>(true).isOn = false;

            Assert.IsFalse(_view.Remember);
        }

        [Test]
        public void BamDangNhap_BanSubmitDungThamSo()
        {
            _view.SetCredentials("gopettest", "abc12345");

            string username = null, password = null;
            var registerFired = false;
            _view.SubmitRequested += (u, p) => { username = u; password = p; };
            _view.RegisterRequested += (_, __) => registerFired = true;

            FindByName<Button>("Button_DangNhap").onClick.Invoke();

            Assert.AreEqual("gopettest", username);
            Assert.AreEqual("abc12345", password);
            Assert.IsFalse(registerFired, "Bấm 'Đăng nhập' không được bắn luôn RegisterRequested.");
        }

        [Test]
        public void BamTaoTaiKhoan_BanRegisterDungThamSo()
        {
            _view.SetCredentials("newuser123", "pass1234");

            string username = null, password = null;
            var submitFired = false;
            _view.RegisterRequested += (u, p) => { username = u; password = p; };
            _view.SubmitRequested += (_, __) => submitFired = true;

            FindByName<Button>("Button_TaoTaiKhoan").onClick.Invoke();

            Assert.AreEqual("newuser123", username);
            Assert.AreEqual("pass1234", password);
            Assert.IsFalse(submitFired, "Bấm 'Tạo tài khoản' không được bắn luôn SubmitRequested.");
        }

        [Test]
        public void BamNut_PhatTiengBamNut()
        {
            FindByName<Button>("Button_DangNhap").onClick.Invoke();

            AudioSource effects = null;
            foreach (var source in _sound.GetComponents<AudioSource>())
            {
                if (!source.loop) effects = source;
            }

            Assert.IsNotNull(effects);
            Assert.IsTrue(effects.isPlaying);
        }

        [Test]
        public void SetNotice_HienDuocCauCuaServer()
        {
            _view.SetNotice("Sai mật khẩu");

            Assert.AreEqual("Sai mật khẩu", _view.NoticeText);
        }

        [Test]
        public void Notice_NamDuoiMatKhau_VaTuDongXuongDong()
        {
            var password = FindByName<InputField>("Field_MatKhau").GetComponent<RectTransform>();
            var notice = FindByName<Text>("Notice");

            Assert.LessOrEqual(notice.rectTransform.anchorMax.y, password.anchorMin.y,
                "Thông báo lỗi phải nằm hoàn toàn bên dưới ô mật khẩu.");
            Assert.AreEqual(HorizontalWrapMode.Wrap, notice.horizontalOverflow);
        }

        [Test]
        public void Notice_ChiChenVaoGiuaKhiCoLoi()
        {
            var password = FindByName<InputField>("Field_MatKhau").GetComponent<RectTransform>();
            var notice = FindByName<Text>("Notice");
            var remember = FindByName<Toggle>("Remember").GetComponent<RectTransform>();
            var submit = FindByName<Button>("Button_DangNhap").GetComponent<RectTransform>();

            _view.SetNotice(null);
            var compactRememberTop = remember.anchorMax.y;
            Assert.IsFalse(notice.gameObject.activeSelf);
            Assert.That(password.anchorMin.y - compactRememberTop, Is.InRange(0f, 0.03f),
                "Khi không có lỗi, hàng ghi nhớ phải nằm sát ô mật khẩu.");
            Assert.That(remember.anchorMin.y - submit.anchorMax.y, Is.InRange(0f, 0.05f),
                "Khi không có lỗi, hàng nút phải nằm sát hàng ghi nhớ.");

            _view.SetNotice("Tên đăng nhập hoặc mật khẩu không chính xác, vui lòng kiểm tra lại.");
            Assert.IsTrue(notice.gameObject.activeSelf);
            Assert.Less(remember.anchorMax.y, compactRememberTop,
                "Khi có lỗi, hàng ghi nhớ phải nhường chỗ cho thông báo.");
            Assert.LessOrEqual(remember.anchorMax.y, notice.rectTransform.anchorMin.y,
                "Thông báo lỗi phải được chèn giữa mật khẩu và hàng ghi nhớ.");
            Assert.That(remember.anchorMin.y - submit.anchorMax.y, Is.InRange(0f, 0.05f),
                "Khi có lỗi, hàng nút phải dịch xuống cùng hàng ghi nhớ.");
        }

        private T FindByName<T>(string name) where T : Component
        {
            foreach (var item in _view.GetComponentsInChildren<T>(true))
            {
                if (item.name == name) return item;
            }

            return null;
        }
    }
}
