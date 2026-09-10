namespace Gopet.UiLogic
{
    /// <summary>Chặng hiện tại của luồng đăng nhập. Tầng view chỉ nhìn cái này để quyết định hiện gì.</summary>
    public enum LoginStage
    {
        Idle,
        Connecting,

        /// <summary>Đã nối, đang chờ server duyệt <c>CLIENT_INFO</c>.</summary>
        Handshaking,

        ChoosingServer,
        EnteringCredentials,
        LoggingIn,

        /// <summary>Tài khoản chưa có nhân vật — nhánh của lần đăng nhập đầu, không phải lỗi.</summary>
        CreatingCharacter,

        Ready,

        /// <summary>Đứt kết nối. <see cref="LoginFlow.Notice"/> có thể rỗng — đó là trạng thái hợp lệ.</summary>
        Disconnected
    }
}
