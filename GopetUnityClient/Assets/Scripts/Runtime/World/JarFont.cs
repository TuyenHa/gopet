using System.Collections.Generic;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Bitmap font của jar (lớp <c>gg</c>, atlas <c>gv.a</c> = <c>mui/4</c>). Glyph xếp DỌC:
    /// ký tự thứ <c>idx</c> nằm ở <c>(0, idx*Height)</c> tính từ ĐỈNH atlas, rộng
    /// <c>Widths[idx]</c>, cao <c>Height</c>. Charset + Widths lấy nguyên từ jar (<c>gv.b()</c>).
    /// Atlas là mask trắng nền trong suốt → tô màu khi vẽ (jar dùng <c>cp.d()</c> = xanh 0x3B5998).
    /// </summary>
    public static class JarFont
    {
        public const int Height = 13;

        private const string Charset =
            " 0123456789.,:!?()+*<>/-%abcdefghijklmnopqrstuvwxyzáàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđABCDEFGHIJKLMNOPQRSTUVWXYZĐ$ĂÁÂ=";

        private static readonly int[] Widths =
        {
            4,6,5,6,6,7,6,6,6,6,6,3,3,3,4,5,4,4,6,5,8,8,6,6,10,6,7,5,7,6,4,7,7,3,4,6,3,9,7,7,7,7,
            5,5,4,7,6,9,6,7,6,6,6,6,6,6,6,6,6,6,7,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,7,6,6,3,3,3,5,3,7,
            7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,8,8,8,8,8,7,7,7,7,8,7,7,7,7,7,6,6,7,7,3,5,
            7,6,10,8,7,7,7,6,7,7,7,7,9,7,7,8,8,6,7,7,7,9
        };

        /// <summary>Màu tên mặc định của jar: <c>cp.d()</c> tô atlas thành 0xFF3B5998 (xanh dương-xám).</summary>
        public static readonly Color DefaultColor = new Color32(0x3B, 0x59, 0x98, 0xFF);

        private static Texture2D _atlas;
        private static readonly Dictionary<char, Sprite> _glyphs = new Dictionary<char, Sprite>();

        private static Texture2D Atlas()
        {
            if (_atlas == null)
            {
                var sprite = JarSkin.Bank("mui", 4);
                _atlas = sprite != null ? sprite.texture : null;
            }
            return _atlas;
        }

        private static int Index(char c)
        {
            var idx = Charset.IndexOf(c);
            var rows = _atlas != null ? _atlas.height / Height : Charset.Length;
            if (idx < 0 || idx >= rows) return 0; // ngoài atlas → dùng ô trắng đầu (space)
            return idx;
        }

        public static int Width(char c) => Widths[Index(c)];

        public static int Width(string text)
        {
            var w = 0;
            if (text != null) foreach (var c in text) w += Widths[Index(c)];
            return w;
        }

        /// <summary>Sprite của một glyph (pivot đáy-trái, 1 texel = 1 unit). Cache theo ký tự.</summary>
        public static Sprite Glyph(char c)
        {
            var atlas = Atlas();
            if (atlas == null) return null;
            if (_glyphs.TryGetValue(c, out var cached) && cached != null) return cached;
            var idx = Index(c);
            // Hàng idx tính từ ĐỈNH atlas; Texture2D gốc ở ĐÁY nên lật trục Y.
            var rect = new Rect(0f, atlas.height - (idx + 1) * Height, Widths[idx], Height);
            var sprite = Sprite.Create(atlas, rect, Vector2.zero, 1f);
            _glyphs[c] = sprite;
            return sprite;
        }
    }
}
