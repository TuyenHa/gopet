import { describe, expect, it } from "vitest";
import { computeBanUpdate, MAX_BAN_HOURS } from "@/lib/accounts/ban-calc";

describe("computeBanUpdate", () => {
  const now = 1_800_000_000_000;

  it("unban → isBaned=0, banTime=0", () => {
    expect(computeBanUpdate("unban", undefined, now)).toEqual({ isBaned: 0, banTime: "0" });
  });

  it("permanent → isBaned=2, banTime=0 (không dùng banTime)", () => {
    expect(computeBanUpdate("permanent", 999, now)).toEqual({ isBaned: 2, banTime: "0" });
  });

  it("temporary → isBaned=1, banTime = now + giờ*3600000", () => {
    expect(computeBanUpdate("temporary", 24, now)).toEqual({ isBaned: 1, banTime: String(now + 24 * 3_600_000) });
  });

  it("temporary thiếu/âm/0 giờ → ném lỗi", () => {
    expect(() => computeBanUpdate("temporary", undefined, now)).toThrow();
    expect(() => computeBanUpdate("temporary", 0, now)).toThrow();
    expect(() => computeBanUpdate("temporary", -5, now)).toThrow();
  });

  it("temporary vượt MAX_BAN_HOURS → ném lỗi (Low: chặn tràn bigint banTime)", () => {
    expect(() => computeBanUpdate("temporary", MAX_BAN_HOURS + 1, now)).toThrow();
    expect(() => computeBanUpdate("temporary", 1e300, now)).toThrow();
  });

  it("temporary đúng biên MAX_BAN_HOURS → hợp lệ", () => {
    expect(computeBanUpdate("temporary", MAX_BAN_HOURS, now)).toEqual({
      isBaned: 1,
      banTime: String(now + MAX_BAN_HOURS * 3_600_000),
    });
  });
});
