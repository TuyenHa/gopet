namespace Gopet.UiLogic
{
    /// <summary>Ánh xạ ItemInfo.Type → nhãn ngắn cho UI dải buff trong trận. Khớp whitelist
    /// server phase-04 (<c>PetBattle.BuffTypeWhitelist</c>). Type ngoài whitelist → nhãn
    /// generic để không crash nếu server sau này gửi thêm.</summary>
    public static class BuffTypeLabels
    {
        public static string For(int typeId)
        {
            switch (typeId)
            {
                case 8: return "+DMG kỹ năng";
                case 9: return "+DEF";
                case 10: return "+DEF %";
                case 11: return "+STR";
                case 15: return "+DMG";
                case 17: return "Hút MP";
                case 19: return "-STR 4 lượt";
                case 21: return "Xuyên giáp";
                case 22: return "-STR 3 lượt";
                case 23: return "+DEF 3 lượt";
                case 24: return "Hồi HP";
                case 25: return "-Chính xác";
                case 27: return "Độc %";
                case 28: return "Độc 5 lượt";
                case 29: return "-STR 1 lượt";
                case 30: return "Choáng";
                case 31: return "+ATK 3 lượt";
                case 32: return "Phản đòn 4 lượt";
                case 36: return "% Choáng";
                case 37: return "+DEF 3 lượt";
                case 38: return "Hút MP theo ATK";
                case 39: return "Hút máu";
                case 43: return "Độc dài";
                case 44: return "+DEF 4 lượt";
                case 45: return "Hồi HP 4 lượt";
                case 49: return "% Choáng khi đánh";
                default: return $"Buff #{typeId}";
            }
        }
    }
}
