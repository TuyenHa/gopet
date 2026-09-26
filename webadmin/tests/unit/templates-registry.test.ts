import { describe, expect, it } from "vitest";
import { decodePk, encodePk } from "@/lib/templates/pk-codec";
import { REF_SOURCES } from "@/lib/templates/ref-sources";
import { getTableConfig, TABLE_GROUPS, type TableConfig } from "@/lib/templates/table-registry";

const ALL_TABLES: TableConfig[] = TABLE_GROUPS.flatMap((g) => g.tables);

describe("getTableConfig (allowlist bảng)", () => {
  it("bảng không có trong registry → undefined (route phải notFound())", () => {
    expect(getTableConfig("player")).toBeUndefined();
    expect(getTableConfig("clan")).toBeUndefined();
    expect(getTableConfig("exchange_gold")).toBeUndefined();
    expect(getTableConfig("option_descrtiption")).toBeUndefined();
    expect(getTableConfig("'; DROP TABLE item; --")).toBeUndefined();
  });
  it("bảng vòng 2a có trong registry đúng PK", () => {
    expect(getTableConfig("item")?.pk).toEqual(["itemId"]);
    expect(getTableConfig("shop")?.pk).toEqual(["id"]);
    expect(getTableConfig("field")?.pk).toEqual(["FieldName"]);
    expect(getTableConfig("server")?.db).toBe("web");
  });
  it("bảng PK ghép giữ đúng thứ tự cột", () => {
    expect(getTableConfig("gopet_map_moblvl")?.pk).toEqual(["mapID", "petId", "lvlFrom", "lvlTo"]);
    expect(getTableConfig("gopet_mob_location")?.pk).toEqual(["mapID", "x", "y"]);
  });
  it("bảng không PK là readOnly, không có nút sửa/xoá", () => {
    const mob = getTableConfig("gopet_mob");
    expect(mob?.pk).toEqual([]);
    expect(mob?.readOnly).toBe(true);
  });
});

describe("tính toàn vẹn registry (mọi bảng)", () => {
  it("không trùng tên bảng", () => {
    const names = ALL_TABLES.map((t) => t.table);
    expect(new Set(names).size).toBe(names.length);
  });
  it("mỗi cột PK phải nằm trong danh sách columns", () => {
    for (const t of ALL_TABLES) {
      for (const p of t.pk) {
        expect(t.columns.some((c) => c.name === p), `${t.table}.${p} thiếu trong columns`).toBe(true);
      }
    }
  });
  it("mỗi searchCol phải nằm trong danh sách columns", () => {
    for (const t of ALL_TABLES) {
      for (const s of t.searchCols) {
        expect(t.columns.some((c) => c.name === s), `${t.table}.searchCols có "${s}" không tồn tại`).toBe(true);
      }
    }
  });
  it("cột type=ref chỉ dùng RefKind hợp lệ trong REF_SOURCES", () => {
    for (const t of ALL_TABLES) {
      for (const c of t.columns) {
        if (c.type === "ref") expect(c.ref && c.ref in REF_SOURCES, `${t.table}.${c.name}`).toBeTruthy();
      }
    }
  });
  it("autoIncrement chỉ hợp lệ khi PK đơn (1 cột)", () => {
    for (const t of ALL_TABLES) {
      if (t.autoIncrement) expect(t.pk.length, `${t.table} autoIncrement nhưng PK ghép`).toBe(1);
    }
  });
  it("bảng readOnly không có noCreate/noDelete thừa gây hiểu nhầm và luôn pk rỗng hoặc có pk", () => {
    for (const t of ALL_TABLES) {
      if (t.readOnly) expect(t.pk).toEqual([]);
    }
  });
});

describe("encodePk / decodePk (PK ghép qua ?pk=)", () => {
  it("round-trip PK đơn", () => {
    const config = getTableConfig("item")!;
    const json = encodePk(config, { itemId: 5159, name: "x" });
    expect(decodePk(json, config)).toEqual({ itemId: 5159 });
  });
  it("round-trip PK ghép giữ đúng số cột", () => {
    const config = getTableConfig("gopet_mob_location")!;
    const json = encodePk(config, { mapID: 19, x: 10, y: 20 });
    expect(decodePk(json, config)).toEqual({ mapID: 19, x: 10, y: 20 });
  });
  it("từ chối JSON thiếu cột", () => {
    const config = getTableConfig("gopet_mob_location")!;
    expect(decodePk(JSON.stringify({ mapID: 19, x: 10 }), config)).toBeNull();
  });
  it("từ chối JSON thừa cột lạ", () => {
    const config = getTableConfig("item")!;
    expect(decodePk(JSON.stringify({ itemId: 1, evil: "DROP TABLE" }), config)).toBeNull();
  });
  it("từ chối giá trị không phải string/number (mảng/object/null)", () => {
    const config = getTableConfig("item")!;
    expect(decodePk(JSON.stringify({ itemId: [1] }), config)).toBeNull();
    expect(decodePk(JSON.stringify({ itemId: null }), config)).toBeNull();
  });
  it("từ chối JSON hỏng cú pháp", () => {
    const config = getTableConfig("item")!;
    expect(decodePk("{not json", config)).toBeNull();
  });
  it("bảng readOnly (pk rỗng) luôn trả null", () => {
    const config = getTableConfig("gopet_mob")!;
    expect(decodePk("{}", config)).toBeNull();
  });
});
