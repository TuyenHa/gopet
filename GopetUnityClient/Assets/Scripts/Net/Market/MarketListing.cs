namespace Gopet.Net.Market
{
    /// <summary>
    /// Một dòng ki-ốt của popup "Chợ trời" — dùng chung cho tab Chợ
    /// (<see cref="MarketListState"/>, sub 48) và tab Gian hàng của tôi
    /// (<see cref="MarketMineState"/>, sub 51).
    ///
    /// <para>Field order khớp <c>MarketPacketWriter.WriteRow</c>
    /// (<c>GServer/Data/Market/MarketPacketWriter.cs:16-31</c>). Server đã tự ẩn dòng
    /// bị chỉ định cho người khác khỏi tab Chợ (<c>MarketQuery.IsVisibleToViewer</c>) —
    /// dòng nào có <see cref="AssignedName"/> mà KHÔNG phải <see cref="IsMine"/> nghĩa là
    /// đang được chỉ định cho chính người xem.</para>
    /// </summary>
    public sealed class MarketListingRow
    {
        public sbyte KioskType;
        public int ListingId;
        public bool IsMine;
        public string Name;
        public string IconPath;
        public long Price;
        public int Count;
        public string SellerName;
        public int SecondsLeft;
        public string Desc;
        public string AssignedName;

        /// <summary>Đã có người được chỉ định mua riêng (rỗng = ai cũng mua được).</summary>
        public bool HasAssignedBuyer => !string.IsNullOrEmpty(AssignedName);

        public static MarketListingRow Parse(JavaBinaryReader r)
        {
            return new MarketListingRow
            {
                KioskType = r.ReadSByte(),
                ListingId = r.ReadInt(),
                IsMine = r.ReadBool(),
                Name = r.ReadUtf(),
                IconPath = r.ReadUtf(),
                Price = r.ReadLong(),
                Count = r.ReadInt(),
                SellerName = r.ReadUtf(),
                SecondsLeft = r.ReadInt(),
                Desc = r.ReadUtf(),
                AssignedName = r.ReadUtf(),
            };
        }
    }
}
