namespace Gopet.UiLogic
{
    /// <summary>
    /// Tính bố cục N nút của hộp thoại chọn (ChoiceDialogView): hàng ngang khi ≤3 nút (giữ
    /// nguyên pixel với giao diện cũ), xếp DỌC khi nhiều hơn — nhãn dài như tuỳ chọn NPC
    /// (vd 7 chức năng của NPC Trần Chấn) không đọc được nếu ép trên một hàng ngang hẹp.
    /// Thuần C# — testable ngoài Unity runtime.
    /// </summary>
    public static class ChoiceDialogLayout
    {
        public const float PanelWidth = 420f;
        public const float DefaultPanelHeight = 190f;
        public const float ButtonHeight = 42f;
        public const float HorizontalGap = 12f;
        public const float VerticalGap = 8f;
        public const float MinRowHeight = 30f;
        public const float BottomPadding = 16f;
        public const int DefaultFontSize = 16;
        public const int CompactFontSize = 14;

        /// <summary>Vùng dành cho tiêu đề/nội dung phía trên khối nút khi xếp dọc.</summary>
        public const float TopPadding = 48f;

        private const float MaxPanelHeightRatio = 0.88f;
        private const float HorizontalMaxButtonWidth = 170f;

        /// <summary>Từ 4 nút trở lên mới xếp dọc — ≤3 nút vẫn là các hộp quen thuộc (YesNo,
        /// Confirm, kênh...), giữ nguyên bố cục hàng ngang.</summary>
        private const int VerticalThreshold = 4;

        public readonly struct Result
        {
            public readonly bool Vertical;
            public readonly float RowWidth;
            public readonly float RowHeight;
            public readonly float Gap;
            public readonly float PanelHeight;
            public readonly int FontSize;

            /// <summary>Khoảng cách từ đáy panel tới đỉnh khối nút — dùng để neo vùng message
            /// phía trên khối nút khi xếp dọc.</summary>
            public readonly float ButtonsTop;

            public Result(bool vertical, float rowWidth, float rowHeight, float gap,
                float panelHeight, int fontSize, float buttonsTop)
            {
                Vertical = vertical;
                RowWidth = rowWidth;
                RowHeight = rowHeight;
                Gap = gap;
                PanelHeight = panelHeight;
                FontSize = fontSize;
                ButtonsTop = buttonsTop;
            }
        }

        public static Result Compute(int count, float screenHeight)
        {
            if (count < 1) count = 1;

            if (count < VerticalThreshold)
            {
                var width = Min(HorizontalMaxButtonWidth,
                    (PanelWidth - 48f - HorizontalGap * (count - 1)) / count);
                var buttonsTop = BottomPadding + ButtonHeight;
                return new Result(false, width, ButtonHeight, HorizontalGap, DefaultPanelHeight,
                    DefaultFontSize, buttonsTop);
            }

            var rowHeight = ButtonHeight;
            var fontSize = DefaultFontSize;
            var maxPanelHeight = screenHeight * MaxPanelHeightRatio;
            var panelHeight = TopPadding + count * rowHeight + (count - 1) * VerticalGap + BottomPadding;

            if (panelHeight > maxPanelHeight)
            {
                var availableForButtons = maxPanelHeight - TopPadding - BottomPadding
                    - (count - 1) * VerticalGap;
                rowHeight = Max(MinRowHeight, availableForButtons / count);
                fontSize = CompactFontSize;
                // KHÔNG ép panelHeight về maxPanelHeight nữa: khi rowHeight đã chạm sàn
                // MinRowHeight, availableForButtons/count có thể vẫn nhỏ hơn MinRowHeight,
                // nghĩa là khối nút thật sự cần nhiều chỗ hơn 88% màn hình. Panel phải theo
                // đúng chiều cao nội dung để nút không tràn ra ngoài — chấp nhận vượt 88%
                // còn hơn hiện nút bị cắt (đúng lỗi mà tính năng này sinh ra để sửa).
                panelHeight = TopPadding + count * rowHeight + (count - 1) * VerticalGap + BottomPadding;
            }

            var top = BottomPadding + count * rowHeight + (count - 1) * VerticalGap;
            return new Result(true, PanelWidth - 48f, rowHeight, VerticalGap, panelHeight, fontSize, top);
        }

        private static float Min(float a, float b) => a < b ? a : b;
        private static float Max(float a, float b) => a > b ? a : b;
    }
}
