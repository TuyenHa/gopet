using System;
using System.Collections.Generic;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nạp chuỗi lấy từ client J2ME cũ (<c>tools/extract-jar-strings</c>), theo
    /// chỉ số, có cache. Cùng vai trò với <see cref="JarSkin"/> nhưng cho văn bản.
    ///
    /// <para>Mặc định lấy tiếng Việt; người dùng có thể đổi VN/EN trong Settings và
    /// lựa chọn được lưu bằng <see cref="PlayerPrefs"/>.</para>
    /// </summary>
    public enum Language { Vi, En }

    public static class JarStrings
    {
        private static Dictionary<int, string> _vi;
        private static Dictionary<int, string> _en;
        private const string PrefsKey = "gopet.language";

        private static Language _current = LoadPref();

        /// <summary>Ngôn ngữ hiện tại — client-side, persist qua PlayerPrefs.</summary>
        public static Language Current
        {
            get => _current;
            set
            {
                if (_current == value) return;
                _current = value;
                try
                {
                    PlayerPrefs.SetInt(PrefsKey, (int)value);
                    PlayerPrefs.Save();
                }
                catch { /* WebGL/private mode */ }
                Changed?.Invoke(value);
            }
        }

        /// <summary>Bắn khi language toggle. Các view dùng bảng chuỗi resubscribe để re-render.</summary>
        public static event Action<Language> Changed;

        public static void ToggleLanguage() => Current = Current == Language.Vi ? Language.En : Language.Vi;

        public static string Get(int index) =>
            Current == Language.Vi ? Vi(index) : En(index);

        public static string Vi(int index) => Get(ref _vi, "vi", index);

        public static string En(int index) => Get(ref _en, "en", index);

        private static Language LoadPref()
        {
            try
            {
                var v = PlayerPrefs.GetInt(PrefsKey, (int)Language.Vi);
                return v == (int)Language.En ? Language.En : Language.Vi;
            }
            catch { return Language.Vi; }
        }

        private static string Get(ref Dictionary<int, string> cache, string lang, int index)
        {
            if (cache == null) cache = Load(lang);

            if (!cache.TryGetValue(index, out var text))
            {
                throw new InvalidOperationException(
                    $"Không có chuỗi [{lang}][{index}]. Đã chạy " +
                    "`node tools/extract-jar-strings/index.js` chưa, hay jar đã đổi bảng chuỗi?");
            }

            return text;
        }

        private static Dictionary<int, string> Load(string lang)
        {
            var path = $"Jar/Strings/strings-{lang}";
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy Resources/{path}. Đã chạy `node tools/extract-jar-strings/index.js` chưa?");
            }

            return JarStringTable.Parse(asset.text);
        }
    }
}
