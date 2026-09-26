using Gopet.Data.GopetItem;

namespace Gopet.Data.Map
{
    /// <summary>
    /// Kết quả 1 thao tác ki ốt (treo/mua/gỡ/chỉ định). Không tự gửi dialog cho client — caller
    /// (luồng NPC map 22 hoặc popup chợ trời phase 2) tự quyết định gọi okDialog/redDialog với
    /// <see cref="Message"/>.
    /// </summary>
    public readonly struct KioskResult
    {
        public bool Ok { get; }
        public string Message { get; }
        public SellItem Listing { get; }

        private KioskResult(bool ok, string message, SellItem listing)
        {
            Ok = ok;
            Message = message;
            Listing = listing;
        }

        public static KioskResult Success(string message = null, SellItem listing = null) => new(true, message, listing);
        public static KioskResult Fail(string message) => new(false, message, null);
    }

    /// <summary>
    /// Yêu cầu treo bán dùng chung cho luồng NPC (phase 1) và popup Đăng bán (phase 2).
    /// <see cref="Source"/> theo quy ước <see cref="Gopet.Data.Market.MarketItemCategory"/>:
    /// 0 = trang bị (EQUIP_PET_INVENTORY), 1 = đồ thường (NORMAL_INVENTORY),
    /// 4 = ngọc (GEM_INVENTORY), -1 = pet.
    /// </summary>
    public readonly record struct ListRequest(sbyte Source, int ItemOrPetId, int Count, int Price);
}
