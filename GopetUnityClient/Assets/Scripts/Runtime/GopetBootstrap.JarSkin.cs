using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime
{
    /// <summary>
    /// Nửa "giống bản jar" của <see cref="GopetBootstrap"/>: khoá hướng màn hình,
    /// âm thanh, khung pixel-perfect, splash. Nửa kia (<c>GopetBootstrap.cs</c>) lo
    /// giao thức và màn hình chung của P5.
    /// </summary>
    public sealed partial class GopetBootstrap
    {
        /// <summary>
        /// Khoá ngang, cả hai chiều lật máy — không khoá cứng một chiều để người
        /// chơi vẫn xoay máy thoải mái, miễn không rơi về dọc.
        ///
        /// <para>Đặt bằng code, không chỉ dựa vào <c>ProjectSettings</c>: file đó cố
        /// tình không commit (xem README — Unity ghi đè theo phiên bản của nó), nên
        /// không thể coi là nguồn sự thật đáng tin cậy giữa các máy dựng dự án khác
        /// nhau. Bản jar chạy 320×240 — 4:3 NẰM NGANG — nên khung tham chiếu của
        /// <see cref="PixelCanvas"/> chỉ đúng khi máy cũng đang nằm ngang.</para>
        /// </summary>
        private static void LockLandscape()
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
