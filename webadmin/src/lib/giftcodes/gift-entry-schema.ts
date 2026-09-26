import { z } from "zod";
import { GIFT_TYPE } from "./gift-type-labels";
import { INT32_MAX } from "@/lib/accounts/int32-range";

/** Server đọc `gift_data` thành `int[][]` (GiftCodeData.cs:15) — MỌI số phải nằm trong int32,
 * không chỉ dương, nếu không Newtonsoft/JSON ghi được nhưng server đọc sai/tràn số (H6/M1). */
const posInt = z.coerce.number().int().positive().max(INT32_MAX);
const nonNegInt = z.coerce.number().int().min(0).max(INT32_MAX);
/** Cap số dòng ngẫu nhiên để `gift_data` (varchar(15000)) không bị 1 mục RANDOM_ITEM chiếm hết
 * chỗ — mỗi dòng ~2 số int32 (tối đa ~22 ký tự kể cả dấu phẩy) → 300 dòng ~ 6.6k ký tự, vẫn
 * còn chỗ cho ≤49 mục quà khác trong cùng giftcode (giftDataSchema giới hạn 50 mục/giftcode). */
const MAX_RANDOM_ITEMS = 300;

/** min/giờ/ngày/tháng/năm — dùng cho danh hiệu & pet thử nghiệm (GameController.cs:4331-4336, 4376-4381). */
const fullDurationSchema = z.object({
  minutes: nonNegInt.default(0),
  hours: nonNegInt.default(0),
  days: nonNegInt.default(0),
  months: nonNegInt.default(0),
  years: nonNegInt.default(0),
});

const goldEntry = z.object({ type: z.literal(GIFT_TYPE.GOLD), amount: posInt });
const coinEntry = z.object({ type: z.literal(GIFT_TYPE.COIN), amount: posInt });
const itemEntry = z.object({
  type: z.literal(GIFT_TYPE.ITEM),
  itemId: posInt,
  count: posInt,
  canTrade: z.boolean().default(true),
});
const itemPercentEntry = z.object({
  type: z.literal(GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE),
  itemId: posInt,
  // int[][] — percent thập phân (vd 12.5) sẽ bị Newtonsoft đọc sai kiểu int (M1).
  percent: z.coerce.number().int().min(0).max(100),
  count: posInt,
});
const expEntry = z.object({ type: z.literal(GIFT_TYPE.EXP), amount: posInt });
const energyEntry = z.object({ type: z.literal(GIFT_TYPE.ENERGY), amount: posInt });
const randomItemEntry = z.object({
  type: z.literal(GIFT_TYPE.RANDOM_ITEM),
  numGift: posInt,
  // itemId có thể âm (mã nhóm đặc biệt, vd -123 = bạc ngẫu nhiên) nên không dùng posInt, vẫn
  // chặn trong khoảng int32 (2 chiều) để không tràn khi server đọc int[][].
  items: z
    .array(z.object({ count: posInt, itemId: z.coerce.number().int().min(-INT32_MAX).max(INT32_MAX) }))
    .min(1, "Cần ít nhất 1 vật phẩm trong nhóm ngẫu nhiên")
    .max(MAX_RANDOM_ITEMS, `Tối đa ${MAX_RANDOM_ITEMS} dòng ngẫu nhiên mỗi mục (giới hạn dung lượng cột gift_data)`),
});
const itemMaxOptionEntry = z.object({
  type: z.literal(GIFT_TYPE.ITEM_MAX_OPTION),
  itemId: posInt,
  count: posInt,
});
const eventPointEntry = z.object({ type: z.literal(GIFT_TYPE.EVENT_POINT), amount: posInt });
const fundClanEntry = z.object({ type: z.literal(GIFT_TYPE.FUND_CLAN), amount: posInt });
const titleEntry = z.object({
  type: z.literal(GIFT_TYPE.TITLE),
  titleId: posInt,
  isInfinite: z.boolean(),
  duration: fullDurationSchema.default({ minutes: 0, hours: 0, days: 0, months: 0, years: 0 }),
});
// Server SKIN chỉ đọc min/giờ/ngày (GameController.cs:4361-4364), bỏ qua tháng/năm — nhưng
// dùng chung `fullDurationSchema` với TITLE/PET_TRIAL để giữ 1 shape Duration duy nhất cho
// UI (giftEntryToArray tự bỏ tháng/năm khi build mảng cho SKIN).
const skinEntry = z.object({
  type: z.literal(GIFT_TYPE.SKIN),
  itemId: posInt,
  isInfinite: z.boolean(),
  duration: fullDurationSchema.default({ minutes: 0, hours: 0, days: 0, months: 0, years: 0 }),
});
const petTrialEntry = z.object({
  type: z.literal(GIFT_TYPE.PET_TRIAL),
  petId: posInt,
  isInfinite: z.boolean(),
  duration: fullDurationSchema.default({ minutes: 0, hours: 0, days: 0, months: 0, years: 0 }),
});

/** 1 mục quà — union theo `type`. Mỗi nhánh là ZodObject thuần (không `.refine`) để giữ được
 * tối ưu discriminatedUnion; ràng buộc chéo (thời hạn > 0 khi không vĩnh viễn) kiểm ở
 * {@link giftDataSchema}. */
export const giftEntrySchema = z.discriminatedUnion("type", [
  goldEntry,
  coinEntry,
  itemEntry,
  itemPercentEntry,
  expEntry,
  energyEntry,
  randomItemEntry,
  itemMaxOptionEntry,
  eventPointEntry,
  fundClanEntry,
  titleEntry,
  skinEntry,
  petTrialEntry,
]);
export type GiftEntry = z.infer<typeof giftEntrySchema>;

function hasPositiveDuration(d: { minutes: number; hours: number; days: number; months?: number; years?: number }): boolean {
  return d.minutes > 0 || d.hours > 0 || d.days > 0 || (d.months ?? 0) > 0 || (d.years ?? 0) > 0;
}

type TimedEntry = Extract<GiftEntry, { isInfinite: boolean }>;

function isTimedEntry(e: GiftEntry): e is TimedEntry {
  return e.type === GIFT_TYPE.TITLE || e.type === GIFT_TYPE.SKIN || e.type === GIFT_TYPE.PET_TRIAL;
}

/** Danh sách quà của 1 giftcode. `gift_code.gift_data` DB lưu `int[][]` — dùng
 * `giftDataToArrays`/`giftDataToJson` (gift-data-serialize.ts) để chuyển sang dạng đó. */
export const giftDataSchema = z
  .array(giftEntrySchema)
  .min(1, "Cần ít nhất 1 mục quà")
  .max(50, "Tối đa 50 mục quà mỗi giftcode")
  .superRefine((entries, ctx) => {
    entries.forEach((e, i) => {
      if (isTimedEntry(e) && !e.isInfinite && !hasPositiveDuration(e.duration)) {
        ctx.addIssue({
          code: "custom",
          message: "Cần nhập thời hạn > 0 khi không chọn vĩnh viễn",
          path: [i, "duration"],
        });
      }
    });
  });
