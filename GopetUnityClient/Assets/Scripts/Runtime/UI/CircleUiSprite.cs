using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Sprite hình tròn trắng đặc, dùng màu của <c>Image</c> để tô — cho các huy hiệu
    /// tròn (nền dấu chân ở badge tiêu đề cửa hàng).
    ///
    /// <para>Anh em với <see cref="RoundedUiSprite"/> nhưng KHÔNG 9-slice được: kéo
    /// giãn một hình tròn 9-slice ra thì bốn góc tròn còn phần giữa thành ống, không
    /// còn là hình tròn nữa. Nên vẽ hẳn một texture tròn và luôn dùng ở khung vuông.</para>
    /// </summary>
    public static class CircleUiSprite
    {
        private const int Size = 64;
        private static Sprite _sprite;

        public static Sprite Get()
        {
            if (_sprite != null) return _sprite;

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = "Gopet Circle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };

            var pixels = new Color32[Size * Size];
            var centre = Size * 0.5f;
            var radius = centre - 0.5f;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var dx = x + 0.5f - centre;
                    var dy = y + 0.5f - centre;
                    // Alpha giảm dần trong 1px cuối để mép không răng cưa.
                    var alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                    pixels[y * Size + x] = new Color32(255, 255, 255,
                        (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            _sprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size),
                new Vector2(0.5f, 0.5f), 100f);
            _sprite.name = "Gopet Circle";
            _sprite.hideFlags = HideFlags.HideAndDontSave;
            return _sprite;
        }
    }
}
