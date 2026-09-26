import { describe, expect, it } from "vitest";
import { parseLossless, stringifyLossless, toNumber, RawNumber } from "@/lib/game-json/lossless-json";

describe("lossless-json — round-trip số giữ nguyên định dạng gốc", () => {
  it("giữ nguyên '5.0' (không đổi thành '5')", () => {
    const node = parseLossless('{"a":5.0}');
    expect(stringifyLossless(node)).toBe('{"a":5.0}');
  });

  it("mảng/object lồng nhau", () => {
    const json = '{"pets":[{"petId":1,"exp":9007199254},{"petId":2}]}';
    expect(stringifyLossless(parseLossless(json))).toBe(json);
  });

  it("toNumber đọc RawNumber", () => {
    const node = parseLossless("42") as RawNumber;
    expect(toNumber(node)).toBe(42);
  });
});

describe("lossless-json parser — key __proto__ không gây ô nhiễm prototype (Low finding)", () => {
  it("__proto__ được lưu như 1 thuộc tính dữ liệu bình thường, không đổi prototype của object", () => {
    const node = parseLossless('{"__proto__":{"polluted":true},"safe":1}') as Record<string, unknown>;

    // Không bị leo thang lên Object.prototype — object thường mới tạo không có field lạ.
    expect(({} as Record<string, unknown>).polluted).toBeUndefined();

    // Key vẫn đọc lại được như 1 field dữ liệu bình thường (không bị "nuốt mất").
    expect(Object.keys(node)).toContain("__proto__");
    expect(Object.keys(node)).toContain("safe");
    expect((node["__proto__"] as { polluted: boolean }).polluted).toBe(true);
  });

  it("round-trip qua stringifyLossless vẫn giữ được key __proto__", () => {
    const json = '{"__proto__":{"a":1},"b":2}';
    expect(stringifyLossless(parseLossless(json))).toBe(json);
  });
});
