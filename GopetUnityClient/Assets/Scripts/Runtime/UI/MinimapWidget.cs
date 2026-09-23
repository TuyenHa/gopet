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
        /// <summary>Lề phải của minimap — sát mép màn hình, bằng lề trên <see cref="TopFrac"/>.</summary>
        public const float MarginFrac = 0.003f;
        private const float TopRowFrac = ShopServiceEventHud.TopMarginFrac;

        /// <summary>
        /// Minimap nhô cao hơn hàng nút, sát mép trên màn hình, để ô map to hơn một chút.
        /// </summary>
        private const float TopFrac = 0.003f;

        /// <summary>
        /// Đáy minimap ngang ĐÁY CHỮ "Sự kiện" bên cạnh, không phải ngang đáy icon — nhìn
        /// mới thành một hàng liền khối. Mép trên kéo lên sát lề (<see cref="TopFrac"/>).
        /// </summary>
        public const float BottomFrac = TopRowFrac + SizeFrac * (1f + ShopServiceEventHud.LabelBottomFrac);
        /// <summary>Viền mảnh thôi — viền dày ăn mất phần map vốn đã bé tí.</summary>
        private const float BorderPx = 2f;

        /// <summary>Xanh dương cho viền, cùng tông với khung HUD còn lại.</summary>
        private static readonly Color BorderColor = new Color(0.14f, 0.53f, 0.96f, 1f);

        /// <summary>
        /// Màu ruột khi CHƯA có ảnh map. RawImage không texture thì Unity vẽ nguyên một
        /// mảng TRẮNG — đúng cái ô trắng trơn nhìn như widget hỏng.
        /// </summary>
        private static readonly Color EmptyMapColor = new Color(0.04f, 0.09f, 0.13f, 1f);

        /// <summary>
        /// Dải tên map trên đỉnh ô, nằm trên nền tối. Ảnh map phủ phần còn lại bên dưới
        /// nên chữ không bao giờ đè lên map.
        /// </summary>
        private const float TitleFrac = 0.24f;
        private const float MapAreaTop = 1f - TitleFrac;
        private RawImage _mapImage;
        private RectTransform _viewport, _dot;
        private Text _fallback, _title;
        private PlayerAvatar _follow;
        private int _mapWidth, _mapHeight;
        public event Action Clicked;
        public static MinimapWidget Create(Transform parent, Font font)
        {
            var root = new GameObject("MinimapWidget", typeof(RectTransform), typeof(Image), typeof(Button)); root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(1f - MarginFrac - SizeFrac, 1f - BottomFrac);
            rect.anchorMax = new Vector2(1f - MarginFrac, 1f - TopFrac);
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
            // Ảnh map phủ KÍN vùng dưới dải tên (trái-phải sát viền), chấp nhận co giãn
            // nhẹ theo tỉ lệ ô — letterbox để lại hai dải đen hai bên trông như map bị hụt.
            view._viewport.anchorMax = new Vector2(1f, MapAreaTop);
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
            view._fallback.rectTransform.anchorMax = new Vector2(1f, MapAreaTop);
            view._title = CreateTitle(root.transform, font);
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
            _title.text = mapId > 0 ? MapDisplayNames.Get(mapId) : "Bản đồ";
            _mapWidth = mapWidthPixels; _mapHeight = mapHeightPixels;
            _follow = null;
            _dot.gameObject.SetActive(false);
            if (texture == null || _mapWidth <= 0 || _mapHeight <= 0)
            {
                _mapImage.texture = null;
                ShowFallback();
                return;
            }

            _mapImage.texture = texture; _mapImage.color = Color.white;
            _mapImage.enabled = true; _fallback.enabled = false;
        }

        /// <summary>Tên map hiện tại trên đỉnh minimap.</summary>
        public string MapName => _title.text;
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
        private static Text CreateTitle(Transform parent, Font font)
        {
            var title = UiBuilder.MakeText(parent, font, "Map Title", 11, true);
            var rect = title.rectTransform;
            rect.anchorMin = new Vector2(0f, MapAreaTop);
            rect.offsetMin = new Vector2(BorderPx + 2f, 0f);
            rect.offsetMax = new Vector2(-BorderPx - 2f, -BorderPx);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            // Tên map dài tự thu nhỏ cho vừa ô thay vì tràn ra ngoài.
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.verticalOverflow = VerticalWrapMode.Truncate;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 6;
            title.resizeTextMaxSize = 11;
            title.raycastTarget = false;
            title.text = "Bản đồ";
            return title;
        }

        private void ShowFallback()
        {
            // Giữ RawImage BẬT nhưng không texture + màu tối: tắt hẳn thì lộ nguyên nền
            // viền, cả ô thành một mảng xanh đặc.
            _mapImage.enabled = true; _mapImage.color = EmptyMapColor;
            // Tên map đã nằm ở dải trên đỉnh; ruột chỉ cần báo là chưa có ảnh.
            _fallback.enabled = true;
        }
    }
}
