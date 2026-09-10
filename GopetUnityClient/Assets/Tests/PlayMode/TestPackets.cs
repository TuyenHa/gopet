using System;
using Gopet.Net;
using Gopet.Net.Guider;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Dựng gói y như server dựng, và bọc lại thành gói nhận được.
    ///
    /// <para>Dùng chung cho mọi PlayMode test để chỉ có MỘT chỗ mô tả định dạng —
    /// nếu mỗi file tự dựng lấy thì lúc server đổi format sẽ phải sửa nhiều chỗ, và
    /// chỗ nào quên sửa thì test vẫn xanh với dữ liệu đã lỗi thời.</para>
    /// </summary>
    internal static class TestPackets
    {
        public static MenuItemInfo Item(int itemId, string title, bool canSelect = true,
                                        bool showDialog = false, bool closeAfter = false)
        {
            return new MenuItemInfo
            {
                ItemId = itemId,
                ImagePath = "icon.png",
                Title = title,
                Description = "mô tả " + title,
                CanSelect = canSelect,
                ShowDialog = showDialog,
                DialogText = showDialog ? "Mua " + title + "?" : null,
                LeftCommandText = showDialog ? "Đồng ý" : null,
                RightCommandText = showDialog ? "Thôi" : null,
                CloseScreenAfterClick = closeAfter,
                PaymentOptions = Array.Empty<MenuItemInfo.PaymentOption>()
            };
        }

        public static MenuScreen Screen(int listId, params MenuItemInfo[] items)
        {
            return new MenuScreen { ListId = listId, Type = 0, Title = "Thử", Items = items };
        }

        /// <summary>Gói SHOW_MENU_ITEM, thứ tự field theo <c>GameController.showMenuItem()</c>.</summary>
        public static Message MenuWire(int listId, params MenuItemInfo[] items)
        {
            var m = Message.Create(GopetCmd.SHOW_MENU_ITEM)
                .PutInt(listId).PutSByte(0).PutUtf("Màn hình").PutInt(items.Length);

            foreach (var item in items)
            {
                m.PutInt(item.ItemId).PutUtf(item.ImagePath).PutUtf(item.Title).PutUtf(item.Description)
                 .PutSByte(item.CanSelect ? 1 : 0).PutBool(item.ShowDialog);

                if (item.ShowDialog)
                {
                    m.PutUtf(item.DialogText).PutUtf(item.LeftCommandText).PutUtf(item.RightCommandText);
                }

                m.PutSByte(item.SaleStatus).PutBool(item.CloseScreenAfterClick).PutInt(0);
            }

            return m;
        }

        public static Message YesNoWire(int dialogId, string text)
        {
            return Message.Create(GopetCmd.SEND_YES_NO).PutInt(dialogId).PutUtf(text);
        }

        public static Message InputWire(int dialogId, params string[] labels)
        {
            var m = Message.Create(GopetCmd.TYPE_DIALOG_INPUT)
                .PutInt(dialogId).PutUtf("Nhập").PutInt(labels.Length);

            foreach (var label in labels) m.PutUtf(label).PutSByte(0);
            return m;
        }

        /// <summary>
        /// Bọc bao ngoài rồi đẩy qua router THẬT.
        ///
        /// <para>Không gọi tắt vào sự kiện của handler: làm vậy là bỏ qua đúng phần
        /// dễ sai nhất — bóc bao ngoài và định tuyến sub-command.</para>
        /// </summary>
        public static void Dispatch(MessageRouter router, Message built,
                                    sbyte envelope = GopetCmd.COMMAND_GUIDER)
        {
            using (built)
            {
                var body = built.ToWire();
                var wire = new byte[body.Length + 1];
                wire[0] = unchecked((byte)envelope);
                Array.Copy(body, 0, wire, 1, body.Length);

                router.Dispatch(Message.FromWire(wire, false));
            }
        }

        /// <summary>Đọc int big-endian tại vị trí byte cho trước.</summary>
        public static int ReadInt(byte[] wire, int offset)
        {
            return (wire[offset] << 24) | (wire[offset + 1] << 16) | (wire[offset + 2] << 8) | wire[offset + 3];
        }
    }
}
