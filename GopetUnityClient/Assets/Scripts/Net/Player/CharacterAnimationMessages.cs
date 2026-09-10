namespace Gopet.Net.Player
{
    public sealed class CharacterAnimation
    {
        public sbyte FrameCount;
        public string FrameImagePath;
        public short OffsetX;
        public short OffsetY;
        public bool DrawAtEnd;
        public bool MirrorWithCharacter;
        public sbyte Type;
    }

    public sealed class CharacterAnimationUpdate
    {
        public int UserId;
        public CharacterAnimation[] Animations;
    }
}
