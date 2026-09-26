import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";
import { kioskTypeLabel, parseKioskData } from "@/lib/market/kiosk-parser";

// Trích từ 1 dòng thật của bảng `gopettae_tae2`.`market` (cột Data), rút gọn các mảng lớn
// không dùng tới (skill/tiemnang...) — cấu trúc và giá trị còn lại giữ nguyên.
const fixturePath = path.resolve(import.meta.dirname, "../fixtures/market-sample.json");
const sample = readFileSync(fixturePath, "utf-8");

describe("parseKioskData — fixture thật từ DB", () => {
  it("bỏ qua 5 ki ốt rỗng, chỉ lấy listing pet đang bán", () => {
    const listings = parseKioskData(sample);
    expect(listings).toHaveLength(1);
    expect(listings[0]).toMatchObject({
      kioskType: 4,
      sellerName: "kzhd9x",
      sellerUserId: 1458,
      itemTemplateId: null,
      petName: "Rua Test",
      count: 1,
      remainingPrice: 59999,
    });
  });

  it("kioskTypeLabel trả về nhãn tiếng Việt đúng loại", () => {
    expect(kioskTypeLabel(4)).toBe("Thú cưng");
    expect(kioskTypeLabel(0)).toBe("Nón");
    expect(kioskTypeLabel(99)).toBe("Loại 99");
  });
});

describe("parseKioskData — item thường + trường hợp biên", () => {
  it("parse listing ItemSell, lấy itemTemplateId/count từ ItemSell", () => {
    const data = JSON.stringify([
      { kioskType: 1, kioskItems: [] },
      {
        kioskType: 5,
        kioskItems: [
          {
            ItemSell: { itemTemplateId: 42, count: 3 },
            pet: null,
            price: 1000,
            sumVal: 200,
            hasSell: false,
            hasRemoved: false,
            itemId: 111,
            user_id: 7,
            SellerName: "seller7",
            TotalCount: 3,
          },
        ],
      },
    ]);
    const listings = parseKioskData(data);
    expect(listings).toHaveLength(1);
    expect(listings[0]).toMatchObject({
      kioskType: 5,
      itemTemplateId: 42,
      count: 3,
      // RemainingPrice không có trong payload → fallback price - sumVal.
      remainingPrice: 800,
    });
  });

  it("lọc bỏ listing đã hasRemoved=true (đã bán/hủy tại thời điểm lưu)", () => {
    const data = JSON.stringify([
      {
        kioskType: 0,
        kioskItems: [
          {
            ItemSell: { itemTemplateId: 1, count: 1 },
            pet: null,
            price: 10,
            sumVal: 0,
            hasSell: true,
            hasRemoved: true,
            itemId: 5,
            user_id: 1,
            SellerName: "x",
            TotalCount: 1,
          },
        ],
      },
    ]);
    expect(parseKioskData(data)).toHaveLength(0);
  });

  it("SellerName rỗng/null → fallback '???'", () => {
    const data = JSON.stringify([
      {
        kioskType: 0,
        kioskItems: [
          {
            ItemSell: { itemTemplateId: 1, count: 1 },
            pet: null,
            price: 10,
            sumVal: 0,
            hasSell: false,
            hasRemoved: false,
            itemId: 5,
            user_id: 1,
            SellerName: null,
            TotalCount: 1,
          },
        ],
      },
    ]);
    expect(parseKioskData(data)[0].sellerName).toBe("???");
  });

  it("JSON không đúng khuôn (khuyết field bắt buộc) → ném lỗi để caller hiện raw JSON", () => {
    expect(() => parseKioskData(JSON.stringify([{ kioskType: 0 }]))).toThrow();
  });

  it("chuỗi không phải JSON → ném lỗi", () => {
    expect(() => parseKioskData("not-json")).toThrow();
  });
});
