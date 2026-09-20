using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Camera phụ chụp TRỌN map hiện tại vào một <see cref="RenderTexture"/> cho minimap
    /// góc HUD.
    ///
    /// <para>Trước đây minimap tự "bake" từng ô tile ra texture. Cách đó chỉ dựng được
    /// lớp nền: cây, nhà, cổng, NPC, người chơi đều là object riêng nên minimap trống
    /// trải, nhìn không ra khung cảnh. Camera thì chụp đúng những gì đang có trên map,
    /// kể cả người chơi khác đang chạy qua.</para>
    ///
    /// <para><b>Camera bật liên tục.</b> URP không cho gọi <c>Camera.Render()</c> thủ
    /// công nên không thể throttle bằng cách tắt camera rồi tự render. Bù lại texture
    /// chỉ tối đa <see cref="MaxTextureSize"/> px cạnh dài và cảnh là sprite 2D phẳng.</para>
    /// </summary>
    public sealed class MinimapCamera : MonoBehaviour
    {
        /// <summary>Cạnh dài nhất của ảnh minimap. Ô HUD chỉ ~90px nên 256 là thừa nét.</summary>
        public const int MaxTextureSize = 256;

        /// <summary>Lùi hẳn ra sau mặt phẳng sprite (z≈0) để không cắt mất lớp nào.</summary>
        private const float CameraZ = -50f;

        /// <summary>Nền lộ ra khi map không lấp kín khung — cùng tông với ruột minimap.</summary>
        private static readonly Color Backdrop = new Color(0.04f, 0.09f, 0.13f, 1f);

        private Camera _cam;
        private RenderTexture _target;

        /// <summary>Ảnh map đang chụp, hoặc <c>null</c> khi chưa có map.</summary>
        public RenderTexture Target => _target;

        public static MinimapCamera Attach(Transform parent)
        {
            var go = new GameObject("Minimap Camera", typeof(Camera));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<MinimapCamera>();
            view._cam = go.GetComponent<Camera>();
            view._cam.orthographic = true;
            view._cam.clearFlags = CameraClearFlags.SolidColor;
            view._cam.backgroundColor = Backdrop;
            view._cam.nearClipPlane = 0.03f;
            view._cam.farClipPlane = 200f;
            view._cam.allowHDR = false;
            view._cam.allowMSAA = false;
            view._cam.useOcclusionCulling = false;
            // Không có targetTexture là camera này vẽ đè lên màn hình chính.
            view._cam.enabled = false;
            return view;
        }

        /// <summary>
        /// Đưa camera về giữa map và zoom vừa đủ thấy trọn map. Ảnh được tạo ĐÚNG tỉ lệ
        /// map nên minimap không phải chèn thêm viền đen hai bên.
        /// </summary>
        public void Frame(int mapWidthPixels, int mapHeightPixels)
        {
            if (_cam == null) return;
            if (mapWidthPixels <= 0 || mapHeightPixels <= 0)
            {
                _cam.enabled = false;
                return;
            }

            var scale = Mathf.Min(1f, (float)MaxTextureSize / Mathf.Max(mapWidthPixels, mapHeightPixels));
            EnsureTarget(Mathf.Max(1, Mathf.RoundToInt(mapWidthPixels * scale)),
                Mathf.Max(1, Mathf.RoundToInt(mapHeightPixels * scale)));

            // Map trải từ (0,0) tới (W,H) trong world (xem MapPlacement.JarToWorld).
            var width = mapWidthPixels / MapPlacement.PixelsPerUnit;
            var height = mapHeightPixels / MapPlacement.PixelsPerUnit;
            transform.localPosition = new Vector3(width * 0.5f, height * 0.5f, CameraZ);
            _cam.orthographicSize = height * 0.5f;
            _cam.enabled = true;
        }

        private void EnsureTarget(int width, int height)
        {
            if (_target != null && _target.width == width && _target.height == height) return;
            Release();
            _target = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            {
                name = "Minimap",
                filterMode = FilterMode.Bilinear
            };
            _target.Create();
            _cam.targetTexture = _target;
        }

        private void Release()
        {
            if (_target == null) return;
            if (_cam != null) _cam.targetTexture = null;
            _target.Release();
            Destroy(_target);
            _target = null;
        }

        private void OnDestroy() => Release();
    }
}
