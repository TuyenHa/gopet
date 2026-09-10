using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Canvas riêng cho màn hình "giống bản jar" (splash, đăng nhập): phóng khung
    /// tham chiếu 320×240 theo bội số nguyên, canh giữa. Toán học nằm ở
    /// <see cref="PixelCanvasLayout"/>, thuần C# và test được ngoài Editor — class
    /// này chỉ dựng hình và áp lại kết quả mỗi khi màn hình đổi kích thước.
    ///
    /// <para><b>Vì sao tách canvas riêng, không dùng chung với <c>UiRoot</c>:</b>
    /// 162 màn menu của server (P5) đã có kích thước tự nhiên trên canvas co giãn
    /// thường (<c>CanvasScaler.ScaleWithScreenSize</c>) và có test PlayMode phủ.
    /// Ép chúng vào khung 240-đơn-vị-cao của jar sẽ bắt phải tính lại mọi hằng số
    /// (chiều cao dòng, cỡ chữ) của P5 — vi phạm đúng ràng buộc "P5.1 không đụng một
    /// dòng logic nào" của phase này. Splash và đăng nhập là màn MỚI, không mang theo
    /// nợ đó, nên chỉ chúng lấy khung pixel-perfect.</para>
    ///
    /// <para>Toàn app vẫn dùng chung một hướng màn hình (khoá ngang từ
    /// <c>ProjectSettings</c>), nên hai canvas luôn phủ cùng một vùng vật lý —
    /// "một hệ toạ độ" ở tầng vật lý, còn tầng logic thì mỗi bên tự quyết đơn vị
    /// của mình.</para>
    /// </summary>
    public sealed class PixelCanvas : MonoBehaviour
    {
        /// <summary>
        /// Thấp hơn <c>GopetBootstrap</c>'s canvas chung một cách CỐ Ý.
        ///
        /// <para>Hai Canvas <c>ScreenSpaceOverlay</c> cùng <c>sortingOrder</c> (mặc
        /// định đều là 0) thì thứ tự vẽ giữa chúng KHÔNG được đảm bảo — đây là đúng
        /// loại lỗi layering đã dính ở P5 giữa <c>UiRoot</c> và <c>LoginScreens</c>,
        /// chỉ khác là lần này ở cấp Canvas thay vì cấp sibling.</para>
        ///
        /// <para>Và hướng chênh lệch PHẢI đúng chiều: server có thể gửi OTP
        /// (<c>TYPE_DIALOG_INPUT</c>, qua <c>UiRoot</c>) đúng lúc
        /// màn hình pixel-art đang hiện trên canvas này. Hộp
        /// OTP phải nằm TRÊN màn đăng nhập giống bản jar, không phải ngược lại — nếu
        /// không thì tái diễn đúng lỗi H1 của P5 (OTP bị che, không bấm được), chỉ
        /// đổi từ ranh giới sibling sang ranh giới Canvas.</para>
        /// </summary>
        public const int SortingOrder = 0;

        private RectTransform _content;
        private Vector2Int _lastScreenSize;

        /// <summary>Nơi đặt widget theo toạ độ gốc của bản jar — kích thước logic là <c>(LogicalWidth, 240)</c>.</summary>
        public RectTransform Content => _content;

        public PixelCanvasLayout Layout { get; private set; }

        public static PixelCanvas Create(Transform parent)
        {
            var go = new GameObject("PixelCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // THẤP HƠN canvas chung của GopetBootstrap một cách TƯỜNG MINH — xem
            // hằng số SortingOrder để hiểu vì sao đây không phải chi tiết vặt.
            canvas.sortingOrder = SortingOrder;

            // ConstantPixelSize, scaleFactor = 1: 1 unit uGUI = đúng 1 pixel màn hình.
            // Bắt buộc — PixelCanvasLayout tính N theo Screen.width/height thật; nếu
            // canvas tự co theo ScaleWithScreenSize thì con số N sẽ sai lệch với
            // pixel thật, và "phóng nguyên khối" mất hết ý nghĩa.
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            var pixelCanvas = go.AddComponent<PixelCanvas>();

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(go.transform, false);
            pixelCanvas._content = (RectTransform)content.transform;

            pixelCanvas.Recompute();
            return pixelCanvas;
        }

        private void Update()
        {
            var current = new Vector2Int(Screen.width, Screen.height);
            if (current == _lastScreenSize) return;

            Recompute();
        }

        private void Recompute()
        {
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            // Một số môi trường (trình chạy test -nographics, khung hình đầu của
            // batchmode) báo Screen.width/height bằng 0 ở đúng frame `Create()` chạy.
            // `PixelCanvasLayout.Compute` ném ở kích thước đó thay vì tự lo — và ném
            // thẳng ra khỏi `GopetBootstrap.Start()` thì cả app không mở được. Bỏ qua
            // lần này; `Update()` sẽ tự thử lại ở frame kế, lúc kích thước đã hợp lệ.
            if (Screen.width <= 0 || Screen.height <= 0) return;

            ApplyLayout(PixelCanvasLayout.Compute(Screen.width, Screen.height));
        }

        /// <summary>
        /// Áp một layout đã tính sẵn vào <see cref="Content"/>. Tách khỏi
        /// <see cref="Recompute"/> để test được: <see cref="PixelCanvasLayout"/> đã
        /// tự test bằng xunit với các cỡ máy cụ thể — ở đây chỉ cần kiểm việc ÁP kết
        /// quả đó vào RectTransform có đúng không, không cần phụ thuộc <c>Screen.*</c>
        /// (giá trị của nó trong PlayMode test phụ thuộc Editor Game View, không
        /// đoán trước được).
        /// </summary>
        public void ApplyLayout(PixelCanvasLayout layout)
        {
            Layout = layout;

            // Neo giữa màn hình, kích thước logic là (LogicalWidth, 240); localScale
            // phóng nó lên đúng N lần để chiều cao khớp bội số nguyên của 240.
            _content.anchorMin = new Vector2(0.5f, 0.5f);
            _content.anchorMax = new Vector2(0.5f, 0.5f);
            _content.pivot = new Vector2(0.5f, 0.5f);
            _content.sizeDelta = new Vector2(layout.LogicalWidth, PixelCanvasLayout.ReferenceHeight);
            _content.anchoredPosition = Vector2.zero;
            _content.localScale = new Vector3(layout.Scale, layout.Scale, 1f);
        }

        /// <summary>
        /// Đổi toạ độ jar (gốc trên-trái, trục Y hướng XUỐNG, đơn vị pixel gốc J2ME)
        /// sang <c>anchoredPosition</c> của uGUI (gốc giữa, trục Y hướng LÊN).
        /// </summary>
        public Vector2 ToAnchoredPosition(float jarX, float jarY)
        {
            var x = jarX - Layout.LogicalWidth / 2f;
            var y = PixelCanvasLayout.ReferenceHeight / 2f - jarY;
            return new Vector2(x, y);
        }
    }
}
