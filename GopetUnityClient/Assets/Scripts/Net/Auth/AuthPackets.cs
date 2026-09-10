namespace Gopet.Net.Auth
{
    /// <summary>Các gói client gửi lên trong luồng đăng nhập.</summary>
    public static class AuthPackets
    {
        /// <summary>
        /// Số byte 0 client J2ME gửi thừa ở cuối gói <c>LOGIN</c>. Server không đọc,
        /// nhưng giữ lại để dump hai client khớp từng byte — thiếu chúng thì
        /// packet-diff báo lệch độ dài và mất công truy một lỗi không tồn tại.
        /// </summary>
        private const int LoginPaddingBytes = 8;

        /// <summary>
        /// Gói <c>LOGIN</c> (1). Wire thật của client cũ là <b>bốn</b> UTF + 8 byte 0:
        ///
        /// <code>
        /// 01 0009 "gopettest" 0008 "abc12345"
        ///    0026 "ref-mcb22qre14br-144706345912071136064"
        ///    0005 "1.4.3"
        ///    0000000000000000
        /// </code>
        ///
        /// Server chỉ đọc <b>ba</b> UTF (<c>Player.cs:134</c>) và gọi tham số thứ ba
        /// là <c>version</c> — nhưng nó thực nhận refcode, rồi không dùng vào đâu cả.
        /// Chuỗi version thật và 8 byte cuối nằm lại trong buffer.
        ///
        /// Gửi 3 UTF thì server vẫn cho đăng nhập; sai sót chỉ lộ ra lúc diff dump.
        /// </summary>
        public static Message Login(string username, string password, string refCode, string version)
        {
            var m = Message.Create(GopetCmd.LOGIN)
                .PutUtf(username)
                .PutUtf(password)
                .PutUtf(refCode)
                .PutUtf(version);

            for (var i = 0; i < LoginPaddingBytes; i++)
            {
                m.PutSByte(0);
            }

            return m;
        }

        /// <summary>Xin danh sách máy chủ. Không có thân gói.</summary>
        public static Message RequestServerList()
        {
            return Message.Create(GopetCmd.SERVER_LIST);
        }

        /// <summary>Đăng ký tài khoản (35). Ràng buộc phía server: xem <see cref="AuthRules"/>.</summary>
        public static Message Register(string username, string password)
        {
            return Message.Create(GopetCmd.REGISTER)
                .PutUtf(username)
                .PutUtf(password);
        }

        /// <summary>
        /// Tạo nhân vật (<c>CREATE_CHAR</c>, 21) — nhánh của lần đăng nhập ĐẦU TIÊN,
        /// khi tài khoản chưa có nhân vật nào.
        ///
        /// <para>Ràng buộc server (<c>GameController.cs:718</c>): tên khớp
        /// <c>^[a-z0-9]+$</c>, dài 5-20 ký tự.</para>
        ///
        /// <para><b>Tạo xong server ĐÓNG kết nối</b> (<c>GameController.cs:740</c>).
        /// Client phải kết nối lại và đăng nhập lần nữa — đừng chờ
        /// <c>LOGIN_SUCCES</c> trên kết nối cũ.</para>
        /// </summary>
        public static Message CreateCharacter(string name, sbyte gender)
        {
            return Message.Create(GopetCmd.CREATE_CHAR)
                .PutUtf(name)
                .PutSByte(gender);
        }

        /// <summary>
        /// Trả lời <c>CHECK_SPEED</c>. Gói này đi trong bao ngoài <c>PET_SERVICE</c>
        /// (<c>GameController.messagePetService</c>), không phải opcode đứng riêng.
        /// Không trả lời thì server đóng kết nối sau 15-30 giây.
        /// </summary>
        public static Message CheckSpeedReply()
        {
            return Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.CHECK_SPEED);
        }
    }
}
