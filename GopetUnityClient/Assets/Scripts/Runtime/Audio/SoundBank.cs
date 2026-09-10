using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.Audio
{
    /// <summary>
    /// Nạp <see cref="AudioClip"/> lấy từ client J2ME cũ, theo tên, có cache. Cùng
    /// vai trò với <see cref="Gopet.Runtime.UI.JarSkin"/> nhưng cho âm thanh — một
    /// chỗ duy nhất gọi <c>Resources.Load</c>.
    /// </summary>
    public static class SoundBank
    {
        private const string AudioRoot = "Jar/Audio/";

        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        /// <summary>Tên không kèm đuôi <c>.wav</c>. Ví dụ: <c>Get("s_login")</c>, <c>Get("s_button")</c>.</summary>
        public static AudioClip Get(string name)
        {
            // Kiểm "!= null": cache tĩnh sống qua các lần Play, còn clip thì có thể
            // đã bị dọn — trả cái xác về thì AudioSource im lặng, không báo lỗi gì.
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            var clip = Resources.Load<AudioClip>(AudioRoot + name);
            if (clip == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy âm thanh \"{name}\" tại Resources/{AudioRoot}{name}. " +
                    "Đã chạy `node tools/unpack-jar-dat/index.js` chưa?");
            }

            Cache[name] = clip;
            return clip;
        }
    }
}
