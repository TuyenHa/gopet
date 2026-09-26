import { describe, expect, it } from "vitest";
import { INT32_MAX, isValidInt32Amount, isValidInt32Delta } from "@/lib/accounts/int32-range";

describe("isValidInt32Delta", () => {
  it("chấp nhận trong khoảng ±2^31-1", () => {
    expect(isValidInt32Delta(0)).toBe(true);
    expect(isValidInt32Delta(INT32_MAX)).toBe(true);
    expect(isValidInt32Delta(-INT32_MAX)).toBe(true);
  });
  it("từ chối vượt giới hạn hoặc không nguyên", () => {
    expect(isValidInt32Delta(INT32_MAX + 1)).toBe(false);
    expect(isValidInt32Delta(-(INT32_MAX + 1))).toBe(false);
    expect(isValidInt32Delta(1.5)).toBe(false);
  });
});

describe("isValidInt32Amount", () => {
  it("chấp nhận 0..2^31-1", () => {
    expect(isValidInt32Amount(0)).toBe(true);
    expect(isValidInt32Amount(INT32_MAX)).toBe(true);
  });
  it("từ chối âm hoặc vượt giới hạn", () => {
    expect(isValidInt32Amount(-1)).toBe(false);
    expect(isValidInt32Amount(INT32_MAX + 1)).toBe(false);
  });
});
