namespace Gopet.Net.Npc
{
    /// <summary>
    /// Bảng option ID cho 5 NPC của map "Thành Phố Linh Thú" (mapId 11).
    ///
    /// <para>Sau khi client gửi <c>GuiderPackets.TalkToNpc(npcId)</c>, server bơm menu
    /// list options. User chọn 1 dòng → <c>GuiderPackets.SelectNpcOption(npcId, optionId)</c>
    /// với optionId khớp 1 trong các hằng số dưới. Server dispatch flow tương ứng
    /// (server bơm ListOption/InputDialog/MenuItem tiếp theo qua Guider generic — Unity
    /// render tự nhiên).</para>
    ///
    /// <para>Không có packet mới — dùng <c>GuiderPackets.SelectNpcOption</c> hiện có.
    /// Class này chỉ là bảng hằng số cho developer/QA test cụ thể từng flow.</para>
    ///
    /// <para>Nguồn: <c>server_db.sql:2183-2207</c> — <c>optionId</c> column của NPC.</para>
    /// </summary>
    public static class LinhThuCityNpcOptions
    {
        // NPC -1 TRAN CHAN (Trần Trấn) — vị trí (374, 117)
        public const int TranChanNhanPetMienPhi = 1;
        public const int TranChanShopPet        = 2;
        public const int TranChanTopPet         = 3;
        public const int TranChanTopDaiGia      = 4;
        public const int TranChanTopPhuHo       = 41;
        public const int TranChanNhapMaQuaTang  = 60;
        public const int TranChanGopDoServerCu  = 81;

        // NPC -7 BAC SI XI TIN (Bác sĩ xì tin) — vị trí (470, 344)
        public const int BacSiHoiSinhPet          = 22;
        public const int BacSiNhanNhiemVuHangNgay = 23;
        public const int BacSiTayGym              = 24;

        // NPC -15 SU GIA BANG HOI (Sứ giả bang hội) — vị trí (326, 236)
        public const int SuGiaVaoKhuVucBang    = 44;
        public const int SuGiaTopLvlBangHoi    = 45;
        public const int SuGiaTaoBangHoi       = 46;
        public const int SuGiaSuKienBangHoi    = 47;
        public const int SuGiaCongHienBangHoi  = 48;

        // NPC -24 ONG GIA NOEL (Ông già Noel) — vị trí (226, 396)
        public const int NoelDiemDanh = 87;

        // NPC -25 SU GIA THIEN THAN (Sứ giả thiên thần) — vị trí (322, 332)
        public const int ThienThanHuongDanLenThienDinh = 88;
        public const int ThienThanHienTangThuCung      = 89;
    }
}
