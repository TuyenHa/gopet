namespace Gopet.Net.MiniGame
{
    /// <summary>
    /// 4 mini-game bàn cờ — Caro / Cờ tướng / Tiến lên / Phỏm.
    /// Trigger opcode chung: <c>PET_SERVICE (81) / sub 2 / sbyte gameType</c>
    /// (jar <c>dc.d(int)</c> line 45-51).
    ///
    /// <para><b>Đây là STUB.</b> Chỉ có trigger để server mở room game. Sau đó
    /// server chạy 1 luồng khác hoàn toàn (in-game moves, board state) — cần
    /// dựng scene riêng cho từng game. 4 mini-game full-implementation = 6-8w mỗi
    /// cái, ngoài scope plan này. Xem phase-08 outline.</para>
    /// </summary>
    public static class MiniGamePackets
    {
        public const sbyte GameCaro     = 1;
        public const sbyte GameCoTuong  = 2;
        public const sbyte GameTienLen  = 3;
        public const sbyte GamePhom     = 4;

        /// <summary>Wire: <c>81 / 2 / gameType</c>. Server mở room + trả invite dialog.</summary>
        public static Message OpenMiniGame(sbyte gameType) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(2).PutSByte(gameType);
    }
}
