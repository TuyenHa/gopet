/**
 * Nguồn DUY NHẤT mô tả các loại quà giftcode có thể chọn trong UI.
 *
 * Đối chiếu `GameController.onReiceiveGift` (GameController.cs:4106-4390): switch chỉ có
 * case cho các giá trị dưới đây. `GIFT_ITEM_PERCENT=3`, `GIFT_ITEM_MERGE_PET=5`,
 * `GIFT_ITEM_MERGE_ITEM=6` (GopetManager.cs:202-232) KHÔNG có case → server vẫn trừ lượt
 * dùng của code nhưng bỏ qua phần quà đó. TUYỆT ĐỐI không thêm 3/5/6 vào danh sách này.
 */
export const GIFT_TYPE = {
  GOLD: 0,
  COIN: 1,
  ITEM: 2,
  ITEM_PERCENT_NO_DROP_MORE: 4,
  EXP: 7,
  ENERGY: 8,
  RANDOM_ITEM: 9,
  ITEM_MAX_OPTION: 10,
  EVENT_POINT: 11,
  FUND_CLAN: 12,
  TITLE: 13,
  SKIN: 14,
  PET_TRIAL: 15,
} as const;

export type GiftTypeValue = (typeof GIFT_TYPE)[keyof typeof GIFT_TYPE];

export const GIFT_TYPE_LABELS: Record<GiftTypeValue, string> = {
  [GIFT_TYPE.GOLD]: "Vàng",
  [GIFT_TYPE.COIN]: "Ngọc",
  [GIFT_TYPE.ITEM]: "Vật phẩm",
  [GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE]: "Vật phẩm theo tỉ lệ % (chỉ rơi tối đa 1 lần / lượt đổi)",
  [GIFT_TYPE.EXP]: "Kinh nghiệm cho pet đang theo",
  [GIFT_TYPE.ENERGY]: "Năng lượng (sao)",
  [GIFT_TYPE.RANDOM_ITEM]: "Vật phẩm ngẫu nhiên (rút nhiều lần trong danh sách)",
  [GIFT_TYPE.ITEM_MAX_OPTION]: "Vật phẩm chỉ số tối đa",
  [GIFT_TYPE.EVENT_POINT]: "Điểm sự kiện",
  [GIFT_TYPE.FUND_CLAN]: "Quỹ bang hội (chỉ cộng nếu người đổi đang trong 1 bang)",
  [GIFT_TYPE.TITLE]: "Danh hiệu",
  [GIFT_TYPE.SKIN]: "Trang phục (skin)",
  [GIFT_TYPE.PET_TRIAL]: "Thú cưng thử nghiệm",
};

export const GIFT_TYPE_OPTIONS: { value: GiftTypeValue; label: string }[] = Object.values(GIFT_TYPE).map((value) => ({
  value,
  label: GIFT_TYPE_LABELS[value],
}));
