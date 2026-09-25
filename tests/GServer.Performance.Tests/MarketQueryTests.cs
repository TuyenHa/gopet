using Gopet.Data.GopetItem;
using Gopet.Data.Market;
using Gopet.Util;
using System.Collections.Generic;
using System.Linq;

static class MarketQueryTests
{
    const int ViewerUserId = 100;
    const int SellerUserId = 101;
    const int AssigneeUserId = 102;

    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    static List<MarketListing> CreateTestListings()
    {
        var listings = new List<MarketListing>();

        // Create listings in each category
        for (sbyte kioskType = 0; kioskType <= 5; kioskType++)
        {
            for (int i = 0; i < 3; i++)
            {
                var item = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
                {
                    itemId = (kioskType * 100) + i,
                    user_id = SellerUserId,
                    price = 1000 + (i * 100),
                    SellerName = "Seller",
                    expireTime = Utilities.TimeHours(24) - (i * 3600000L) // spread expiration
                };
                item.ItemSell = new Item { itemTemplateId = 1, count = 1 };

                listings.Add(new MarketListing(kioskType, item));
            }
        }

        return listings;
    }

    public static void FilterBySingleKioskType()
    {
        var listings = CreateTestListings();
        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", 0, 0, 0);

        Check(result.Rows.All(r => r.KioskType == 0), "filter 0 should only return kiosk type 0");
        Check(result.Rows.Count <= MarketQuery.PageSize, "should respect page size");
    }

    public static void FilterAllTypes()
    {
        var listings = CreateTestListings();
        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 0, 0);

        Check(result.Rows.Count > 0, "filter -1 (all) should return results");
    }

    public static void SortNewest()
    {
        var listings = CreateTestListings();
        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 0, 0);

        for (int i = 1; i < result.Rows.Count; i++)
        {
            Check(result.Rows[i - 1].Item.expireTime >= result.Rows[i].Item.expireTime,
                "sort 0 (newest) should order by expireTime descending");
        }
    }

    public static void SortPriceAscending()
    {
        var listings = CreateTestListings();
        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 1, 0);

        for (int i = 1; i < result.Rows.Count; i++)
        {
            Check(result.Rows[i - 1].Item.MathPrice <= result.Rows[i].Item.MathPrice,
                "sort 1 (price asc) should order by price ascending");
        }
    }

    public static void SortPriceDescending()
    {
        var listings = CreateTestListings();
        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 2, 0);

        for (int i = 1; i < result.Rows.Count; i++)
        {
            Check(result.Rows[i - 1].Item.MathPrice >= result.Rows[i].Item.MathPrice,
                "sort 2 (price desc) should order by price descending");
        }
    }

    public static void SortStable()
    {
        // Create listings with same price to test secondary sort
        var listings = new List<MarketListing>();
        for (int i = 0; i < 10; i++)
        {
            var item = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
            {
                itemId = i,
                user_id = SellerUserId,
                price = 10000, // same price
                SellerName = "Seller",
                expireTime = Utilities.TimeHours(24) - (i * 1000L)
            };
            item.ItemSell = new Item { itemTemplateId = 1, count = 1 };
            listings.Add(new MarketListing(0, item));
        }

        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 1, 0);

        // When prices are equal, should sort by expireTime descending
        for (int i = 1; i < result.Rows.Count; i++)
        {
            Check(result.Rows[i - 1].Item.MathPrice == result.Rows[i].Item.MathPrice,
                "prices should be equal for stability test");
            Check(result.Rows[i - 1].Item.expireTime >= result.Rows[i].Item.expireTime,
                "equal prices should sort by expireTime descending");
        }
    }

    public static void PaginationClamps()
    {
        var listings = CreateTestListings();

        // Request page way beyond total
        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 0, 9999);

        Check(result.Page < result.TotalPages, "page should be clamped");
        Check(result.Page == result.TotalPages - 1, "page should clamp to last page");
        Check(result.Rows.Count > 0, "last page should have rows");
    }

    public static void IsMineVisibility()
    {
        var listings = new List<MarketListing>();

        var myItem = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
        {
            itemId = 1,
            user_id = ViewerUserId,
            price = 10000,
            SellerName = "Me"
        };
        myItem.ItemSell = new Item { itemTemplateId = 1, count = 1 };

        var otherItem = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
        {
            itemId = 2,
            user_id = SellerUserId,
            price = 10000,
            SellerName = "Other"
        };
        otherItem.ItemSell = new Item { itemTemplateId = 1, count = 1 };

        listings.Add(new MarketListing(0, myItem));
        listings.Add(new MarketListing(0, otherItem));

        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 0, 0);

        var myRow = result.Rows.FirstOrDefault(r => r.Item.itemId == 1);
        Check(myRow.Item != null && myRow.Item.user_id == ViewerUserId, "should see own item");
    }

    public static void AssignedItemHiddenFromThirdParty()
    {
        var listings = new List<MarketListing>();

        var assignedItem = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
        {
            itemId = 1,
            user_id = SellerUserId,
            price = 10000,
            SellerName = "Seller",
            AssignedName = "Assignee"
        };
        assignedItem.ItemSell = new Item { itemTemplateId = 1, count = 1 };

        listings.Add(new MarketListing(0, assignedItem));

        // Third party should NOT see it
        var result = MarketQuery.Page(listings, ViewerUserId, "ThirdParty", -1, 0, 0);
        Check(result.Rows.Count == 0, "third party should not see assigned item");

        // Seller SHOULD see it
        result = MarketQuery.Page(listings, SellerUserId, "Seller", -1, 0, 0);
        Check(result.Rows.Count == 1, "seller should see assigned item");

        // Assignee SHOULD see it (case-insensitive)
        result = MarketQuery.Page(listings, AssigneeUserId, "assignee", -1, 0, 0);
        Check(result.Rows.Count == 1, "assignee should see assigned item (case-insensitive)");
    }

    public static void UnassignedItemVisibleToAll()
    {
        var listings = new List<MarketListing>();

        var publicItem = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
        {
            itemId = 1,
            user_id = SellerUserId,
            price = 10000,
            SellerName = "Seller",
            AssignedName = null
        };
        publicItem.ItemSell = new Item { itemTemplateId = 1, count = 1 };

        listings.Add(new MarketListing(0, publicItem));

        // Anyone should see it
        var result = MarketQuery.Page(listings, ViewerUserId, "Anyone", -1, 0, 0);
        Check(result.Rows.Count == 1, "unassigned item should be visible to anyone");

        result = MarketQuery.Page(listings, 99999, "Unknown", -1, 0, 0);
        Check(result.Rows.Count == 1, "unassigned item should be visible to strangers");
    }

    public static void TotalPagesCalculation()
    {
        var listings = new List<MarketListing>();

        // Create 13 listings (should be 3 pages with PageSize=5)
        for (int i = 0; i < 13; i++)
        {
            var item = new SellItem(GopetManager.HOUR_UPLOAD_ITEM)
            {
                itemId = i,
                user_id = SellerUserId,
                price = 10000,
                SellerName = "Seller"
            };
            item.ItemSell = new Item { itemTemplateId = 1, count = 1 };
            listings.Add(new MarketListing(0, item));
        }

        var result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 0, 0);

        Check(result.TotalPages == 3, $"13 items with PageSize=5 should give 3 pages, got {result.TotalPages}");

        result = MarketQuery.Page(listings, ViewerUserId, "Viewer", -1, 0, 2);
        Check(result.Page == 2 && result.Rows.Count == 3, "last page should have 3 rows");
    }
}
