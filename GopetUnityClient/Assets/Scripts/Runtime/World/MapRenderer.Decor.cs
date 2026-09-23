using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Vật trang trí client tự thêm vào map, KHÔNG có trong file .dat.
    ///
    /// <para>Cùng cách làm với vườn bắc Thành Phố Linh Thú: thêm cảnh cho map mà không
    /// phải sửa một byte nào của map nhị phân, nên bản jar gốc vẫn giải được y nguyên.</para>
    ///
    /// <para><b>Chỉ là CẢNH.</b> Lớp va chạm nằm trong .dat nên vật ở đây không chặn
    /// đường — nhân vật đi xuyên qua được. Chọn chỗ đặt sao cho đó không thành chuyện
    /// (góc trống, không nằm trên lối đi chính).</para>
    /// </summary>
    public sealed partial class MapRenderer
    {
        /// <summary>Ao sen do <c>tools/image-gen/make-lotus-pond.py</c> sinh — 132x84, neo giữa-đáy.</summary>
        private const int LotusPondImageId = 14200;

        /// <summary>
        /// Chỗ đặt ao, toạ độ pixel của map (gốc trên-trái), điểm neo là GIỮA-ĐÁY ao.
        ///
        /// <para>Khoảng trống duy nhất trên map 15 đủ chỗ cho ao 132x84 mà không đụng
        /// tán cây hay cổng dịch chuyển nào — quét toàn map ra đúng một vùng quanh
        /// (494..506, 264..276). Đổi số ở đây — hay đổi cỡ ao — là phải quét lại.</para>
        /// </summary>
        private const int LotusPondX = 500;
        private const int LotusPondY = 268;

        private void BuildGreatSpiritViewPond()
        {
            var pond = new GameObject("Great Spirit View Lotus Pond");
            pond.transform.SetParent(transform, false);

            // DƯỚI mọi vật thể của .dat (chỉ số 0 là thấp nhất, trừ thêm 1 để chắc chắn):
            // ao là mặt nước nằm trên nền đất, cây cỏ trước ao phải đè lên được. Đặt cùng
            // dải vật thể chứ không dùng số tự chế — vẫn phải nằm trên các Tilemap nền.
            var order = MapPlacement.ObjectSortingOrder(0) - 1;
            var sprite = TileAssetProvider.FootObject($"newMapData/{LotusPondImageId}");
            PlaceObject(pond.transform, sprite, LotusPondX, LotusPondY, order);
        }

        /// <summary>Hồ sen có con ếch (tools/image-gen, arena-lotus-pond-frog) — 110x63, neo giữa-đáy.</summary>
        private const int ArenaPondImageId = 14201;

        /// <summary>
        /// Đặt vào giữa mảnh sân riêng của cửa hàng Thức ăn cũ: ô nền x 192..384, y 24..96
        /// (dưới là đường lát đá, bắt đầu từ y = 96). Neo theo TÂM MẢNH SÂN (x = 288) chứ
        /// không theo toạ độ cửa hàng (274) — cửa hàng vốn lệch trái trên sân của nó.
        /// Đáy 92 chừa 4px trước mép đường đá.
        /// </summary>
        private const int ArenaPondX = 288;
        private const int ArenaPondY = 92;

        /// <summary>
        /// Vật thể .dat của Đấu trường bị bỏ vì hồ sen chiếm chỗ: nền + hoạt ảnh cửa hàng
        /// Thức ăn (189/190), hai bụi cây nhỏ hai bên cửa hàng (175 ở y=68) và đống tuyết
        /// dưới chân nó (30 tại (253, 91)) — để lại thì chúng mọc lên giữa mặt nước.
        /// </summary>
        private bool IsHiddenArenaObject(int resourceId, JarMapObject item)
        {
            if (_mapId != ArenaMapId) return false;
            if (resourceId == ShopBuildingAnimationId || resourceId == ShopBuildingBaseImageId) return true;
            if (resourceId == 175 && item.Y == 68 && item.X > 200 && item.X < 340) return true;
            return resourceId == 30 && item.X == 253 && item.Y == 91;
        }

        private void BuildArenaPond()
        {
            var pond = new GameObject("Arena Lotus Pond");
            pond.transform.SetParent(transform, false);
            // Như ao Đại Linh Cảnh: dưới mọi vật thể .dat, trên các lớp nền.
            var order = MapPlacement.ObjectSortingOrder(0) - 1;
            var sprite = TileAssetProvider.FootObject($"newMapData/{ArenaPondImageId}");
            PlaceObject(pond.transform, sprite, ArenaPondX, ArenaPondY, order);
        }
    }
}
