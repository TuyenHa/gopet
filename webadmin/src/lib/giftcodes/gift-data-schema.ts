/**
 * Điểm truy cập DUY NHẤT cho schema `gift_data` — mọi nơi khác (actions, builder UI, test)
 * import từ file này, không import trực tiếp các file con. Tách file con chỉ để giữ mỗi file
 * dưới ~200 dòng (`development-rules.md`):
 * - `gift-type-labels.ts`   — hằng số loại quà + nhãn tiếng Việt (khớp GopetManager.cs:202-232)
 * - `gift-entry-schema.ts`  — zod discriminatedUnion theo loại (khớp GameController.cs:4106-4390)
 * - `gift-data-serialize.ts` — GiftEntry[] ↔ int[][] (định dạng cột `gift_code.gift_data`)
 */
export { GIFT_TYPE, GIFT_TYPE_LABELS, GIFT_TYPE_OPTIONS, type GiftTypeValue } from "./gift-type-labels";
export { giftEntrySchema, giftDataSchema, type GiftEntry } from "./gift-entry-schema";
export {
  giftEntryToArray,
  giftDataToArrays,
  giftDataToJson,
  parseGiftDataJson,
  parseGiftDataArrays,
} from "./gift-data-serialize";
