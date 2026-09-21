using System;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;
namespace Gopet.Runtime.UI
{
    public sealed class MinimapWidget : MonoBehaviour
    {
        /// <summary>
        /// Minimap là Ô NGOÀI CÙNG BÊN PHẢI của hàng Cửa hàng/Dịch vụ/Sự kiện/Hộp thư —
        /// cùng chiều cao, cùng mép trên, ngay cạnh "Hộp thư". Mọi kích thước lấy từ
        /// <see cref="ShopServiceEventHud"/> nên hai bên không thể lệch hàng nhau.
        /// </summary>
        private const float SizeFrac = ShopServiceEventHud.MinimapSlotFrac;
        private const float MarginFrac = ShopServiceEventHud.ReservedRightFrac;
        private const float TopRowFrac = ShopServiceEventHud.TopMarginFrac;

        /// <summary>
        /// Cao hơn nút một chút: đáy minimap ngang ĐÁY CHỮ "Sự kiện" bên cạnh, không
        /// phải ngang đáy icon — nhìn mới thành một hàng liền khối.
        /// </summary>
        private const float HeightFrac = SizeFrac * (1f + ShopServiceEventHud.LabelBottomFrac);
        /// <summary>Viền mảnh thôi — viền dày ăn mất phần map vốn đã bé tí.</summary>
        private const float BorderPx = 2f;

        /// <summary>Xanh dương cho viền, cùng tông với khung HUD còn lại.</summary>
        private static readonly Color BorderColor = new Color(0.14f, 0.53f, 0.96f, 1f);

        /// <summary>
        /// Màu ruột khi CHƯA có ảnh map. RawImage không texture thì Unity vẽ nguyên một
        /// mảng TRẮNG — đúng cái ô trắng trơn nhìn như widget hỏng.
        /// </summary>
        private static readonly Color EmptyMapColor = new Color(0.04f, 0.09f, 0.13f, 1f);
        private RawImage _mapImage;
        private RectTransform _viewport, _dot;
        private Text _fallback;
        private PlayerAvatar _follow;
        private int _mapWidth, _mapHeight;
        public event Action Clicked;
        public static MinimapWidget Create(Transform parent, Font font)
        {
            var root = new GameObject("MinimapWidget", typeof(RectTransform), typeof(Image), typeof(Button)); root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(1f - MarginFrac - SizeFrac, 1f - TopRowFrac - HeightFrac);
            rect.anchorMax = new Vector2(1f - MarginFrac, 1f - TopRowFrac);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var view = root.AddComponent<MinimapWidget>();
            // Dùng chính nền chữ nhật làm viền qua inset BorderPx của ảnh map;
            // không tạo Border GameObject và không bo góc.
            var background = root.GetComponent<Image>();
            background.color = BorderColor;

            // Ruột TỐI lót kín bên trong viền. Không có nó thì chỗ ảnh map không phủ
            // tới (map và ô HUD khác tỉ lệ nên luôn thừa trên-dưới hoặc hai bên) lộ ra
            // chính màu viền, nhìn thành một cái viền dày cộp.
            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(root.transform, false);
            var innerRect = (RectTransform)inner.transform;
            UiBuilder.Stretch(innerRect);
            innerRect.offsetMin = new Vector2(BorderPx, BorderPx);
            innerRect.offsetMax = new Vector2(-BorderPx, -BorderPx);
            var innerImage = inner.GetComponent<Image>();
            innerImage.color = EmptyMapColor;
            innerImage.raycastTarget = false;

            var viewport = new GameObject("Map", typeof(RectTransform), typeof(RawImage));
            viewport.transform.SetParent(root.transform, false);
            view._viewport = (RectTransform)viewport.transform;
            UiBuilder.Stretch(view._viewport);
            view._viewport.offsetMin = new Vector2(BorderPx, BorderPx);
            view._viewport.offsetMax = new Vector2(-BorderPx, -BorderPx);
            view._mapImage = viewport.GetComponent<RawImage>();
            view._mapImage.raycastTarget = false;
            view._mapImage.color = EmptyMapColor;
            var dot = new GameObject("PlayerDot", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(view._viewport, false);
            view._dot = (RectTransform)dot.transform;
            view._dot.sizeDelta = new Vector2(10f, 10f);
            dot.SetActive(false);
            var dotImage = dot.GetComponent<Image>();
            dotImage.color = new Color(1f, 0.12f, 0.08f);
            dotImage.raycastTarget = false;
            RoundedUiSprite.Apply(dotImage);
            view._fallback = UiBuilder.MakeText(root.transform, font, "Fallback", 11, true);
            view._fallback.alignment = TextAnchor.MiddleCenter; view._fallback.text = "Bản đồ";
            root.GetComponent<Button>().onClick.AddListener(() => view.Clicked?.Invoke());
            return view;
        }
        /// <summary>
        /// Gắn ảnh map đang được <see cref="World.MinimapCamera"/> chụp trực tiếp. Ảnh
        /// là live: cây cối, NPC, người chơi khác đổi chỗ thì minimap đổi theo, không
        /// phải chụp một lần rồi thôi.
        /// </summary>
        /// <param name="texture">Ảnh camera minimap, <c>null</c> khi chưa có map.</param>
        public void SetLiveMap(Texture texture, int mapWidthPixels, int mapHeightPixels, int mapId)
        {
            _mapWidth = mapWidthPixels; _mapHeight = mapHeightPixels;
            _follow = null;
            _dot.gameObject.SetActive(false);
            if (texture == null || _mapWidth <= 0 || _mapHeight <= 0)
            {
                _mapImage.texture = null;
                ShowFallback(mapId);
                return;
            }

            _mapImage.texture = texture; _mapImage.color = Color.white;
            _mapImage.enabled = true; _fallback.enabled = false;
            FitViewport(_mapWidth, _mapHeight);
        }
        public void BindPlayer(PlayerAvatar avatar)
        {
            _follow = avatar;
            _dot.gameObject.SetActive(avatar != null && _mapWidth > 0 && _mapHeight > 0);
        }
        private void Update()
        {
            if (_follow == null || _mapWidth <= 0 || _mapHeight <= 0) return;
            var (x, y) = _follow.JarPosition;
            var anchor = new Vector2(Mathf.Clamp01((float)x / _mapWidth),
                1f - Mathf.Clamp01((float)y / _mapHeight));
            _dot.anchorMin = _dot.anchorMax = anchor;
            _dot.anchoredPosition = Vector2.zero;
        }
        private void FitViewport(float width, float height)
        {
            var ratio = width / height;
            _viewport.offsetMin = new Vector2(BorderPx, BorderPx);
            _viewport.offsetMax = new Vector2(-BorderPx, -BorderPx);
            if (ratio >= 1f)
            {
                var pad = (1f - 1f / ratio) * 0.5f;
                _viewport.anchorMin = new Vector2(0f, pad); _viewport.anchorMax = new Vector2(1f, 1f - pad);
            }
            else
            {
                var pad = (1f - ratio) * 0.5f;
                _viewport.anchorMin = new Vector2(pad, 0f); _viewport.anchorMax = new Vector2(1f - pad, 1f);
            }
        }
        private void ShowFallback(int mapId)
        {
            // Giữ RawImage BẬT nhưng không texture + màu tối: tắt hẳn thì lộ nguyên nền
            // viền, cả ô thành một mảng xanh đặc.
            _mapImage.enabled = true; _mapImage.color = EmptyMapColor;
            _fallback.enabled = true;
            _fallback.text = mapId > 0 ? $"Map {mapId}" : "Bản đồ";
        }
    }
}
