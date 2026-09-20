using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Vòng tròn xoay báo "đang chờ server". Vẽ bằng texture sinh lúc chạy, không thêm
    /// asset nào — cùng cách <see cref="RoundedUiSprite"/> đang làm.
    ///
    /// <para>Vòng có ĐUÔI mờ dần: một vòng đặc quay tròn thì mắt không thấy nó quay,
    /// phải có chỗ đậm chỗ nhạt mới nhận ra chuyển động.</para>
    /// </summary>
    public sealed class LoadingSpinner : MonoBehaviour
    {
        private const int TextureSize = 64;
        private const float OuterRadius = 30f;
        private const float Thickness = 7f;

        /// <summary>Độ/giây. Nhanh hơn nữa thì ở 30 khung/giây vòng bắt đầu giật.</summary>
        private const float DegreesPerSecond = 300f;

        /// <summary>Đuôi mờ nhất vẫn giữ chút alpha để vòng không đứt hẳn làm đôi.</summary>
        private const float TailMinAlpha = 0.12f;

        private static Sprite _ring;

        private RectTransform _rect;

        /// <summary>Gắn một vòng xoay vào giữa <paramref name="parent"/>.</summary>
        /// <param name="size">Đường kính, tính bằng đơn vị UI của canvas cha.</param>
        public static LoadingSpinner Attach(RectTransform parent, float size, Color color)
        {
            var go = new GameObject("Vòng xoay chờ", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(size, size);

            var image = go.GetComponent<Image>();
            image.sprite = Ring();
            image.color = color;
            // Vòng nằm đè lên nút đang bị khoá; ăn luôn cú chạm thì nút mất cả trạng
            // thái disabled của Unity, nên để cú chạm đi xuyên qua.
            image.raycastTarget = false;

            var spinner = go.AddComponent<LoadingSpinner>();
            spinner._rect = rect;
            return spinner;
        }

        private void Update()
        {
            // unscaledDeltaTime: màn đăng nhập có thể chạy lúc Time.timeScale = 0
            // (chuyển cảnh), vòng vẫn phải quay chứ không đứng hình.
            _rect.localRotation *= Quaternion.Euler(
                0f, 0f, -DegreesPerSecond * Time.unscaledDeltaTime);
        }

        /// <summary>Texture vòng tròn có đuôi mờ. Cache tĩnh: mỗi lần bấm nút lại sinh
        /// một texture 64x64 mới thì bấm nhiều là rác chất đống.</summary>
        private static Sprite Ring()
        {
            if (_ring != null) return _ring;

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            texture.name = "Gopet Loading Ring";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            var pixels = new Color32[TextureSize * TextureSize];
            var half = TextureSize * 0.5f;
            var inner = OuterRadius - Thickness;
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    var dx = x + 0.5f - half;
                    var dy = y + 0.5f - half;
                    var radius = Mathf.Sqrt(dx * dx + dy * dy);

                    // Phủ mép trong lẫn mép ngoài trong một phép clamp — viền răng cưa
                    // ở cỡ này nhìn thấy rõ.
                    var band = Mathf.Clamp01(OuterRadius + 0.5f - radius) *
                               Mathf.Clamp01(radius - inner + 0.5f);

                    // 0 ở đầu vòng, 1 ở cuối đuôi. Atan2 trả (-pi, pi] nên đưa về [0, 1).
                    var turn = (Mathf.Atan2(dy, dx) + Mathf.PI) / (2f * Mathf.PI);
                    var alpha = band * Mathf.Lerp(TailMinAlpha, 1f, turn);

                    pixels[y * TextureSize + x] = new Color32(255, 255, 255,
                        (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            _ring = Sprite.Create(texture, new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f), 100f);
            _ring.name = "Gopet Loading Ring";
            _ring.hideFlags = HideFlags.HideAndDontSave;
            return _ring;
        }
    }
}
