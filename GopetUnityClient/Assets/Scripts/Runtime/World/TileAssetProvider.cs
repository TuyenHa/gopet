using System.Collections.Generic;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Cắt sprite 24×24 từ ảnh dải trong <c>newMapData/</c>, cache tĩnh cho toàn app.
    ///
    /// <para><b>Cache TĨNH</b> vì mỗi <see cref="Sprite.Create"/> sinh object mới:
    /// cache cục bộ theo màn (như bản đầu của <c>JarMapBackground</c> ở P5.1) nghĩa
    /// là mỗi lần chuyển map lại bỏ lại vài chục sprite không ai thu hồi.</para>
    ///
    /// <para>Dùng chung cho cả <see cref="JarMapBackground"/> (UI Canvas, màn đăng
    /// nhập) và <c>MapRenderer</c> (world-space, gameplay) — DRY, một nguồn cắt sprite.</para>
    ///
    /// <para><b>PPU = 1</b> nhất quán với <see cref="MapPlacement.PixelsPerUnit"/>. Import
    /// setting của asset là 32 — cứ dùng thẳng thì 24 px = 0.75 world unit, camera view
    /// 240 unit sẽ hiện tile bằng 1/300 màn (đốm li ti). Ép PPU khi <c>Sprite.Create</c>
    /// là chỗ duy nhất kiểm soát được cho tile và object sinh động lúc chạy.</para>
    /// </summary>
    public static class TileAssetProvider
    {
        /// <summary>PPU dùng cho mọi sprite world-space: 1 pixel jar = 1 world unit. Khớp <see cref="MapPlacement.PixelsPerUnit"/>.</summary>
        public const float WorldPixelsPerUnit = 1f;

        /// <summary>Khoá cache = imageId × 100 + cellIndex. Dùng imageId thay vì chỉ số strip vì hai map khác nhau có cùng chỉ số nhưng khác ảnh.</summary>
        private static readonly Dictionary<int, Sprite> Cache = new Dictionary<int, Sprite>();

        /// <summary>Cache riêng cho ảnh nguyên (avatar, vật thể) đã chuẩn hoá về PPU=1.</summary>
        private static readonly Dictionary<string, Sprite> ObjectCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<int, Tile> TileCache = new Dictionary<int, Tile>();

        /// <summary>Lấy sprite một ô 24×24 theo id ảnh dải + chỉ số ô, hoặc null nếu ô nằm ngoài ảnh.</summary>
        public static Sprite Cell(int imageId, int cellIndex)
        {
            var key = imageId * 100 + cellIndex;

            // Kiểm "!= null" vì cache tĩnh giữ tham chiếu qua Play Mode, còn sprite
            // do Sprite.Create sinh bị huỷ lúc Editor thoát Play — lần chạy sau map
            // hiện toàn ô trống nếu không kiểm.
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;

            var source = JarSkin.Raw($"newMapData/{imageId}");
            var texture = source.texture;
            var x = cellIndex * JarMapLayout.TileSize;

            // Ô nằm ngoài ảnh dải: dữ liệu map trỏ sai. Bỏ qua thay vì ném — một ô
            // trống dễ thấy hơn hẳn màn hình không dựng nổi.
            if (x + JarMapLayout.TileSize > texture.width) return Cache[key] = null;

            var rect = new Rect(x, 0f, JarMapLayout.TileSize, JarMapLayout.TileSize);
            return Cache[key] = Sprite.Create(texture, rect, new Vector2(0f, 1f), WorldPixelsPerUnit);
        }

        /// <summary>Đọc sprite cho một ô trong map — tiện gọi từ vòng lặp Layers.</summary>
        public static Sprite CellFromMap(JarMapLayout map, int strip, int cell)
        {
            if (strip < 0 || strip >= map.ImageCount) return null;
            return Cell(map.ResourceIds[strip], cell);
        }

        public static Tile TileFromMap(JarMapLayout map, int strip, int cell)
        {
            if (strip < 0 || strip >= map.ImageCount) return null;
            return TileFromImage(map.ResourceIds[strip], cell);
        }

        /// <summary>
        /// Lấy tile trực tiếp theo id ảnh. MapRenderer dùng hàm này cho các skin map cục bộ
        /// có cùng bố cục ô với strip gốc (ví dụ viền đá riêng của Thành Phố Linh Thú).
        /// </summary>
        public static Tile TileFromImage(int imageId, int cell)
        {
            var key = imageId * 100 + cell;
            if (TileCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var sprite = Cell(imageId, cell);
            if (sprite == null) return null;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            return TileCache[key] = tile;
        }

        /// <summary>
        /// Lấy sprite nguyên (vật thể / avatar) từ jar và chuẩn hoá về <see cref="WorldPixelsPerUnit"/>.
        /// Import PPU của asset là 32 — không dùng thẳng cho world-space vì làm sprite bằng
        /// 1/32 kích thước mong muốn. Ép <c>Sprite.Create</c> với PPU=1.
        /// </summary>
        public static Sprite Object(string relativePath)
        {
            return Object(relativePath, null);
        }

        /// <summary>Sprite object map: toạ độ server/JAR là điểm giữa đáy ảnh.</summary>
        public static Sprite FootObject(string relativePath)
        {
            return Object(relativePath, new Vector2(0.5f, 0f));
        }

        private static Sprite Object(string relativePath, Vector2? forcedPivot)
        {
            var key = forcedPivot.HasValue ? relativePath + "|foot" : relativePath;
            if (ObjectCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var source = JarSkin.Raw(relativePath);
            var pivot = forcedPivot ??
                new Vector2(source.pivot.x / source.rect.width, source.pivot.y / source.rect.height);
            return ObjectCache[key] = Sprite.Create(source.texture, source.rect, pivot, WorldPixelsPerUnit);
        }

        /// <summary>Chỉ dùng cho test — xoá cache để mỗi test bắt đầu sạch.</summary>
        internal static void ClearForTests()
        {
            Cache.Clear();
            ObjectCache.Clear();
            TileCache.Clear();
        }
    }
}
