namespace Gopet.Net.Guider
{
    /// <summary>
    /// Một dòng trong menu generic. Mirror <c>GServer/Data/dialog/MenuItemInfo.cs</c>,
    /// thứ tự field theo <c>GameController.showMenuItem()</c> (<c>GameController.cs:835-866</c>).
    ///
    /// <para><b>Client không được biết dòng này "là gì".</b> Nó không phải vật phẩm shop
    /// hay ô kho đồ — nó chỉ là một dòng có icon, tiêu đề, mô tả. Chọn dòng thứ N thì
    /// gửi lại N. Server quyết định phần còn lại.</para>
    /// </summary>
    public sealed class MenuItemInfo
    {
        /// <summary>ID riêng của item, hoặc chỉ số dòng nếu item không có ID (<c>showMenuItem</c> tự thay).</summary>
        public int ItemId;

        /// <summary>Đường dẫn ảnh, đưa thẳng cho <c>RemoteAssetCache</c>.</summary>
        public string ImagePath;

        public string Title;
        public string Description;

        /// <summary>Server gửi <c>sbyte</c> 1/0 chứ không phải bool.</summary>
        public bool CanSelect;

        /// <summary>
        /// Quyết định có ĐỌC ba chuỗi tiếp theo hay không. Đọc sai cờ này là lệch
        /// toàn bộ phần còn lại của gói, và triệu chứng sẽ hiện ra ở một dòng khác
        /// hẳn — rất khó lần.
        /// </summary>
        public bool ShowDialog;

        public string DialogText;
        public string LeftCommandText;
        public string RightCommandText;

        public sbyte SaleStatus;
        public bool CloseScreenAfterClick;

        public PaymentOption[] PaymentOptions;

        public sealed class PaymentOption
        {
            public int Id;
            public string MoneyText;
            public sbyte IsEnabled;
        }

        /// <summary>Số lựa chọn thanh toán tối đa cho một dòng. Chặn gói dị dạng.</summary>
        private const int MaxPaymentOptions = 64;

        public static MenuItemInfo Parse(JavaBinaryReader reader)
        {
            // Substitute (sao)/(saoden) tags → ★/☆ ở lớp parse để mọi consumer
            // (dialog xác nhận, menu row, popup pet) đều nhận text sạch. DRY.
            var item = new MenuItemInfo
            {
                ItemId = reader.ReadInt(),
                ImagePath = reader.ReadUtf(),
                Title = GameTextTags.Substitute(reader.ReadUtf()),
                Description = GameTextTags.Substitute(reader.ReadUtf()),
                CanSelect = reader.ReadSByte() == 1,
                ShowDialog = reader.ReadBool()
            };

            if (item.ShowDialog)
            {
                item.DialogText = GameTextTags.Substitute(reader.ReadUtf());
                item.LeftCommandText = reader.ReadUtf();
                item.RightCommandText = reader.ReadUtf();
            }

            item.SaleStatus = reader.ReadSByte();
            item.CloseScreenAfterClick = reader.ReadBool();

            var count = reader.ReadInt();
            if (count < 0 || count > MaxPaymentOptions)
            {
                throw new ProtocolException($"Dòng menu khai {count} lựa chọn thanh toán — ngoài khoảng hợp lệ.");
            }

            item.PaymentOptions = new PaymentOption[count];
            for (var i = 0; i < count; i++)
            {
                item.PaymentOptions[i] = new PaymentOption
                {
                    Id = reader.ReadInt(),
                    MoneyText = reader.ReadUtf(),
                    IsEnabled = reader.ReadSByte()
                };
            }

            return item;
        }
    }
}
