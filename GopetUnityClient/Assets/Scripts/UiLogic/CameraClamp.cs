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

        /// <summary>Nửa chiều cao khung nhìn lớn nhất mà map vẫn phủ kín được màn hình.
        ///
        /// <para>Camera cao cố định 240 pixel jar, nhưng bề ngang thì bằng 240 × tỉ lệ màn.
        /// Màn 16:9 cho khung nhìn rộng ~468, trong khi map 12 (ải) chỉ rộng 360 — hai mép
        /// lộ <b>màu nền camera</b> thành hai dải xanh navy. Kẹp cứng toạ độ không cứu được:
        /// map hẹp hơn khung nhìn thì dịch đi đâu cũng hở.</para>
        ///
        /// <para>Nên thu tầm nhìn lại cho tới khi map phủ kín. Đổi lại là zoom vào gần hơn,
        /// nhưng thà thấy ít map hơn còn hơn thấy nền trống.</para></summary>
        /// <param name="desiredSize">Nửa chiều cao mong muốn, world unit.</param>
        /// <param name="aspect">Rộng chia cao của khung nhìn camera.</param>
        public static float FitOrthographicSize(float desiredSize, int mapW, int mapH, float aspect)
        {
            // Dữ liệu vô lý (map rỗng, aspect 0 lúc màn chưa dựng xong) thì giữ nguyên
            // thay vì trả 0 — orthographicSize 0 làm camera không vẽ được gì.
            if (mapW <= 0 || mapH <= 0 || aspect <= 0f) return desiredSize;

            var byWidth = mapW / (2f * aspect);
            var byHeight = mapH / 2f;
            return System.Math.Min(desiredSize, System.Math.Min(byWidth, byHeight));
        }

        /// <summary>
        /// Thu tầm nhìn sao cho MỘT pixel nguồn phủ đúng một số NGUYÊN pixel màn hình.
        ///
        /// <para>Art là pixel art vẽ ở 240 px chiều cao, còn màn hình thì cao bao nhiêu
        /// cũng có. 1080/240 = 4.5 nghĩa là pixel nguồn này chiếm 4 pixel màn, pixel kế
        /// bên chiếm 5 — bộ lọc Point không cứu được, mắt đọc ra thành nhoè. Chốt tỉ lệ
        /// về số nguyên thì mọi pixel nguồn to bằng nhau.</para>
        ///
        /// <para>Làm tròn LÊN chứ không xuống: tròn lên là thu tầm nhìn lại, luôn ≤ cỡ
        /// đang có nên không lộ ra ngoài biên map mà <see cref="FitOrthographicSize"/>
        /// vừa kẹp. Tròn xuống sẽ mở rộng tầm nhìn và phá mất cái kẹp đó.</para>
        /// </summary>
        /// <param name="screenHeightPx">Chiều cao khung vẽ, pixel.</param>
        public static float SnapOrthographicSize(float desiredSize, int screenHeightPx)
        {
            if (desiredSize <= 0f || screenHeightPx <= 0) return desiredSize;

            var scale = screenHeightPx / (desiredSize * 2f);

            // Màn thấp hơn cả khung nhìn (chưa tới 1 pixel màn cho 1 pixel nguồn) thì
            // không có tỉ lệ nguyên nào dùng được — giữ nguyên, thà đúng khung còn hơn.
            if (scale <= 1f) return desiredSize;

            return screenHeightPx / (2f * (float)System.Math.Ceiling(scale));
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
