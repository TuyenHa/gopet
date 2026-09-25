
using Gopet.Data.Map;

public class MarketPlace : GopetPlace
{

    public static Kiosk[] kiosks { get; private set; } = new Kiosk[]{
        new Kiosk(GopetManager.KIOSK_HAT),
        new Kiosk(GopetManager.KIOSK_WEAPON),
        new Kiosk(GopetManager.KIOSK_AMOUR),
        new Kiosk(GopetManager.KIOSK_GEM),
        new Kiosk(GopetManager.KIOSK_PET),
        new Kiosk(GopetManager.KIOSK_OTHER)
    };

    public static void setKiosks(Kiosk[] kiosks_)
    {
        foreach (var item in kiosks_)
        {
            var kioskNeedFind = kiosks.Where(p => p.kioskType == item.kioskType).ToList();
            if(kioskNeedFind.Any())
            {
                kioskNeedFind.First().kioskItems = item.kioskItems;
            }
        }
    }

    public MarketPlace(GopetMap m, int ID) : base(m, ID)
    {
        if (m.mapID != 22)
        {
            throw new UnsupportedOperationException("Map này méo phải map chợ trời");
        }
        maxPlayer = 50;
    }

    // Expire ki ốt chạy ở MarketExpiryTicker (1 lần/tick RuntimeServer), không còn ở đây.
    // Map 22 tạo 10 MarketPlace (xem MarketMap.createZoneDefault) nên trước đây expire chạy
    // trùng 10 lần/tick và chỉ chạy khi có người online cập nhật map 22.

    public static Kiosk getKiosk(sbyte type)
    {
        foreach (Kiosk kiosk in kiosks)
        {
            if (kiosk.kioskType == type)
            {
                return kiosk;
            }
        }
        return null;
    }
}
