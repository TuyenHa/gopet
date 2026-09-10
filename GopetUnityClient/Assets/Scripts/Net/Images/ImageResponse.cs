namespace Gopet.Net.Images
{
    /// <summary>
    /// Trả lời của <c>COMMAND_IMAGE</c> (<c>GameController.cs:924-930</c>):
    ///
    /// <code>
    /// sbyte  gameType
    /// sbyte  type
    /// UTF    originPath     &lt;- đường dẫn GỐC client đã gửi, không phải đường đã giải
    /// int    length
    /// byte[] pngData
    /// </code>
    ///
    /// <para><c>originPath</c> là khoá đối chiếu: khi client gửi một số (id vật phẩm),
    /// server tra <c>itemAssetsIcon</c> ra đường dẫn thật nhưng vẫn dội lại con số
    /// ban đầu (<c>GameController.cs:876-889</c>). Ghép theo đường đã giải là ghép trượt.</para>
    /// </summary>
    public sealed class ImageResponse
    {
        /// <summary>Ảnh lớn nhất trong <c>GServer/assets</c> còn xa dưới mức này. Chặn gói dị dạng khai độ dài vô lý.</summary>
        private const int MaxImageBytes = 8 * 1024 * 1024;

        public sbyte GameType;
        public sbyte Type;
        public string Path;
        public byte[] Png;

        public static ImageResponse Parse(Message message)
        {
            var reader = message.Reader;

            var result = new ImageResponse
            {
                GameType = reader.ReadSByte(),
                Type = reader.ReadSByte(),
                Path = reader.ReadUtf()
            };

            var length = reader.ReadInt();
            if (length < 0 || length > MaxImageBytes)
            {
                throw new ProtocolException($"COMMAND_IMAGE khai độ dài {length} byte — ngoài khoảng hợp lệ.");
            }

            result.Png = reader.ReadBytes(length);
            reader.ExpectFullyConsumed("COMMAND_IMAGE");
            return result;
        }
    }
}
