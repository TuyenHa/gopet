namespace Gopet.Net.Map
{
    /// <summary>
    /// Bản ghi khởi tạo nhân vật của chính client. Server gửi INIT_PLAYER (31) một lần
    /// sau LOGIN_SUCCES, trước khi bơm entity của map. Xem <c>GopetPlace.initPlayer</c>.
    /// </summary>
    public sealed class PlayerInit
    {
        public int UserId;
        public string Name;
        public int Gender;
    }

    /// <summary>
    /// Người chơi (mình hoặc khác) bước vào map. Server gửi opcode 24
    /// (<c>ON_PLAYER_ENTER_MAP</c>) trong <c>GopetPlace.sendNewPlayer</c>.
    /// </summary>
    public sealed class PlayerEnterMap
    {
        public int UserId;
        public string Name;
        public int Gender;

        /// <summary>Quan hệ với client — bạn/không quen. Server hiện luôn gửi 0.</summary>
        public int Relation;

        public int Speed;

        /// <summary>Hướng mặt (0..7). Client dựng animation theo đây.</summary>
        public int FaceDir;

        /// <summary>Chỉ số waypoint đang đứng. -1 hoặc trùng waypoint hợp lệ tuỳ ngữ cảnh.</summary>
        public int WaypointIndex;

        public int X;
        public int Y;
    }

    /// <summary>Người chơi rời map (opcode 30, <c>ON_PLAYER_EXIT_PLACE</c>).</summary>
    public sealed class PlayerExitPlace
    {
        public int UserId;
    }

    /// <summary>
    /// Cập nhật toàn bộ trạng thái map cho người vừa vào (opcode 29 <c>ON_UPDATE_PLAYER_IN_MAP</c>,
    /// gửi qua <c>GopetPlace.loadInfo</c>).
    ///
    /// <para><b>Đây mới là gói mang toạ độ spawn của SELF</b>, không phải
    /// <see cref="PlayerEnterMap"/> — vì <c>sendNewPlayer</c> broadcast trước khi thêm player
    /// vào <c>players</c>, nên self không nhận được gói ENTER_MAP của chính mình. Server bù
    /// bằng gói này (self + toàn bộ người chơi khác đã có trong khu vực).</para>
    /// </summary>
    public sealed class MapUpdate
    {
        public int MapId;
        public int ZoneId;

        /// <summary>Toạ độ spawn của SELF theo server (tính từ vị trí lưu trong PlayerData).</summary>
        public int SelfWaypointIndex;
        public int SelfX;
        public int SelfY;

        /// <summary>Người chơi khác đã có sẵn trong khu vực.</summary>
        public PlayerEnterMap[] Others;
    }

    /// <summary>
    /// Người chơi đang di chuyển. Opcode 27 <c>ON_OTHER_USER_MOVE</c>, phía server phát cho
    /// cả khu vực sau khi nhận được gói cùng opcode từ client di chuyển.
    ///
    /// <para><b>Vị trí cuối = (X, Y)</b>, chính là <c>points[len-2], points[len-1]</c>. Toàn
    /// bộ mảng <see cref="Points"/> là chuỗi cặp (x,y) nối tiếp — client cũ vẽ đường đi
    /// mượt qua từng cặp; client mới chỉ cần điểm cuối là đủ chân thật.</para>
    /// </summary>
    public sealed class PlayerMoved
    {
        public int UserId;

        /// <summary>Hướng cuối (0..7).</summary>
        public int Direction;

        /// <summary>Toạ độ đích, đã trích từ 2 phần tử cuối của <see cref="Points"/>.</summary>
        public int X;
        public int Y;

        /// <summary>Toàn bộ mảng điểm gốc — interleaved x,y. Dùng khi muốn vẽ đường đi.</summary>
        public int[] Points;
    }
}
