import { describe, expect, it } from "vitest";
import { INT32_MAX } from "@/lib/accounts/int32-range";
import { parseOptimisticFieldValue } from "@/lib/players/optimistic-field-parse";

describe("parseOptimisticFieldValue (Low: parseInt leniency + gender enum)", () => {
  it("field số hợp lệ → trả về number", () => {
    expect(parseOptimisticFieldValue("star", "5")).toBe(5);
    expect(parseOptimisticFieldValue("pkPoint", "-3")).toBe(-3);
  });

  it("avatarPath → giữ nguyên chuỗi, chặn quá dài", () => {
    expect(parseOptimisticFieldValue("avatarPath", "anim/1.png")).toBe("anim/1.png");
    expect(() => parseOptimisticFieldValue("avatarPath", "a".repeat(300))).toThrow();
  });

  it("từ chối chuỗi số có rác đuôi (Number.parseInt('12abc') cũ chấp nhận 12)", () => {
    expect(() => parseOptimisticFieldValue("star", "12abc")).toThrow();
    expect(() => parseOptimisticFieldValue("star", "  5")).toThrow();
    expect(() => parseOptimisticFieldValue("star", "5.5")).toThrow();
    expect(() => parseOptimisticFieldValue("star", "")).toThrow();
  });

  it("chặn vượt int32", () => {
    expect(() => parseOptimisticFieldValue("EventPoint", String(INT32_MAX + 1))).toThrow();
    expect(parseOptimisticFieldValue("EventPoint", String(INT32_MAX))).toBe(INT32_MAX);
  });

  it("gender chỉ nhận -1/0/1", () => {
    expect(parseOptimisticFieldValue("gender", "-1")).toBe(-1);
    expect(parseOptimisticFieldValue("gender", "0")).toBe(0);
    expect(parseOptimisticFieldValue("gender", "1")).toBe(1);
    expect(() => parseOptimisticFieldValue("gender", "2")).toThrow();
    expect(() => parseOptimisticFieldValue("gender", "3")).toThrow();
  });
});
