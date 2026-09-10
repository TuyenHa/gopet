using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.Input;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Bàn phím đi qua ĐÚNG đường Input System — bơm sự kiện phím thật vào một thiết
    /// bị ảo, không gọi tắt vào <c>Cancelled</c>.
    ///
    /// <para><b>Vì sao phải vậy:</b> gọi thẳng sự kiện thì binding sai (gõ nhầm tên
    /// phím, quên <c>Enable()</c>, quên tham chiếu asmdef tới Unity.InputSystem) vẫn
    /// cho test xanh, trong khi trên máy thật không phím nào ăn. Cùng một bài học
    /// với <c>PointerPathTests</c>.</para>
    /// </summary>
    public sealed class InputRouterTests : InputTestFixture
    {
        private GameObject _host;
        private InputRouter _router;
        private Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();

            _host = new GameObject("InputHost", typeof(RectTransform));
            _router = InputRouter.Create(_host.transform);
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            base.TearDown();
        }

        [Test]
        public void Esc_BanSuKienHuy()
        {
            var cancelled = 0;
            _router.Cancelled += () => cancelled++;

            Press(_keyboard.escapeKey);
            Release(_keyboard.escapeKey);

            Assert.AreEqual(1, cancelled);
        }

        [Test]
        public void Enter_BanSuKienXacNhan()
        {
            var submitted = 0;
            _router.Submitted += () => submitted++;

            Press(_keyboard.enterKey);
            Release(_keyboard.enterKey);

            Assert.AreEqual(1, submitted);
        }

        [Test]
        public void PhimKhac_KhongBanGiCa()
        {
            var fired = 0;
            _router.Cancelled += () => fired++;
            _router.Submitted += () => fired++;

            Press(_keyboard.aKey);
            Release(_keyboard.aKey);

            Assert.AreEqual(0, fired);
        }

        /// <summary>
        /// Esc phải đóng ĐÚNG màn hình trên cùng — đây là toàn bộ lý do
        /// <see cref="InputRouter"/> tồn tại.
        /// </summary>
        [Test]
        public void Esc_DongManHinhTrenCung()
        {
            var router = new MessageRouter();
            var guider = new GuiderHandler(m => m.Dispose());
            guider.RegisterOn(router);

            var ui = UiRoot.Create(_host.transform, null);
            ui.Initialize(guider, null);
            _router.Cancelled += () => ui.Back();

            TestPackets.Dispatch(router, TestPackets.MenuWire(1039, TestPackets.Item(0, "ATM")), GopetCmd.COMMAND_GUIDER);
            Assert.IsNotNull(ui.Current);

            Press(_keyboard.escapeKey);
            Release(_keyboard.escapeKey);

            Assert.IsNull(ui.Current, "Esc không đóng được màn hình đang mở.");
        }

        /// <summary>Không còn gì để đóng thì Esc phải im lặng, không ném.</summary>
        [Test]
        public void Esc_KhiKhongConManHinhNao_ThiKhongNem()
        {
            var ui = UiRoot.Create(_host.transform, null);
            ui.Initialize(new GuiderHandler(m => m.Dispose()), null);
            _router.Cancelled += () => ui.Back();

            Press(_keyboard.escapeKey);
            Release(_keyboard.escapeKey);

            Assert.IsNull(ui.Current);
        }
    }
}
