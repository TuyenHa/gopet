import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool, webPool } from "@/lib/db/pools";
import { query, queryOne } from "@/lib/db/query";
import { likeContains, parsePage, parseQuery, type SearchParams } from "@/lib/pagination";
import { isProtectedAdminAccount } from "./admin-account-guard";

/** Danh sách/`chi tiết — KHÔNG BAO GIỜ SELECT `password`/`secretKey`, chỉ cờ dẫn xuất [RT#6]. */
export interface AccountListRow {
  user_id: number;
  username: string;
  role: number;
  isBaned: number;
  banTime: string;
  coin: number;
  tongnap: number;
  create_date: string;
  isBcrypt: number;
  has2fa: number;
  online: boolean;
}

const SAFE_COLUMNS = `
  user_id, username, role, isBaned, banTime, coin, tongnap, create_date,
  (password LIKE '$2%') AS isBcrypt,
  (secretKey IS NOT NULL) AS has2fa
`;

/** Bảng `player_online` (phase 4) có thể chưa tồn tại — coi như "không biết" thay vì lỗi cả trang. */
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

export interface AccountListResult {
  rows: AccountListRow[];
  total: number;
  page: number;
  size: number;
}

export async function listAccounts(sp: SearchParams): Promise<AccountListResult> {
  await requireAdmin();
  const { page, size, offset } = parsePage(sp);
  const q = parseQuery(sp);

  const asId = Number.parseInt(q, 10);
  const hasId = q !== "" && Number.isInteger(asId) && String(asId) === q;
  const where = q ? "WHERE username LIKE ? ESCAPE '\\\\' OR email LIKE ? ESCAPE '\\\\' OR user_id = ?" : "";
  const whereParams = q ? [likeContains(q), likeContains(q), hasId ? asId : -1] : [];

  const [rows, totalRow] = await Promise.all([
    query<Omit<AccountListRow, "online">>(
      webPool(),
      `SELECT ${SAFE_COLUMNS} FROM user ${where} ORDER BY user_id DESC LIMIT ? OFFSET ?`,
      [...whereParams, size, offset],
    ),
    queryOne<{ n: number }>(webPool(), `SELECT COUNT(*) AS n FROM user ${where}`, whereParams),
  ]);

  const online = await onlineUserIds(rows.map((r) => r.user_id));
  return {
    rows: rows.map((r) => ({ ...r, online: online.has(r.user_id) })),
    total: Number(totalRow?.n ?? 0),
    page,
    size,
  };
}

export interface AccountDetail extends Omit<AccountListRow, "online"> {
  banReason: string;
  email: string;
  phone: string | null;
  online: boolean;
  isProtectedAdmin: boolean;
}

export async function getAccountDetail(userId: number): Promise<AccountDetail | null> {
  await requireAdmin();
  const row = await queryOne<Omit<AccountListRow, "online"> & { banReason: string; email: string; phone: string | null }>(
    webPool(),
    `SELECT ${SAFE_COLUMNS}, banReason, email, phone FROM user WHERE user_id = ?`,
    [userId],
  );
  if (!row) return null;
  const [online, isProtectedAdmin] = await Promise.all([onlineUserIds([userId]), isProtectedAdminAccount(userId)]);
  return { ...row, online: online.has(userId), isProtectedAdmin };
}
