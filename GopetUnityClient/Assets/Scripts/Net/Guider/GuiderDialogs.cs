namespace Gopet.Net.Guider
{
    /// <summary>
    /// Danh sách lựa chọn khi nói chuyện với NPC.
    /// <c>MenuController.showNpcOption()</c> (<c>MenuController.cs:764-790</c>).
    /// </summary>
    public sealed class NpcOptions
    {
        private const int MaxOptions = 256;

        public int NpcId;
        public Option[] Options;

        public sealed class Option
        {
            public int Id;
            public string Text;
        }

        public static NpcOptions Parse(Message message)
        {
            var reader = message.Reader;
            var result = new NpcOptions { NpcId = reader.ReadInt() };

            var count = reader.ReadInt();
            if (count < 0 || count > MaxOptions)
            {
                throw new ProtocolException($"NPC_OPTION khai {count} lựa chọn — ngoài khoảng hợp lệ.");
            }

            result.Options = new Option[count];
            for (var i = 0; i < count; i++)
            {
                result.Options[i] = new Option
                {
                    Id = reader.ReadInt(),
                    Text = GameTextTags.Substitute(reader.ReadUtf()),
                };
            }

            reader.ExpectFullyConsumed("NPC_OPTION");
            return result;
        }
    }

    /// <summary>
    /// Hộp thoại nhập liệu. <c>GameController.showInputDialog()</c>
    /// (<c>GameController.cs:3545-3565</c>).
    ///
    /// <para>Số ô nhập phải &lt;= 32: server kiểm lại đúng ngưỡng đó khi nhận
    /// (<c>GameController.cs:790</c>), gửi nhiều hơn là bị ngắt kết nối.</para>
    /// </summary>
    public sealed class InputDialogSpec
    {
        /// <summary>Server chặn ở 32 khi đọc gói trả lời, nên đừng dựng nhiều hơn.</summary>
        public const int MaxFields = 32;

        public int DialogId;
        public string Title;
        public Field[] Fields;

        public sealed class Field
        {
            public string Label;

            /// <summary>Kiểu ô nhập server yêu cầu; 0 là mặc định.</summary>
            public sbyte InputType;
        }

        public static InputDialogSpec Parse(Message message)
        {
            var reader = message.Reader;
            var spec = new InputDialogSpec { DialogId = reader.ReadInt(), Title = reader.ReadUtf() };

            var count = reader.ReadInt();
            if (count < 0 || count > MaxFields)
            {
                throw new ProtocolException($"TYPE_DIALOG_INPUT khai {count} ô nhập — ngoài khoảng hợp lệ.");
            }

            spec.Fields = new Field[count];
            for (var i = 0; i < count; i++)
            {
                spec.Fields[i] = new Field { Label = reader.ReadUtf(), InputType = reader.ReadSByte() };
            }

            reader.ExpectFullyConsumed("TYPE_DIALOG_INPUT");
            return spec;
        }
    }

    /// <summary>
    /// Hộp thoại Có/Không.
    ///
    /// <para><b>KHÔNG</b> đi trong <c>COMMAND_GUIDER</c> như mọi dialog khác, mà trong
    /// <c>SERVER_MESSAGE</c> (45) — xem <c>MenuController.showYNDialog()</c>
    /// (<c>MenuController.cs:1092-1100</c>) và <c>GameController.serverMessage()</c>
    /// (<c>GameController.cs:704-711</c>).</para>
    /// </summary>
    public sealed class YesNoRequest
    {
        public int DialogId;
        public string Text;

        public static YesNoRequest Parse(Message message)
        {
            var reader = message.Reader;
            var request = new YesNoRequest { DialogId = reader.ReadInt(), Text = reader.ReadUtf() };

            reader.ExpectFullyConsumed("SEND_YES_NO");
            return request;
        }
    }
}
