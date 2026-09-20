using System.Collections.Generic;

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

        /// <summary>Linh Lâm — lối tuyết thành đường đất, cỏ giữ nguyên.</summary>
        public const int SpiritForestMapId = 13;

        /// <summary>Đại Linh Cảnh — nền đất y như Linh Lâm, cây thông thành cây dừa, nhà gỗ thành nhà lá.</summary>
        public const int GreatSpiritViewMapId = 15;

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

        /// <summary>Mặt tuyết của lối đi — dùng chung cho nhiều map rừng/núi.</summary>
        private const int SnowPathImageId = 151;

        /// <summary>Bản thay mặt tuyết của lối đi, do <c>tools/image-gen/make-path-reskin-tiles.py</c>
        /// sinh: id bản mới = tiền tố + id gốc (12151, 12161, 12162…).
        ///
        /// <para>Cần RIÊNG bộ 161/162 chứ không dùng được 11161/11162: bộ 11xxx đã là bản cỏ
        /// xanh của áo mùa hè, đổi theo nó thì phần tuyết thành cỏ và lối đi biến mất vào bãi
        /// cỏ — map mất luôn đường.</para></summary>
        private const int DirtVariantPrefix = 12000;

        /// <summary>Vật thể mùa đông của Đại Linh Cảnh và bản nhiệt đới thay chúng, do
        /// <c>tools/image-gen/</c> sinh. Bản thay giữ ĐÚNG khung và neo giữa-đáy của bản gốc,
        /// nên chỗ đứng trên map không đổi một pixel nào.</summary>
        private static readonly Dictionary<int, int> TropicalObjects = new Dictionary<int, int>
        {
            { 158, 14158 },   // cây thông có tuyết -> cây dừa   (make-palm-tree.py)
            { 177, 14177 },   // nhà gỗ phủ tuyết   -> nhà lá    (make-thatch-house.py)
        };

        /// <summary>Id ảnh dải thật sự phải vẽ cho <paramref name="imageId"/> của map này.</summary>
        public static int ResolveImageId(int mapId, int imageId)
        {
            if (mapId == MountainPathMapId)
            {
                if (imageId == SnowGroundImageId) return GreenGroundImageId;
                if (imageId == SnowCliffImageId) return GreenCliffImageId;
            }

            // Hai map rừng dùng CHUNG bộ đất: cùng một dải lối đi, cùng một tông cát.
            if (mapId == SpiritForestMapId || mapId == GreatSpiritViewMapId)
                return DirtPathReskin(imageId);

            if (!UsesSummerSkin(mapId)) return imageId;
            if (imageId == SnowGrassImageIdA) return GrassImageIdA;
            if (imageId == SnowGrassImageIdB) return GrassImageIdB;
            if (imageId == SnowBorderImageId) return StoneBorderImageId;
            return imageId;
        }

        /// <summary>Ba dải mang mặt tuyết của lối đi đổi sang bản đất; dải khác giữ nguyên.
        /// Chỉ cộng tiền tố vì generator đặt tên bản mới theo đúng id gốc.</summary>
        private static int DirtPathReskin(int imageId) =>
            imageId == SnowPathImageId || imageId == SnowGrassImageIdA || imageId == SnowGrassImageIdB
                ? DirtVariantPrefix + imageId
                : imageId;

        /// <summary>Id ảnh VẬT THỂ thật sự phải vẽ. Tách khỏi <see cref="ResolveImageId"/> vì
        /// vật thể và ô nền là hai đường vẽ khác nhau: ô nền tra theo dải, vật thể tra thẳng
        /// theo id ảnh. Gộp chung thì đổi nền sẽ vô tình đổi cả vật thể cùng id.</summary>
        public static int ResolveObjectImageId(int mapId, int imageId) =>
            mapId == GreatSpiritViewMapId && TropicalObjects.TryGetValue(imageId, out var swap)
                ? swap
                : imageId;

        /// <summary>Map mặc áo cỏ xanh + viền đá thay cho bộ tile mùa đông gốc.</summary>
        private static bool UsesSummerSkin(int mapId) =>
            mapId == BeastCityMapId || mapId == ArenaMapId || mapId == MountainPathMapId;
    }
}
