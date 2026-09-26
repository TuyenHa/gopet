import "server-only";
import { webPool } from "@/lib/db/pools";
import { query } from "@/lib/db/query";
import { requireAdmin } from "@/lib/auth/require-admin";
import { likeContains, type PageInfo } from "@/lib/pagination";

export interface AuditLogRow {
  id: string; // bigint → chuỗi (dateStrings/bigNumberStrings)
  admin_user_id: number;
  admin_username: string;
  action: string;
  target: string | null;
  detail: string | null;
  ip: string | null;
  created_at: string;
}

export interface AuditLogFilter {
  admin?: string;
  action?: string;
  target?: string;
  from?: string;
  to?: string;
}

const DEFAULT_WINDOW_DAYS = 7;

/** `admin_audit_log` (gopettae_gopet_web) — bảng do chính web admin ghi, nhỏ hơn history nhưng
 * vẫn tránh COUNT(*) và luôn có mặc định 7 ngày để nhất quán với các trang log khác. */
export async function listAuditLog(filter: AuditLogFilter, info: PageInfo): Promise<{ rows: AuditLogRow[]; hasNext: boolean }> {
  await requireAdmin();

  const where: string[] = [];
  const params: (string | number)[] = [];

  if (filter.admin) {
    where.push("admin_username LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.admin));
  }
  if (filter.action) {
    where.push("action LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.action));
  }
  if (filter.target) {
    where.push("target LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.target));
  }
  const hasNarrowFilter = !!filter.admin || !!filter.action || !!filter.target;
  if (filter.from) {
    where.push("created_at >= ?");
    params.push(`${filter.from} 00:00:00`);
  } else if (!hasNarrowFilter) {
    where.push("created_at >= DATE_SUB(NOW(), INTERVAL ? DAY)");
    params.push(DEFAULT_WINDOW_DAYS);
  }
  if (filter.to) {
    where.push("created_at <= ?");
    params.push(`${filter.to} 23:59:59`);
  }

  const sql = `SELECT id, admin_user_id, admin_username, action, target, detail, ip, created_at FROM admin_audit_log
    ${where.length ? `WHERE ${where.join(" AND ")}` : ""}
    ORDER BY created_at DESC LIMIT ? OFFSET ?`;
  const rows = await query<AuditLogRow>(webPool(), sql, [...params, info.size + 1, info.offset]);
  const hasNext = rows.length > info.size;
  return { rows: rows.slice(0, info.size), hasNext };
}
