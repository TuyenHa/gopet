using System;
using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>Vị trí một NPC trên map, dùng để chọn NPC nào hiện nút "Nói chuyện".
    /// Thuần C# — không phụ thuộc UnityEngine để test được ngoài Unity runtime.</summary>
    public readonly struct NpcPoint
    {
        public readonly int Id;
        public readonly float X;
        public readonly float Y;

        public NpcPoint(int id, float x, float y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Chọn NPC gần người chơi nhất để hiện nút "Nói chuyện" nổi trên đầu NPC đó.
    /// Có hysteresis (ShowRadius nhỏ hơn HideRadius) để nút không nhấp nháy khi người
    /// chơi đứng ở biên tầm tương tác.
    /// </summary>
    public static class NpcProximity
    {
        /// <summary>Sentinel "không có NPC nào được chọn". NPC id có thể ÂM (xem
        /// LinhThuCityNpcOptions — TRAN CHAN id = -1) nên KHÔNG được dùng -1 làm sentinel.</summary>
        public const int None = int.MinValue;

        /// <summary>NPC mới chỉ được chọn khi người chơi vào trong bán kính này (pixel jar).
        /// Tính từ vị trí người chơi (chân) tới ĐIỂM THAM CHIẾU của NPC — caller nên đẩy điểm
        /// tham chiếu này lên giữa thân NPC (chân + ~32px) để cảm giác "gần" khớp với việc
        /// người chơi đứng CẠNH NPC (thay vì phải đứng CHỒNG chân NPC).</summary>
        public const float ShowRadius = 60f;

        /// <summary>NPC đang được chọn chỉ mất nút khi người chơi ra khỏi bán kính này. Gap
        /// giữa Show/Hide nhỏ (10px) để chống nhấp nháy ở biên, nhưng KHÔNG tạo "vùng chết"
        /// lớn khiến người chơi phải đi qua đi lại mới trigger.</summary>
        public const float HideRadius = 70f;

        /// <summary>
        /// Trả về id NPC nên hiện nút "Nói chuyện", hoặc <see cref="None"/> nếu không NPC
        /// nào trong tầm. Ưu tiên giữ nguyên <paramref name="currentId"/> nếu nó vẫn còn
        /// trong bán kính nhả (hysteresis); nếu không, chọn NPC gần nhất trong bán kính bắt.
        /// </summary>
        /// <param name="showRadius">Bán kính BẮT mục tiêu mới. Quái to hơn NPC nên nút đánh
        /// truyền bán kính rộng hơn mặc định.</param>
        /// <param name="hideRadius">Bán kính NHẢ mục tiêu đang giữ; phải lớn hơn
        /// <paramref name="showRadius"/>, nếu không mục tiêu nhấp nháy ở biên.</param>
        public static int Pick(float playerX, float playerY, IReadOnlyList<NpcPoint> npcs, int currentId,
            float showRadius = ShowRadius, float hideRadius = HideRadius)
        {
            if (npcs == null) throw new ArgumentNullException(nameof(npcs));
            if (npcs.Count == 0) return None;

            if (currentId != None)
            {
                for (var i = 0; i < npcs.Count; i++)
                {
                    if (npcs[i].Id != currentId) continue;
                    if (DistanceSquared(playerX, playerY, npcs[i]) <= hideRadius * hideRadius)
                        return currentId;
                    break;
                }
            }

            var bestId = None;
            var bestDistanceSquared = showRadius * showRadius;
            for (var i = 0; i < npcs.Count; i++)
            {
                var distanceSquared = DistanceSquared(playerX, playerY, npcs[i]);
                if (distanceSquared > bestDistanceSquared) continue;
                bestDistanceSquared = distanceSquared;
                bestId = npcs[i].Id;
            }
            return bestId;
        }

        private static float DistanceSquared(float playerX, float playerY, NpcPoint npc)
        {
            var dx = playerX - npc.X;
            var dy = playerY - npc.Y;
            return dx * dx + dy * dy;
        }
    }
}
