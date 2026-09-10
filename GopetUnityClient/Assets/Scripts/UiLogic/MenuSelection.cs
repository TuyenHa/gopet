using System;
using Gopet.Net.Guider;

namespace Gopet.UiLogic
{
    /// <summary>Việc cần làm khi người dùng chạm vào một dòng menu.</summary>
    public enum MenuAction
    {
        /// <summary>Dòng không cho chọn — không gửi gì, không mở gì.</summary>
        Blocked,

        /// <summary>Hỏi xác nhận trước; đồng ý mới gửi.</summary>
        Confirm,

        /// <summary>Gửi lựa chọn ngay.</summary>
        Send
    }

    /// <summary>
    /// Quyết định chuyện gì xảy ra khi chọn một dòng — tách hẳn khỏi Unity để test
    /// được ngoài Editor.
    ///
    /// <para><b>Không có chỗ nào rẽ nhánh theo <c>listId</c>.</b> Đó là điều kiện
    /// sống còn của phase này: server có 162 màn hình đi qua đúng một định dạng, nên
    /// client chỉ được phép nhìn vào CỜ của từng dòng. Thêm một
    /// <c>switch (listId)</c> là bắt đầu nợ 162 nhánh.</para>
    /// </summary>
    public static class MenuSelection
    {
        public static MenuAction Decide(MenuScreen screen, int index)
        {
            if (screen == null) throw new ArgumentNullException(nameof(screen));

            if (index < 0 || index >= screen.Items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Dòng {index} nằm ngoài màn hình {screen.ListId} ({screen.Items.Length} dòng).");
            }

            var item = screen.Items[index];

            if (!item.CanSelect) return MenuAction.Blocked;

            return item.ShowDialog ? MenuAction.Confirm : MenuAction.Send;
        }

        /// <summary>
        /// Nội dung hộp xác nhận, lấy nguyên từ server.
        ///
        /// <para>Nhãn nút do server quyết định — đừng hardcode "OK"/"Huỷ". Cùng một
        /// dòng menu có thể hỏi "Mua?" ở màn này và "Bán?" ở màn khác.</para>
        /// </summary>
        public static ConfirmPrompt PromptFor(MenuScreen screen, int index)
        {
            if (Decide(screen, index) != MenuAction.Confirm)
            {
                throw new InvalidOperationException($"Dòng {index} không cần xác nhận.");
            }

            var item = screen.Items[index];
            return new ConfirmPrompt
            {
                Text = item.DialogText,
                ConfirmLabel = item.LeftCommandText,
                CancelLabel = item.RightCommandText
            };
        }

        /// <summary>Sau khi gửi lựa chọn, có đóng màn hình đang mở không.</summary>
        public static bool ShouldCloseAfter(MenuScreen screen, int index)
        {
            if (screen == null) throw new ArgumentNullException(nameof(screen));
            if (index < 0 || index >= screen.Items.Length) return false;

            return screen.Items[index].CloseScreenAfterClick;
        }

        public sealed class ConfirmPrompt
        {
            public string Text;
            public string ConfirmLabel;
            public string CancelLabel;
        }
    }
}
