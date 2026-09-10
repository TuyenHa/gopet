using Gopet.Net.Auth;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Cấu hình <see cref="ClientInfo"/> dùng cho harness.
    ///
    /// <para>Cố ý KHÔNG dựng lại gói bằng tay: bản sao chỉ xác thực chính bản sao.
    /// Check D so byte của gói này với hằng số viết tay, nên nó phải là đúng
    /// class mà Unity ship.</para>
    /// </summary>
    internal static class SmokeClientInfo
    {
        public static Gopet.Net.Message Build()
        {
            return new ClientInfo
            {
                // Nhận diện phiên của harness trong dump server.
                Info = "unity-live-smoke"

                // Các field còn lại giữ mặc định — trùng giá trị client J2ME gửi.
            }.ToMessage();
        }
    }
}
