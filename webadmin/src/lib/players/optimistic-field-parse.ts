/**
 * Parse/validate giá trị cho `OptimisticFieldForm` (player-actions.ts:updatePlayerFieldAction).
 * Tách riêng (không "use server") để unit-test được — file action chỉ được export async
 * function (ràng buộc Next.js "use server").
 */
import { INT32_MAX } from "@/lib/accounts/int32-range";

export const OPTIMISTIC_FIELD_NAMES = ["star", "pkPoint", "EventPoint", "AccumulatedPoint", "avatarPath", "gender"] as const;
export type OptimisticField = (typeof OPTIMISTIC_FIELD_NAMES)[number];

/** -1 = chưa đặt (giá trị mặc định cột `gender` khi tạo nhân vật), 0/1 = 2 giới tính GServer đọc (GameController.cs:4944). */
const VALID_GENDER_VALUES = new Set([-1, 0, 1]);
const STRICT_INT_RE = /^-?\d+$/;
const AVATAR_PATH_MAX_LEN = 255;

/**
 * Chuyển chuỗi form thành giá trị ghi DB. Ném lỗi (message tiếng Việt, an toàn hiện cho
 * admin) khi không hợp lệ — KHÔNG dùng `Number.parseInt` (chấp nhận rác đuôi kiểu "12abc").
 */
export function parseOptimisticFieldValue(field: OptimisticField, raw: string): string | number {
  if (field === "avatarPath") {
    if (raw.length > AVATAR_PATH_MAX_LEN) throw new Error(`Đường dẫn avatar quá dài (tối đa ${AVATAR_PATH_MAX_LEN} ký tự).`);
    return raw;
  }
  if (!STRICT_INT_RE.test(raw)) throw new Error("Giá trị số không hợp lệ.");
  const n = Number(raw);
  if (!Number.isSafeInteger(n) || Math.abs(n) > INT32_MAX) throw new Error("Giá trị số không hợp lệ.");
  if (field === "gender" && !VALID_GENDER_VALUES.has(n)) {
    throw new Error("Giới tính chỉ nhận -1 (chưa đặt), 0 hoặc 1.");
  }
  return n;
}
