using System.Reflection;
using Gopet.Data.GopetItem;
using Gopet.Data.Market;
using Gopet.IO;

/// <summary>
/// Regression cho các fix từ code-reviewer report 260926 (Chợ trời) — mỗi test khớp 1 finding cụ
/// thể. Xem plans/260925-2253-cho-troi-market-popup/reports/code-reviewer-260926-market-popup.md.
/// </summary>
static class MarketFixesTests
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    /// <summary>#5: đồ không xếp chồng (item.count=0, vd ngọc vừa tháo ra) phải được chuẩn hoá
    /// về count=1 khi gửi cho client — count=0 làm client khoá nút "Đăng bán" vô lý do.</summary>
    public static void WriteSellableRowNormalizesNonStackableCount()
    {
        var m = new Message(GopetCMD.TYPE_MARKET_SELLABLE_STATE);
        MarketPacketWriter.WriteSellableRow(m, MarketItemCategory.SourceGem, 42, "Ngọc lửa", "icon/gem", 0, null, "mô tả");

        var parsed = new Message(m.getBuffer());
        Check(parsed.readsbyte() == MarketItemCategory.SourceGem, "source lệch");
        Check(parsed.readInt() == 42, "id lệch");
        Check(parsed.readUTF() == "Ngọc lửa", "name lệch");
        Check(parsed.readUTF() == "icon/gem", "iconPath lệch");
        int count = parsed.readInt();
        Check(count == 1, $"count=0 (non-stackable) phải chuẩn hoá về 1, got {count}");
    }

    /// <summary>#5 (đối chứng): đồ xếp chồng bình thường vẫn giữ nguyên count thật, không bị ép về 1.</summary>
    public static void WriteSellableRowKeepsStackableCount()
    {
        var m = new Message(GopetCMD.TYPE_MARKET_SELLABLE_STATE);
        MarketPacketWriter.WriteSellableRow(m, MarketItemCategory.SourceNormal, 7, "Bình máu", "icon/potion", 20, null, "mô tả");

        var parsed = new Message(m.getBuffer());
        parsed.readsbyte();
        parsed.readInt();
        parsed.readUTF();
        parsed.readUTF();
        Check(parsed.readInt() == 20, "count đồ xếp chồng bị ghi đè sai");
    }

    /// <summary>#6: SellItem.RemainingPrice — property dùng chung cho giá HIỂN THỊ
    /// (MarketPacketWriter.WriteRow) và giá THỰC TRỪ (Kiosk.Trade.TryBuyWhole). Tách khỏi
    /// MathPrice (công thức khác, dùng cho luồng NPC cũ) để 2 nơi không còn lệch công thức.</summary>
    public static void RemainingPriceClampsAtZeroAndMatchesOutstanding()
    {
        var sellItem = new SellItem(GopetManager.HOUR_UPLOAD_ITEM) { price = 100_000, sumVal = 0 };
        Check(sellItem.RemainingPrice == 100_000, "chưa bán lẻ dở thì remaining = price gốc");

        sellItem.sumVal = 30_000;
        Check(sellItem.RemainingPrice == 70_000, $"expected 70000, got {sellItem.RemainingPrice}");

        sellItem.sumVal = 100_000;
        Check(sellItem.RemainingPrice == 0, "bán lẻ đủ giá thì remaining phải = 0");

        sellItem.sumVal = 150_000; // không nên xảy ra (sumVal > price) nhưng vẫn phải kẹp về 0
        Check(sellItem.RemainingPrice == 0, "remaining price không được âm");
    }

    /// <summary>#10: FlushMarketSaveIfDirty phải GIỮ cờ dirty khi saveMarket() lỗi — ở đây chắc
    /// chắn lỗi vì GServer.Performance.Tests không có App.config nối MySQL (MYSQLManager.Resolve
    /// throw ConfigurationErrorsException), mô phỏng đúng tình huống mất kết nối DB. Trước fix
    /// #10, cờ bị hạ TRƯỚC khi lưu nên 1 lần lưu lỗi bị lặng lẽ bỏ qua, listing mới nhất không
    /// được ghi lại cho tới mutation kế tiếp.</summary>
    public static void FlushMarketSaveIfDirtyKeepsFlagOnFailedSave()
    {
        var dirtyField = typeof(GopetManager).GetField("_marketDirty", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new Exception("GopetManager._marketDirty không còn tồn tại — cập nhật lại test theo tên field mới");

        GopetManager.RequestMarketSave();
        Check((bool)dirtyField.GetValue(null), "RequestMarketSave phải set cờ dirty");

        // Không được throw ra ngoài dù saveMarket() lỗi — FlushMarketSaveIfDirty tự nuốt lỗi DB.
        GopetManager.FlushMarketSaveIfDirty();

        Check((bool)dirtyField.GetValue(null),
            "cờ dirty phải còn true khi lưu thất bại — không được hạ cờ trước rồi mới lưu (bug #10)");
    }
}
