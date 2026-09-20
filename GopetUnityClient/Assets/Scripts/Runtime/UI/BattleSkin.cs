using UnityEngine;

namespace Gopet.Runtime.UI
{
    public static class BattleSkin
    {
        /// <summary>Hệ số phóng dùng chung cho MỌI pixel art trong màn đấu: sprite pet và
        /// hoạt cảnh hiệu ứng. Art gốc của jar vẽ cho màn hình ~240px nên ở tỉ lệ 1× trông
        /// tí xíu trên máy hiện nay — pet chỉ 19×29 pixel.
        ///
        /// <para><b>PHẢI là số NGUYÊN.</b> Point filtering giữ nét khi phóng bằng bội số
        /// nguyên (mỗi pixel gốc thành đúng N×N pixel). Số lẻ như 2.5 làm chỗ 2px chỗ 3px —
        /// đó mới là "vỡ hình", không phải do phóng to.</para>
        ///
        /// <para>Một hằng số duy nhất để pet và hiệu ứng KHÔNG lệch nhau. Từng lệch: pet vẽ
        /// 2× còn hiệu ứng 1×, khiến hiệu ứng bé bằng nửa và mọi offset hoạt cảnh co lại,
        /// dồn xuống sát chân pet — nhìn như không có hiệu ứng.</para></summary>
        public const float SpriteScale = 3f;

        /// <summary>
        /// <see cref="SpriteScale"/> đã chốt sao cho một pixel nguồn phủ đúng một số
        /// NGUYÊN pixel màn hình.
        ///
        /// <para>Canvas trận đấu dùng ScaleWithScreenSize 960x540 khớp theo chiều cao,
        /// nên <c>scaleFactor</c> = chiều cao màn / 540 — gần như không bao giờ tròn
        /// (màn cao 604 px cho 1.148). Nhân với 3 ra 3.44 pixel màn cho mỗi pixel
        /// nguồn: sprite bị kéo giãn lẻ nên nhoè, trong khi CHỮ vẫn nét vì font được
        /// vẽ lại ở đúng độ phân giải thật. Đây là lý do "chữ nét mà sprite mờ".</para>
        /// </summary>
        public static float SnappedSpriteScale(Component context)
        {
            var factor = DeviceScaleFactor(context);
            if (factor <= 0f) return SpriteScale;

            // Tối thiểu 1 pixel màn cho 1 pixel nguồn — làm tròn xuống 0 là sprite biến mất.
            return Mathf.Max(1f, Mathf.Round(SpriteScale * factor)) / factor;
        }

        /// <summary>
        /// Hệ số phóng LỚN NHẤT là bội nguyên của pixel màn mà ảnh <paramref name="frameWidth"/>×
        /// <paramref name="frameHeight"/> vẫn lọt trong ô vuông cạnh <paramref name="boxUnits"/>.
        ///
        /// <para>Dùng cho ô ảnh có kích thước CỐ ĐỊNH: nhét thẳng ảnh vào ô là vừa phóng
        /// lẻ vừa méo tỉ lệ. Thà ảnh nhỏ hơn ô một chút mà nét và đúng dáng.</para>
        /// </summary>
        public static float SnappedFitScale(Component context, float boxUnits,
            float frameWidth, float frameHeight)
        {
            var factor = DeviceScaleFactor(context);
            if (factor <= 0f || frameWidth <= 0f || frameHeight <= 0f) return 1f;

            var boxDevice = boxUnits * factor;
            var fit = Mathf.Min(boxDevice / frameWidth, boxDevice / frameHeight);
            return Mathf.Max(1f, Mathf.Floor(fit)) / factor;
        }

        /// <summary>Số pixel màn cho mỗi đơn vị canvas, hoặc 0 nếu chưa gắn vào canvas nào.</summary>
        private static float DeviceScaleFactor(Component context)
        {
            var canvas = context != null ? context.GetComponentInParent<Canvas>() : null;
            return canvas != null ? canvas.scaleFactor : 0f;
        }

        public static Sprite Load(string key, string fallback = null)
        {
            var sprite = string.IsNullOrWhiteSpace(key) ? null : Resources.Load<Sprite>(key);
            return sprite ?? (string.IsNullOrWhiteSpace(fallback) ? null : Resources.Load<Sprite>(fallback));
        }
    }
}
