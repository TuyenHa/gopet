import { GIFT_TYPE } from "./gift-type-labels";
import { giftEntrySchema, type GiftEntry } from "./gift-entry-schema";

/** `gift_code.gift_data varchar(15000)` (server_db.sql) — chặn tổng dung lượng dù mỗi mục/dòng
 * đã bị cap riêng (M1), phòng trường hợp nhiều mục RANDOM_ITEM cộng dồn vượt cột.
 * File này (và `gift-entry-schema.ts`) cũng được import bởi 1 Client Component
 * (`gift-data-builder.tsx`, xem preview JSON) — KHÔNG được import gì phụ thuộc "server-only"
 * (vd `@/lib/actions/action-result`) ở đây, nếu không build lỗi "cannot be imported from a
 * Client Component". Vì vậy ném `Error` thường; 2 action gọi `giftDataToJson` tự bọc lại
 * thành `UserFacingError`. */
const GIFT_DATA_MAX_JSON_LEN = 15_000;

/** 1 mục quà (dạng đã validate) → mảng `int[]` đúng thứ tự server đọc (GameController.cs:4106-4390). */
export function giftEntryToArray(e: GiftEntry): number[] {
  switch (e.type) {
    case GIFT_TYPE.GOLD:
    case GIFT_TYPE.COIN:
    case GIFT_TYPE.EXP:
    case GIFT_TYPE.ENERGY:
    case GIFT_TYPE.EVENT_POINT:
    case GIFT_TYPE.FUND_CLAN:
      return [e.type, e.amount];
    case GIFT_TYPE.ITEM:
      return [e.type, e.itemId, e.count, e.canTrade ? 1 : 0];
    case GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE:
      return [e.type, e.itemId, e.percent, e.count];
    case GIFT_TYPE.RANDOM_ITEM:
      return [e.type, e.numGift, ...e.items.flatMap((it) => [it.count, it.itemId])];
    case GIFT_TYPE.ITEM_MAX_OPTION:
      return [e.type, e.itemId, e.count];
    case GIFT_TYPE.TITLE:
      return e.isInfinite
        ? [e.type, e.titleId, 1]
        : [e.type, e.titleId, 0, e.duration.minutes, e.duration.hours, e.duration.days, e.duration.months, e.duration.years];
    case GIFT_TYPE.SKIN:
      return e.isInfinite ? [e.type, e.itemId, 1] : [e.type, e.itemId, 0, e.duration.minutes, e.duration.hours, e.duration.days];
    case GIFT_TYPE.PET_TRIAL:
      return e.isInfinite
        ? [e.type, e.petId, 1]
        : [e.type, e.petId, 0, e.duration.minutes, e.duration.hours, e.duration.days, e.duration.months, e.duration.years];
  }
}

export function giftDataToArrays(entries: GiftEntry[]): number[][] {
  return entries.map(giftEntryToArray);
}

export function giftDataToJson(entries: GiftEntry[]): string {
  const json = JSON.stringify(giftDataToArrays(entries));
  if (json.length > GIFT_DATA_MAX_JSON_LEN) {
    throw new Error(
      `Danh sách quà quá dài (${json.length}/${GIFT_DATA_MAX_JSON_LEN} ký tự sau khi mã hoá) — giảm bớt mục quà hoặc số dòng ngẫu nhiên.`,
    );
  }
  return json;
}

/** `gift_code.gift_data` (chuỗi JSON) → `int[][]`; JSON hỏng → mảng rỗng (không chặn hiển thị). */
export function parseGiftDataJson(json: string): number[][] {
  try {
    const val: unknown = JSON.parse(json);
    return Array.isArray(val) ? val.filter((x): x is number[] => Array.isArray(x)) : [];
  } catch {
    return [];
  }
}

/** Dựng lại 1 mục quà dạng object thô từ `int[]` để validate lại qua {@link giftEntrySchema}.
 * Trả `null` khi loại không rõ (giftcode cũ chứa loại không có handler, vd 3/5/6). */
function arrayToCandidate(arr: number[]): unknown {
  const type = arr[0];
  switch (type) {
    case GIFT_TYPE.GOLD:
    case GIFT_TYPE.COIN:
    case GIFT_TYPE.EXP:
    case GIFT_TYPE.ENERGY:
    case GIFT_TYPE.EVENT_POINT:
    case GIFT_TYPE.FUND_CLAN:
      return { type, amount: arr[1] };
    case GIFT_TYPE.ITEM:
      return { type, itemId: arr[1], count: arr[2], canTrade: arr[3] === 1 };
    case GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE:
      return { type, itemId: arr[1], percent: arr[2], count: arr[3] };
    case GIFT_TYPE.RANDOM_ITEM: {
      const items: { count: number; itemId: number }[] = [];
      for (let i = 2; i + 1 < arr.length; i += 2) items.push({ count: arr[i], itemId: arr[i + 1] });
      return { type, numGift: arr[1], items };
    }
    case GIFT_TYPE.ITEM_MAX_OPTION:
      return { type, itemId: arr[1], count: arr[2] };
    case GIFT_TYPE.TITLE:
      return {
        type,
        titleId: arr[1],
        isInfinite: arr[2] === 1,
        duration: { minutes: arr[3] ?? 0, hours: arr[4] ?? 0, days: arr[5] ?? 0, months: arr[6] ?? 0, years: arr[7] ?? 0 },
      };
    case GIFT_TYPE.SKIN:
      return {
        type,
        itemId: arr[1],
        isInfinite: arr[2] === 1,
        duration: { minutes: arr[3] ?? 0, hours: arr[4] ?? 0, days: arr[5] ?? 0, months: 0, years: 0 },
      };
    case GIFT_TYPE.PET_TRIAL:
      return {
        type,
        petId: arr[1],
        isInfinite: arr[2] === 1,
        duration: { minutes: arr[3] ?? 0, hours: arr[4] ?? 0, days: arr[5] ?? 0, months: arr[6] ?? 0, years: arr[7] ?? 0 },
      };
    default:
      return null;
  }
}

/** `int[][]` (đọc từ DB) → danh sách `GiftEntry` để hiển thị builder. Mục không hợp lệ/không
 * có handler bị bỏ qua lặng lẽ (không chặn trang sửa). */
export function parseGiftDataArrays(raw: number[][]): GiftEntry[] {
  const out: GiftEntry[] = [];
  for (const arr of raw) {
    if (!Array.isArray(arr) || arr.length === 0) continue;
    const candidate = arrayToCandidate(arr);
    if (candidate === null) continue;
    const parsed = giftEntrySchema.safeParse(candidate);
    if (parsed.success) out.push(parsed.data);
  }
  return out;
}
