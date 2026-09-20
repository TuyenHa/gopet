namespace Gopet.UiLogic
{
    /// <summary>
    /// Thay ảnh dải ô nền theo từng map. Bản jar xài chung một bộ tile MÙA ĐÔNG cho
    /// nhiều map; ở đây đổi sang bộ cỏ xanh + viền đá cho hợp khung cảnh mà KHÔNG phải
    /// sửa file map nhị phân — các dải thay thế giữ nguyên thứ tự ô nên cỏ, cạnh và góc
    /// vẫn khớp.
    ///
    /// <para>Thuần C#, không UnityEngine — test được ngoài Unity.</para>
    /// </summary>
    public static class MapSkinOverrides
    {
        /// <summary>Thành Phố Linh Thú — thay cả nền cỏ lẫn viền.</summary>
        public const int BeastCityMapId = 11;

        /// <summary>Đấu trường — dùng chung bộ cỏ xanh + viền đá với thành phố.</summary>
        public const int ArenaMapId = 19;

        /// <summary>Đường lên đỉnh núi — thêm mặt cỏ thay mặt tuyết.</summary>
        public const int MountainPathMapId = 16;

        private const int SnowGrassImageIdA = 161;
        private const int SnowGrassImageIdB = 162;
        private const int SnowBorderImageId = 3;
        private const int GrassImageIdA = 11161;
        private const int GrassImageIdB = 11162;
        private const int StoneBorderImageId = 11003;

        /// <summary>Mặt tuyết và vách đá phủ tuyết của map núi.</summary>
        private const int SnowGroundImageId = 180;
        private const int SnowCliffImageId = 179;

        /// <summary>Bản hết tuyết do <c>tools/image-gen/make-mountain-grass-tiles.py</c> sinh.</summary>
        private const int GreenGroundImageId = 11180;
        private const int GreenCliffImageId = 11179;

        /// <summary>Id ảnh dải thật sự phải vẽ cho <paramref name="imageId"/> của map này.</summary>
        public static int ResolveImageId(int mapId, int imageId)
        {
            if (mapId == MountainPathMapId)
            {
                if (imageId == SnowGroundImageId) return GreenGroundImageId;
                if (imageId == SnowCliffImageId) return GreenCliffImageId;
            }

            if (!UsesSummerSkin(mapId)) return imageId;
            if (imageId == SnowGrassImageIdA) return GrassImageIdA;
            if (imageId == SnowGrassImageIdB) return GrassImageIdB;
            if (imageId == SnowBorderImageId) return StoneBorderImageId;
            return imageId;
        }

        /// <summary>Map mặc áo cỏ xanh + viền đá thay cho bộ tile mùa đông gốc.</summary>
        private static bool UsesSummerSkin(int mapId) =>
            mapId == BeastCityMapId || mapId == ArenaMapId || mapId == MountainPathMapId;
    }
}
