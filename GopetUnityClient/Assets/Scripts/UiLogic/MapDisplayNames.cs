namespace Gopet.UiLogic
{
    /// <summary>Tên map cục bộ dùng cho HUD; protocol đổi map hiện chỉ gửi mapId.</summary>
    public static class MapDisplayNames
    {
        public static string Get(int mapId)
        {
            switch (mapId)
            {
                case 11: return "Thành Phố Linh Thú";
                case 12: return "Ải";
                case 13: return "Linh Lâm";
                case 14: return "Linh Mộc";
                case 15: return "Đại Linh Cảnh";
                case 16: return "Đường lên đỉnh núi";
                case 17: return "Thung lũng Hoàng Nham";
                case 18: return "Núi Phục Quang";
                case 19: return "Đấu trường";
                case 20: return "Lôi đài";
                case 21: return "Thạch Động";
                case 22: return "Chợ trời";
                case 23: return "Băng động 1";
                case 24: return "Sông băng";
                case 25: return "Băng động 2";
                case 26: return "Vùng đất phong ấn";
                case 27: return "Đài tưởng niệm";
                case 28: return "Quảng trường chính";
                case 29: return "TP Thiên Thần";
                case 30: return "Khu vực bang hội";
                case 31: return "Ải thượng giới";
                case 32: return "Chốt chặn cuối cùng";
                case 33: return "Những cây cầu";
                case 34: return "Vùng chiến sự";
                default: return $"Map {mapId}";
            }
        }
    }
}
