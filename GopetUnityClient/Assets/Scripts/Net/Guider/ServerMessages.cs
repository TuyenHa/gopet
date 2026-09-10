namespace Gopet.Net.Guider
{
    /// <summary>Payload UTF duy nhất của POPUP_MESSAGE và BANNER_MESSAGE.</summary>
    public static class ServerTextMessage
    {
        public static string Parse(Message message, string label)
        {
            var text = message.Reader.ReadUtf();
            message.Reader.ExpectFullyConsumed(label);
            return text;
        }
    }

    /// <summary>
    /// Image dialog do server điều khiển. Captcha dùng id 0, ảnh 160x80 và một frame.
    /// </summary>
    public sealed class ImageDialogSpec
    {
        private const int MaxDimension = 2048;
        private const int MaxFrames = 256;

        public int DialogId;
        public int Width;
        public int Height;
        public string ImagePath;
        public int FrameCount;
        public int FrameDelayMs;

        public static ImageDialogSpec Parse(Message message)
        {
            var r = message.Reader;
            var result = new ImageDialogSpec
            {
                DialogId = r.ReadInt(),
                Width = r.ReadInt(),
                Height = r.ReadInt(),
                ImagePath = r.ReadUtf(),
                FrameCount = r.ReadInt(),
                FrameDelayMs = r.ReadInt()
            };

            if (result.Width <= 0 || result.Width > MaxDimension ||
                result.Height <= 0 || result.Height > MaxDimension)
                throw new ProtocolException($"Image dialog có kích thước không hợp lệ: {result.Width}x{result.Height}.");
            if (result.FrameCount <= 0 || result.FrameCount > MaxFrames)
                throw new ProtocolException($"Image dialog có số frame không hợp lệ: {result.FrameCount}.");
            if (result.FrameDelayMs < 0)
                throw new ProtocolException($"Image dialog có frame delay âm: {result.FrameDelayMs}.");

            r.ExpectFullyConsumed("GUIDER_IMGDIALOG");
            return result;
        }
    }
}
