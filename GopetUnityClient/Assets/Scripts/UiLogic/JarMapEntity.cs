namespace Gopet.UiLogic
{
    /// <summary>
    /// Điểm tương tác trên map — nhà, NPC, cổng dịch chuyển. Đọc từ <c>eg.a(dv, DataInputStream)</c>
    /// trong <c>eg.java</c>. 9 byte cố định, thêm nhánh mở rộng nếu <c>kind != 0</c>.
    /// </summary>
    public sealed class JarMapEntity
    {
        /// <summary>Cờ phân loại — 0 = nhà (dùng <see cref="BuildingType"/>), khác 0 = NPC/cổng có tên.</summary>
        public int Kind;

        /// <summary>Kiểu nhà 0-9 (nhà hẻm, nhà mặt tiền, biệt thự…), chỉ có nghĩa khi <see cref="Kind"/> == 0.</summary>
        public int BuildingType;

        public int X;
        public int Y;

        /// <summary>5 byte thô sau toạ độ — chưa rõ ý nghĩa hết. Giữ nguyên để P6 khai thác dần.</summary>
        public byte[] Raw5;

        /// <summary>Chỉ có khi <see cref="Kind"/> != 0. Byte đầu nhánh mở rộng.</summary>
        public int ExtraA;

        /// <summary>Chỉ có khi <see cref="Kind"/> != 0. Byte thứ hai nhánh mở rộng.</summary>
        public int ExtraB;

        /// <summary>Tên hiển thị. Rỗng nếu <see cref="Kind"/> == 0.</summary>
        public string Name;
    }

    /// <summary>
    /// Điểm mốc trên map. Dạng đơn giản: kind + x + y. Đọc từ <c>z.java</c>.
    /// Chưa rõ <see cref="Kind"/> phân loại gì — server và client cùng dùng, đối chiếu sau.
    /// </summary>
    public sealed class JarMapWaypoint
    {
        public int Kind;
        public int X;
        public int Y;
    }
}
