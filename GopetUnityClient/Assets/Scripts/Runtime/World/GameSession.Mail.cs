using System;
using System.Linq;
using Gopet.Net.Social;
using Gopet.Runtime.UI;
using UnityEngine;
// `using System` kéo theo System.Object, đụng UnityEngine.Object mà file này gọi Destroy.
using Object = UnityEngine.Object;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private YesNoDialog _letterDeleteDialog;

        /// <summary>
        /// Số lần ta tự xin LETTER_BOX chỉ để ĐẾM thư, không phải để mở hộp thư. Gói trả về
        /// của hai việc là MỘT, server không phân biệt, nên phải tự nhớ ai là người xin.
        /// </summary>
        private int _silentMailboxRequests;

        /// <summary>Khớp <c>Letter.ADMIN</c> của server (<c>Data/User/Letter.cs:13</c>).</summary>
        private const sbyte AdminLetterType = 2;

        /// <summary>Số thư chưa đọc đổi — HUD gắn huy hiệu số lên icon hộp thư.</summary>
        public event Action<int> UnreadMailCountChanged;

        /// <summary>
        /// Xin hộp thư chỉ để đếm, KHÔNG mở giao diện.
        ///
        /// <para>Server chỉ bắn cờ có/không thư mới (HAS_LETTER là một byte 0/1 —
        /// <c>GameController.sendHasLetter</c> đếm <c>!IsMark</c> rồi vứt con số đi), nên muốn
        /// có số phải lấy cả hộp thư về mà đếm. Đổi gói HAS_LETTER cho nó mang số thì gọn hơn
        /// nhưng client jar cũ đang dùng chung server này cũng đọc gói đó.</para>
        /// </summary>
        private void RefreshUnreadMailCount()
        {
            // Đã có một lần đang bay thì thôi: server bắn HAS_LETTER cho MỖI thư đến, một
            // loạt thư về cùng lúc mà cứ thế xin là dội cả chùm LETTER_BOX vô ích.
            if (_silentMailboxRequests > 0) return;
            _silentMailboxRequests++;
            _client.Send(LetterPackets.RequestMailbox());
        }

        /// <summary>
        /// LETTER_BOX về: luôn cập nhật số đếm, nhưng chỉ MỞ hộp thư khi gói này là của một
        /// lần người chơi thật sự bấm — xem <see cref="_silentMailboxRequests"/>.
        /// </summary>
        private void OnMailboxReceived(Mailbox mailbox)
        {
            UnreadMailCountChanged?.Invoke(CountUnread(mailbox));

            var silent = _silentMailboxRequests > 0;
            if (silent) _silentMailboxRequests--;

            // Đang mở thì thay dữ liệu TẠI CHỖ, giữ nguyên tab người chơi đang xem —
            // dựng lại cả popup là văng về tab đầu sau mỗi lần đánh dấu đã đọc.
            if (_mailboxView != null)
            {
                _mailboxView.Bind(mailbox);
                return;
            }
            if (silent) return;
            ShowMailbox(mailbox);
        }

        /// <summary>Chưa đọc = chưa được đánh dấu, khớp <c>sendHasLetter</c> của server.</summary>
        private static int CountUnread(Mailbox mailbox) =>
            mailbox?.Letters == null ? 0 : mailbox.Letters.Count(letter => !letter.IsMark);

        private void ShowMailbox(Mailbox mailbox)
        {
            CloseMailbox();
            _mailboxView = MailboxView.Create(_hudParent, mailbox);
            _mailboxView.CloseRequested += CloseMailbox;
            _mailboxView.MarkRequested += id => UpdateLetter(LetterPackets.Mark(id));
            _mailboxView.RemoveRequested += ConfirmRemoveLetter;
            _mailboxView.SendRequested += SendLetter;
        }

        /// <summary>
        /// Gửi thư soạn ngay trong popup. Xin lại hộp thư để danh sách và số thư chưa đọc
        /// khớp ngay, khỏi đợi lần mở sau.
        /// </summary>
        private void SendLetter(string recipient, string content)
        {
            // KHÔNG báo "đã gửi" ở đây: gửi xong chưa chắc đã thành công, server còn trả
            // về "Người chơi không tồn tại". Báo trước là nói sai. Để server tự báo.
            _client.Send(LetterPackets.Send(recipient, content));
            _client.Send(LetterPackets.RequestMailbox());
        }

        private void CloseMailbox()
        {
            CloseLetterDeleteDialog();
            if (_mailboxView != null) Object.Destroy(_mailboxView.gameObject);
            _mailboxView = null;
        }

        /// <summary>
        /// Thư của ban quản trị hỏi kỹ hơn: đó thường là thông báo đền bù hoặc xử phạt,
        /// xoá nhầm là mất bằng chứng và không có đường lấy lại.
        /// </summary>
        private void ConfirmRemoveLetter(Letter letter)
        {
            if (_letterDeleteDialog != null) Object.Destroy(_letterDeleteDialog.gameObject);
            var question = letter.Type == AdminLetterType
                ? "Đây là thư của ban quản trị. Xoá rồi không lấy lại được. Vẫn xoá?"
                : "Bạn có chắc muốn xoá thư này?";
            _letterDeleteDialog = YesNoDialog.Create(_hudParent, question, "Xoá", "Huỷ");
            _letterDeleteDialog.Confirmed += () =>
            {
                UpdateLetter(LetterPackets.Remove(letter.LetterId));
                CloseLetterDeleteDialog();
            };
            _letterDeleteDialog.Cancelled += CloseLetterDeleteDialog;
        }

        private void CloseLetterDeleteDialog()
        {
            if (_letterDeleteDialog != null) Object.Destroy(_letterDeleteDialog.gameObject);
            _letterDeleteDialog = null;
        }

        private void UpdateLetter(Gopet.Net.Message message)
        {
            _client.Send(message);
            _client.Send(LetterPackets.RequestMailbox());
        }

    }
}
