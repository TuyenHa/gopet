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

            // Bám lưới điểm ảnh của màn hình.
            //
            // Canvas khớp CHIỀU RỘNG 720, nên với khung hình 16:9 chiều cao quy ra
            // 405 ref-unit — số lẻ. Popup căn giữa vì thế rơi vào nửa đơn vị
            // ((405−300)/2 = 52.5), và mọi cạnh ngang của nó nằm lệch nửa điểm ảnh.
            // Quad vẽ ở toạ độ lẻ thì lấy mẫu bilinear giữa hai điểm ảnh — góc bo và
            // chữ đều nhoè đi một nửa điểm ảnh, không sprite nào cứu được.
            //
            // Đánh đổi: phần tử UI đang DI CHUYỂN sẽ nhích theo từng điểm ảnh thay vì
            // trôi mượt. Popup và dialog đều đứng yên nên không ảnh hưởng.
            canvas.pixelPerfect = true;

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
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem))
                    .GetComponent<EventSystem>();
            }

            // A scene or an imported package may already provide an EventSystem with
            // StandaloneInputModule. The project uses the new Input System, so keeping
            // that module silently makes uGUI controls (including the movement joystick)
            // ignore touch and mouse input.
            var legacy = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacy != null) legacy.enabled = false;

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
        }
    }
}
