using Gopet.Net.Auth;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nhánh danh sách máy chủ của <see cref="LoginScreens"/>. Hiện chỉ có đúng một
    /// máy chủ nên <see cref="LoginFlow"/> tự chọn luôn, khỏi hiện màn chọn — nhưng
    /// đường nhiều máy chủ (server trả về &gt;1) vẫn phải chạy đúng cho tương lai.
    /// Xem <c>LoginScreensTests.cs</c> cho phần còn lại (đăng nhập, tạo nhân vật,
    /// mất kết nối).
    /// </summary>
    public sealed class LoginScreensServerListTests
    {
        private GameObject _canvas;
        private LoginFlow _flow;
        private LoginScreens _screens;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _flow = new LoginFlow();

            _screens = LoginScreens.Create(_canvas.transform, null);
            _screens.Initialize(_flow);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
        }

        [Test]
        public void CoDanhSachMayChu_HienDungTenServer()
        {
            _flow.Start("127.0.0.1", 19180);
            _flow.OnConnected();
            _flow.OnClientAccepted(true);
            _flow.OnServerList(new[]
            {
                new ServerEntry { Name = "Máy chủ 1", Address = "1.1.1.1", Port = 1 },
                new ServerEntry { Name = "Máy chủ 2", Address = "2.2.2.2", Port = 2 }
            });

            Assert.IsNotNull(_screens.ServerPicker);
            Assert.AreEqual(2, _screens.ServerPicker.Buttons.Count);
            Assert.IsNull(_screens.Form, "Màn chọn máy chủ không được để lẫn biểu mẫu cũ.");
        }

        /// <summary>Nhiều máy chủ (tương lai) vẫn qua màn chọn, và bấm chọn đẩy được luồng đi tiếp.</summary>
        [Test]
        public void ChonMayChu_DayLuongSangManDangNhap()
        {
            _flow.Start("127.0.0.1", 19180);
            _flow.OnConnected();
            _flow.OnClientAccepted(true);
            _flow.OnServerList(new[]
            {
                new ServerEntry { Name = "s1", Address = "127.0.0.1", Port = 19180 },
                new ServerEntry { Name = "s2", Address = "10.0.0.9", Port = 20000 }
            });

            _screens.ServerPicker.Choose(0);

            Assert.AreEqual(LoginStage.EnteringCredentials, _flow.Stage);
        }

        /// <summary>Đúng một máy chủ (trường hợp thật hiện nay) thì khỏi hiện màn chọn — vào thẳng màn đăng nhập.</summary>
        [Test]
        public void DungMotMayChu_KhongHienManChon_VaoThangDangNhap()
        {
            _flow.Start("127.0.0.1", 19180);
            _flow.OnConnected();
            _flow.OnClientAccepted(true);
            _flow.OnServerList(new[] { new ServerEntry { Name = "s1", Address = "127.0.0.1", Port = 19180 } });

            Assert.IsNull(_screens.ServerPicker, "Chỉ một máy chủ mà vẫn hiện màn chọn.");
            Assert.IsNotNull(_screens.Form, "Phải vào thẳng màn đăng nhập.");
            Assert.AreEqual(LoginStage.EnteringCredentials, _flow.Stage);
        }
    }
}
