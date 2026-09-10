using Gopet.Net.Auth;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Chặng nhập tài khoản của <see cref="LoginScreens"/> dùng
    /// <see cref="LoginFormView"/> (bộ art riêng), KHÔNG dùng <see cref="FormView"/>
    /// chung như các chặng khác. Phần còn lại (chọn máy chủ, tạo nhân vật, mất kết
    /// nối) không đổi — xem <c>LoginScreensTests.cs</c>.
    /// </summary>
    public sealed class LoginScreensFormTests
    {
        private GameObject _canvas;

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
        }

        private LoginScreens ReachCredentials()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));

            var flow = new LoginFlow();
            var screens = LoginScreens.Create(_canvas.transform, null);
            screens.Initialize(flow);

            flow.Start("127.0.0.1", 19180);
            flow.OnConnected();
            flow.OnClientAccepted(true);
            flow.OnServerList(new[] { new ServerEntry { Name = "s1", Address = "127.0.0.1", Port = 19180 } });

            // Đúng một máy chủ thì OnServerList tự chọn luôn (xem LoginFlow.ServerList.cs).
            if (flow.Stage == LoginStage.ChoosingServer) flow.ChooseServer(0);

            return screens;
        }

        [Test]
        public void ManDangNhap_DungLoginFormView_KhongDungFormViewChung()
        {
            var screens = ReachCredentials();

            Assert.IsNotNull(screens.LoginForm);
            Assert.IsNull(screens.Form, "Không được dựng cả hai loại biểu mẫu cho cùng một chặng.");
        }

        /// <summary>Rời chặng đăng nhập phải dọn sạch form, nếu không nó che mất màn kế tiếp.</summary>
        [Test]
        public void RoiChangDangNhap_DonSachForm()
        {
            var screens = ReachCredentials();
            Assert.IsNotNull(screens.LoginForm);

            screens.LoginForm.SetCredentials("gopettest", "abc12345");
            screens.LoginForm.SubmitDefault();

            Assert.IsNull(screens.LoginForm, "Chuyển sang chặng 'đang đăng nhập' mà form cũ vẫn còn.");
        }
    }
}
