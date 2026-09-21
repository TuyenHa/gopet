using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Sprite trắng 9-slice bo góc, dùng màu của Image để tạo panel động.</summary>
    public static class RoundedUiSprite
    {
        /// <summary>Bán kính mặc định, hợp với panel và nút cỡ thường.</summary>
        public const float DefaultRadius = 10f;

        private static readonly Dictionary<int, Sprite> Cache = new Dictionary<int, Sprite>();

        public static Sprite Get() => Get(DefaultRadius);

        /// <summary>
        /// Sprite bo góc với bán kính chỉ định, tính bằng ref-unit của canvas.
        ///
        /// <para><b>Biên 9-slice phải nhỏ hơn nửa ô sẽ vẽ.</b> Unity ép biên xuống
        /// theo TỪNG TRỤC RIÊNG (<c>Image.GetAdjustedBorders</c>): ô thấp hơn tổng
        /// biên dọc thì biên dọc co lại còn biên ngang giữ nguyên, và bốn góc bị kéo
        /// dài thành hình bầu dục. Nên biên ở đây chỉ nhỉnh hơn bán kính 2 đơn vị —
        /// muốn góc tròn hơn thì tăng <paramref name="radius"/>, đừng dựa vào việc
        /// Unity co biên giúp.</para>
        /// </summary>
        public static Sprite Get(float radius)
        {
            var key = Mathf.RoundToInt(radius * 4f);
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var border = Mathf.CeilToInt(radius) + 2;
            var size = border * 2 + 2;   // chừa 2 đơn vị phẳng ở giữa cho slice center
            var sprite = Build(size, radius, border);
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Số texel dựng cho mỗi ref-unit.
        ///
        /// <para>Dựng 1-1 thì dải khử răng cưa ở mép cong rộng đúng một ref-unit —
        /// tức khoảng 2 px trên màn hình — và mọi góc bo trông như bị lè ra. Dày gấp
        /// 4 thì dải đó còn 1/4 đơn vị, mép ăn đứt gọn. Đổi lại texture to gấp 16 lần,
        /// nhưng mỗi bán kính chỉ dựng một lần rồi cache nên vẫn là vài chục KB.</para>
        ///
        /// <para>Bù lại bằng <c>pixelsPerUnit</c> của sprite: đặt 100 × hệ số này thì
        /// Unity vẫn quy biên 9-slice về đúng số ref-unit như cũ.</para>
        ///
        /// <para>Chọn 4 sau khi dựng lại đường vẽ ngoài Unity và đo sai lệch so với
        /// hình lý tưởng, ở góc bo 6 đơn vị (tab cửa hàng), theo từng hệ số canvas:</para>
        /// <code>
        /// canvas |  T=1  |  T=2  |  T=4  | T=4 + mipmap
        ///  1.64  | 6.91  | 1.44  | 1.48  |    2.82
        ///  2.67  | 9.41  | 3.63  | 1.21  |    2.53
        ///  3.50  | 9.87  | 4.17  | 0.95  |    1.45
        /// </code>
        /// <para><b>KHÔNG bật mipmap.</b> Trực giác bảo nên bật vì góc bị thu nhỏ lúc
        /// vẽ, nhưng số đo nói ngược: mức mip gần nhất thô hơn cỡ cần vẽ, trilinear
        /// trộn vào là nhoè thêm gấp đôi. Chỉ ở canvas ≈ 1.0 (cửa sổ game hẹp 720px)
        /// mipmap mới thắng, và đó không phải cỡ chạy thật.</para>
        /// </summary>
        private const int TexelsPerUnit = 4;

        private static Sprite Build(int sizeUnits, float radius, int borderUnits)
        {
            var size = sizeUnits * TexelsPerUnit;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
            {
                name = $"Gopet Rounded Rectangle r{radius}",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };

            var pixels = new Color32[size * size];
            var half = size * 0.5f;
            var radiusTexels = radius * TexelsPerUnit;
            var inner = half - radiusTexels;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inner, 0f);
                    var dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inner, 0f);
                    var alpha = Mathf.Clamp01(radiusTexels + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color32(255, 255, 255,
                        (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var borderTexels = borderUnits * TexelsPerUnit;
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 100f * TexelsPerUnit, 0, SpriteMeshType.FullRect,
                new Vector4(borderTexels, borderTexels, borderTexels, borderTexels));
            sprite.name = $"Gopet Rounded Rectangle r{radius} (9-slice)";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        public static void Apply(Image image) => Apply(image, DefaultRadius);

        public static void Apply(Image image, float radius)
        {
            image.sprite = Get(radius);
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
        }
    }
}
