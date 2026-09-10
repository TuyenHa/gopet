namespace Gopet.Net.Auth
{
    /// <summary>
    /// Gói <c>CLIENT_INFO</c> (-36) — gói đầu tiên sau handshake, và là hai cổng
    /// chặn của server (<c>Player.cs:104-129</c>):
    ///
    /// <list type="number">
    /// <item><c>LanguageCode</c> không có trong <c>GopetManager.Language</c>
    ///       → <c>setClientOK(false)</c> rồi đóng kết nối.</item>
    /// <item><c>Version</c> nhỏ hơn 1.4.2 → dialog đỏ rồi đóng.</item>
    /// </list>
    ///
    /// Sai một trong hai thì triệu chứng chỉ là "kết nối được rồi tự rớt".
    /// Kiểm hai trường này trước khi đi tìm chỗ khác.
    /// </summary>
    public sealed class ClientInfo
    {
        /// <summary>Server so với <c>VERSION_142</c>; nhỏ hơn là bị từ chối.</summary>
        public string Version = "1.4.3";

        /// <summary>Chuỗi mô tả nền tảng. Server chỉ lưu, không kiểm.</summary>
        public string Info = "unity";

        /// <summary>
        /// Đặt cùng giá trị với emulator (320x240) khi đang đối chiếu gói tin —
        /// server gửi nội dung khác nhau theo độ phân giải.
        /// </summary>
        public int DisplayWidth = 320;

        public int DisplayHeight = 240;

        /// <summary>Phải là key có trong <c>GopetManager.Language</c>.</summary>
        public string LanguageCode = "vi";

        /// <summary>Lấy từ <c>MANIFEST.MF</c> của client cũ.</summary>
        public int Provider = 4;

        public sbyte ClientType = 0;

        /// <summary>
        /// Server đặt tên field này là <c>Refcode</c> (<c>Player.cs:120</c>) nhưng
        /// client J2ME thật gửi <c>"2.4.9"</c> vào đây — KHÔNG phải refcode.
        /// Refcode thật đi trong gói <c>LOGIN</c>.
        /// </summary>
        public string TrailingField = "2.4.9";

        /// <summary>Thứ tự field bắt buộc, theo đúng <c>Player.cs:106-120</c>.</summary>
        public Message ToMessage()
        {
            return Message.Create(GopetCmd.CLIENT_INFO)
                .PutSByte(ClientType)
                .PutInt(Provider)
                .PutUtf(Version)
                .PutUtf(Info)
                .PutInt(DisplayWidth)
                .PutInt(DisplayHeight)
                .PutUtf(LanguageCode)
                .PutUtf(TrailingField);
        }
    }
}
