namespace Gopet.UiLogic
{
    /// <summary>
    /// Chỗ đứng của nhân vật ngay sau khi sang map mới.
    ///
    /// <para>Server KHÔNG biết bố cục map (bảng <c>map</c> chỉ có mapId/tên/npc/boss), nên khi
    /// nhận <c>ON_PLAYER_WARPING</c> nó đặt cứng <c>x = y = 360</c> cho MỌI map
    /// (<c>GameController.cs:318</c>) rồi gửi kèm <c>waypointIndex</c> của cổng vừa đi. Việc
    /// đưa nhân vật về đúng mốc là của client — dữ liệu mốc (<c>z[]</c> trong
    /// <c>maps/&lt;n&gt;.dat</c>) chỉ tồn tại ở đây.</para>
    ///
    /// <para>Bỏ qua bước này thì mọi map đều nhả nhân vật ra giữa map. Ở 3 map băng (23, 24,
    /// 25) điểm (360,360) rơi đúng giữa vũng nước nên vừa vào map đã thấy mình đứng dưới nước;
    /// các map khác thì hiện ra ở giữa đồng thay vì cạnh cổng.</para>
    ///
    /// <para>Cổng mang sẵn chỉ số mốc của map đích (<c>JarMapEntity.ExtraB</c>): cổng map 24 →
    /// map 23 index 2/3 ứng với mốc (117,86) và (98,311) — hai bờ bắc/nam, đều trên cạn.</para>
    /// </summary>
    public static class MapSpawnPoint
    {
        /// <summary>Toạ độ server đặt cứng khi warp, dùng làm cờ "chưa biết chỗ đứng".</summary>
        public const int WarpSentinel = 360;

        public static bool IsWarpSentinel(int x, int y) => x == WarpSentinel && y == WarpSentinel;

        /// <summary>Toạ độ thật để đặt nhân vật. Trả nguyên toạ độ server nếu đó không phải
        /// gói warp, hoặc map không có mốc khớp — không đoán bừa.</summary>
        public static void Resolve(JarMapLayout map, int waypointIndex, ref int x, ref int y)
        {
            if (!IsWarpSentinel(x, y)) return;
            var waypoint = Find(map, waypointIndex);
            if (waypoint == null) return;
            x = waypoint.X;
            y = waypoint.Y;
        }

        /// <summary>Khớp theo <see cref="JarMapWaypoint.Kind"/> trước, rồi mới tới thứ tự mảng.
        /// Ở 3 map băng hai cách cho cùng kết quả (mảng xếp đúng kind 0,1,2,3), nhưng map 11
        /// xếp [kind 2, kind 1, kind 0] — lấy theo thứ tự mảng sẽ nhả người chơi ra mép tây
        /// thay vì giữa thành.</summary>
        private static JarMapWaypoint Find(JarMapLayout map, int index)
        {
            var waypoints = map?.Waypoints;
            if (waypoints == null || waypoints.Length == 0 || index < 0) return null;
            foreach (var waypoint in waypoints)
            {
                if (waypoint != null && waypoint.Kind == index) return waypoint;
            }
            return index < waypoints.Length ? waypoints[index] : null;
        }
    }
}
