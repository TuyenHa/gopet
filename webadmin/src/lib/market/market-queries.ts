import "server-only";
import { gamePool } from "@/lib/db/pools";
import { query, queryOne } from "@/lib/db/query";
import { requireAdmin } from "@/lib/auth/require-admin";
import type { PageInfo } from "@/lib/pagination";

export interface MarketRow {
  Id: string; // bigint(255) → chuỗi
  Data: string;
  TimeSave: string;
}

/** Dòng `market` mới nhất — snapshot chợ trời, server ghi đè/prune, web CHỈ ĐỌC. */
export async function getLatestMarketSnapshot(): Promise<MarketRow | null> {
  await requireAdmin();
  return queryOne<MarketRow>(gamePool(), "SELECT Id, Data, TimeSave FROM market ORDER BY TimeSave DESC LIMIT 1");
}

/** Tra tên item template theo itemId (bảng `item`, template — không phải item instance). */
export async function getItemNames(itemTemplateIds: number[]): Promise<Map<number, string>> {
  await requireAdmin();
  const ids = [...new Set(itemTemplateIds)];
  if (ids.length === 0) return new Map();
  const rows = await query<{ itemId: number; name: string }>(
    gamePool(),
    `SELECT itemId, name FROM item WHERE itemId IN (${ids.map(() => "?").join(",")})`,
    ids,
  );
  return new Map(rows.map((r) => [r.itemId, r.name]));
}

export interface KioskRecoveryRow {
  kioskType: number;
  user_id: number;
  item: string;
}

/** `kiosk_recovery` — không PK, không cột thời gian → không có cách lọc theo ngày; chỉ LIMIT. */
export async function listKioskRecovery(info: PageInfo): Promise<{ rows: KioskRecoveryRow[]; hasNext: boolean }> {
  await requireAdmin();
  const rows = await query<KioskRecoveryRow>(
    gamePool(),
    "SELECT kioskType, user_id, item FROM kiosk_recovery LIMIT ? OFFSET ?",
    [info.size + 1, info.offset],
  );
  const hasNext = rows.length > info.size;
  return { rows: rows.slice(0, info.size), hasNext };
}
