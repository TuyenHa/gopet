using System.Collections.Generic;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nền động của màn đăng nhập: map 11 của bản jar, trôi ngang qua lại.
    ///
    /// <para><b>Đây mới là nền thật của <c>fb.java</c>.</b> Lớp cha <c>fw</c> có
    /// <c>b()</c> tô một màu phẳng (<c>gs.a</c>), nhưng <c>fb</c> GHI ĐÈ nó bằng
    /// <c>this.a.a(this.f, 0, true)</c> — vẽ map. Đọc nhầm hàm của lớp cha là lý do
    /// bản port trước đây chỉ có nền xanh navy trơn.</para>
    ///
    /// <para><b>Cuộn theo THỜI GIAN, không theo frame.</b> Jar cộng <c>this.h = 2</c>
    /// pixel mỗi nhịp game (~15 nhịp/giây trên máy J2ME). Cộng 2 pixel mỗi frame ở
    /// Unity 60 fps sẽ chạy nhanh gấp bốn, và tốc độ còn đổi theo máy khoẻ/yếu.</para>
    /// </summary>
    public sealed class JarMapBackground : MonoBehaviour
    {
        /// <summary>Map nền màn đăng nhập — <c>fb.java</c>: <c>new ef(11, …)</c>.</summary>
        public const int LoginMapId = 11;

        /// <summary>2 pixel/nhịp × ~15 nhịp/giây của bản jar.</summary>
        public const float PixelsPerSecond = 30f;

        private RectTransform _content;
        private JarMapScroll _scroll;

        /// <summary>Vị trí camera ngang hiện tại, pixel — tương đương <c>fb.this.f</c>.</summary>
        public float ScrollX => _scroll.X;

        public float MaxScrollX => _scroll.Max;

        /// <param name="viewWidth">Bề ngang khung nhìn, pixel jar (320 với <see cref="PixelCanvas"/>).</param>
        public static JarMapBackground Create(Transform parent, int mapId, float viewWidth, float viewHeight)
        {
            var go = new GameObject("JarMapBackground", typeof(RectTransform), typeof(RectMask2D));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(viewWidth, viewHeight);
            rect.anchoredPosition = Vector2.zero;

            var view = go.AddComponent<JarMapBackground>();
            view.Build(JarMaps.Load(mapId), viewWidth);
            return view;
        }

        private void Build(JarMapLayout map, float viewWidth)
        {
            var contentGo = new GameObject("MapContent", typeof(RectTransform));
            contentGo.transform.SetParent(transform, false);

            _content = (RectTransform)contentGo.transform;
            _content.anchorMin = _content.anchorMax = _content.pivot = new Vector2(0f, 1f);
            _content.sizeDelta = new Vector2(map.WidthPixels, map.HeightPixels);

            _scroll = new JarMapScroll(map.WidthPixels - viewWidth);

            BuildTiles(map);
            BuildObjects(map);
            Apply();
        }

        private void BuildTiles(JarMapLayout map)
        {
            foreach (var layer in map.Layers)
            {
                for (var row = 0; row < map.HeightTiles; row++)
                {
                    for (var col = 0; col < map.WidthTiles; col++)
                    {
                        var tile = layer[row][col];
                        var strip = JarMapLayout.StripOf(tile);
                        if (strip < 0 || strip >= map.ImageCount) continue;

                        var sprite = TileAssetProvider.CellFromMap(map, strip, JarMapLayout.CellOf(tile));
                        if (sprite == null) continue;

                        Place(sprite, col * JarMapLayout.TileSize, row * JarMapLayout.TileSize, new Vector2(0f, 1f));
                    }
                }
            }
        }

        /// <summary>
        /// Vật thể xếp theo Y tăng dần rồi dựng lần lượt: uGUI vẽ theo thứ tự anh em,
        /// nên cây ở gần (Y lớn) phải dựng SAU để che cây ở xa, đúng như jar.
        /// </summary>
        private void BuildObjects(JarMapLayout map)
        {
            var ordered = new List<JarMapObject>(map.Objects);
            ordered.Sort((a, b) => a.Y.CompareTo(b.Y));

            foreach (var item in ordered)
            {
                var index = item.ResourceIndex;
                if (index < 0 || index >= map.ResourceIds.Length) continue;

                var id = map.ResourceIds[index];

                // Vật thể hoạt ảnh nằm ở "<id>_a.png"; ở đây chỉ lấy khung đầu, phần
                // chạy hoạt ảnh thuộc P6.
                var path = map.ResourceTypes[index] == JarMapLayout.TypeAnimation
                    ? $"newMapData/{id}_a"
                    : $"newMapData/{id}";

                // Toạ độ là CHÂN vật thể: neo giữa-dưới, và jar vẽ tại y - yOffset.
                Place(JarSkin.Raw(path), item.X, item.Y - item.YOffset, new Vector2(0.5f, 1f));
            }
        }

        /// <param name="pivot">Điểm neo của sprite trong ô: (0,1) cho ô nền, (0.5,1) cho chân vật thể.</param>
        private void Place(Sprite sprite, float jarX, float jarY, Vector2 pivot)
        {
            var go = new GameObject(sprite.name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_content, false);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false; // nền không được nuốt cú chạm của ô nhập phía trên

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = pivot;
            rect.sizeDelta = sprite.rect.size;

            // Hệ jar: gốc trên-trái, Y hướng XUỐNG. uGUI: Y hướng LÊN.
            rect.anchoredPosition = new Vector2(jarX, -jarY);
        }

        private void Update()
        {
            _scroll.Advance(Time.deltaTime, PixelsPerSecond);
            Apply();
        }

        /// <summary>
        /// Cả hai neo đều ở góc TRÊN-TRÁI, nên <c>anchoredPosition = (0,0)</c> là map
        /// khớp mép khung nhìn. Cuộn sang phải = kéo map sang trái.
        /// </summary>
        private void Apply()
        {
            _content.anchoredPosition = new Vector2(-_scroll.X, 0f);
        }
    }
}
