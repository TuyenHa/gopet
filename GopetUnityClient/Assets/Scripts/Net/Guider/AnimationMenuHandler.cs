using System;

namespace Gopet.Net.Guider
{
    /// <summary>Parser cho PET_SERVICE/ANIMATION_MENU dùng bởi achievement và rich dialog.</summary>
    public sealed class AnimationMenuHandler
    {
        public event Action<AnimationMenuScreen> ScreenShown;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.ANIMATION_MENU, OnScreen);
        }

        private void OnScreen(Message message)
        {
            var screen = new AnimationMenuScreen
            {
                Type = message.Reader.ReadSByte(),
                MenuId = message.Reader.ReadInt(),
                Title = message.Reader.ReadUtf()
            };
            var elementCount = ReadCount(message.Reader.ReadInt(), 256, "element");
            screen.Elements = new AnimationMenuElement[elementCount];
            for (var i = 0; i < elementCount; i++) screen.Elements[i] = ReadElement(message);
            var commandCount = ReadCount(message.Reader.ReadInt(), 16, "command");
            screen.Commands = new AnimationMenuCommand[commandCount];
            for (var i = 0; i < commandCount; i++)
            {
                screen.Commands[i] = new AnimationMenuCommand
                {
                    Id = message.Reader.ReadInt(), Type = message.Reader.ReadSByte(),
                    Name = message.Reader.ReadUtf(), ClosesScreen = message.Reader.ReadBool(),
                    RepliesToServer = message.Reader.ReadBool()
                };
            }
            message.Reader.ExpectFullyConsumed("ANIMATION_MENU");
            ScreenShown?.Invoke(screen);
        }

        private static AnimationMenuElement ReadElement(Message message)
        {
            var kind = message.Reader.ReadSByte();
            if (kind != 0 && kind != 1)
                throw new ProtocolException($"ANIMATION_MENU element kind không hợp lệ: {kind}.");
            var element = new AnimationMenuElement
            {
                IsImage = kind == 1,
                CanSelect = message.Reader.ReadBool(),
                TextOrPath = message.Reader.ReadUtf(),
                Type = message.Reader.ReadSByte()
            };
            if (element.IsImage)
            {
                element.Animated = message.Reader.ReadBool();
                element.FrameCount = message.Reader.ReadInt();
                element.FrameDelay = message.Reader.ReadInt();
            }
            else element.FontStyle = message.Reader.ReadSByte();
            return element;
        }

        private static int ReadCount(int count, int max, string label)
        {
            if (count < 0 || count > max)
                throw new ProtocolException($"ANIMATION_MENU có {count} {label} — ngoài khoảng hợp lệ.");
            return count;
        }
    }
}
