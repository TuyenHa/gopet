import "server-only";
import { webPool } from "@/lib/db/pools";
import { execute } from "@/lib/db/query";
import type { AdminContext } from "@/lib/auth/require-admin";

// Khoá không bao giờ được vào audit, kể cả khi lỡ truyền vào detail.
const SECRET_KEYS = /^(password|newPassword|secretKey|hash|confirmPassword)$/i;

function sanitize(detail: unknown): string | null {
  if (detail === undefined || detail === null) return null;
  return JSON.stringify(detail, (k, v) => {
    if (SECRET_KEYS.test(k)) return "[redacted]";
    return typeof v === "bigint" ? v.toString() : v;
  });
}

export interface AuditEntry {
  action: string;
  target?: string | null;
  detail?: unknown;
}

type Actor = Pick<AdminContext, "userId" | "username" | "ip">;

/** INSERT 1 dòng audit. Ném lỗi nếu ghi thất bại — người gọi quyết định fail-closed. */
export async function writeAuditLog(actor: Actor, e: AuditEntry): Promise<void> {
  await execute(
    webPool(),
    "INSERT INTO admin_audit_log (admin_user_id, admin_username, action, target, detail, ip) VALUES (?,?,?,?,?,?)",
    [
      actor.userId,
      actor.username.slice(0, 20),
      e.action.slice(0, 64),
      e.target?.slice(0, 128) ?? null,
      sanitize(e.detail),
      actor.ip.slice(0, 64),
    ],
  );
}

/**
 * Ghi audit trước (fail-closed: audit lỗi → KHÔNG mutate), chạy mutate, rồi ghi thêm dòng
 * `<action>:done` hoặc `<action>:failed` (bảng chỉ INSERT, không UPDATE được dòng cũ).
 */
export async function audited<T>(ctx: Actor, e: AuditEntry, mutate: () => Promise<T>): Promise<T> {
  await writeAuditLog(ctx, e);
  try {
    const result = await mutate();
    await writeAuditLog(ctx, { action: `${e.action}:done`, target: e.target }).catch((err) =>
      console.error("[audit] ghi :done lỗi", err),
    );
    return result;
  } catch (err) {
    const msg = err instanceof Error ? err.message : String(err);
    await writeAuditLog(ctx, { action: `${e.action}:failed`, target: e.target, detail: { error: msg } }).catch(
      (err2) => console.error("[audit] ghi :failed lỗi", err2),
    );
    throw err;
  }
}
