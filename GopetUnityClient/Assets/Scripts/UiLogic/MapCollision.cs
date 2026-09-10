namespace Gopet.UiLogic
{
    /// <summary>Kiểm tra điểm đứng theo mặt nạ va chạm 24×24 của client J2ME.</summary>
    public static class MapCollision
    {
        // ef.java:13 — mỗi giá trị 1..14 mô tả bốn phần tư 12×12 của một ô.
        private static readonly byte[,,] Quadrants =
        {
            { { 0, 0 }, { 0, 1 } }, { { 0, 0 }, { 1, 0 } },
            { { 0, 0 }, { 1, 1 } }, { { 0, 1 }, { 0, 0 } },
            { { 0, 1 }, { 0, 1 } }, { { 0, 1 }, { 1, 0 } },
            { { 0, 1 }, { 1, 1 } }, { { 1, 0 }, { 0, 0 } },
            { { 1, 0 }, { 0, 1 } }, { { 1, 0 }, { 1, 0 } },
            { { 1, 0 }, { 1, 1 } }, { { 1, 1 }, { 0, 0 } },
            { { 1, 1 }, { 0, 1 } }, { { 1, 1 }, { 1, 0 } }
        };

        public static bool CanStand(JarMapLayout map, int x, int y)
        {
            if (map == null || x < 0 || y < 0 || x >= map.WidthPixels || y >= map.HeightPixels)
                return false;

            var cellX = x / JarMapLayout.TileSize;
            var cellY = y / JarMapLayout.TileSize;
            var mask = map.Collision[cellY][cellX];
            if (mask == 0) return true;
            if (mask == 15 || mask > 15) return false;

            var quadrantX = x % JarMapLayout.TileSize / 12;
            var quadrantY = y % JarMapLayout.TileSize / 12;
            return Quadrants[mask - 1, quadrantY, quadrantX] == 0;
        }
    }
}
