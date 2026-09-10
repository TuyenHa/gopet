namespace Gopet.UiLogic
{
    /// <summary>
    /// Chuyển đổi toạ độ jar (pixel, Y hướng xuống, gốc trên-trái) sang toạ độ world
    /// của Unity (unit, Y hướng lên, gốc dưới-trái của map). Thuần C# — testable.
    ///
    /// <para>Tách khỏi <c>MapRenderer</c> vì đây là math, không cần MonoBehaviour: mọi
    /// vòng lặp qua ô/vật thể đều gọi cùng công thức, và bug lệch trục Y là loại lỗi
    /// dễ tìm nhất khi có test đứng riêng.</para>
    /// </summary>
    public static class MapPlacement
    {
        /// <summary>1 pixel jar = 1 unit world. Đơn giản, không lẫn khi debug camera.</summary>
        public const float PixelsPerUnit = 1f;

        /// <summary>Chuyển toạ độ jar (jarX pixel, jarY pixel từ TRÊN xuống) sang world (X, Y từ DƯỚI lên).</summary>
        public static (float x, float y) JarToWorld(int jarX, int jarY, int mapHeightPixels)
        {
            return (jarX / PixelsPerUnit, (mapHeightPixels - jarY) / PixelsPerUnit);
        }

        /// <summary>Ngược lại: world → jar. Dùng khi chuyển toạ độ click chuột thành đích di chuyển.</summary>
        public static (int jarX, int jarY) WorldToJar(float worldX, float worldY, int mapHeightPixels)
        {
            return ((int)(worldX * PixelsPerUnit), (int)(mapHeightPixels - worldY * PixelsPerUnit));
        }

        /// <summary>
        /// Đổi vector (dx, dy) trên trục JAR (Y hướng xuống) thành hướng 4 chiều 0..3.
        /// 0=đông, 1=tây, 2=nam (xuống trên jar), 3=bắc (lên trên jar). Server chỉ dùng
        /// để log/animation, không validate, nên mapping này an toàn để giữ nguyên.
        /// </summary>
        public static int Direction4(int dx, int dy)
        {
            if (System.Math.Abs(dx) >= System.Math.Abs(dy)) return dx >= 0 ? 0 : 1;
            return dy >= 0 ? 2 : 3;
        }

        /// <summary>Base cho lớp vật thể — luôn trên các lớp nền tile.</summary>
        public const int ObjectBaseOrder = 1000;

        /// <summary>
        /// Khoảng cách sorting giữa hai vật thể liên tiếp. Vật thể hoạt ảnh gồm nhiều part
        /// (tối đa 14 trong dữ liệu jar) dùng <c>order + i</c>, nên stride phải &gt; số part
        /// để part của vật thể này không lẫn sorting với vật thể kế tiếp.
        /// </summary>
        public const int ObjectOrderStride = 16;

        /// <summary>
        /// Sorting order cho vật thể theo ĐÚNG THỨ TỰ TRONG FILE map (painter's algorithm),
        /// KHÔNG sort theo Y. <c>ef.java</c> vẽ vật thể lần lượt theo thứ tự lưu trong .dat;
        /// tác giả map cố ý xếp thứ tự để lớp chồng đúng (vd nền 189 đặt TRƯỚC mascot 190 để
        /// mascot vẽ đè lên). Sort theo Y sẽ đảo thứ tự đó và cắt mất nửa dưới mascot.
        /// </summary>
        /// <param name="fileIndex">Chỉ số vật thể theo thứ tự đọc từ .dat.</param>
        public static int ObjectSortingOrder(int fileIndex) =>
            ObjectBaseOrder + fileIndex * ObjectOrderStride;

        /// <summary>
        /// Base cho actor động (người chơi, NPC, mob). Cao hơn hẳn dải vật thể map
        /// (<see cref="ObjectSortingOrder"/> = 1000 + Y). HeightTiles là byte nên map cao
        /// tối đa 255×24 = 6120 px → dải vật thể ≤ 7120; đặt actor tại 10000 để luôn trên.
        /// Overlay world-space (bong bóng chat, hiệu ứng pet) đặt trên actor: xem
        /// <c>ChatBubble</c>, <c>PetInteractionEffect</c>.
        /// </summary>
        public const int ActorBaseOrder = 10_000;

        /// <summary>
        /// Sorting order cho actor: nằm trong dải riêng trên mọi vật thể map, nhưng vẫn
        /// Y-sort với nhau (actor gần đáy vẽ đè actor ở xa) để che nhau đúng chiều sâu.
        /// Bằng cách này nhân vật LUÔN hiện trên cây/nhà thay vì bị che khi đứng phía sau.
        /// </summary>
        public static int ActorSortingOrder(int jarY) => ActorBaseOrder + jarY;

        /// <summary>Sorting order cho một lớp nền — layer 0 = mặt đất, các layer cao hơn đè lên.</summary>
        public static int LayerSortingOrder(int layerIndex) => layerIndex;

        /// <summary>Kích thước map theo world unit.</summary>
        public static (float width, float height) MapSizeWorld(JarMapLayout map)
        {
            return (map.WidthPixels / PixelsPerUnit, map.HeightPixels / PixelsPerUnit);
        }

        /// <summary>Tâm map — camera đặt tại đây thấy toàn cảnh.</summary>
        public static (float x, float y) MapCenterWorld(JarMapLayout map)
        {
            var (w, h) = MapSizeWorld(map);
            return (w / 2f, h / 2f);
        }
    }
}
