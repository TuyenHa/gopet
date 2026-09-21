using Gopet.Net.Guider;
namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        /// <summary>
        /// Báo lỗi từ server (opcode 10 — <c>Player.redDialog</c>), ví dụ "Không đủ tiền".
        /// </summary>
        public void ShowServerError(string text)
        {
            ShowServerNotice(text);
            RefreshAtmAfterNotice();
        }

        public void ShowServerSuccess(string text)
        {
            ShowServerNotice(text);
            RefreshAtmAfterNotice();
        }

        private void RefreshAtmAfterNotice()
        {
            // Hiện thông báo trước để nó không bị lần request lại menu ATM nuốt mất.
            // Request này chỉ cập nhật số dư/menu sau đó.
            if (_atmPopup != null) _atmPopup.RequestAtm();
        }

        /// <summary>
        /// Thông báo một chiều của server đi ra <b>toast</b>, không phải dialog chặn.
        ///
        /// <para>Bản trước dựng <c>ServerNoticeDialog</c>: một tấm popup to có nút OK
        /// cho mỗi câu "Không đủ tiền". Người chơi phải bấm tắt mới chơi tiếp được,
        /// trong khi câu đó chỉ để BÁO chứ không chờ trả lời gì — đúng định nghĩa của
        /// toast. Test <c>ServerMessages_AreNonBlockingToasts</c> đã đòi hành vi này
        /// từ trước.</para>
        /// </summary>
        private void ShowServerNotice(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            ShowToast(text);
        }

        private void ShowPopup(string text)
        {
            ShowToast(text);
        }

        private void ShowImageDialog(ImageDialogSpec spec)
        {
            var view = ImageDialogView.Create(transform, _font);
            view.Bind(spec, _assets);
            view.Confirmed += () =>
            {
                _guider.SelectImageDialog(spec.DialogId);
                Close(view);
            };
            Push(view, view.gameObject);
        }
    }
}
