using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Khung popup dùng chung của game: nền bo góc viền xanh, badge tiêu đề hình viên
    /// thuốc chờm lên mép trên, nút X đỏ ở góc trên-phải và băng chân tuỳ chọn.
    ///
    /// <para>Mọi con số đo từ ảnh mẫu cửa hàng rồi quy về ref-unit của canvas (ảnh
    /// rộng 1631px ↔ 720 ref, hệ số 0.4415). Popup nào cũng dựng qua đây thì bốn góc,
    /// màu viền và chỗ đặt nút X không thể lệch nhau giữa các màn.</para>
    ///
    /// <para>Người gọi chỉ đụng vào <see cref="Content"/> — vùng đã trừ sẵn badge tiêu
    /// đề và băng chân. Component của màn hình (ví dụ <see cref="ShopPopupView"/>) gắn
    /// lên CÙNG GameObject với khung này, nên <c>DialogStack</c> vẫn quản một object.</para>
    /// </summary>
    public sealed partial class GamePopupFrame : MonoBehaviour
    {
        // Chiều cao khả kiến của canvas khi chạy landscape 16:9 chỉ ~405 ref-unit
        // (CanvasScaler khớp CHIỀU RỘNG 720) — popup phải thấp hơn ngưỡng đó, kể cả
        // khi tính cả badge chờm lên trên.
        public const float DefaultWidth = 400f;
        public const float DefaultHeight = 300f;

        /// <summary>Lề hai bên, cũng là lề của <see cref="Content"/>.</summary>
        public const float SidePadding = 11f;

        /// <summary>Mép trên của <see cref="Content"/>, tính từ đỉnh popup.</summary>
        public const float ContentTop = 18f;

        private const float BadgeWidth = 174f;
        /// <summary>Cao hơn 31 là badge chờm xuống đè lên nội dung.</summary>
        private const float BadgeHeight = 31f;
        private const float CloseSize = 34f;
        private const float FooterHeight = 20f;
        private const float FooterIconSize = 16f;
        /// <summary>Khe giữa dấu chân và chữ ở băng chân.</summary>
        private const float FooterIconGap = 5f;
        private const float FooterBottom = 9f;

        private Text _footer;

        public event Action Closed;

        /// <summary>Vùng đặt nội dung của màn hình, đã trừ badge tiêu đề và băng chân.</summary>
        public RectTransform Content { get; private set; }

        /// <summary>Bề ngang của <see cref="Content"/>, tính sẵn để khỏi đợi layout pass.</summary>
        public float ContentWidth { get; private set; }

        public static GamePopupFrame Create(Transform parent, Font font, string title,
            float width = DefaultWidth, float height = DefaultHeight, string footer = null)
        {
            var go = new GameObject("PopupFrame", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            // GIỮA MÀN HÌNH — popup là dialog chính, không phải sidebar.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;

            // Viền 2 đơn vị, dày hơn các viền mảnh bên trong vì đây là đường bao ngoài cùng.
            RoundedBorder.Apply(go, 14f, PopupPalette.Panel, PopupPalette.Border, 2f);

            var frame = go.AddComponent<GamePopupFrame>();
            frame.ContentWidth = width - SidePadding * 2f;
            frame.BuildContent(footer != null);
            if (footer != null) frame.BuildFooter(font, footer);
            frame.BuildHeader(font, title);
            frame.BuildCloseButton(font);
            return frame;
        }

        /// <summary>Đổi chữ băng chân. Không bật băng chân lúc tạo thì gọi vào đây là vô hiệu.</summary>
        public void SetFooter(string text)
        {
            if (_footer != null) _footer.text = text;
        }

        private void BuildContent(bool hasFooter)
        {
            var go = new GameObject("Content", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            Content = (RectTransform)go.transform;
            Content.anchorMin = Vector2.zero;
            Content.anchorMax = Vector2.one;
            Content.offsetMin = new Vector2(SidePadding,
                hasFooter ? FooterBottom + FooterHeight + 7f : FooterBottom);
            Content.offsetMax = new Vector2(-SidePadding, -ContentTop);
        }

        /// <summary>
        /// Badge tiêu đề: viên thuốc xanh chờm lên mép trên, trong có dấu chân nằm
        /// trong vòng tròn trắng. Dựng SAU nội dung để nằm trên cùng thứ tự vẽ.
        /// </summary>
        private void BuildHeader(Font font, string title)
        {
            var go = new GameObject("Header", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(BadgeWidth, BadgeHeight);
            // Tâm badge gần như trùng mép trên popup: nửa trên nhô ra ngoài, nửa dưới
            // dừng NGAY TRÊN mép Content.
            rect.anchoredPosition = new Vector2(0f, 1f);

            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius, PopupPalette.HeaderBlue,
                Color.white, 2f);
            PawBadge.Create(go.transform, 24f, new Vector2(5f, 0f));

            var label = UiBuilder.MakeText(go.transform, font, "Title", 17, true);
            label.text = title;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.rectTransform.offsetMin = new Vector2(26f, 0f);
        }
    }
}
