using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Sprite cắt từ ảnh mẫu HUD (Cửa hàng / Dịch vụ / Sự kiện + X + coin).
    ///
    /// <para>Cùng khuôn mẫu <see cref="LoginSkin"/>: <b>thiếu file thì trả
    /// <c>null</c> chứ không ném</b> — nút vẫn tồn tại nhờ nhãn chữ fallback,
    /// và một game vẫn chơi được dù thiếu icon HUD.</para>
    /// </summary>
    public static class HudSkin
    {
        public const string Root = "Ui/Hud/";

        public const string Shop = "shop";
        public const string Guild = "guild";
        public const string Service = "service";
        public const string Event = "event";
        /// <summary>Hộp thư — sinh bằng tools/image-gen, cùng khuôn 128×128 nền trong.</summary>
        public const string Mail = "mail";
        public const string Close = "close";
        public const string Coin = "coin";

        // Icon của popup cửa hàng — sinh bằng tools/image-gen, nền trong suốt.
        /// <summary>Dấu chân thú: badge tiêu đề và dòng gợi ý dưới đáy popup cửa hàng.</summary>
        public const string Paw = "paw";
        public const string StatAttack = "stat-attack";
        public const string StatDefense = "stat-defense";
        /// <summary>Đồng vàng trên nút giá. Khác <see cref="Coin"/> (24px, không alpha) của HUD cũ.</summary>
        public const string CoinGold = "coin-gold";

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string name)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>(Root + name);
            if (sprite == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu sprite HUD: Resources/{Root}{name} — dùng tạm nhãn chữ.");
            }

            Cache[name] = sprite;
            return sprite;
        }
    }
}
