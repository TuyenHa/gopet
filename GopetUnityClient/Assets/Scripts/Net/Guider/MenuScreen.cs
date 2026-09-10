namespace Gopet.Net.Guider
{
    /// <summary>
    /// Gói <c>SHOW_MENU_ITEM</c> — một màn hình danh sách generic.
    ///
    /// <para>Đây là định dạng dùng chung cho <b>toàn bộ</b> menu và dialog của game:
    /// shop, kho đồ, clan, kiosk, nhiệm vụ, xăm, ghép đồ, chọn pet… Server không gửi
    /// "màn hình shop", nó gửi một danh sách. Nhờ vậy client chỉ cần MỘT bộ render.</para>
    ///
    /// <para><b>Đừng phân nhánh theo <see cref="ListId"/>.</b> Thấy <c>switch (listId)</c>
    /// trong code UI là đã đi sai hướng: mỗi nhánh như vậy là một màn hình phải bảo trì
    /// tay, và server có tới 162 cái.</para>
    /// </summary>
    public sealed class MenuScreen
    {
        /// <summary>Số dòng tối đa của một màn hình. Chặn gói dị dạng khai số lượng vô lý.</summary>
        private const int MaxItems = 4096;

        /// <summary>ID màn hình. Gửi NGƯỢC LẠI khi người dùng chọn, không dùng để phân nhánh.</summary>
        public int ListId;

        public sbyte Type;
        public string Title;
        public MenuItemInfo[] Items;

        public static MenuScreen Parse(Message message)
        {
            var reader = message.Reader;

            var screen = new MenuScreen
            {
                ListId = reader.ReadInt(),
                Type = reader.ReadSByte(),
                Title = reader.ReadUtf()
            };

            var count = reader.ReadInt();
            if (count < 0 || count > MaxItems)
            {
                throw new ProtocolException($"SHOW_MENU_ITEM khai {count} dòng — ngoài khoảng hợp lệ.");
            }

            screen.Items = new MenuItemInfo[count];
            for (var i = 0; i < count; i++)
            {
                screen.Items[i] = MenuItemInfo.Parse(reader);
            }

            reader.ExpectFullyConsumed("SHOW_MENU_ITEM");
            return screen;
        }
    }
}
