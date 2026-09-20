using System;
using Gopet.Net.Guider;

namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        /// <summary>Menu ki-ốt pet của server (<c>MenuController.MENU_KIOSK_PET</c>).</summary>
        private const int KioskPetMenuId = 81028;

        /// <summary>Ai đó bấm "Xem hình xăm" cho một món hàng trong ki-ốt pet. Tham số là
        /// <c>MenuItem.ItemId</c> — đúng id server tra trong danh sách hàng đang bán.</summary>
        public event Action<int> KioskPetTattooRequested;

        /// <summary>Dòng đang được xử lý theo đường CHỌN thường, tạm tắt override để khỏi
        /// tự gọi lại chính mình thành vòng lặp.</summary>
        private int _kioskBypassIndex = -1;

        /// <summary>
        /// Mỗi con pet đang bán có hai việc làm được: mua (chọn như mọi menu khác) và XEM HÌNH
        /// XĂM trước khi mua. Menu chuẩn của server chỉ có đường thứ nhất, nên chen một hộp
        /// chọn vào giữa — cùng cách đã làm cho kho cánh.
        /// </summary>
        private bool TryShowKioskPetActions(GenericMenuView menu, MenuScreen screen, int index)
        {
            if (index < 0 || index >= screen.Items.Length) return false;
            if (index == _kioskBypassIndex) return false;

            var item = screen.Items[index];
            // Dòng không mang itemId (tiêu đề, "quay lại"…) thì để menu xử lý như cũ.
            if (item.ItemId <= 0) return false;

            var choices = new[] { "Chọn", "Xem hình xăm", "Đóng" };
            var dialog = ChoiceDialogView.Create(transform, _font);
            dialog.Bind(item.Title, choices);
            dialog.Chosen += choice =>
            {
                Close(dialog);
                if (choice == 0)
                {
                    _kioskBypassIndex = index;
                    menu.OnRowClicked(index);
                    _kioskBypassIndex = -1;
                }
                else if (choice == 1)
                {
                    KioskPetTattooRequested?.Invoke(item.ItemId);
                }
            };
            Push(dialog, dialog.gameObject);
            return true;
        }
    }
}
