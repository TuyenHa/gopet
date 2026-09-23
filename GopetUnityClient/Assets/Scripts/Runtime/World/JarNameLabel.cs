using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Nhãn tên trong world-space: chữ TRẮNG in đậm, canh giữa theo trục X, đáy chữ nằm đúng
    /// gốc toạ độ. Dùng chung cho người chơi, pet, NPC/quái, cổng map, nhà, bang hội.
    ///
    /// <para><b>Dùng font TTF của HUD</b> (<see cref="UiBuilder.DefaultFont"/>) thay cho bitmap
    /// font jar. Bảng glyph của jar  thiếu phần lớn CHỮ HOA có dấu — nó
    /// chỉ có <c>Đ Ă Á Â</c> — nên tên nào chứa chữ hoa có dấu khác đều rơi về ô trắng.</para>
    ///
    /// <para><c>TextMesh</c> chứ không phải canvas world-space: mỗi canvas là một batch riêng,
    /// mà map đông NPC thì số nhãn lên tới hàng chục.</para>
    /// </summary>
    public sealed class JarNameLabel : MonoBehaviour
    {
        /// <summary>Chiều cao chữ, world unit — CHỈNH Ở ĐÂY là đổi cỡ mọi nhãn tên: người chơi,
        /// pet, NPC/quái, cổng map, nhà, bang hội.
        ///
        /// <para>Không còn buộc vào chiều cao glyph bitmap jar (13) nữa. Mọi bố cục quanh nhãn
        /// (nút "Vào" của cổng) đọc thẳng hằng này nên tự giãn theo.</para></summary>
        public const float Height = 22f;

        /// <summary>Bề rộng TRUNG BÌNH một ký tự, world unit. Chỉ để ƯỚC LƯỢNG hộp bao —
        /// bề rộng thật của chữ TTF không đọc được trước khi mesh dựng xong, mà hộp chạm thì
        /// chỉ cần xấp xỉ. Tỉ lệ 0.51 lấy từ bề rộng trung bình của bảng glyph jar cũ
        /// (6.6 trên chiều cao 13); nhân theo <see cref="Height"/> để chỉnh cỡ chữ là hộp
        /// chạm giãn theo, không phải sửa hai chỗ.</summary>
        private const float AvgCharWidth = Height * 0.51f;

        /// <summary><c>fontSize</c> chỉ đổi độ nét atlas, <c>characterSize</c> mới quyết định cỡ
        /// chữ thật trong world-space. Mốc quy đổi lấy từ <c>ChatBubble</c>/<c>NpcTalkPrompt</c>:
        /// characterSize 2 ≈ 18 unit, 1.3 ≈ 12 ⇒ cao ≈ characterSize × 9.</summary>
        private const int FontSize = 32;

        private const float UnitsPerCharSize = 9f;

        private TextMesh _mesh;
        private MeshRenderer _renderer;

        /// <summary>Bề rộng ước lượng của nhãn, world unit — xem <see cref="AvgCharWidth"/>.</summary>
        public static float EstimateWidth(string text) =>
            string.IsNullOrEmpty(text) ? 0f : text.Length * AvgCharWidth;

        public static JarNameLabel Create(Transform parent, Vector3 localPos, float scale, string text)
        {
            var go = new GameObject("Name");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * scale;
            var label = go.AddComponent<JarNameLabel>();
            label.Build();
            label.SetText(text);
            return label;
        }

        public void SetText(string text)
        {
            if (_mesh != null) _mesh.text = text ?? string.Empty;
        }

        /// <summary>
        /// Đỉnh nét chữ thật (không phải khung dòng) trong hệ toạ độ <paramref name="space"/>.
        /// Tên rỗng thì không có nét nào — trả <paramref name="fallback"/>.
        /// </summary>
        public float VisibleTopIn(Transform space, float fallback) =>
            // Renderer tắt/ẩn trả bounds rỗng ở gốc world → danh hiệu văng xuống đáy map.
            _renderer == null || !_renderer.enabled || !_renderer.gameObject.activeInHierarchy
                || string.IsNullOrWhiteSpace(_mesh.text)
                ? fallback
                : space.InverseTransformPoint(_renderer.bounds.max).y;

        public void SetSortingOrder(int order)
        {
            if (_renderer != null) _renderer.sortingOrder = order;
        }

        private void Build()
        {
            var go = new GameObject("t", typeof(TextMesh));
            go.transform.SetParent(transform, false);

            _mesh = go.GetComponent<TextMesh>();
            _mesh.font = UiBuilder.DefaultFont();
            _mesh.fontSize = FontSize;
            _mesh.characterSize = Height / UnitsPerCharSize;
            // In ĐẬM thay cho viền đen: nét dày tự nó đã tách chữ khỏi nền, mà chỉ tốn một
            // mesh thay vì năm (bản trước vẽ thêm 4 bản đen lệch 4 hướng làm viền).
            UiBuilder.SetFontStyle(_mesh, FontStyle.Bold);
            // Đáy chữ ở gốc toạ độ, canh giữa ngang — đúng như glyph bitmap cũ (pivot đáy-trái,
            // vẽ từ -Width/2). Đổi sang MiddleCenter sẽ đẩy mọi nhãn tụt xuống nửa dòng.
            _mesh.anchor = TextAnchor.LowerCenter;
            _mesh.alignment = TextAlignment.Center;
            _mesh.color = Color.white;

            _renderer = go.GetComponent<MeshRenderer>();
            _renderer.sharedMaterial = _mesh.font.material;
        }
    }
}
