using System;

namespace Gopet.Net.Social
{
    public static class LetterPackets
    {
        public static Message RequestMailbox() => Command(GopetCmd.LETTER_BOX);

        public static Message Mark(int letterId) => Command(GopetCmd.LETTER_COMMAND_SET_MARK).PutInt(letterId);
        public static Message Remove(int letterId) => Command(GopetCmd.LETTER_COMMAND_REMOVE_LETTER).PutInt(letterId);

        public static Message Send(string recipient, string content)
        {
            if (string.IsNullOrWhiteSpace(recipient)) throw new ArgumentException("Recipient is required.", nameof(recipient));
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required.", nameof(content));
            return Command(GopetCmd.LETTER_COMMAND_SEND_LETTER)
                .PutUtf(recipient.Trim()).PutUtf(content.Trim());
        }

        private static Message Command(sbyte sub) => Message.Create(GopetCmd.LETTER_COMMAND).PutSByte(sub);
    }
}
