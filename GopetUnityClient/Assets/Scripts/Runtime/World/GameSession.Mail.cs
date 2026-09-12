using Gopet.Net.Social;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private LetterDetailView _letterDetail;
        private ComposeLetterView _composeLetter;
        private YesNoDialog _letterDeleteDialog;

        private void ShowMailbox(Mailbox mailbox)
        {
            CloseMailbox();
            _mailboxView = MailboxView.Create(_hudParent, mailbox);
            _mailboxView.CloseRequested += CloseMailbox;
            _mailboxView.LetterSelected += OpenLetter;
            _mailboxView.ComposeRequested += OpenComposeLetter;
        }

        private void CloseMailbox()
        {
            CloseLetter();
            CloseComposeLetter();
            CloseLetterDeleteDialog();
            if (_mailboxView != null) Object.Destroy(_mailboxView.gameObject);
            _mailboxView = null;
        }

        private void OpenLetter(Letter letter)
        {
            CloseLetter();
            _letterDetail = LetterDetailView.Create(_hudParent, letter);
            _letterDetail.CloseRequested += CloseLetter;
            _letterDetail.MarkRequested += id => UpdateLetter(LetterPackets.Mark(id));
            _letterDetail.RemoveRequested += ConfirmRemoveLetter;
        }

        private void ConfirmRemoveLetter(int id)
        {
            if (_letterDeleteDialog != null) Object.Destroy(_letterDeleteDialog.gameObject);
            _letterDeleteDialog = YesNoDialog.Create(_hudParent, "Bạn có chắc muốn xoá thư này?", "Xoá", "Huỷ");
            _letterDeleteDialog.Confirmed += () =>
            {
                UpdateLetter(LetterPackets.Remove(id));
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
            CloseLetter();
        }

        private void CloseLetter()
        {
            if (_letterDetail != null) Object.Destroy(_letterDetail.gameObject);
            _letterDetail = null;
        }

        private void OpenComposeLetter()
        {
            CloseComposeLetter();
            _composeLetter = ComposeLetterView.Create(_hudParent);
            _composeLetter.CloseRequested += CloseComposeLetter;
            _composeLetter.SendRequested += (recipient, content) =>
            {
                _client.Send(LetterPackets.Send(recipient, content));
                ShowToast("Đã gửi thư");
                CloseComposeLetter();
            };
        }

        private void CloseComposeLetter()
        {
            if (_composeLetter != null) Object.Destroy(_composeLetter.gameObject);
            _composeLetter = null;
        }
    }
}
