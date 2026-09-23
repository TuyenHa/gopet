using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Sprite của màn đăng nhập, cắt từ bộ art do dự án cung cấp (KHÔNG phải từ jar —
    /// xem <see cref="JarSkin"/> cho asset của jar).
    ///
    /// <para><b>Thiếu file thì trả <c>null</c> chứ không ném</b>, ngược với
    /// <see cref="JarSkin"/>: mỗi mảnh ở đây đều có đường lùi bằng màu phẳng, và một
    /// màn đăng nhập xấu vẫn đăng nhập được — ném ở đây là chặn hẳn đường vào game vì
    /// một chuyện thuần trang trí.</para>
    /// </summary>
    public static class LoginSkin
    {
        public const string Root = "Ui/Login/";

        public const string Panel = "panel";
        public const string Field = "field";
        public const string Logo = "logo";
        public const string IconUser = "icon-user";
        public const string IconLock = "icon-lock";
        public const string IconEye = "icon-eye";
        public const string CheckOn = "check-on";
        public const string ButtonLogin = "button-login";
        public const string ButtonRegister = "button-register";
        public const string SoundOn = "sound-on";
        public const string SoundOff = "sound-off";

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string name)
        {
            // "!= null" chứ không chỉ kiểm có trong cache: thoát Play Mode là Unity
            // huỷ mọi đối tượng runtime, nhưng Dictionary TĨNH thì sống tiếp qua lần
            // Play sau và trả về cái xác đó. Image nhận sprite đã huỷ thì coi như
            // không có sprite — ô nhập mất sạch bo góc, mà chỉ lộ ra từ lần chạy THỨ HAI.
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>(Root + name);
            if (sprite == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu sprite màn đăng nhập: Resources/{Root}{name} — dùng tạm màu phẳng.");
            }

            Cache[name] = sprite;
            return sprite;
        }

        /// <summary>
        /// Bản 9-slice của một sprite khung: bốn góc giữ nguyên, phần giữa kéo giãn.
        ///
        /// <para>Ô nhập trong bộ art rộng 120px nhưng khung thật rộng gấp nhiều lần —
        /// vẽ kiểu Simple thì bo góc bị kéo dẹt thành hình bầu dục. Viền cắt sẵn ở đây
        /// thay vì đặt trong Sprite Editor: file <c>.meta</c> do Unity sinh, sửa tay là
        /// thứ dễ mất khi ai đó re-import.</para>
        /// </summary>
        /// <param name="border">Viền 9-slice (px sprite). Âm = tự tính cho ảnh bo góc một màu.</param>
        public static Sprite GetSliced(string name, float border = -1f)
        {
            var key = name + "#9slice" + border;
            // Sprite ở đây do Sprite.Create sinh ra lúc chạy nên CHẮC CHẮN bị huỷ khi
            // thoát Play — xem ghi chú ở Get.
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var source = Get(name);
            if (source == null) return Cache[key] = null;

            // Viền ôm trọn phần bo góc, chừa lại một dải mỏng ở giữa để uGUI kéo.
            //
            // Dải giữa PHẢI nằm trong vùng một màu: ảnh cắt từ sheet chênh nhau 1-2 mức
            // màu, kéo dải ấy ra vài trăm pixel là thành sọc dọc thấy rõ. Sprite ô nhập
            // vì vậy được vẽ lại thành hình bo góc một màu, không dùng bản cắt.
            //
            // Viền tính từ MÉP sprite vào 4 pixel quá phần bo góc. Không lấy theo phần
            // trăm: viền 9-slice đo bằng pixel của SPRITE, không co giãn theo khung
            // hiển thị, nên phần trăm trên một sprite cao sẽ cho ra góc tròn to hơn
            // hẳn ảnh gốc — đúng cái đã xảy ra khi để 35%.
            var inset = border >= 0f ? border : Mathf.Min(source.rect.width, source.rect.height) * 0.5f - 4f;

            var sliced = Sprite.Create(source.texture, source.rect, new Vector2(0.5f, 0.5f),
                source.pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(inset, inset, inset, inset));
            sliced.name = source.name + " (9-slice)";
            return Cache[key] = sliced;
        }
    }
}
