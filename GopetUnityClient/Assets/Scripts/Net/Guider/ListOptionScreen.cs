namespace Gopet.Net.Guider
{
    /// <summary>
    /// Màn hình danh sách lựa chọn đơn giản — không icon, không dialog, không giá tiền.
    /// <c>GameController.sendListOption()</c> (<c>GameController.cs:4605-4622</c>).
    ///
    /// <para><b>Trùng số với gói đi ngược chiều.</b> Sub-command 3 là
    /// <c>GUIDER_LIST_OPTION</c> khi server GỬI XUỐNG, và <c>SELECT_MENU_ELEMENT</c>
    /// khi client GỬI LÊN. Cùng một byte, hai định dạng khác hẳn nhau — chỉ phân biệt
    /// được bằng chiều. Router của client chỉ xử lý chiều xuống nên không nhập nhằng,
    /// nhưng ai đọc dump sẽ thấy sub 3 ở cả hai phía và dễ tưởng là một.</para>
    ///
    /// <para>Chọn một dòng ở đây vẫn trả lời bằng <c>SELECT_MENU_ELEMENT</c> — giống
    /// hệt <see cref="MenuScreen"/>, nên tầng UI dùng chung một đường chọn.</para>
    /// </summary>
    public sealed class ListOptionScreen
    {
        private const int MaxOptions = 1024;

        public int ListId;

        /// <summary>Tiêu đề màn hình.</summary>
        public string Title;

        /// <summary>Nhãn nút giữa, ví dụ "OK". Lấy từ server, đừng hardcode.</summary>
        public string CommandText;

        public Option[] Options;

        public sealed class Option
        {
            public int Id;
            public string Text;

            /// <summary>Trạng thái server gán cho dòng; 0 thường là không chọn được.</summary>
            public sbyte Status;
        }

        public static ListOptionScreen Parse(Message message)
        {
            var reader = message.Reader;

            var screen = new ListOptionScreen { ListId = reader.ReadInt() };

            // Server ghi listId HAI lần (GameController.cs:4609-4610). Không rõ vì
            // sao, nhưng phải đọc đủ cả hai, nếu không lệch toàn bộ phần sau.
            reader.ReadInt();

            screen.Title = reader.ReadUtf();
            screen.CommandText = reader.ReadUtf();

            var count = reader.ReadInt();
            if (count < 0 || count > MaxOptions)
            {
                throw new ProtocolException($"GUIDER_LIST_OPTION khai {count} lựa chọn — ngoài khoảng hợp lệ.");
            }

            screen.Options = new Option[count];
            for (var i = 0; i < count; i++)
            {
                screen.Options[i] = new Option
                {
                    Id = reader.ReadInt(),
                    Text = reader.ReadUtf(),
                    Status = reader.ReadSByte()
                };
            }

            reader.ExpectFullyConsumed("GUIDER_LIST_OPTION");
            return screen;
        }
    }
}
