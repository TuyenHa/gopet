using System;
using System.Collections.Generic;
using System.Linq;
using Gopet.Data.GopetItem;

namespace Gopet.Data.Market
{
    /// <summary>
    /// 1 listing gắn kèm loại ki ốt gốc của nó. Cần vì <see cref="Gopet.Data.GopetItem.SellItem"/>
    /// không tự lưu lại nó đang nằm trong ki ốt nào — tab Chợ gộp cả 6 ki ốt thành 1 danh sách
    /// duy nhất nên phải mang theo kioskType để lọc/hiển thị.
    /// </summary>
    public readonly record struct MarketListing(sbyte KioskType, SellItem Item);

    /// <summary>
    /// Lọc/sắp xếp/phân trang thuần cho tab Chợ — không đụng DB/mạng nên test trực tiếp được.
    /// Ẩn listing đã <c>AssignedName</c> khỏi mọi người trừ người bán và người được chỉ định
    /// (so tên không phân biệt hoa thường). Xem plans/260925-2253-cho-troi-market-popup/phase-02.
    /// </summary>
    public static class MarketQuery
    {
        public const int PageSize = 5;

        public readonly record struct Result(IReadOnlyList<MarketListing> Rows, int TotalPages, int Page);

        /// <summary>
        /// Lọc theo <paramref name="filter"/> (-1 = tất cả, 0..5 = kioskType), sắp theo
        /// <paramref name="sort"/> (0 mới nhất, 1 giá tăng, 2 giá giảm — giá bằng nhau thì mới
        /// nhất trước) rồi cắt trang <see cref="PageSize"/> dòng. <paramref name="page"/> vượt
        /// phạm vi sẽ được kẹp lại, không throw.
        /// </summary>
        public static Result Page(IEnumerable<MarketListing> all, int viewerUserId, string viewerName, sbyte filter, sbyte sort, int page)
        {
            IEnumerable<MarketListing> visible = all.Where(l => IsVisibleToViewer(l.Item, viewerUserId, viewerName));
            if (filter >= 0)
            {
                visible = visible.Where(l => l.KioskType == filter);
            }
            List<MarketListing> ordered = Sort(visible, sort).ToList();

            int totalPages = Math.Max(1, (ordered.Count + PageSize - 1) / PageSize);
            int clampedPage = Math.Clamp(page, 0, totalPages - 1);
            List<MarketListing> rows = ordered.Skip(clampedPage * PageSize).Take(PageSize).ToList();
            return new Result(rows, totalPages, clampedPage);
        }

        /// <summary>Listing chưa chỉ định ai thì công khai; đã chỉ định thì chỉ chủ và người được chỉ định thấy.</summary>
        public static bool IsVisibleToViewer(SellItem item, int viewerUserId, string viewerName)
        {
            if (string.IsNullOrEmpty(item.AssignedName))
            {
                return true;
            }
            if (item.user_id == viewerUserId)
            {
                return true;
            }
            return !string.IsNullOrEmpty(viewerName) && string.Equals(item.AssignedName, viewerName, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<MarketListing> Sort(IEnumerable<MarketListing> items, sbyte sort)
        {
            return sort switch
            {
                1 => items.OrderBy(l => l.Item.MathPrice).ThenByDescending(l => l.Item.expireTime),
                2 => items.OrderByDescending(l => l.Item.MathPrice).ThenByDescending(l => l.Item.expireTime),
                _ => items.OrderByDescending(l => l.Item.expireTime),
            };
        }
    }
}
