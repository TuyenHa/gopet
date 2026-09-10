using System;

namespace Gopet.Net.Images
{
    /// <summary>
    /// Gói <c>COMMAND_IMAGE</c> (96) — client xin ảnh theo đường dẫn, server trả PNG thô.
    ///
    /// <para>Là opcode ĐỨNG RIÊNG trong switch của <c>GameController</c>
    /// (<c>GameController.cs:341</c>), không phải sub-command.</para>
    /// </summary>
    public static class ImagePackets
    {
        /// <summary>
        /// Chỉ <c>gameType == 0</c> mới được server phục vụ
        /// (<c>GameController.cs:895</c>). Giá trị khác bị bỏ qua im lặng.
        /// </summary>
        public const sbyte GameType = 0;

        /// <summary>Ảnh NPC — giá trị client J2ME dùng cho <c>npcs/*.png</c>.</summary>
        public const sbyte TypeNpc = 2;

        /// <summary>Icon — client J2ME dùng cho <c>anim_characters/</c>, <c>gameMisc/</c>, <c>tatoos/</c>.</summary>
        public const sbyte TypeIcon = 3;

        /// <summary>
        /// Đường dẫn server CỐ Ý không trả gì (<c>GameController.cs:878</c>).
        /// Xin nó là chờ tới hết hạn một cách vô ích.
        /// </summary>
        public const string EmptyImagePath = "dialog/empty.png";

        /// <summary>
        /// Tiền tố đường dẫn captcha. Đường dẫn thật do server sinh kèm 6 số ngẫu
        /// nhiên phía sau (<c>GameController.cs:195</c>) nên mỗi phiên một khác —
        /// tuyệt đối không cache.
        /// </summary>
        public const string CaptchaPathPrefix = "img/captcha.png";

        /// <summary>
        /// <paramref name="type"/> server không dùng để tra ảnh, chỉ dội ngược lại;
        /// nó chỉ từ chối 10 và 11 (<c>GameController.cs:897</c>). Giữ đúng giá trị
        /// client J2ME gửi để dump hai bên còn so được.
        /// </summary>
        public static Message Request(string path, sbyte type)
        {
            return Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(GameType)
                .PutSByte(type)
                .PutUtf(path);
        }

        /// <summary>
        /// So Ordinal chứ không theo văn hoá: đây là đường dẫn giao thức, không phải
        /// văn bản hiển thị — so theo locale vừa chậm vừa có thể ra kết quả khác nhau
        /// giữa các máy.
        /// </summary>
        public static bool IsCaptcha(string path)
        {
            return path != null && path.StartsWith(CaptchaPathPrefix, StringComparison.Ordinal);
        }
    }
}
