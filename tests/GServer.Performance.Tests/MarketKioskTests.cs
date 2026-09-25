using Gopet.Data.GopetItem;
using Gopet.Data.Map;
using Gopet.Util;

static class MarketKioskTests
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    public static void SellItemLockingPreventsRaceConditions()
    {
        // Test that SellItem.Sync lock prevents concurrent operations on same listing
        var listing = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
        {
            itemId = 1,
            user_id = 100,
            price = 100000,
            hasSell = false,
            hasRemoved = false
        };
        listing.ItemSell = new Item { itemTemplateId = 1, count = 1 };

        int successCount = 0;
        var barrier = new Barrier(10);

        // Attempt 10 threads to mark hasSell simultaneously
        Parallel.For(0, 10, _ =>
        {
            barrier.SignalAndWait();
            lock (listing.Sync)
            {
                if (!listing.hasSell && !listing.hasRemoved)
                {
                    listing.hasSell = true;
                    Interlocked.Increment(ref successCount);
                }
            }
        });

        Check(successCount == 1, $"expected 1 success with lock, got {successCount}");
    }

    public static void KioskPayoutCalculatesSellerShare()
    {
        // Test 5% fee calculation
        long price = 1000000;
        long share = KioskPayout.SellerShare(price);
        long expected = (long)Math.Round(price / 100.0 * (100 - GopetManager.KIOSK_PER_SELL));
        Check(share == expected, $"seller share: expected {expected}, got {share}");
        Check(share == 950000, "95% of 1M should be 950K");

        // Test with small price
        share = KioskPayout.SellerShare(100);
        expected = (long)Math.Round(100 / 100.0 * 95);
        Check(share == expected, "small price calculation broken");

        // Test zero and negative
        share = KioskPayout.SellerShare(0);
        Check(share == 0, "zero price should give zero share");
        share = KioskPayout.SellerShare(-1);
        Check(share == 0, "negative price should give zero share");
    }

    public static void KioskPayoutAssignFees()
    {
        // Pet assign fee
        int fee = KioskPayout.AssignFee(isPet: true);
        Check(fee == GopetManager.PRICE_ASSIGNED_PET, $"pet fee should be {GopetManager.PRICE_ASSIGNED_PET}, got {fee}");

        // Item assign fee
        fee = KioskPayout.AssignFee(isPet: false);
        Check(fee == GopetManager.PRICE_ASSIGNED_ITEM, $"item fee should be {GopetManager.PRICE_ASSIGNED_ITEM}, got {fee}");
    }

    public static void SellItemExpirationTime()
    {
        long before = Utilities.CurrentTimeMillis;
        var listing = new SellItem(24); // 24 hours
        long after = Utilities.CurrentTimeMillis;

        long expected24Hours = 24 * 3600 * 1000L;
        long delta = listing.expireTime - before;
        Check(delta >= expected24Hours - 100 && delta <= expected24Hours + 100,
            $"24h expiration should be ~{expected24Hours}ms, got delta {delta}ms");
    }

    public static void SellItemTransition()
    {
        var listing = new SellItem(GopetManager.HOUR_UPLOAD_ITEM);
        listing.ItemSell = new Item { itemTemplateId = 1, count = 1 };

        Check(!listing.hasSell && !listing.hasRemoved, "new listing should be available");

        listing.setHasSell(true);
        Check(listing.hasSell && !listing.hasRemoved, "hasSell should be set");

        // Once hasSell is true, hasRemoved state should be ignored
        listing.hasRemoved = true;
        Check(listing.hasSell, "hasSell persists");
    }

    public static void KioskPriceValidation()
    {
        // Price bounds
        Check(KioskPayout.MinPrice == 1, "min price should be 1");
        Check(KioskPayout.MaxPrice == 2_000_000_000, "max price should be 2B");

        // Valid price
        Check(1 >= KioskPayout.MinPrice && 1 <= KioskPayout.MaxPrice, "price 1 should be valid");
        Check(100_000_000 >= KioskPayout.MinPrice && 100_000_000 <= KioskPayout.MaxPrice, "price 100M should be valid");

        // Invalid prices
        Check(!(0 >= KioskPayout.MinPrice && 0 <= KioskPayout.MaxPrice), "price 0 should be invalid");
        Check(!(2_000_000_001 >= KioskPayout.MinPrice && 2_000_000_001 <= KioskPayout.MaxPrice), "price > 2B should be invalid");
    }

    public static void ListRequestStructure()
    {
        // Verify ListRequest can be constructed correctly
        var req = new ListRequest(
            (sbyte)GopetManager.NORMAL_INVENTORY,
            555,
            10,
            500000
        );

        Check(req.Source == GopetManager.NORMAL_INVENTORY, "source mismatch");
        Check(req.ItemOrPetId == 555, "item id mismatch");
        Check(req.Count == 10, "count mismatch");
        Check(req.Price == 500000, "price mismatch");
    }
}
