import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool, webPool } from "@/lib/db/pools";
import { query, queryOne, type SqlParam } from "@/lib/db/query";
import type { GiftCodeRow } from "@/lib/db/types/gift-code-row";
import { likeContains, parsePage, parseQuery, type PageInfo, type SearchParams } from "@/lib/pagination";

export type GiftcodeStatus = "active" | "expired" | "full";

export interface GiftcodeListItem extends GiftCodeRow {
  status: GiftcodeStatus;
}

/** Trạng thái tính ngay trong SQL bằng `NOW()` của chính DB — khớp đồng hồ server dùng để so
 * hạn (`GiftCodeData.getExpire()` so với `Utilities.CurrentTimeMillis`), khỏi tự quy đổi giờ. */
const STATUS_CASE = `CASE WHEN expire < NOW() THEN 'expired' WHEN currentUser >= maxUser THEN 'full' ELSE 'active' END AS status`;

export async function listGiftcodes(
  sp: SearchParams,
): Promise<{ rows: GiftcodeListItem[]; total: number; info: PageInfo }> {
  await requireAdmin();
  const info = parsePage(sp);
  const q = parseQuery(sp);
  const where = q ? "WHERE code LIKE ? ESCAPE '\\\\'" : "";
  const params: SqlParam[] = q ? [likeContains(q)] : [];

  const totalRow = await queryOne<{ c: number }>(gamePool(), `SELECT COUNT(*) AS c FROM gift_code ${where}`, params);
  const rows = await query<GiftcodeListItem>(
    gamePool(),
    `SELECT *, ${STATUS_CASE} FROM gift_code ${where} ORDER BY id DESC LIMIT ? OFFSET ?`,
    [...params, info.size, info.offset],
  );
  return { rows, total: totalRow?.c ?? 0, info };
}

export async function getGiftcodeById(id: number): Promise<GiftCodeRow | null> {
  await requireAdmin();
  return queryOne<GiftCodeRow>(gamePool(), "SELECT * FROM gift_code WHERE id = ?", [id]);
}

export async function getGiftcodeByCode(code: string): Promise<GiftCodeRow | null> {
  await requireAdmin();
  return queryOne<GiftCodeRow>(gamePool(), "SELECT * FROM gift_code WHERE code = ?", [code]);
}

export interface GiftCodeUserRef {
  id: number;
  label: string;
}

/** `usersOfUseThis` (JSON id): id là `clanId` khi `isClanCode`, ngược lại là `user_id`
 * (MenuController.inputDialog.cs:131-133). Map sang tên hiển thị; id không tra được (bang đã
 * giải tán / tài khoản đã xoá) vẫn hiện để không mất dấu vết. */
export async function resolveGiftUsers(row: Pick<GiftCodeRow, "usersOfUseThis" | "isClanCode">): Promise<GiftCodeUserRef[]> {
  await requireAdmin();
  let ids: number[] = [];
  try {
    const parsed: unknown = JSON.parse(row.usersOfUseThis);
    if (Array.isArray(parsed)) ids = parsed.filter((x): x is number => typeof x === "number");
  } catch {
    ids = [];
  }
  if (ids.length === 0) return [];

  const placeholders = ids.map(() => "?").join(",");
  if (row.isClanCode) {
    const clans = await query<{ clanId: number; name: string }>(
      gamePool(),
      `SELECT clanId, name FROM clan WHERE clanId IN (${placeholders})`,
      ids,
    );
    const byId = new Map(clans.map((c) => [c.clanId, c.name]));
    return ids.map((id) => ({ id, label: byId.get(id) ?? `Bang #${id} (đã giải tán?)` }));
  }
  const users = await query<{ user_id: number; username: string }>(
    webPool(),
    `SELECT user_id, username FROM user WHERE user_id IN (${placeholders})`,
    ids,
  );
  const byId = new Map(users.map((u) => [u.user_id, u.username]));
  return ids.map((id) => ({ id, label: byId.get(id) ?? `#${id} (tài khoản đã xoá?)` }));
}
