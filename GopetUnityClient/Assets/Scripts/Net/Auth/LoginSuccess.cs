namespace Gopet.Net.Auth
{
    /// <summary>
    /// Gói <c>LOGIN_SUCCES</c> (3), theo <c>GameController.loginOK()</c>
    /// (<c>GameController.cs:1584-1589</c>):
    ///
    /// <code>
    /// int  user_id
    /// UTF  name
    /// UTF  name        &lt;- gửi HAI lần, cùng một giá trị
    /// UTF  serverIP
    /// int  serverPort
    /// </code>
    ///
    /// <para>Tên gửi hai lần là đúng như vậy, không phải đọc sai. Bỏ lần thứ hai
    /// thì mọi field sau đó lệch.</para>
    /// </summary>
    public sealed class LoginSuccess
    {
        public int UserId;
        public string Name;
        public string ServerAddress;
        public int ServerPort;

        public static LoginSuccess Parse(Message message)
        {
            var reader = message.Reader;

            var result = new LoginSuccess { UserId = reader.ReadInt(), Name = reader.ReadUtf() };

            var repeated = reader.ReadUtf();
            if (repeated != result.Name)
            {
                // Server gửi cùng một chuỗi hai lần. Khác nhau nghĩa là đang đọc
                // lệch, và nói ra ngay tại đây rẻ hơn nhiều so với truy ngược từ
                // triệu chứng ở màn hình sau đó.
                throw new ProtocolException(
                    $"LOGIN_SUCCES: hai tên khác nhau (\"{result.Name}\" vs \"{repeated}\") — parser lệch.");
            }

            result.ServerAddress = reader.ReadUtf();
            result.ServerPort = reader.ReadInt();

            reader.ExpectFullyConsumed("LOGIN_SUCCES");
            return result;
        }

        public override string ToString() => $"#{UserId} {Name} @ {ServerAddress}:{ServerPort}";
    }
}
