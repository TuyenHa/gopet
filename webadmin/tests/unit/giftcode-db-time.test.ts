import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

/**
 * `DB_TIMEZONE_OFFSET_MIN` được đọc 1 lần lúc module load (`db-time.ts`) — mỗi test phải
 * `vi.resetModules()` rồi `import()` động sau khi đổi `process.env` để lấy giá trị mới.
 */
describe("db-time", () => {
  const ORIGINAL = process.env.DB_TIMEZONE_OFFSET_MIN;

  beforeEach(() => {
    vi.resetModules();
  });
  afterEach(() => {
    if (ORIGINAL === undefined) delete process.env.DB_TIMEZONE_OFFSET_MIN;
    else process.env.DB_TIMEZONE_OFFSET_MIN = ORIGINAL;
  });

  it("mặc định 420 (DB đã đổi sang giờ VN sau phase 11) → giữ nguyên giờ, không quy đổi", async () => {
    delete process.env.DB_TIMEZONE_OFFSET_MIN;
    const { vnInputToDbDateTime, dbDateTimeToVnInputValue, DB_TIMEZONE_OFFSET_MIN } = await import("@/lib/time/db-time");
    expect(DB_TIMEZONE_OFFSET_MIN).toBe(420);
    expect(vnInputToDbDateTime("2026-10-01T20:00")).toBe("2026-10-01 20:00:00");
    expect(dbDateTimeToVnInputValue("2026-10-01 20:00:00")).toBe("2026-10-01T20:00");
  });

  it("DB_TIMEZONE_OFFSET_MIN=0 (DB hiện đang UTC) → trừ 7 giờ khi ghi, cộng lại khi đọc", async () => {
    process.env.DB_TIMEZONE_OFFSET_MIN = "0";
    const { vnInputToDbDateTime, dbDateTimeToVnInputValue } = await import("@/lib/time/db-time");
    expect(vnInputToDbDateTime("2026-10-01T20:00")).toBe("2026-10-01 13:00:00");
    expect(dbDateTimeToVnInputValue("2026-10-01 13:00:00")).toBe("2026-10-01T20:00");
  });

  it("quy đổi đúng qua nửa đêm (VN 02:00 → DB UTC hôm trước 19:00)", async () => {
    process.env.DB_TIMEZONE_OFFSET_MIN = "0";
    const { vnInputToDbDateTime } = await import("@/lib/time/db-time");
    expect(vnInputToDbDateTime("2026-10-02T02:00")).toBe("2026-10-01 19:00:00");
  });
});
