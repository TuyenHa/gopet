using System;

namespace Gopet.Net.Social
{
    /// <summary>
    /// Nhận gói opcode <c>LETTER_COMMAND = 121</c> (server → client).
    ///
    /// <para><b>Wire asymmetry:</b> server gửi ra ghi sub-command là <c>INT</c>
    /// (<c>letterMessage.putInt(cmd)</c>, GameController.cs:461), trong khi client
    /// gửi lên ghi sub là <c>sbyte</c> (server <c>letter(sbyte, msg)</c> đọc bằng
    /// <c>readsbyte()</c>). Không thể dùng <see cref="MessageRouter.RegisterSub"/>
    /// vì nó đọc sub là <c>sbyte</c> — nên register top-level ở đây và tự dispatch
    /// theo INT.</para>
    /// </summary>
    public sealed class LetterHandler
    {
        // Sub-command hằng số (khớp GopetCMD.cs server).
        public const int LetterBox = 13;
        public const int HasLetter = 18;

        public event Action<Mailbox> MailboxReceived;
        public event Action<HasLetterNotice> HasLetterReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.Register(121, OnLetter);
        }

        private void OnLetter(Message message)
        {
            var sub = message.Reader.ReadInt();
            switch (sub)
            {
                case LetterBox: ReadMailbox(message); break;
                case HasLetter: ReadHasLetter(message); break;
                // Còn nhiều sub khác (SEND_LETTER response, SET_MARK…) — chưa cần Phase 4.
            }
        }

        private void ReadMailbox(Message message)
        {
            var r = message.Reader;
            var count = r.ReadInt();
            if (count < 0 || count > 4096)
                throw new ProtocolException($"LETTER_BOX đếm {count} thư — ngoài khoảng hợp lệ.");

            var mailbox = new Mailbox { Letters = new Letter[count] };
            for (var i = 0; i < count; i++)
            {
                mailbox.Letters[i] = new Letter
                {
                    LetterId = r.ReadInt(),
                    Type = r.ReadSByte(),
                    Title = r.ReadUtf(),
                    ShortContent = r.ReadUtf(),
                    Content = r.ReadUtf(),
                    IsMark = r.ReadBool()
                };
            }
            MailboxReceived?.Invoke(mailbox);
        }

        private void ReadHasLetter(Message message)
        {
            var flag = message.Reader.ReadSByte();
            HasLetterReceived?.Invoke(new HasLetterNotice { HasUnread = flag != 0 });
        }
    }
}
