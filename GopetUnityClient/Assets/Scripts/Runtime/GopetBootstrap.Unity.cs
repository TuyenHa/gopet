using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Gopet.Runtime
{
    /// <summary>
    /// Phần dựng thành phần Unity (Canvas, EventSystem) tách khỏi
    /// <see cref="GopetBootstrap"/> chính — file gốc chạm rule 200 dòng, tách theo mối
    /// quan tâm "hạ tầng Unity thuần" cho gọn.
    /// </summary>
    public sealed partial class GopetBootstrap
    {
        private static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // CAO HƠN PixelCanvas một cách tường minh (xem PixelCanvas.SortingOrder):
            // canvas này chứa UiRoot, và server có thể gửi hộp OTP qua đó đúng lúc
            // màn đăng nhập đang hiện. OTP phải luôn thắng.
            canvas.sortingOrder = UI.PixelCanvas.SortingOrder + 1;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1280f);

            return canvas;
        }

        /// <summary>
        /// Không có <c>EventSystem</c> thì không cú chạm nào tới được uGUI, và triệu
        /// chứng là "UI hiện ra nhưng bấm không ăn" — rất dễ đổ oan cho layout.
        ///
        /// <para>Project đặt <c>activeInputHandler = 1</c> (chỉ Input System mới), nên
        /// phải dùng <see cref="InputSystemUIInputModule"/>; <c>StandaloneInputModule</c>
        /// sẽ ném lúc chạy.</para>
        /// </summary>
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
