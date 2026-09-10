using System;

namespace Gopet.UiLogic
{
    /// <summary>Một vật thể đặt trên map: cây, nhà, đèn… Toạ độ theo pixel, gốc trên-trái của map.</summary>
    public sealed class JarMapObject
    {
        /// <summary>Chỉ số trong <see cref="JarMapLayout.ResourceIds"/>, KHÔNG phải id ảnh.</summary>
        public int ResourceIndex;

        public int X;
        public int Y;

        /// <summary>Bù trục Y — <c>ef.java</c> vẽ tại <c>y - yOffset</c>, không phải tại <c>y</c>.</summary>
        public int YOffset;

        /// <summary>Khung bao tương tác của object hoạt ảnh; bốn byte có dấu từ <c>gy</c>.</summary>
        public int[] Bounds;

        /// <summary>Clip trong metadata <c>newMapData/&lt;resourceId&gt;_b</c>.</summary>
        public int AnimationIndex;
    }

    /// <summary>
    /// Bố cục map của bản jar (<c>maps/&lt;n&gt;.dat</c>), giải theo <c>ef.java:104-190</c>.
    /// Thuần C#, không UnityEngine — test được ngoài Editor như <see cref="JarStringTable"/>.
    ///
    /// <para><b>Nền màn đăng nhập chính là map này.</b> <c>fb.java</c> dựng
    /// <c>new ef(11, …)</c> rồi override <c>b()</c> của lớp cha để vẽ map thay cho
    /// nền màu phẳng — xem <c>JarMapBackground</c>.</para>
    ///
    /// <para><b>Ô nền nén trong MỘT byte:</b> 4 bit cao là chỉ số ảnh dải, 4 bit thấp
    /// là ô thứ mấy trong dải đó CỘNG MỘT (0 = ô trống, không vẽ gì).</para>
    /// </summary>
    public sealed class JarMapLayout
    {
        /// <summary>Cạnh một ô nền, pixel. <c>ef.java</c> gán cứng 24 ở khắp nơi.</summary>
        public const int TileSize = 24;

        /// <summary>Kiểu tài nguyên: ảnh dải ô nền.</summary>
        public const int TypeTileStrip = 2;

        /// <summary>Kiểu tài nguyên: hoạt ảnh — mỗi vật thể dùng nó mang thêm 5 byte.</summary>
        public const int TypeAnimation = 1;

        public int[] ResourceIds;
        public int[] ResourceTypes;

        /// <summary>Số tài nguyên ĐẦU danh sách là ảnh dải dùng cho lớp nền; phần còn lại là vật thể.</summary>
        public int ImageCount;

        public int WidthTiles;
        public int HeightTiles;

        /// <summary>[lớp][hàng][cột], mỗi phần tử là byte ô nền đã nén.</summary>
        public byte[][][] Layers;

        /// <summary>Lớp va chạm: [hàng][cột], mỗi byte tra vào bảng ô va chạm 24×24. <c>0</c> = đi được, <c>15</c> = chặn.</summary>
        public byte[][] Collision;

        public JarMapObject[] Objects;

        /// <summary>Nhà + NPC + cổng dịch chuyển. Đọc sau <see cref="Objects"/>.</summary>
        public JarMapEntity[] Entities;

        /// <summary>Điểm mốc — cạnh map, spawn, mốc di chuyển. Đọc sau <see cref="Entities"/>.</summary>
        public JarMapWaypoint[] Waypoints;

        public int WidthPixels => WidthTiles * TileSize;

        public int HeightPixels => HeightTiles * TileSize;

        /// <summary>Chỉ số ảnh dải của một ô, hoặc -1 nếu ô trống.</summary>
        public static int StripOf(byte tile) => (tile & 15) == 0 ? -1 : tile >> 4;

        /// <summary>Ô thứ mấy trong ảnh dải, hoặc -1 nếu ô trống.</summary>
        public static int CellOf(byte tile) => (tile & 15) - 1;

        public static JarMapLayout Parse(byte[] data)
        {
            var r = new JarBigEndianReader(data);
            var map = new JarMapLayout();

            map.ImageCount = r.Byte();
            var total = map.ImageCount + r.Byte();

            map.ResourceIds = new int[total];
            map.ResourceTypes = new int[total];
            for (var i = 0; i < total; i++)
            {
                map.ResourceIds[i] = r.Short();
                map.ResourceTypes[i] = r.Byte();
            }

            map.WidthTiles = r.Byte();
            map.HeightTiles = r.Byte();
            var layerCount = r.Byte();

            map.Layers = new byte[layerCount][][];
            for (var layer = 0; layer < layerCount; layer++)
            {
                map.Layers[layer] = new byte[map.HeightTiles][];
                for (var row = 0; row < map.HeightTiles; row++)
                {
                    map.Layers[layer][row] = r.Bytes(map.WidthTiles);
                }
            }

            // Lớp va chạm — P5.1 đọc rồi vứt (chỉ cần cho parser đi tới phần vật thể);
            // P6 dùng để chặn nhân vật đi vào vật cản, nên phơi ra ngoài.
            map.Collision = new byte[map.HeightTiles][];
            for (var row = 0; row < map.HeightTiles; row++) map.Collision[row] = r.Bytes(map.WidthTiles);

            var objectCount = r.Int();
            map.Objects = new JarMapObject[objectCount];
            for (var i = 0; i < objectCount; i++)
            {
                var index = r.Byte();
                var item = new JarMapObject
                {
                    ResourceIndex = index,
                    X = r.Short(),
                    Y = r.Short(),

                    // CÓ DẤU: dữ liệu thật có giá trị âm (map 11 có vật thể yOffset = -14).
                    // Đọc không dấu thì vật thể đó tụt xuống 270 pixel, ra ngoài map.
                    YOffset = r.SignedByte()
                };

                // gy(x,y,w,h) + clip index. Đây là dữ liệu cần để không vẽ nguyên cả
                // atlas `_a.png` (nguyên nhân các frame chồng/lệch trong gameplay).
                if (index >= 0 && index < total && map.ResourceTypes[index] == TypeAnimation)
                {
                    item.Bounds = new[] { r.SignedByte(), r.SignedByte(), r.SignedByte(), r.SignedByte() };
                    item.AnimationIndex = r.SignedByte();
                }

                map.Objects[i] = item;
            }

            // eg[]: nhà / NPC / cổng dịch chuyển. int count, mỗi record 9 byte + nhánh
            // mở rộng nếu kind != 0. Xem eg.java: eg.a(dv, DataInputStream).
            var entityCount = r.Int();
            map.Entities = new JarMapEntity[entityCount];
            for (var i = 0; i < entityCount; i++)
            {
                var kind = r.Byte();
                var building = r.Byte();
                var x = r.Short();
                var y = r.Short();
                var raw = r.Bytes(5);
                var entity = new JarMapEntity
                {
                    Kind = kind,
                    BuildingType = building,
                    X = x,
                    Y = y,
                    Raw5 = raw,
                    Name = string.Empty
                };
                if (kind != 0)
                {
                    entity.ExtraA = r.Byte();
                    entity.ExtraB = r.Byte();
                    entity.Name = r.Utf();
                    r.Byte(); // eg.java đọc thêm 1 byte cuối, chưa rõ dùng vào việc gì
                }
                map.Entities[i] = entity;
            }

            // z[]: điểm mốc. byte count, mỗi record = byte kind + short x + short y.
            var waypointCount = r.Byte();
            map.Waypoints = new JarMapWaypoint[waypointCount];
            for (var i = 0; i < waypointCount; i++)
            {
                map.Waypoints[i] = new JarMapWaypoint { Kind = r.Byte(), X = r.Short(), Y = r.Short() };
            }

            return map;
        }
    }
}
