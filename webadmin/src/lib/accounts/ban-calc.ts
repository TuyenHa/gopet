/** Luật ban tài khoản — hàm thuần, dùng chung cho action + test. Khớp `isAccountBlocked` (GServer). */
export type BanMode = "temporary" | "permanent" | "unban";

export interface BanUpdate {
  isBaned: number; // 0 | 1 | 2
  banTime: string; // bigint epoch ms dạng chuỗi (0 khi không dùng)
}

/** ~10 năm — chặn giá trị khổng lồ tràn thành `String(1e+300)` (lỗi strict-mode chung chung
 * khi ghi cột bigint) thay vì thông báo rõ ràng cho admin (Low finding). */
export const MAX_BAN_HOURS = 87_600;

/**
 * @param durationHours bắt buộc trong khoảng (0, {@link MAX_BAN_HOURS}] khi mode = "temporary", bỏ qua ở mode khác.
 */
export function computeBanUpdate(mode: BanMode, durationHours: number | undefined, nowMs = Date.now()): BanUpdate {
  if (mode === "unban") return { isBaned: 0, banTime: "0" };
  if (mode === "permanent") return { isBaned: 2, banTime: "0" };
  if (!durationHours || !Number.isFinite(durationHours) || durationHours <= 0 || durationHours > MAX_BAN_HOURS) {
    throw new Error(`Thời hạn ban phải trong khoảng (0, ${MAX_BAN_HOURS}] giờ.`);
  }
  return { isBaned: 1, banTime: String(nowMs + Math.round(durationHours * 3_600_000)) };
}
