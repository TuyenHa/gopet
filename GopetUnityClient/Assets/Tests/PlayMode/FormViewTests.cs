using NUnit.Framework;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Biểu mẫu do client dựng (đăng nhập, tạo nhân vật, báo mất kết nối).
    ///
    /// <para>Kiểm cả CHIỀU RỘNG THẬT và đường con trỏ thật, không chỉ gọi
    /// <c>Press</c>: một hàng rộng 0 pixel vẫn hiện chữ và vẫn cho test gọi thẳng
    /// xanh, nhưng trên máy thật thì không bấm được. Đã trả giá đúng lỗi này ở
    /// <c>MenuItemRow</c>.</para>
    /// </summary>
    public sealed class FormViewTests
    {
        private GameObject _canvas;
        private GameObject _eventSystem;
        private FormView _form;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            ((RectTransform)_canvas.transform).sizeDelta = new Vector2(800f, 600f);
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));

            _form = FormView.Create(_canvas.transform, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
        }

        [Test]
        public void DungDungSoOVaSoNut()
        {
            _form.Bind("Đăng nhập", new[] { "Tài khoản", "Mật khẩu" }, new[] { "Đăng nhập", "Quên" });

            Assert.AreEqual(2, _form.Fields.Count);
            Assert.AreEqual(2, _form.Buttons.Count);
            Assert.AreEqual("Đăng nhập", _form.Title);
        }

        /// <summary>Hàng phải có chiều rộng thật, nếu không thì không bấm vào đâu được.</summary>
        [Test]
        public void MoiHang_CoChieuRongThat()
        {
            _form.Bind("Đăng nhập", new[] { "Tài khoản" }, new[] { "OK" });
            Canvas.ForceUpdateCanvases();

            foreach (var button in _form.Buttons)
            {
                var rect = (RectTransform)button.transform;
                Assert.Greater(rect.rect.width, 50f, "Nút rộng gần 0 — neo sai.");
                Assert.Greater(rect.rect.height, 10f);
            }

            foreach (var field in _form.Fields)
            {
                Assert.Greater(((RectTransform)field.transform).rect.width, 50f, "Ô nhập rộng gần 0 — neo sai.");
            }
        }

        /// <summary>Hai hàng liền nhau không được chồng lên nhau.</summary>
        [Test]
        public void CacHang_KhongChongLenNhau()
        {
            _form.Bind("Tiêu đề", new[] { "A", "B" }, new[] { "OK" });
            Canvas.ForceUpdateCanvases();

            var first = ((RectTransform)_form.Fields[0].transform).anchoredPosition.y;
            var second = ((RectTransform)_form.Fields[1].transform).anchoredPosition.y;

            Assert.Less(second, first - FormView.RowHeight + 1f, "Hàng thứ hai đè lên hàng đầu.");
        }

        [Test]
        public void BamQuaConTro_BanSuKien()
        {
            _form.Bind("Tiêu đề", new string[0], new[] { "Một", "Hai" });

            var pressed = -1;
            _form.Pressed += i => pressed = i;

            Click(_form.Buttons[1]);

            Assert.AreEqual(1, pressed);
        }

        [Test]
        public void Enter_BamNutMacDinh()
        {
            _form.Bind("Tiêu đề", new string[0], new[] { "Một", "Hai" });
            _form.DefaultButton = 1;

            var pressed = -1;
            _form.Pressed += i => pressed = i;

            _form.SubmitDefault();

            Assert.AreEqual(1, pressed);
        }

        /// <summary>Không đặt nút mặc định thì Enter không được tự đoán.</summary>
        [Test]
        public void Enter_KhongCoNutMacDinh_ThiKhongLamGi()
        {
            _form.Bind("Tiêu đề", new string[0], new[] { "Một" });

            var pressed = -1;
            _form.Pressed += i => pressed = i;

            _form.SubmitDefault();

            Assert.AreEqual(-1, pressed);
        }

        [Test]
        public void NutBiVoHieu_KhongBanSuKien()
        {
            _form.Bind("Tiêu đề", new string[0], new[] { "Một" });
            _form.Buttons[0].interactable = false;

            var pressed = -1;
            _form.Pressed += i => pressed = i;

            _form.Press(0);
            Click(_form.Buttons[0]);

            Assert.AreEqual(-1, pressed);
        }

        [Test]
        public void OMatKhau_BiChe()
        {
            _form.Bind("Đăng nhập", new[] { "Tài khoản", "Mật khẩu" }, new[] { "OK" });
            _form.SetSecret(1);

            Assert.AreEqual(InputField.ContentType.Standard, _form.Fields[0].contentType);
            Assert.AreEqual(InputField.ContentType.Password, _form.Fields[1].contentType);
        }

        /// <summary>
        /// Đổi biểu mẫu phải thu hồi hàng cũ NGAY, không đợi cuối frame: đếm số phần
        /// tử trong danh sách thì luôn đúng dù có huỷ hay không, nên ca này phải nhìn
        /// vào chính GameObject cũ.
        /// </summary>
        [Test]
        public void BindLai_ThuHoiONgayChuKhongDoiCuoiFrame()
        {
            _form.Bind("Đăng nhập", new[] { "Tài khoản", "Mật khẩu" }, new[] { "OK", "Quên" });

            var oldField = _form.Fields[0].gameObject;
            var oldButton = _form.Buttons[1].gameObject;

            _form.Bind("Mất kết nối", new string[0], new[] { "Thử lại" });

            Assert.AreEqual(0, _form.Fields.Count);
            Assert.AreEqual(1, _form.Buttons.Count);
            Assert.IsFalse(oldField.activeSelf, "Ô nhập cũ còn sống hết frame và vẫn nhận được click.");
            Assert.IsFalse(oldButton.activeSelf, "Nút cũ còn sống hết frame và vẫn gửi được gói.");
        }

        [Test]
        public void Notice_HienDuocVaXoaDuoc()
        {
            _form.Bind("Đăng nhập", new string[0], new string[0]);
            _form.SetNotice("Sai mật khẩu");

            Assert.AreEqual("Sai mật khẩu", _form.NoticeText);

            _form.SetNotice(null);
            Assert.AreEqual(string.Empty, _form.NoticeText);
        }

        private static void Click(Button button)
        {
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
        }
    }
}
