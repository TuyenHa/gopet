using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class LoginFlowConnectionNoticeTests
    {
        [Fact]
        public void LoiTuSocketDuocDichSangTiengViet()
        {
            var flow = new LoginFlow();

            flow.OnDisconnected("No connection could be made because the target machine actively refused it.");

            Assert.Equal(LoginStage.Disconnected, flow.Stage);
            Assert.Equal("Không thể kết nối tới máy chủ. Vui lòng thử lại.", flow.Notice);
        }
    }
}
