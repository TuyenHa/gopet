using System;

namespace Gopet.Net.Map
{
    /// <summary>
    /// Dịch gói tin liên quan tới map thành sự kiện — thuần C#, không đụng UnityEngine
    /// nên test được ngoài Editor. Cùng pattern với <c>AuthHandler</c>.
    ///
    /// <para>Phạm vi: <c>INIT_PLAYER</c> (31), <c>ON_PLAYER_ENTER_MAP</c> (24),
    /// <c>ON_PLAYER_EXIT_PLACE</c> (30), <c>ON_OTHER_USER_MOVE</c> (27, cả 2 chiều).</para>
    /// </summary>
    public sealed class MapHandler
    {
        private readonly Action<Message> _send;

        /// <param name="send">Đẩy gói lên server (dùng cho <see cref="SendMove"/>). Null nếu chỉ dùng để nhận.</param>
        public MapHandler(Action<Message> send = null)
        {
            _send = send;
        }

        /// <summary>Server gửi INIT_PLAYER khi client vào map lần đầu.</summary>
        public event Action<PlayerInit> PlayerInitReceived;

        /// <summary>Người chơi (mình hoặc khác) vào map hiện tại.</summary>
        public event Action<PlayerEnterMap> PlayerEntered;

        /// <summary>Người chơi rời map — client xoá avatar tương ứng.</summary>
        public event Action<PlayerExitPlace> PlayerExited;

        /// <summary>Người chơi (mình hoặc khác) di chuyển — vị trí đích ở event.</summary>
        public event Action<PlayerMoved> PlayerMoved;

        /// <summary>Server đẩy trạng thái map cho self vừa vào — chứa TOẠ ĐỘ SPAWN của self + danh sách khác.</summary>
        public event Action<MapUpdate> MapUpdated;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));

            router.Register(GopetCmd.INIT_PLAYER, OnInitPlayer);
            router.Register(GopetCmd.ON_PLAYER_ENTER_MAP, OnEnterMap);
            router.Register(GopetCmd.ON_PLAYER_EXIT_PLACE, OnExitPlace);
            router.Register(GopetCmd.ON_OTHER_USER_MOVE, OnMove);
            router.Register(GopetCmd.ON_UPDATE_PLAYER_IN_MAP, OnMapUpdate);
        }

        /// <summary>Wire: int userId, UTF name, int gender, sbyte 0, int 0. Xem <c>GopetPlace.initPlayer</c>.</summary>
        private void OnInitPlayer(Message msg)
        {
            var r = msg.Reader;
            var evt = new PlayerInit
            {
                UserId = r.ReadInt(),
                Name = r.ReadUtf(),
                Gender = r.ReadInt()
            };
            // 5 byte đuôi (sbyte + int) đang là padding — bỏ qua, không cần đọc.
            PlayerInitReceived?.Invoke(evt);
        }

        /// <summary>Wire: int userId, UTF name, sbyte gender, sbyte relation, sbyte speed, sbyte faceDir, sbyte waypointIndex, int x, int y.</summary>
        private void OnEnterMap(Message msg)
        {
            var r = msg.Reader;
            var evt = new PlayerEnterMap
            {
                UserId = r.ReadInt(),
                Name = r.ReadUtf(),
                Gender = r.ReadSByte(),
                Relation = r.ReadSByte(),
                Speed = r.ReadSByte(),
                FaceDir = r.ReadSByte(),
                WaypointIndex = r.ReadSByte(),
                X = r.ReadInt(),
                Y = r.ReadInt()
            };
            PlayerEntered?.Invoke(evt);
        }

        /// <summary>Wire: int userId, sbyte faceDir, int 0. Client chỉ cần userId.</summary>
        private void OnExitPlace(Message msg)
        {
            var r = msg.Reader;
            var evt = new PlayerExitPlace { UserId = r.ReadInt() };
            PlayerExited?.Invoke(evt);
        }

        /// <summary>
        /// Gửi gói di chuyển lên server. <paramref name="points"/> interleaved cặp (x,y),
        /// tối thiểu 2 phần tử (đích), tối đa 64 (server verify độ dài trong khoảng đó).
        /// </summary>
        public void SendMove(int userId, int direction, int mapId, int[] points)
        {
            if (_send == null) throw new InvalidOperationException("MapHandler khởi tạo không có `send` — không gửi được.");
            if (points == null || points.Length < 2 || points.Length > 64)
                throw new ArgumentException("points phải có 2-64 phần tử (verify của server).", nameof(points));

            var msg = Message.Create(GopetCmd.ON_OTHER_USER_MOVE)
                .PutInt(userId)
                .PutSByte(direction)
                .PutInt(mapId);
            msg.PutInt(points.Length);
            foreach (var p in points) msg.PutInt(p);
            _send(msg);
        }

        /// <summary>Đi qua cổng: int map đích, int waypoint đích, int phiên bản map.</summary>
        public void SendWarp(int mapId, int waypointIndex, int mapVersion = 1)
        {
            if (_send == null) throw new InvalidOperationException("MapHandler khởi tạo không có `send` — không gửi được.");
            _send(Message.Create(GopetCmd.ON_PLAYER_WARPING)
                .PutInt(mapId)
                .PutInt(waypointIndex)
                .PutInt(mapVersion));
        }

        /// <summary>
        /// Wire: int mapId, int zoneId, sbyte selfWaypoint, int selfX, int selfY,
        /// rồi lặp danh sách người khác {int userId, UTF name, sbyte gender, sbyte relation,
        /// sbyte speed, sbyte faceDir, int x, int y} cho đến hết gói.
        /// Xem <c>GopetPlace.loadInfo</c>.
        /// </summary>
        private void OnMapUpdate(Message msg)
        {
            var r = msg.Reader;
            var evt = new MapUpdate
            {
                MapId = r.ReadInt(),
                ZoneId = r.ReadInt(),
                SelfWaypointIndex = r.ReadSByte(),
                SelfX = r.ReadInt(),
                SelfY = r.ReadInt()
            };
            var others = new System.Collections.Generic.List<PlayerEnterMap>();
            while (r.Remaining > 0)
            {
                others.Add(new PlayerEnterMap
                {
                    UserId = r.ReadInt(),
                    Name = r.ReadUtf(),
                    Gender = r.ReadSByte(),
                    Relation = r.ReadSByte(),
                    Speed = r.ReadSByte(),
                    FaceDir = r.ReadSByte(),
                    WaypointIndex = -1,
                    X = r.ReadInt(),
                    Y = r.ReadInt()
                });
            }
            evt.Others = others.ToArray();
            MapUpdated?.Invoke(evt);
        }

        /// <summary>Wire: int userId, sbyte dir, int mapId, int pointCount, int[] points (cặp x,y).</summary>
        private void OnMove(Message msg)
        {
            var r = msg.Reader;
            var userId = r.ReadInt();
            var dir = r.ReadSByte();
            r.ReadInt(); // mapId — client đã biết mình ở map nào
            var count = r.ReadInt();
            var points = new int[count];
            for (var i = 0; i < count; i++) points[i] = r.ReadInt();

            // Server luôn gửi pointCount >= 2 (đã verify readArrayLength(2, 64)).
            var evt = new PlayerMoved
            {
                UserId = userId,
                Direction = dir,
                X = points[count - 2],
                Y = points[count - 1],
                Points = points
            };
            PlayerMoved?.Invoke(evt);
        }
    }
}
