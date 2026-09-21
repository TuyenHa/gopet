using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Bảng màu chung của các popup trong game, bám ảnh mẫu cửa hàng: nền trắng sáng,
    /// viền xanh dương, tab vàng khi chọn, nút giá xanh lá.
    ///
    /// <para>Gom vào một chỗ vì nhiều lớp cùng dùng (<see cref="GamePopupFrame"/>,
    /// <see cref="ShopPopupView"/>, <see cref="AtmPopupView"/>, <see cref="ShopItemRow"/>,
    /// <see cref="ShopChip"/>) — rải màu hex khắp nơi thì sửa tông một lần phải lần
    /// từng file và chắc chắn sót.</para>
    /// </summary>
    public static class PopupPalette
    {
        /// <summary>Nền popup.</summary>
        public static readonly Color Panel = new Color(0.94f, 0.97f, 1f, 1f);

        /// <summary>Viền ngoài popup và viền badge tiêu đề.</summary>
        public static readonly Color Border = new Color(0.36f, 0.66f, 0.98f, 1f);

        /// <summary>Nền badge tiêu đề và tab chưa chọn (tab dùng bản nhạt hơn).</summary>
        public static readonly Color HeaderBlue = new Color(0.30f, 0.55f, 0.88f, 1f);

        /// <summary>
        /// Nút hành động trong thân popup. Nhạt hơn <see cref="HeaderBlue"/> một nấc
        /// cho bớt nặng, nhưng KHÔNG nhạt hơn nữa: chữ trắng đậm trên nền này đã chỉ
        /// còn tương phản ~2.6:1, nhạt thêm là phải đổi chữ sang navy.
        /// </summary>
        public static readonly Color ButtonBlue = new Color(0.40f, 0.63f, 0.93f, 1f);

        public static readonly Color TabActive = new Color(0.98f, 0.76f, 0.22f, 1f);
        public static readonly Color TabInactive = new Color(0.69f, 0.84f, 1f, 1f);

        /// <summary>Chữ trên tab chưa chọn và trên hầu hết nhãn tối.</summary>
        public static readonly Color TextDark = new Color(0.16f, 0.26f, 0.46f, 1f);

        public static readonly Color TextMuted = new Color(0.46f, 0.52f, 0.60f, 1f);

        /// <summary>Nền khung danh sách bên trong popup.</summary>
        public static readonly Color ListBg = new Color(1f, 1f, 1f, 1f);

        /// <summary>
        /// Viền khung danh sách, vạch ngăn giữa các dòng, viền chip.
        ///
        /// <para>Đậm hơn một nấc so với bản đầu (0.84, 0.89, 0.95): nét chỉ dày 1 đơn
        /// vị, mà hai mép của nó đều bị khử răng cưa ăn vào nên phần đặc chỉ còn nửa
        /// đơn vị. Màu nhạt quá thì ở góc bo — nơi cả hai cung cùng mờ dần — nét gần
        /// như biến mất và góc trông như bị trắng.</para>
        /// </summary>
        public static readonly Color Hairline = new Color(0.72f, 0.81f, 0.91f, 1f);

        // Lấy đúng mã màu đo trên ảnh mẫu: ruột rgb(95,195,55), mép dưới sẫm hơn
        // rgb(74,167,47) — dùng làm màu viền.
        public static readonly Color PriceGreen = new Color(0.373f, 0.765f, 0.216f, 1f);
        public static readonly Color PriceGreenEdge = new Color(0.290f, 0.655f, 0.184f, 1f);

        /// <summary>Dòng server khoá hẳn (<c>CanSelect = false</c>). Thiếu tiền KHÔNG dùng màu này.</summary>
        public static readonly Color PriceLocked = new Color(0.72f, 0.75f, 0.79f, 1f);
        public static readonly Color PriceLockedEdge = new Color(0.58f, 0.61f, 0.66f, 1f);
    }
}
