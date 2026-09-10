namespace Gopet.Net.Guider
{
    public sealed class AnimationMenuScreen
    {
        public sbyte Type;
        public int MenuId;
        public string Title;
        public AnimationMenuElement[] Elements;
        public AnimationMenuCommand[] Commands;
    }

    public sealed class AnimationMenuElement
    {
        public bool IsImage;
        public bool CanSelect;
        public string TextOrPath;
        public sbyte Type;
        public sbyte FontStyle;
        public bool Animated;
        public int FrameCount;
        public int FrameDelay;
    }

    public sealed class AnimationMenuCommand
    {
        public int Id;
        public sbyte Type;
        public string Name;
        public bool ClosesScreen;
        public bool RepliesToServer;
    }
}
