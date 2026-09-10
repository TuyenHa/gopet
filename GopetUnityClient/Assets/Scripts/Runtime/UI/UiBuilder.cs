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

        /// <summary>
        /// Font dựng sẵn của Unity. Unity 6 đổi tên nó thành <c>LegacyRuntime.ttf</c>;
        /// bản cũ hơn là <c>Arial.ttf</c>.
        ///
        /// <para>Lấy trượt thì <c>Text.font</c> bằng <c>null</c> và <b>không một chữ
        /// nào được vẽ</b> — màn hình trống trơn, không có lỗi nào báo. Nên hụt cả hai
        /// tên thì phải hét lên.</para>
        /// </summary>
        public static Font BuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                       ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (font == null)
            {
                Debug.LogError("[Gopet] Không lấy được font dựng sẵn — mọi chữ sẽ không hiện.");
            }

            return font;
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
