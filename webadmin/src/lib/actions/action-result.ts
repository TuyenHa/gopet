import { unstable_rethrow } from "next/navigation";
import { LockBusyError } from "@/lib/db/named-lock";

/** Kết quả chuẩn của mọi Server Action (dùng với useActionState / gọi trực tiếp). */
export type ActionResult<T = undefined> =
  | { ok: true; message?: string; data?: T }
  | { ok: false; error: string };

/** Lỗi có thông điệp an toàn để hiện cho admin. Lỗi khác chỉ hiện câu chung. */
export class UserFacingError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "UserFacingError";
  }
}

/**
 * Bọc thân Server Action: đổi lỗi thành ActionResult, cho redirect/notFound đi qua
 * (unstable_rethrow), không lộ chi tiết lỗi DB ra client.
 */
export async function runAction<T>(fn: () => Promise<ActionResult<T>>): Promise<ActionResult<T>> {
  try {
    return await fn();
  } catch (err) {
    unstable_rethrow(err);
    if (err instanceof UserFacingError || err instanceof LockBusyError) return { ok: false, error: err.message };
    if (isZodError(err)) return { ok: false, error: err.issues.map((i) => i.message).join("; ") };
    console.error("[action] lỗi:", err);
    return { ok: false, error: "Lỗi máy chủ, xem log webadmin." };
  }
}

function isZodError(err: unknown): err is { issues: { message: string }[] } {
  return typeof err === "object" && err !== null && (err as { name?: string }).name === "ZodError";
}
