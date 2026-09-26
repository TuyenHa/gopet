import { describe, expect, it } from "vitest";
import { getTableConfig } from "@/lib/templates/table-registry";
import { editableColumnNames, formDataToRecord, zodSchemaFromColumns } from "@/lib/templates/zod-from-columns";

function formData(obj: Record<string, string>): FormData {
  const f = new FormData();
  for (const [k, v] of Object.entries(obj)) f.set(k, v);
  return f;
}

describe("zodSchemaFromColumns — PK theo mode", () => {
  it("create bảng PK không AUTO_INCREMENT (item) → itemId là trường bắt buộc", () => {
    const config = getTableConfig("item")!;
    expect(editableColumnNames(config, "create")).toContain("itemId");
  });
  it("update bảng PK không AUTO_INCREMENT (item) → itemId bị loại (không sửa được PK)", () => {
    const config = getTableConfig("item")!;
    expect(editableColumnNames(config, "update")).not.toContain("itemId");
  });
  it("create bảng PK AUTO_INCREMENT (shop) → id bị loại, DB tự sinh", () => {
    const config = getTableConfig("shop")!;
    expect(editableColumnNames(config, "create")).not.toContain("id");
  });
  it("PK ghép (gopet_map_moblvl) → create cần đủ 4 cột, update loại cả 4", () => {
    const config = getTableConfig("gopet_map_moblvl")!;
    expect(editableColumnNames(config, "create")).toEqual(["mapID", "petId", "lvlFrom", "lvlTo"]);
    expect(editableColumnNames(config, "update")).toEqual([]);
  });
});

describe("zodSchemaFromColumns — validate theo type", () => {
  it("cột json hợp lệ thì pass, hỏng cú pháp thì fail", () => {
    const config = getTableConfig("shop")!;
    const schema = zodSchemaFromColumns(config, "update");
    const base = { ShopId: "6", inventoryType: "1", itemTemTempleId: "", petId: "", count: "1", isSellItem: "1", moneyType: "[0,1]", price: "[1000,10000]", clanLvl: "0", perCount: "0" };
    expect(schema.safeParse(base).success).toBe(true);
    expect(schema.safeParse({ ...base, moneyType: "[0,1" }).success).toBe(false);
  });
  it("cột nullable rỗng → NULL (không lỗi bắt buộc)", () => {
    const config = getTableConfig("shop")!;
    const schema = zodSchemaFromColumns(config, "update");
    const base = { ShopId: "6", inventoryType: "1", itemTemTempleId: "", petId: "", count: "1", isSellItem: "1", moneyType: "[0,1]", price: "[1000,10000]", clanLvl: "0", perCount: "0" };
    const parsed = schema.parse(base) as Record<string, unknown>;
    expect(parsed.itemTemTempleId).toBeNull();
    expect(parsed.petId).toBeNull();
  });
  it("cột bắt buộc (NOT NULL, không nullable) rỗng → lỗi", () => {
    const config = getTableConfig("item")!;
    const schema = zodSchemaFromColumns(config, "create");
    const withoutName = Object.fromEntries(editableColumnNames(config, "create").map((n) => [n, "1"]));
    withoutName.name = "";
    expect(schema.safeParse(withoutName).success).toBe(false);
  });
  it("cột int/bool coerce từ chuỗi form", () => {
    const config = getTableConfig("item")!;
    const schema = zodSchemaFromColumns(config, "create");
    const data = Object.fromEntries(editableColumnNames(config, "create").map((n) => [n, "0"]));
    data.itemId = "5159";
    data.name = "Kiếm gỗ";
    data.description = "Mô tả";
    const parsed = schema.parse(data) as Record<string, unknown>;
    expect(parsed.itemId).toBe(5159);
    expect(parsed.isStackable).toBe(0);
  });
  it("cột bigint chỉ nhận chuỗi số nguyên", () => {
    const config = getTableConfig("item")!;
    const schema = zodSchemaFromColumns(config, "update");
    const data = Object.fromEntries(editableColumnNames(config, "update").map((n) => [n, "0"]));
    data.name = "x";
    data.description = "y";
    data.expire = "abc";
    expect(schema.safeParse(data).success).toBe(false);
    data.expire = "1234567890123";
    expect(schema.safeParse(data).success).toBe(true);
  });
});

describe("formDataToRecord", () => {
  it("cột bool thiếu key trong FormData (checkbox bỏ chọn) → mặc định '0'", () => {
    const config = getTableConfig("item")!;
    const form = formData({ itemId: "1", name: "a", description: "b" });
    const rec = formDataToRecord(form, config, "create");
    expect(rec.isStackable).toBe("0");
    expect(rec.canTrade).toBe("0");
  });
  it("chỉ đọc đúng tập cột editable của mode (không lẫn PK khi update)", () => {
    const config = getTableConfig("item")!;
    const form = formData({ itemId: "999", name: "a", description: "b" });
    const rec = formDataToRecord(form, config, "update");
    expect(rec.itemId).toBeUndefined();
  });
});
