using System;
using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Chỗ đứng của từng map trên tranh nền bản đồ thế giới
    /// (<c>Resources/Ui/WorldMap/world-map-background</c>).
    ///
    /// <para>Toạ độ là TỈ LỆ 0..1 của chính tấm tranh (0,0 = góc dưới-trái), không phải
    /// pixel màn hình: tranh được phủ kín màn và cắt bớt tuỳ tỉ lệ máy, neo theo tỉ lệ
    /// thì pin luôn dính đúng chỗ trên tranh.</para>
    ///
    /// <para>Đây là dữ liệu TRÌNH BÀY. Danh sách map người chơi vào được vẫn do server
    /// chốt (TELE_MENU); map lạ không có trong bảng vẫn hiện, rơi vào cụm "Khác".</para>
    ///
    /// <para><b>Không dùng kiểu UnityEngine</b>: thư mục <c>UiLogic</c> còn compile dưới
    /// netstandard2.1 không có UnityEngine để chạy unit test headless.</para>
    /// </summary>
    public static class WorldMapLayout
    {
        /// <summary>Cụm cuối cùng — chỗ chứa map không khai báo trong <see cref="Members"/>.</summary>
        public const int OtherRegion = 5;

        public static readonly string[] RegionNames =
        {
            "Thành thị", "Rừng Linh", "Vùng núi", "Băng nguyên", "Thượng giới", "Khác"
        };

        /// <summary>Ô đầu tiên của mỗi cụm, đặt trùng vùng địa hình tương ứng trên tranh.</summary>
        private static readonly float[] AnchorX = { 0.10f, 0.30f, 0.79f, 0.66f, 0.22f, 0.46f };
        private static readonly float[] AnchorY = { 0.44f, 0.68f, 0.46f, 0.88f, 0.90f, 0.07f };

        /// <summary>Số cột của lưới pin trong mỗi cụm.</summary>
        private static readonly int[] RegionColumns = { 2, 2, 2, 3, 4, 3 };

        /// <summary>
        /// Bước dòng riêng từng cụm: Thượng giới có 9 map nên phải xếp khít, nếu không
        /// dòng cuối tụt xuống đè lên cụm Rừng Linh.
        /// </summary>
        private static readonly float[] RegionStepY =
            { -0.085f, -0.085f, -0.085f, -0.085f, -0.065f, -0.085f };

        /// <summary>
        /// Nhãn cụm đặt tay chứ không suy từ ô đầu: cụm sát mép trên mà cộng thêm một
        /// bước là nhãn văng ra ngoài màn.
        /// </summary>
        private static readonly float[] LabelX = { 0.10f, 0.30f, 0.79f, 0.66f, 0.16f, 0.46f };
        private static readonly float[] LabelY = { 0.515f, 0.755f, 0.535f, 0.945f, 0.965f, 0.145f };

        /// <summary>Bước cột dùng chung — đủ thưa để hai nhãn cạnh nhau không chạm.</summary>
        private const float StepX = 0.115f;

        /// <summary>mapId → cụm. Thứ tự trong mảng cũng là thứ tự ô trong lưới của cụm.</summary>
        private static readonly int[][] Members =
        {
            new[] { 11, 22, 19, 20 },                     // Thành thị
            new[] { 12, 13, 14, 15 },                     // Rừng Linh
            new[] { 16, 17, 18, 21 },                     // Vùng núi
            new[] { 23, 24, 25 },                         // Băng nguyên
            new[] { 27, 28, 29, 30, 31, 32, 33, 34 }      // Thượng giới (26 đặt tay, xem FixedNodes)
        };

        /// <summary>
        /// Map đặt TAY, không theo lưới cụm: trên tranh có những chỗ đặc trưng mà lưới
        /// không với tới được. Map ở đây phải bị loại khỏi <see cref="Members"/>, nếu
        /// không lưới của cụm sẽ chừa một ô trống.
        /// </summary>
        private static readonly Dictionary<int, Node> FixedNodes = new Dictionary<int, Node>
        {
            // Vùng đất phong ấn: hòn đảo nhỏ lẻ loi ngoài khơi mép trái, hợp với một
            // vùng bị phong ấn hơn là đứng lẫn trong cụm đảo Thượng giới.
            { 26, new Node(4, 0.05f, 0.65f) }
        };

        /// <summary>Vị trí một pin, theo tỉ lệ 0..1 của tranh nền.</summary>
        public readonly struct Node
        {
            public readonly int RegionId;
            public readonly float X;
            public readonly float Y;

            public Node(int regionId, float x, float y)
            {
                RegionId = regionId;
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Chỗ đứng của một map. Map lạ xếp tuần tự vào cụm "Khác" theo mapId tăng dần —
        /// deterministic, nên hai lần mở màn không thấy pin nhảy chỗ.
        /// </summary>
        /// <param name="unknownMapIds">Mọi mapId lạ của lần bind này, dùng để xếp chỗ ổn định.</param>
        public static Node Of(int mapId, IReadOnlyList<int> unknownMapIds = null)
        {
            if (FixedNodes.TryGetValue(mapId, out var fixedNode)) return fixedNode;
            for (var region = 0; region < Members.Length; region++)
            {
                var slot = Array.IndexOf(Members[region], mapId);
                if (slot >= 0) return SlotNode(region, slot);
            }
            return SlotNode(OtherRegion, CountSmaller(unknownMapIds, mapId));
        }

        /// <summary>Map TẠM ĐÓNG: ẩn khỏi bản đồ thế giới và ẩn cổng dẫn tới nó
        /// (<c>MapRenderer.BuildMapEntities</c>). 22 = Chợ trời (2026-09-26). Server chặn
        /// thật ở <c>MapUnlockRules.Closed</c> — mở lại thì bỏ id ở CẢ hai nơi.</summary>
        private static readonly int[] HiddenMaps = { 22 };

        public static bool IsHidden(int mapId) => Array.IndexOf(HiddenMaps, mapId) >= 0;

        /// <summary>Map này có chỗ cố định trong bảng không (false = sẽ rơi vào cụm "Khác").</summary>
        public static bool IsKnown(int mapId)
        {
            if (FixedNodes.ContainsKey(mapId)) return true;
            foreach (var region in Members)
                if (Array.IndexOf(region, mapId) >= 0) return true;
            return false;
        }

        public static float RegionLabelX(int regionId) => LabelX[regionId];

        public static float RegionLabelY(int regionId) => LabelY[regionId];

        private static Node SlotNode(int regionId, int slot)
        {
            var columns = RegionColumns[regionId];
            return new Node(regionId,
                AnchorX[regionId] + slot % columns * StepX,
                AnchorY[regionId] + slot / columns * RegionStepY[regionId]);
        }

        private static int CountSmaller(IReadOnlyList<int> ids, int mapId)
        {
            if (ids == null) return 0;
            var count = 0;
            for (var i = 0; i < ids.Count; i++)
                if (ids[i] < mapId) count++;
            return count;
        }
    }
}
