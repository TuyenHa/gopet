import "server-only";
import { logPool } from "@/lib/db/pools";
import { query } from "@/lib/db/query";
import { requireAdmin } from "@/lib/auth/require-admin";
import { likeContains, type PageInfo } from "@/lib/pagination";

export interface HistoryRow {
  targetId: number;
  log: string;
  obj: string | null;
  charname: string;
  timeDB: string;
  eventId: string | null;
}

export interface HistoryFilter {
  targetId?: number;
  charname?: string;
  keyword?: string;
  from?: string; // "YYYY-MM-DD"
  to?: string; // "YYYY-MM-DD"
}

const DEFAULT_WINDOW_DAYS = 7;

/**
 * `history` (gp_log) có thể rất lớn (không có PK, chỉ KEY targetId/charname) → luôn ép khoảng
 * thời gian mặc định 7 ngày khi không lọc theo targetId/charname/keyword, KHÔNG bao giờ COUNT(*)
 * toàn bảng — dùng LIMIT size+1 để suy ra hasNext.
 */
export async function listHistory(filter: HistoryFilter, info: PageInfo): Promise<{ rows: HistoryRow[]; hasNext: boolean }> {
  await requireAdmin();

  const where: string[] = [];
  const params: (string | number)[] = [];

  if (filter.targetId !== undefined) {
    where.push("targetId = ?");
    params.push(filter.targetId);
  }
  if (filter.charname) {
    where.push("charname LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.charname));
  }
  if (filter.keyword) {
    where.push("log LIKE ? ESCAPE '\\\\'");
    params.push(likeContains(filter.keyword));
  }
  // Không lọc theo targetId/charname → bắt buộc khoảng thời gian để tránh quét toàn bảng.
  const hasNarrowFilter = filter.targetId !== undefined || !!filter.charname;
  if (filter.from) {
    where.push("timeDB >= ?");
    params.push(`${filter.from} 00:00:00`);
  } else if (!hasNarrowFilter) {
    where.push("timeDB >= DATE_SUB(NOW(), INTERVAL ? DAY)");
    params.push(DEFAULT_WINDOW_DAYS);
  }
  if (filter.to) {
    where.push("timeDB <= ?");
    params.push(`${filter.to} 23:59:59`);
  }

  const sql = `SELECT targetId, log, obj, charname, timeDB, eventId FROM history
    ${where.length ? `WHERE ${where.join(" AND ")}` : ""}
    ORDER BY timeDB DESC LIMIT ? OFFSET ?`;
  const rows = await query<HistoryRow>(logPool(), sql, [...params, info.size + 1, info.offset]);
  const hasNext = rows.length > info.size;
  return { rows: rows.slice(0, info.size), hasNext };
}
