using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Camera bám nhân vật kiểu bản jar. LateUpdate mỗi frame để chạy SAU
    /// <see cref="PlayerAvatar"/> đã nội suy — nếu Update thì camera đi trước avatar 1 frame.
    ///
    /// <para><b>Chỉ mở rộng theo chiều cao</b> — khớp <see cref="PixelCanvasLayout"/>:
    /// jar 240 px cao, còn ngang tuỳ tỉ lệ máy. Camera pixel-perfect (orthographic,
    /// size = 120 world units = 240 jar pixels ÷ 2).</para>
    /// </summary>
    public sealed class CameraFollower : MonoBehaviour
    {
        private Camera _cam;
        private MapScene _scene;

        /// <summary>Nhân vật đang face phải. MovementController cập nhật khi player đổi hướng.</summary>
        public bool FaceRight { get; set; } = true;

        /// <summary>Gắn CameraFollower + cấu hình camera pixel-perfect cho <paramref name="scene"/>.</summary>
        public static CameraFollower Attach(Camera cam, MapScene scene)
        {
            if (cam == null) return null;

            var go = new GameObject("CameraFollower");
            go.transform.SetParent(scene.transform, false);

            var f = go.AddComponent<CameraFollower>();
            f._cam = cam;
            f._scene = scene;

            cam.orthographic = true;
            cam.orthographicSize = PixelCanvasLayout.ReferenceHeight / (2f * MapPlacement.PixelsPerUnit);

            f.Recenter();
            return f;
        }

        /// <summary>
        /// Đặt camera về tâm map hiện tại. Gọi lúc Attach và MỖI KHI đổi map — nếu không,
        /// camera đứng nguyên chỗ cũ, và với map mới nhỏ hơn thì nhìn ra ngoài hoàn toàn.
        /// </summary>
        public void Recenter()
        {
            if (_cam == null || _scene?.Map?.Map == null) return;
            var (cx, cy) = MapPlacement.MapCenterWorld(_scene.Map.Map);
            var z = _cam.transform.position.z;
            _cam.transform.position = new Vector3(cx, cy, z < 0f ? z : -10f);
        }

        private void LateUpdate()
        {
            if (_cam == null || _scene?.Map?.Map == null) return;
            var self = _scene.Self;
            if (self == null) return;

            // ĐỌC LẠI kích thước map MỖI FRAME, không cache lúc Attach: MapScene có thể
            // LoadMap sang map khác (server đưa tới map không phải 11), và số cũ làm
            // clamp sai hoàn toàn — camera nhìn ra ngoài map mới.
            var map = _scene.Map.Map;
            var mapW = map.WidthPixels;
            var mapH = map.HeightPixels;

            // Kích thước viewport THẬT của camera trong world unit: chiều cao = 2×orthographicSize,
            // chiều rộng = cao × aspect. KHÔNG dùng PixelCanvasLayout.LogicalWidth ở đây — cái đó
            // là width logic của UI canvas, không phải camera. Dùng nhầm thì camera offset lệch,
            // map hiện chỉ một góc màn (trả giá đã từng).
            // Thu tầm nhìn nếu map hẹp/thấp hơn khung nhìn, nếu không hai mép lộ màu nền
            // camera. Tính LẠI mỗi frame chứ không đặt một lần lúc Attach: đổi map thì kích
            // thước map đổi, mà xoay máy hay kéo cửa sổ thì aspect cũng đổi.
            // Kẹp theo biên map TRƯỚC, chốt tỉ lệ nguyên SAU: chốt trước rồi kẹp thì cái
            // kẹp lại trả về một cỡ lẻ, mất công chốt.
            _cam.orthographicSize = CameraClamp.SnapOrthographicSize(
                CameraClamp.FitOrthographicSize(
                    PixelCanvasLayout.ReferenceHeight / (2f * MapPlacement.PixelsPerUnit),
                    mapW, mapH, _cam.aspect),
                Screen.height);

            var worldViewH = _cam.orthographicSize * 2f;
            var worldViewW = worldViewH * _cam.aspect;
            var effW = System.Math.Min(mapW, (int)(worldViewW * MapPlacement.PixelsPerUnit));
            var effH = System.Math.Min(mapH, (int)(worldViewH * MapPlacement.PixelsPerUnit));

            var jarX = (int)(self.transform.localPosition.x * MapPlacement.PixelsPerUnit);
            var jarY = mapH - (int)(self.transform.localPosition.y * MapPlacement.PixelsPerUnit);

            var (camX, camY) = CameraClamp.TopLeftFor(jarX, jarY, effW, effH, FaceRight);
            camX = CameraClamp.ClampAxis(camX, effW, mapW);
            camY = CameraClamp.ClampAxis(camY, effH, mapH);

            // Camera Unity đặt ở TÂM viewport (Y up), còn (camX, camY) là góc trên-trái jar.
            var centerX = (camX + effW / 2f) / MapPlacement.PixelsPerUnit;
            var centerY = (mapH - (camY + effH / 2f)) / MapPlacement.PixelsPerUnit;

            // Pixel-perfect: snap camera position vào lưới INTEGER pixel. Tile textures đã
            // FilterMode.Point, nhưng nếu camera đứng ở toạ độ lẻ (say 172.5) thì mỗi
            // sprite pixel vẽ lệch nửa pixel màn hình — sinh viền mờ khi cuộn map.
            centerX = Mathf.Round(centerX);
            centerY = Mathf.Round(centerY);

            var z = _cam.transform.position.z;
            _cam.transform.position = new Vector3(centerX, centerY, z);
        }
    }
}
