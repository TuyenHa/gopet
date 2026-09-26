import "server-only";
import { webPool } from "@/lib/db/pools";
import { query } from "@/lib/db/query";
import { requireAdmin } from "@/lib/auth/require-admin";
import { likeContains, type PageInfo } from "@/lib/pagination";

export interface LoginHistoryRow {
  Id: number;
  UserName: string;
  LoginTime: string;
  IPAddress: string | null;
  IsSuccess: number;
  IsWebLogin: number;
}

export interface LoginHistoryFilter {
  username?: string;
  ip?: string;
  /** undefined = tất cả, true/false = lọc theo IsSuccess. */
  success?: boolean;
  from?: string;
  to?: string;
}

const DEFAULT_WINDOW_DAYS = 7;

/** `login_history` (gopettae_gopet_web) — không COUNT(*), mặc định 7 ngày gần nhất + LIMIT size+1. */
export async function listLoginHistory(
  filter: LoginHistoryFilter,
  info: PageInfo,
): Promise<{ rows: LoginHistoryRow[]; hasNext: boolean }> {
  await requireAdmin();

  const where: string[] = [];
  const params: (string | number)[] = [];

  if (filter.username) {
    where.push("UserName LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.username));
  }
  if (filter.ip) {
    where.push("IPAddress LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.ip));
  }
  if (filter.success !== undefined) {
    where.push("IsSuccess = ?");
    params.push(filter.success ? 1 : 0);
  }
  const hasNarrowFilter = !!filter.username || !!filter.ip;
  if (filter.from) {
    where.push("LoginTime >= ?");
    params.push(`${filter.from} 00:00:00`);
  } else if (!hasNarrowFilter) {
    where.push("LoginTime >= DATE_SUB(NOW(), INTERVAL ? DAY)");
    params.push(DEFAULT_WINDOW_DAYS);
  }
  if (filter.to) {
    where.push("LoginTime <= ?");
    params.push(`${filter.to} 23:59:59`);
  }

  const sql = `SELECT Id, UserName, LoginTime, IPAddress, IsSuccess, IsWebLogin FROM login_history
    ${where.length ? `WHERE ${where.join(" AND ")}` : ""}
    ORDER BY LoginTime DESC LIMIT ? OFFSET ?`;
  const rows = await query<LoginHistoryRow>(webPool(), sql, [...params, info.size + 1, info.offset]);
  const hasNext = rows.length > info.size;
  return { rows: rows.slice(0, info.size), hasNext };
}
