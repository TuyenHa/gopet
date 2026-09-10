using System;

namespace Gopet.UiLogic
{
    /// <summary>Khoảng dòng cần dựng thật, tính từ <see cref="First"/>.</summary>
    public readonly struct VisibleRange : IEquatable<VisibleRange>
    {
        public readonly int First;
        public readonly int Count;

        public VisibleRange(int first, int count)
        {
            First = first;
            Count = count;
        }

        public int LastExclusive => First + Count;

        public bool Contains(int index) => index >= First && index < LastExclusive;

        public bool Equals(VisibleRange other) => First == other.First && Count == other.Count;

        public override bool Equals(object obj) => obj is VisibleRange other && Equals(other);

        public override int GetHashCode() => (First * 397) ^ Count;

        public override string ToString() => $"[{First}..{LastExclusive})";
    }

    /// <summary>
    /// Tính xem với vị trí cuộn hiện tại thì cần dựng những dòng nào.
    ///
    /// <para>Danh sách của game có thể vài trăm dòng. Dựng hết là vài trăm
    /// GameObject cho một màn hình mà mắt chỉ thấy chục dòng — tụt frame ngay trên
    /// máy tầm trung, và đó là thứ phải làm từ đầu chứ không phải tối ưu sau.</para>
    ///
    /// <para>Toán thuần, không UnityEngine: chỗ dễ sai nhất của virtualization là
    /// tính lệch một dòng ở mép, và lỗi đó test được ngoài Editor.</para>
    /// </summary>
    public static class MenuVirtualizer
    {
        /// <summary>
        /// Số dòng dựng dư mỗi phía. Không có đệm thì dòng ở mép hiện ra đúng lúc
        /// nó chạm biên — người chơi thấy giật vào/giật ra khi cuộn.
        /// </summary>
        public const int DefaultOverscan = 2;

        /// <param name="itemCount">Tổng số dòng của danh sách.</param>
        /// <param name="rowHeight">Chiều cao một dòng, phải lớn hơn 0.</param>
        /// <param name="viewportHeight">Chiều cao vùng nhìn thấy.</param>
        /// <param name="scrollY">Đã cuộn xuống bao nhiêu, tính từ đỉnh. Âm coi như 0.</param>
        public static VisibleRange Compute(int itemCount, float rowHeight, float viewportHeight,
                                           float scrollY, int overscan = DefaultOverscan)
        {
            if (rowHeight <= 0f) throw new ArgumentOutOfRangeException(nameof(rowHeight), "Chiều cao dòng phải > 0.");
            if (overscan < 0) throw new ArgumentOutOfRangeException(nameof(overscan), "Đệm không được âm.");

            if (itemCount <= 0 || viewportHeight <= 0f) return new VisibleRange(0, 0);

            if (scrollY < 0f) scrollY = 0f;

            var first = (int)Math.Floor(scrollY / rowHeight) - overscan;
            if (first < 0) first = 0;

            // Ceil chứ không phải floor: cuộn lệch nửa dòng thì vẫn thấy một phần
            // của dòng cuối, thiếu nó là để lại khoảng trắng ở mép dưới.
            var lastExclusive = (int)Math.Ceiling((scrollY + viewportHeight) / rowHeight) + overscan;
            if (lastExclusive > itemCount) lastExclusive = itemCount;

            if (first >= itemCount) first = itemCount - 1;
            if (lastExclusive <= first) return new VisibleRange(first, 0);

            return new VisibleRange(first, lastExclusive - first);
        }

        /// <summary>Tổng chiều cao nội dung, để đặt kích thước vùng cuộn.</summary>
        public static float ContentHeight(int itemCount, float rowHeight)
        {
            if (itemCount <= 0) return 0f;
            return itemCount * rowHeight;
        }

        /// <summary>Toạ độ Y của một dòng trong vùng nội dung, tính từ đỉnh xuống.</summary>
        public static float OffsetOf(int index, float rowHeight) => index * rowHeight;
    }
}
