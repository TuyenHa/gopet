import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { query, queryOne } from "@/lib/db/query";
import { likeContains, parsePage, parseQuery, type SearchParams } from "@/lib/pagination";

export interface PlayerListRow {
  ID: number;
  user_id: number;
  name: string;
  isAdmin: number;
  gold: string;
  coin: string;
  star: number;
  LastTimeOnline: string;
  online: boolean;
}

/** `player_online` (phase 4) có thể chưa tồn tại — coi như "không biết" thay vì lỗi cả trang. */
async function onlineUserIds(userIds: number[]): Promise<Set<number>> {
  if (userIds.length === 0) return new Set();
  try {
    const rows = await query<{ user_id: number }>(
      gamePool(),
      `SELECT user_id FROM player_online WHERE user_id IN (${userIds.map(() => "?").join(",")})`,
      userIds,
    );
    return new Set(rows.map((r) => r.user_id));
  } catch {
    return new Set();
  }
}

export interface PlayerListResult {
  rows: PlayerListRow[];
  total: number;
  page: number;
  size: number;
}

/** Tìm theo tên nhân vật / user_id / ID. */
export async function listPlayers(sp: SearchParams): Promise<PlayerListResult> {
  await requireAdmin();
  const { page, size, offset } = parsePage(sp);
  const q = parseQuery(sp);

  const asNum = Number.parseInt(q, 10);
  const hasNum = q !== "" && Number.isInteger(asNum) && String(asNum) === q;
  const where = q ? "WHERE name LIKE ? ESCAPE '\\\\' OR user_id = ? OR ID = ?" : "";
  const whereParams = q ? [likeContains(q), hasNum ? asNum : -1, hasNum ? asNum : -1] : [];

  const [rows, totalRow] = await Promise.all([
    query<Omit<PlayerListRow, "online">>(
      gamePool(),
      `SELECT ID, user_id, name, isAdmin, gold, coin, star, LastTimeOnline FROM player ${where} ORDER BY ID DESC LIMIT ? OFFSET ?`,
      [...whereParams, size, offset],
    ),
    queryOne<{ n: number }>(gamePool(), `SELECT COUNT(*) AS n FROM player ${where}`, whereParams),
  ]);

  const online = await onlineUserIds(rows.map((r) => r.user_id));
  return {
    rows: rows.map((r) => ({ ...r, online: online.has(r.user_id) })),
    total: Number(totalRow?.n ?? 0),
    page,
    size,
  };
}
