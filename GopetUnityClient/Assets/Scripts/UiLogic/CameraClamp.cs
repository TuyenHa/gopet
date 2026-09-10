namespace Gopet.UiLogic
{
    /// <summary>
    /// Toán camera map: kẹp cạnh, tính offset theo hướng nhìn. Cùng công thức
    /// <c>ew.java:308-312, 545-581, 564-581</c> của bản jar.
    ///
    /// <para><b>Hành vi jar</b>: nhân vật đứng ở 2/3 chiều CAO màn hình (nhìn xuống), và
    /// ở 1/3 hoặc 2/3 chiều RỘNG tuỳ <c>faceDir</c>: face phải thì nhân vật lệch trái
    /// (dùng viewport phải để nhìn xa hơn về phía đang đi), face trái thì ngược lại.</para>
    ///
    /// <para><b>Kẹp cứng</b>: camera không được lộ mép ngoài map. Nếu map nhỏ hơn viewport
    /// thì camera đứng yên tại 0 (viewport chồng map).</para>
    /// </summary>
    public static class CameraClamp
    {
        /// <summary>Cỡ viewport thực = min(kích thước map, kích thước màn hình).</summary>
        public static (int viewW, int viewH) EffectiveViewport(int mapW, int mapH, int screenW, int screenH)
        {
            return (System.Math.Min(mapW, screenW), System.Math.Min(mapH, screenH));
        }

        /// <summary>
        /// Toạ độ góc trên-trái của camera (jar coord), theo <c>ew.a(int)</c>:
        /// face phải (0) → player ở 2/3 viewport, face trái (1) → player ở 1/3 viewport.
        /// Y luôn 2/3 chiều cao.
        /// </summary>
        public static (int camX, int camY) TopLeftFor(int playerX, int playerY, int viewW, int viewH, bool faceRight)
        {
            var camX = faceRight ? playerX - (viewW / 3 * 2) : playerX - viewW / 3;
            var camY = playerY - (viewH / 3 * 2);
            return (camX, camY);
        }

        /// <summary>Kẹp toạ độ góc trên-trái vào biên map.</summary>
        public static int ClampAxis(int c, int viewSize, int mapSize)
        {
            if (mapSize <= viewSize) return 0;
            if (c < 0) return 0;
            if (c > mapSize - viewSize) return mapSize - viewSize;
            return c;
        }
    }
}
