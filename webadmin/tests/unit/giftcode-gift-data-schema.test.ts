import { describe, expect, it } from "vitest";
import { INT32_MAX } from "@/lib/accounts/int32-range";
import {
  GIFT_TYPE,
  giftDataSchema,
  giftDataToArrays,
  giftDataToJson,
  giftEntrySchema,
  parseGiftDataArrays,
} from "@/lib/giftcodes/gift-data-schema";

describe("giftEntrySchema — mỗi loại quà có handler → mảng đúng vị trí server đọc", () => {
  it("GOLD/COIN/EXP/ENERGY/EVENT_POINT/FUND_CLAN: [type, amount]", () => {
    expect(giftDataToArrays([giftEntrySchema.parse({ type: GIFT_TYPE.GOLD, amount: 100 })])).toEqual([[0, 100]]);
    expect(giftDataToArrays([giftEntrySchema.parse({ type: GIFT_TYPE.COIN, amount: 50 })])).toEqual([[1, 50]]);
    expect(giftDataToArrays([giftEntrySchema.parse({ type: GIFT_TYPE.EXP, amount: 200 })])).toEqual([[7, 200]]);
    expect(giftDataToArrays([giftEntrySchema.parse({ type: GIFT_TYPE.ENERGY, amount: 3 })])).toEqual([[8, 3]]);
    expect(giftDataToArrays([giftEntrySchema.parse({ type: GIFT_TYPE.EVENT_POINT, amount: 5 })])).toEqual([[11, 5]]);
    expect(giftDataToArrays([giftEntrySchema.parse({ type: GIFT_TYPE.FUND_CLAN, amount: 1000 })])).toEqual([[12, 1000]]);
  });

  it("ITEM: [2, itemId, count, canTrade] — canTrade mặc định true (=1)", () => {
    const e = giftEntrySchema.parse({ type: GIFT_TYPE.ITEM, itemId: 5, count: 2 });
    expect(giftDataToArrays([e])).toEqual([[2, 5, 2, 1]]);
    const e2 = giftEntrySchema.parse({ type: GIFT_TYPE.ITEM, itemId: 5, count: 2, canTrade: false });
    expect(giftDataToArrays([e2])).toEqual([[2, 5, 2, 0]]);
  });

  it("ITEM_PERCENT_NO_DROP_MORE: [4, itemId, percent, count]", () => {
    const e = giftEntrySchema.parse({ type: GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE, itemId: 10, percent: 50, count: 1 });
    expect(giftDataToArrays([e])).toEqual([[4, 10, 50, 1]]);
  });

  it("RANDOM_ITEM: [9, numGift, count1, itemId1, count2, itemId2, ...] — itemId có thể âm", () => {
    const e = giftEntrySchema.parse({
      type: GIFT_TYPE.RANDOM_ITEM,
      numGift: 2,
      items: [
        { count: 1, itemId: 100 },
        { count: 2, itemId: -123 },
      ],
    });
    expect(giftDataToArrays([e])).toEqual([[9, 2, 1, 100, 2, -123]]);
  });

  it("ITEM_MAX_OPTION: [10, itemId, count]", () => {
    const e = giftEntrySchema.parse({ type: GIFT_TYPE.ITEM_MAX_OPTION, itemId: 7, count: 1 });
    expect(giftDataToArrays([e])).toEqual([[10, 7, 1]]);
  });

  it("TITLE vĩnh viễn: [13, titleId, 1] — không kèm thời hạn", () => {
    const e = giftEntrySchema.parse({ type: GIFT_TYPE.TITLE, titleId: 9, isInfinite: true });
    expect(giftDataToArrays([e])).toEqual([[13, 9, 1]]);
  });

  it("TITLE có hạn: [13, titleId, 0, min, giờ, ngày, tháng, năm]", () => {
    const e = giftEntrySchema.parse({
      type: GIFT_TYPE.TITLE,
      titleId: 9,
      isInfinite: false,
      duration: { minutes: 0, hours: 0, days: 7, months: 0, years: 0 },
    });
    expect(giftDataToArrays([e])).toEqual([[13, 9, 0, 0, 0, 7, 0, 0]]);
  });

  it("SKIN có hạn: [14, itemId, 0, min, giờ, ngày] — khớp mẫu thật trong GopetManager.cs:825", () => {
    const e = giftEntrySchema.parse({
      type: GIFT_TYPE.SKIN,
      itemId: 1000012,
      isInfinite: false,
      duration: { minutes: 0, hours: 0, days: 7, months: 0, years: 0 },
    });
    expect(giftDataToArrays([e])).toEqual([[14, 1000012, 0, 0, 0, 7]]);
  });

  it("PET_TRIAL vĩnh viễn: [15, petId, 1]", () => {
    const e = giftEntrySchema.parse({ type: GIFT_TYPE.PET_TRIAL, petId: 3, isInfinite: true });
    expect(giftDataToArrays([e])).toEqual([[15, 3, 1]]);
  });
});

describe("giftEntrySchema — từ chối input hỏng", () => {
  it("loại không có handler (3/5/6) bị từ chối — không phải literal hợp lệ nào trong union", () => {
    expect(giftEntrySchema.safeParse({ type: 3, itemId: 1, percent: 50 }).success).toBe(false);
    expect(giftEntrySchema.safeParse({ type: 5 }).success).toBe(false);
    expect(giftEntrySchema.safeParse({ type: 6 }).success).toBe(false);
  });

  it("ITEM itemId/count phải dương", () => {
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.ITEM, itemId: 0, count: 1 }).success).toBe(false);
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.ITEM, itemId: 1, count: 0 }).success).toBe(false);
  });

  it("ITEM_PERCENT_NO_DROP_MORE percent ngoài 0-100 bị từ chối", () => {
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE, itemId: 1, percent: 101, count: 1 }).success).toBe(
      false,
    );
  });

  it("RANDOM_ITEM cần ít nhất 1 dòng", () => {
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.RANDOM_ITEM, numGift: 1, items: [] }).success).toBe(false);
  });

  it("H6/M1: mọi số nguyên bị chặn trong int32 (server đọc int[][]) — không chỉ dương", () => {
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.GOLD, amount: INT32_MAX + 1 }).success).toBe(false);
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.GOLD, amount: INT32_MAX }).success).toBe(true);
  });

  it("M1: percent phải là số nguyên (12.5 bị từ chối, không chỉ giới hạn 0-100)", () => {
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE, itemId: 1, percent: 12.5, count: 1 }).success).toBe(
      false,
    );
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE, itemId: 1, percent: 12, count: 1 }).success).toBe(true);
  });

  it("M1: RANDOM_ITEM.items bị cap số dòng (chống JSON tràn varchar(15000))", () => {
    const tooMany = Array.from({ length: 301 }, (_, i) => ({ count: 1, itemId: i + 1 }));
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.RANDOM_ITEM, numGift: 1, items: tooMany }).success).toBe(false);
    const ok = Array.from({ length: 300 }, (_, i) => ({ count: 1, itemId: i + 1 }));
    expect(giftEntrySchema.safeParse({ type: GIFT_TYPE.RANDOM_ITEM, numGift: 1, items: ok }).success).toBe(true);
  });
});

describe("giftDataToJson — chặn tổng dung lượng vượt cột gift_data varchar(15000) (M1)", () => {
  it("ném lỗi khi JSON kết quả vượt 15000 ký tự (nhiều mục RANDOM_ITEM cộng dồn)", () => {
    const bigRandomItemEntry = {
      type: GIFT_TYPE.RANDOM_ITEM,
      numGift: 1,
      items: Array.from({ length: 300 }, (_, i) => ({ count: 999999999, itemId: 999999999 - i })),
    };
    // Mỗi mục ~6.6k ký tự khi mã hoá — 3 mục (dưới giới hạn 50 mục/giftcode) đã đủ vượt 15000.
    const entries = giftDataSchema.parse([bigRandomItemEntry, bigRandomItemEntry, bigRandomItemEntry]);
    expect(() => giftDataToJson(entries)).toThrow();
  });

  it("không ném lỗi với danh sách quà bình thường", () => {
    const entries = giftDataSchema.parse([{ type: GIFT_TYPE.GOLD, amount: 100 }]);
    expect(giftDataToJson(entries)).toBe("[[0,100]]");
  });
});

describe("giftDataSchema — ràng buộc chéo cấp danh sách", () => {
  it("cần ít nhất 1 mục quà", () => {
    expect(giftDataSchema.safeParse([]).success).toBe(false);
  });

  it("TITLE/SKIN/PET_TRIAL không vĩnh viễn nhưng thời hạn toàn 0 → từ chối", () => {
    const res = giftDataSchema.safeParse([
      { type: GIFT_TYPE.TITLE, titleId: 1, isInfinite: false, duration: { minutes: 0, hours: 0, days: 0, months: 0, years: 0 } },
    ]);
    expect(res.success).toBe(false);
  });

  it("TITLE không vĩnh viễn có thời hạn > 0 → hợp lệ", () => {
    const res = giftDataSchema.safeParse([
      { type: GIFT_TYPE.TITLE, titleId: 1, isInfinite: false, duration: { minutes: 0, hours: 1, days: 0, months: 0, years: 0 } },
    ]);
    expect(res.success).toBe(true);
  });
});

describe("parseGiftDataArrays — dựng lại từ int[][] để hiển thị builder khi sửa code", () => {
  it("round-trip qua giftDataToArrays cho các loại tiêu biểu", () => {
    const entries = giftDataSchema.parse([
      { type: GIFT_TYPE.GOLD, amount: 100 },
      { type: GIFT_TYPE.ITEM, itemId: 5, count: 2, canTrade: false },
      { type: GIFT_TYPE.RANDOM_ITEM, numGift: 1, items: [{ count: 1, itemId: -124 }] },
      { type: GIFT_TYPE.SKIN, itemId: 1000012, isInfinite: false, duration: { minutes: 0, hours: 0, days: 7, months: 0, years: 0 } },
    ]);
    const roundTripped = parseGiftDataArrays(giftDataToArrays(entries));
    expect(roundTripped).toEqual(entries);
  });

  it("mảng của loại không có handler (vd cũ còn sót) bị bỏ qua lặng lẽ", () => {
    expect(parseGiftDataArrays([[3, 1, 50]])).toEqual([]);
  });
});
