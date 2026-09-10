using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Sprite trắng 9-slice bo góc, dùng màu của Image để tạo panel động.</summary>
    public static class RoundedUiSprite
    {
        private const int Size = 32;
        private const float Radius = 10f;
        private static Sprite _sprite;

        public static Sprite Get()
        {
            if (_sprite != null) return _sprite;

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.name = "Gopet Rounded Rectangle";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            var pixels = new Color32[Size * Size];
            var half = Size * 0.5f;
            var inner = half - Radius;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inner, 0f);
                    var dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inner, 0f);
                    var alpha = Mathf.Clamp01(Radius + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * Size + x] = new Color32(255, 255, 255,
                        (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            _sprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(12f, 12f, 12f, 12f));
            _sprite.name = "Gopet Rounded Rectangle (9-slice)";
            _sprite.hideFlags = HideFlags.HideAndDontSave;
            return _sprite;
        }

        public static void Apply(Image image)
        {
            image.sprite = Get();
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
        }
    }
}
