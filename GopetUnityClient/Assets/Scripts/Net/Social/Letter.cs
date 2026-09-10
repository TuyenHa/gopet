namespace Gopet.Net.Social
{
    /// <summary>Một thư trong hộp thư người chơi (server <c>Data/User/Letter.cs</c>).</summary>
    public sealed class Letter
    {
        public int LetterId;
        public sbyte Type;
        public string Title;
        public string ShortContent;
        public string Content;
        public bool IsMark;
    }

    /// <summary>Hộp thư đầy đủ do server bơm qua opcode 121 / sub 13 (LETTER_BOX).</summary>
    public sealed class Mailbox
    {
        public Letter[] Letters;
    }

    /// <summary>Cờ báo có/không thư mới (server bơm qua opcode 121 / sub 18 HAS_LETTER).</summary>
    public sealed class HasLetterNotice
    {
        public bool HasUnread;
    }
}
