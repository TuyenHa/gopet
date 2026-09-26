import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";
import {
  applyItemEdit,
  deleteItem,
  findItem,
  itemPetEquipId,
  parsePlayerItems,
  sortAllInvTypes,
  stringifyPlayerItems,
} from "@/lib/game-json/item-json";

// Trích thật từ `gopettae_tae2`.`player`.`items` (ID=102), rút gọn còn 2 item/invType — giữ
// nguyên định dạng số gốc (vd `gemOptionValue` kiểu float vẫn là `5.0`, không rút gọn `5`).
const fixturePath = path.resolve(import.meta.dirname, "../fixtures/player-items-102.json");
const raw = readFileSync(fixturePath, "utf-8");

describe("parsePlayerItems / stringifyPlayerItems — round-trip fixture thật", () => {
  it("không sửa gì -> stringify lại đúng y hệt văn bản gốc (giữ định dạng số, key, thứ tự)", () => {
    const tree = parsePlayerItems(raw);
    expect(stringifyPlayerItems(tree)).toBe(raw);
  });

  it("giữ nguyên gemOptionValue kiểu float (5.0) — không rút gọn thành 5", () => {
    expect(raw).toContain('"gemOptionValue":[5.0,5.0,5.0,5.0]');
    const tree = parsePlayerItems(raw);
    expect(stringifyPlayerItems(tree)).toContain('"gemOptionValue":[5.0,5.0,5.0,5.0]');
  });

  it("mỗi invType đã sắp tăng dần theo itemId (bất biến server)", () => {
    const tree = parsePlayerItems(raw);
    for (const list of Object.values(tree)) {
      const ids = list.map((it) => Number(it.itemId));
      expect(ids).toEqual([...ids].sort((a, b) => a - b));
    }
  });
});

describe("findItem / applyItemEdit — sửa field có sẵn, không đụng field khác", () => {
  it("tìm đúng item theo itemId ở đúng invType", () => {
    const tree = parsePlayerItems(raw);
    const found = findItem(tree, 516587094);
    expect(found?.invType).toBe("0");
    expect(Number(found?.item.itemTemplateId)).toBe(50);
  });

  it("sửa count/lvl/expire/canTrade/durability rồi sort lại — field khác (vd def/atk) không đổi", () => {
    const tree = parsePlayerItems(raw);
    const found = findItem(tree, 516587094)!;
    const originalDef = Number(found.item.def);
    applyItemEdit(found.item, { count: 5, lvl: 2, expire: 1234567890, canTrade: false, durability: 40 });
    sortAllInvTypes(tree);

    const after = findItem(tree, 516587094)!;
    expect(after.item.count).toBe(5);
    expect(after.item.lvl).toBe(2);
    expect(after.item.expire).toBe(1234567890);
    expect(after.item.canTrade).toBe(false);
    expect(after.item.durability).toBe(40);
    expect(Number(after.item.def)).toBe(originalDef);
  });

  it("sửa durability trên item CHƯA từng có field này -> ném lỗi (không tự thêm field mới)", () => {
    const tree = parsePlayerItems(raw);
    // itemId 1200979416 (invType 0, item thứ 2) có durability sẵn theo fixture; lấy 1 item khác
    // invType 1 (vật phẩm thường) chắc chắn không có durability.
    const found = findItem(tree, 39560328);
    expect(found).not.toBeNull();
    expect(found!.item.durability).toBeUndefined();
    expect(() => applyItemEdit(found!.item, { durability: 10 })).toThrow();
  });
});

describe("deleteItem + itemPetEquipId — xoá item và dọn tham chiếu equip", () => {
  it("xoá đúng item khỏi invType, danh sách còn lại vẫn sắp tăng dần", () => {
    const tree = parsePlayerItems(raw);
    const removed = deleteItem(tree, 516587094);
    expect(removed).not.toBeNull();
    expect(findItem(tree, 516587094)).toBeNull();
    expect(tree["0"].map((it) => Number(it.itemId))).toEqual([1200979416]);
  });

  it("xoá item không tồn tại -> trả về null, cây không đổi", () => {
    const tree = parsePlayerItems(raw);
    const before = stringifyPlayerItems(tree);
    expect(deleteItem(tree, 999999999)).toBeNull();
    expect(stringifyPlayerItems(tree)).toBe(before);
  });

  it("itemPetEquipId đọc đúng petEuipId của item đang được pet đeo (fixture thật: pet 635576412)", () => {
    const tree = parsePlayerItems(raw);
    const found = findItem(tree, 516587094)!;
    expect(itemPetEquipId(found.item)).toBe(635576412);

    const notEquipped = findItem(tree, 1200979416)!;
    expect(itemPetEquipId(notEquipped.item)).toBe(-1);
  });
});
