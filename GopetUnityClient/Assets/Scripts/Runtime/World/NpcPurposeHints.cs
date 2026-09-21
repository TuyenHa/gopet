using Gopet.Net.Map;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Lời giới thiệu ngắn của các NPC dịch vụ. ID NPC đến từ bảng <c>npc</c>
    /// phía máy chủ; dùng câu ngắn để người mới nhìn là biết nên ghé NPC nào.

    /// </summary>
    internal static class NpcPurposeHints
    {
        public static string Get(NpcSpawn npc)
        {
            if (npc == null) return string.Empty;

            switch (npc.Id)
            {
                case -1: return "Nhận pet miễn phí và mua pet";
                case -2: return "Đổi quà, tiến hoá và nâng sao pet";
                case -4: return "Đổi pet, cánh và trang bị";
                case -5: return "Mua đồ nâng cấp và xoá xăm";
                case -6: return "Ghép ngọc nguyên tố và dung hợp";
                case -7: return "Hồi sinh pet và nhận nhiệm vụ hằng ngày";
                case -8: return "Nhận các phần quà của bạn";
                case -15:
                case -26: return "Vào khu bang hội hoặc tạo bang mới";
                case -20: return "Nhận và xem tiến độ nhiệm vụ";
                case -21: return "Tham gia hoạt động sự kiện";
                case -28: return "Đấu trường, vượt ải và thách đấu";
                case -29: return "Hướng dẫn các hoạt động Thiên Thần";
                case -30: return "Quản lý và nâng cấp bang hội";
                case -40: return "Tạo bang hội và vào khu bang";
            }

            // Chat của NPC được cấu hình từ server. Đây là đường dự phòng cho
            // NPC/sự kiện mới mà client chưa có ID trong bảng ở trên.
            if (npc.Chat == null) return string.Empty;
            foreach (var line in npc.Chat)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                // Dòng legacy từ server cũ, không còn là chức năng của client hiện tại.
                if (line.IndexOf("Gộp đồ server cũ", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                return line.Trim();
            }
            return string.Empty;
        }
    }
}
