using System;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Toán học của khung tham chiếu 320×240 "giống bản jar", neo theo CHIỀU CAO.
    /// Thuần C# — test được ngoài Editor, giống <see cref="MenuVirtualizer"/>.
    ///
    /// <para>Máy chạy jar là 320×240 (4:3 ngang). Máy hiện đại nằm ngang thì rất
    /// rộng (điện thoại ~20:9, PC ~16:9) — thứ thiếu là CHIỀU CAO, không phải chiều
    /// ngang. Nên phóng theo bội số nguyên của chiều cao, để chiều ngang tràn hết
    /// màn còn chiều cao canh giữa với sọc trên/dưới nếu có dư.</para>
    ///
    /// <code>
    /// N            = floor(caoManHinh / 240)      // bội số nguyên — pixel không bao giờ méo
    /// caoThat      = 240 × N                       // canh giữa theo chiều dọc
    /// letterbox    = caoManHinh − caoThat           // sọc trên/dưới, chia đều
    /// rongLogic    = rongManHinh / N                // để chiều ngang TRÀN hết màn
    /// </code>
    ///
    /// <para>Rộng logic luôn ≥ 320 trên máy rộng hơn 4:3, đúng hành vi bản jar khi
    /// chạy trên máy màn rộng: nó tự canh giữa bố cục 320 và còn dư lề hai bên
    /// (<c>fb.a()</c>: <c>(BaseCanvas.w - var2 >> 1)</c>).</para>
    /// </summary>
    public readonly struct PixelCanvasLayout
    {
        /// <summary>Chiều cao khung tham chiếu — cố định, khớp bản jar.</summary>
        public const int ReferenceHeight = 240;

        /// <summary>Bội số nguyên phóng theo chiều cao. Luôn ≥ 1.</summary>
        public int Scale { get; }

        /// <summary>Chiều rộng logic — nơi đặt widget theo toạ độ jar. Luôn ≥ <see cref="ReferenceHeight"/> × 4/3 = 320 khi máy không hẹp hơn 4:3.</summary>
        public float LogicalWidth { get; }

        /// <summary>Chiều cao thật đã phóng, tính bằng pixel màn hình.</summary>
        public int ScaledHeight { get; }

        /// <summary>Tổng sọc trên+dưới, tính bằng pixel màn hình. Chia đôi để canh giữa.</summary>
        public float Letterbox { get; }

        private PixelCanvasLayout(int scale, float logicalWidth, int scaledHeight, float letterbox)
        {
            Scale = scale;
            LogicalWidth = logicalWidth;
            ScaledHeight = scaledHeight;
            Letterbox = letterbox;
        }

        /// <param name="screenWidth">Chiều rộng màn hình thật, pixel.</param>
        /// <param name="screenHeight">Chiều cao màn hình thật, pixel.</param>
        public static PixelCanvasLayout Compute(float screenWidth, float screenHeight)
        {
            if (screenWidth <= 0f || screenHeight <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(screenHeight), $"Kích thước màn hình vô lý: {screenWidth}x{screenHeight}.");
            }

            // Tối thiểu 1: màn hình thấp hơn 240 (trình chạy test -nographics, cửa sổ
            // Editor bị thu nhỏ) vẫn phải ra một khung dùng được, dù bị crop, thay vì
            // chia cho 0.
            var scale = Math.Max(1, (int)Math.Floor(screenHeight / ReferenceHeight));

            var scaledHeight = ReferenceHeight * scale;
            var letterbox = screenHeight - scaledHeight;
            var logicalWidth = screenWidth / scale;

            return new PixelCanvasLayout(scale, logicalWidth, scaledHeight, letterbox);
        }
    }
}
