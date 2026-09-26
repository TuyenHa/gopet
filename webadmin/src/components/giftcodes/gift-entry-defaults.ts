import { GIFT_TYPE, type GiftEntry, type GiftTypeValue } from "@/lib/giftcodes/gift-data-schema";

/** Giá trị khởi tạo hợp lệ khi thêm mục mới hoặc đổi loại quà của 1 dòng. */
export function defaultEntry(type: GiftTypeValue): GiftEntry {
  switch (type) {
    case GIFT_TYPE.GOLD:
      return { type, amount: 1000 };
    case GIFT_TYPE.COIN:
      return { type, amount: 100 };
    case GIFT_TYPE.ITEM:
      return { type, itemId: 0, count: 1, canTrade: true };
    case GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE:
      return { type, itemId: 0, percent: 100, count: 1 };
    case GIFT_TYPE.EXP:
      return { type, amount: 100 };
    case GIFT_TYPE.ENERGY:
      return { type, amount: 1 };
    case GIFT_TYPE.RANDOM_ITEM:
      return { type, numGift: 1, items: [{ count: 1, itemId: 0 }] };
    case GIFT_TYPE.ITEM_MAX_OPTION:
      return { type, itemId: 0, count: 1 };
    case GIFT_TYPE.EVENT_POINT:
      return { type, amount: 1 };
    case GIFT_TYPE.FUND_CLAN:
      return { type, amount: 1000 };
    case GIFT_TYPE.TITLE:
      return { type, titleId: 0, isInfinite: true, duration: { minutes: 0, hours: 0, days: 0, months: 0, years: 0 } };
    case GIFT_TYPE.SKIN:
      return { type, itemId: 0, isInfinite: true, duration: { minutes: 0, hours: 0, days: 0, months: 0, years: 0 } };
    case GIFT_TYPE.PET_TRIAL:
      return { type, petId: 0, isInfinite: true, duration: { minutes: 0, hours: 0, days: 0, months: 0, years: 0 } };
  }
}
