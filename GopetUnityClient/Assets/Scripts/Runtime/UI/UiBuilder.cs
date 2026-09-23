using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Vài thao tác dựng uGUI bằng code mà mọi view đều cần.
    ///
    /// <para>Dựng bằng code chứ không dùng prefab để PlayMode test tạo được view mà
    /// không cần asset nào — nhưng như vậy thì mỗi view lại tự lo chuyện neo, và neo
    /// sai thì phần tử rộng 0 pixel: vẫn hiện chữ, không bấm được. Gom vào đây để
    /// chỉ có một chỗ làm sai.</para>
    /// </summary>
    public static class UiBuilder
    {
        /// <summary>
        /// Bảng màu tối thiểu.
        ///
        /// <para><b>Phải đặt tường minh:</b> <c>Image</c> và <c>Text</c> của uGUI đều
        /// mặc định màu TRẮNG, nên panel trắng + chữ trắng = màn hình trắng trơn.
        /// Không test nào bắt được vì test đọc <c>text</c> chứ không nhìn màu — chỉ
        /// lộ ra khi có người bấm Play.</para>
        /// </summary>
        public static readonly Color Panel = new Color(0.11f, 0.12f, 0.15f, 0.97f);

        public static readonly Color Field = new Color(0.18f, 0.19f, 0.23f, 1f);

        /// <summary>Nền xanh navy của jar — <c>gs.a = 345451</c> (<c>0x05456B</c>), tô kín màn bằng <c>fillRect(0,0,BaseCanvas.w,BaseCanvas.h)</c> trong <c>fw.b()</c>. KHÁC <see cref="Panel"/> (xám đen trung tính, tự đặt ra cho UI chung, không phải màu jar thật).</summary>
        public static readonly Color JarBackground = new Color(5f / 255f, 69f / 255f, 107f / 255f, 1f);
        public static readonly Color ButtonFace = new Color(0.22f, 0.34f, 0.52f, 1f);
        public static readonly Color TextMain = new Color(0.93f, 0.94f, 0.96f, 1f);
        public static readonly Color TextMuted = new Color(0.68f, 0.71f, 0.76f, 1f);

        private const string DefaultFontPath = "Fonts/BeVietnamPro/BeVietnamPro-Regular";
        private const string SemiBoldFontPath = "Fonts/BeVietnamPro/BeVietnamPro-SemiBold";

        private static Font _defaultFont;
        private static Font _semiBoldFont;
        private static bool _semiBoldLoaded;

        /// <summary>
        /// Font UI mặc định: Be Vietnam Pro — dấu chồng tiếng Việt (ế, ộ, Ặ) đặt đúng
        /// chỗ ở cỡ 11–14, Arial thì dính/đè dòng trên. Cache vì mọi view đều gọi.
        ///
        /// <para>Thiếu asset thì lùi về font dựng sẵn để vẫn có chữ, nhưng phải cảnh
        /// báo — lùi im lặng thì không ai biết UI đã về Arial.</para>
        /// </summary>
        public static Font DefaultFont()
        {
            if (_defaultFont != null) return _defaultFont;

            _defaultFont = Resources.Load<Font>(DefaultFontPath);
            if (_defaultFont == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu font {DefaultFontPath} — dùng font dựng sẵn.");
                _defaultFont = LegacyFont();
            }

            return _defaultFont;
        }

        /// <summary>
        /// Font dựng sẵn của Unity. Unity 6 đổi tên nó thành <c>LegacyRuntime.ttf</c>;
        /// bản cũ hơn là <c>Arial.ttf</c>.
        ///
        /// <para>Lấy trượt thì <c>Text.font</c> bằng <c>null</c> và <b>không một chữ
        /// nào được vẽ</b> — màn hình trống trơn, không có lỗi nào báo. Nên hụt cả hai
        /// tên thì phải hét lên.</para>
        /// </summary>
        private static Font LegacyFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                       ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (font == null)
            {
                Debug.LogError("[Gopet] Không lấy được font dựng sẵn — mọi chữ sẽ không hiện.");
            }

            return font;
        }

        /// <summary>
        /// Đặt kiểu chữ. Legacy <c>Text</c> chỉ gắn được một file font, nên
        /// <c>FontStyle.Bold</c> trên bản Regular là đậm GIẢ (Unity tự làm dày, nhòe viền).
        /// Ở đây Bold đổi hẳn sang file SemiBold; bỏ Bold thì trả về font mặc định.
        /// </summary>
        public static void SetFontStyle(Text text, FontStyle style)
        {
            text.font = ResolveStyledFont(text.font, style, out var synthesized);
            text.fontStyle = synthesized;
        }

        /// <summary>
        /// Như bản <c>Text</c>, cho chữ world-space. <c>TextMesh</c> vẽ bằng material của
        /// font nên đổi font thì phải đổi material theo, không thì ra ô vuông/chữ rác.
        /// </summary>
        public static void SetFontStyle(TextMesh mesh, FontStyle style)
        {
            mesh.font = ResolveStyledFont(mesh.font, style, out var synthesized);
            mesh.fontStyle = synthesized;

            var renderer = mesh.GetComponent<MeshRenderer>();
            if (renderer != null && mesh.font != null) renderer.sharedMaterial = mesh.font.material;
        }

        /// <summary>Chọn file font cho kiểu chữ; <paramref name="synthesized"/> là phần kiểu Unity còn phải tự giả lập.</summary>
        private static Font ResolveStyledFont(Font current, FontStyle style, out FontStyle synthesized)
        {
            var semiBold = SemiBoldFont();
            if (semiBold == null)
            {
                synthesized = style; // thiếu SemiBold → đành dùng đậm giả
                return current;
            }

            if (style == FontStyle.Bold || style == FontStyle.BoldAndItalic)
            {
                synthesized = style == FontStyle.BoldAndItalic ? FontStyle.Italic : FontStyle.Normal;
                return semiBold;
            }

            synthesized = style;
            return current == semiBold ? DefaultFont() : current;
        }

        private static Font SemiBoldFont()
        {
            if (_semiBoldLoaded) return _semiBoldFont;

            _semiBoldLoaded = true;
            _semiBoldFont = Resources.Load<Font>(SemiBoldFontPath);
            if (_semiBoldFont == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu font {SemiBoldFontPath} — chữ đậm dùng đậm giả.");
            }

            return _semiBoldFont;
        }

        /// <summary>Trải kín vùng chứa.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Một hàng ngang, ghim mép trên, cách hai bên <paramref name="padding"/>.
        ///
        /// <para>Đặt bằng <c>offsetMin</c>/<c>offsetMax</c> chứ không phải
        /// <c>sizeDelta</c> + <c>anchoredPosition</c>: với anchor trải ngang thì hai
        /// cách đó đá nhau và kết quả hay ra chiều rộng 0.</para>
        /// </summary>
        public static void PlaceRow(RectTransform rect, float top, float height, float padding)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(padding, -(top + height));
            rect.offsetMax = new Vector2(-padding, -top);
        }

        public static Text MakeText(Transform parent, Font font, string name, int size, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = TextMain;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            if (stretch) Stretch((RectTransform)go.transform);
            return text;
        }
    }
}
