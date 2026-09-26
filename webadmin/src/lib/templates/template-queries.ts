import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool, webPool } from "@/lib/db/pools";
import { query, queryOne, type SqlParam } from "@/lib/db/query";
import { likeContains, type PageInfo } from "@/lib/pagination";
import type { PkValues } from "./pk-codec";
import type { TableConfig } from "./table-registry";

/** Chỉ lấy đúng pool theo registry — không bao giờ suy diễn tên DB từ input. */
function poolOf(config: TableConfig) {
  return config.db === "web" ? webPool() : gamePool();
}

function selectColumnList(config: TableConfig): string {
  return config.columns.map((c) => `\`${c.name}\``).join(", ");
}

function searchWhere(config: TableConfig, q: string): { clause: string; params: SqlParam[] } {
  if (!q || config.searchCols.length === 0) return { clause: "", params: [] };
  const clause = `WHERE (${config.searchCols.map((c) => `\`${c}\` LIKE ? ESCAPE '\\\\'`).join(" OR ")})`;
  const params = config.searchCols.map(() => likeContains(q));
  return { clause, params };
}

/** Cột sắp xếp mặc định: cột PK đầu tiên, hoặc cột đầu tiên của registry nếu readOnly (không PK). */
function defaultOrderBy(config: TableConfig): string {
  const col = config.pk[0] ?? config.columns[0]?.name;
  return col ? `ORDER BY \`${col}\`` : "";
}

export interface TemplateRow {
  [key: string]: unknown;
}

/** Danh sách có phân trang + tìm kiếm LIKE trên `searchCols` (allowlist từ registry). */
export async function listTemplateRows(
  config: TableConfig,
  opts: { q: string; page: PageInfo },
): Promise<TemplateRow[]> {
  await requireAdmin();
  const { clause, params } = searchWhere(config, opts.q);
  const sql = `SELECT ${selectColumnList(config)} FROM \`${config.table}\` ${clause} ${defaultOrderBy(config)} LIMIT ? OFFSET ?`;
  return query<TemplateRow>(poolOf(config), sql, [...params, opts.page.size, opts.page.offset]);
}

/** Đếm tổng số dòng khớp tìm kiếm — bảng template nhỏ nên COUNT(*) rẻ, dùng cho PaginationBar chính xác. */
export async function countTemplateRows(config: TableConfig, q: string): Promise<number> {
  await requireAdmin();
  const { clause, params } = searchWhere(config, q);
  const row = await queryOne<{ cnt: number }>(poolOf(config), `SELECT COUNT(*) AS cnt FROM \`${config.table}\` ${clause}`, params);
  return row?.cnt ?? 0;
}

/** Lấy 1 dòng theo PK (đơn hoặc ghép) — pkValues đã được decodePk() kiểm hợp lệ trước đó. */
export async function getTemplateRow(config: TableConfig, pkValues: PkValues): Promise<TemplateRow | null> {
  await requireAdmin();
  const where = config.pk.map((p) => `\`${p}\`=?`).join(" AND ");
  const sql = `SELECT ${selectColumnList(config)} FROM \`${config.table}\` WHERE ${where} LIMIT 1`;
  return queryOne<TemplateRow>(poolOf(config), sql, config.pk.map((p) => pkValues[p]));
}
