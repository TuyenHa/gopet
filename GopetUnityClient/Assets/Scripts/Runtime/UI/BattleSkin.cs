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

        public static Sprite Load(string key, string fallback = null)
        {
            var sprite = string.IsNullOrWhiteSpace(key) ? null : Resources.Load<Sprite>(key);
            return sprite ?? (string.IsNullOrWhiteSpace(fallback) ? null : Resources.Load<Sprite>(fallback));
        }
    }
}
