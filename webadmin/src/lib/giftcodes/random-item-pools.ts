/**
 * Mã nhóm đặc biệt (itemId âm) mà `GIFT_RANDOM_ITEM` hiểu riêng — server rút ngẫu nhiên 1 id
 * thật trong mảng tương ứng thay vì dùng thẳng làm itemId (GameController.cs:4209-4258).
 * Chỉ để hiển thị gợi ý trong builder; admin vẫn có thể gõ tay itemId dương bình thường.
 */
export const RANDOM_ITEM_POOL_CODES: { value: number; label: string }[] = [
  { value: -123, label: "Bạc ngẫu nhiên (ID_ITEM_SILVER)" },
  { value: -124, label: "Pet bậc 1 ngẫu nhiên (ID_ITEM_PET_TIER_ONE)" },
  { value: -125, label: "Pet bậc 2 ngẫu nhiên (ID_ITEM_PET_TIER_TWO)" },
  { value: -126, label: "Điểm tích luỹ (AccumulatedPoint, không phải vật phẩm)" },
  { value: -127, label: "Bạc (loại 2) ngẫu nhiên (ID_ITEM_SILVER2)" },
  { value: -128, label: "Mảnh pet bậc 3 ngẫu nhiên (ID_ITEM_PART_PET_TIER_THREE)" },
  { value: -129, label: "Pet bậc 1 ngẫu nhiên (ID_ITEM_PET_TIER_ONE)" },
  { value: -130, label: "Mảnh cánh bậc 1 ngẫu nhiên (ID_ITEM_PART_WING_TIER_1)" },
  { value: -131, label: "Mảnh cánh bậc 2 ngẫu nhiên (ID_ITEM_PART_WING_TIER_2)" },
  { value: -132, label: "Mảnh cánh bậc 3 ngẫu nhiên (ID_ITEM_PART_WING_TIER_3)" },
  { value: -133, label: "Mảnh Hải Tặc ngẫu nhiên (ID_ITEM_PART_HAI_TAC)" },
  { value: -134, label: "Mảnh Tinh Vân ngẫu nhiên (ID_ITEM_PART_TINH_VAN)" },
  { value: -135, label: "Mảnh Hoàng Kim ngẫu nhiên (ID_ITEM_PART_HOANG_KIM)" },
  { value: -136, label: "Mảnh pet bậc 4 ngẫu nhiên (ID_ITEM_PART_PET_TIER_FOUR)" },
  { value: -137, label: "Mảnh pet bậc 5 ngẫu nhiên (ID_ITEM_PART_PET_TIER_FIVE)" },
];
