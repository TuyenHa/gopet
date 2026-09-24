using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Kiểu nút chung của game: khung vàng chữ nhật GÓC VUÔNG, ô đá xanh ở 4 góc, mặt xanh bóng
    /// (ảnh <c>Resources/Ui/Generated/button_frame_blue.png</c>, sinh bằng tools/image-gen).
    /// Chữ đặt bằng code nên một ảnh dùng được cho mọi nút.
    ///
    /// <para>Ảnh vẽ kiểu 9-slice: 4 góc (có đá) giữ nguyên, mép và lòng nút co giãn.
    /// Ảnh gốc không có hoa văn ở giữa mép — có thì kéo nút rộng ra là hoa văn méo.</para>
    ///
    /// <para><b>Độ dày viền theo chiều cao nút.</b> Viền 9-slice tính bằng pixel ảnh; để
    /// nguyên thì nút cao 30 cũng mang viền 32 đơn vị và vỡ hình. <see cref="Apply"/> đặt
    /// <c>pixelsPerUnitMultiplier</c> = cao ảnh / cao nút, nên nút nào cũng trông như bản
    /// thu nhỏ của ảnh gốc.</para>
    /// </summary>
    public static class GameButtonSkin
    {
        private const string FramePath = "Ui/Generated/button_frame_blue";

        // Đo trên ảnh 512×145: viền vàng ~15, mặt xanh bắt đầu ở px 16–18, ô đá góc ~30.
        // 32 ôm trọn ô đá + góc lòng nút; nhỏ hơn thì ô đá bị kéo giãn.
        private static readonly Vector4 Border = new Vector4(32f, 32f, 32f, 32f); // L, B, R, T

        private static Sprite _frame;

        /// <summary>
        /// Phủ khung lên <paramref name="image"/> của nút cao <paramref name="height"/>.
        /// Trả <c>false</c> khi thiếu ảnh (build lỗi) — caller giữ nguyên kiểu cũ.
        /// </summary>
        public static bool Apply(Image image, float height)
        {
            var sprite = Frame();
            if (image == null || sprite == null || height <= 0f) return false;

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            image.color = Color.white;
            image.pixelsPerUnitMultiplier = sprite.rect.height / height;
            return true;
        }

        /// <summary>Chữ trên nút: trắng đậm, viền xanh đậm cho nổi trên mặt xanh bóng.</summary>
        public static void StyleLabel(Text label)
        {
            if (label == null) return;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            // Không dùng ?? với UnityEngine.Object — toán tử đó bỏ qua kiểm tra null của Unity.
            var outline = label.GetComponent<Outline>();
            if (outline == null) outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.03f, 0.12f, 0.38f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static Sprite Frame()
        {
            // So sánh null kiểu Unity, không dùng cờ "đã nạp": project tắt Domain Reload khi vào
            // Play Mode nên biến static sống qua các lần Play, còn sprite tạo lúc chạy thì bị huỷ
            // khi thoát Play Mode. Giữ cờ thì từ lần Play thứ hai mọi nút mất khung.
            if (_frame != null) return _frame;
            var texture = Resources.Load<Texture2D>(FramePath);
            if (texture == null) return null;
            _frame = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, Border);
            return _frame;
        }
    }
}
