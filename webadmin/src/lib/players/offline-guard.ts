import "server-only";
import type { PoolConnection } from "mysql2/promise";
import { gamePool, webPool } from "@/lib/db/pools";
import { execute, queryOne, type SqlParam } from "@/lib/db/query";
import { withNamedLock } from "@/lib/db/named-lock";
import { UserFacingError } from "@/lib/actions/action-result";
import { isServerOffSwitchActive } from "./server-off-switch";

const HEARTBEAT_FRESH_SEC = 90;
const LOGIN_LOCK_TIMEOUT_SEC = 5;
/** Giây — cú pháp MariaDB `SET STATEMENT max_statement_time=<giây> FOR ...`. */
const MAX_STATEMENT_SEC = 8;

export class PlayerOnlineError extends UserFacingError {
  constructor() {
    super("Nhân vật đang online — máy chủ game đang giữ dữ liệu, không thể sửa trực tiếp lúc này.");
  }
}

export class ServerNotRespondingError extends UserFacingError {
  constructor() {
    super(
      "Không xác nhận được máy chủ game đang chạy (thiếu heartbeat) — khoá sửa để an toàn. " +
        "Nếu đang bảo trì, bật công tắc \"server đã TẮT\" (super-admin).",
    );
  }
}

/** Bảng phase 4 có thể chưa tồn tại/lỗi truy vấn → fail-closed: coi như KHÔNG xác nhận được. */
async function queryOrFailClosed<T>(conn: PoolConnection, sql: string, params: SqlParam[] = []): Promise<T | null> {
  try {
    return await queryOne<T>(conn, sql, params);
  } catch {
    return null;
  }
}

async function assertServerAliveAndPlayerOffline(conn: PoolConnection, userId: number): Promise<void> {
  if (!isServerOffSwitchActive()) {
    const beat = await queryOrFailClosed<{ age: number | null }>(
      conn,
      "SELECT TIMESTAMPDIFF(SECOND, beat_at, NOW()) AS age FROM server_heartbeat WHERE id = 1 AND protocol_version >= 1",
    );
    if (!beat || beat.age === null || beat.age > HEARTBEAT_FRESH_SEC) throw new ServerNotRespondingError();
  }
  // NOT EXISTS player_online — lỗi truy vấn (vd bảng chưa có) cũng fail-closed như "đang online".
  const online = await queryOrFailClosed<{ x: number }>(
    conn,
    "SELECT 1 AS x FROM player_online WHERE user_id = ? LIMIT 1",
    [userId],
  );
  if (online) throw new PlayerOnlineError();
}

/**
 * Đúng giao thức phase 4 (`phase-04-gserver-isonline-patch.md` § Giao thức với web):
 * conn riêng; GET_LOCK('login_lock_'+username, 5)===1; heartbeat tươi (trừ khi công tắc
 * "server tắt" bật) + NOT EXISTS player_online; chạy `fn` trên CÙNG connection (đã giữ khoá)
 * rồi luôn RELEASE_LOCK + trả connection (xử lý trong `withNamedLock`).
 *
 * CHỈ nhận `playerId` (khoá chính bảng `player`, không lấy từ client) — `user_id`/`username`
 * dùng để khoá + kiểm tra online LUÔN được tra cứu lại ở server từ chính `playerId` này, không
 * bao giờ tin `userId` do client gửi kèm (H3: form/hidden input có thể bị sửa để trỏ `userId`
 * của 1 tài khoản offline trong khi `playerId` thuộc về 1 tài khoản đang online). `fn` nhận
 * thêm `userId` đã tra cứu để caller có thể thêm `AND user_id = ?` vào UPDATE (phòng thủ 2 lớp).
 */
export async function withOfflinePlayer<T>(
  playerId: number,
  fn: (conn: PoolConnection, userId: number) => Promise<T>,
): Promise<T> {
  const player = await queryOne<{ user_id: number }>(gamePool(), "SELECT user_id FROM player WHERE ID = ?", [
    playerId,
  ]);
  if (!player) throw new UserFacingError("Không tìm thấy nhân vật này.");
  const userId = player.user_id;

  const account = await queryOne<{ username: string }>(webPool(), "SELECT username FROM user WHERE user_id = ?", [
    userId,
  ]);
  if (!account) throw new UserFacingError("Không tìm thấy tài khoản của nhân vật này.");

  return withNamedLock(gamePool(), `login_lock_${account.username}`, LOGIN_LOCK_TIMEOUT_SEC, async (conn) => {
    await assertServerAliveAndPlayerOffline(conn, userId);
    return fn(conn, userId);
  });
}

/** UPDATE giới hạn thời gian chạy (tránh khoá bảng `player` lâu) — dùng bên trong `withOfflinePlayer`. */
export async function executeGuardedUpdate(conn: PoolConnection, sql: string, params: SqlParam[]) {
  return execute(conn, `SET STATEMENT max_statement_time=${MAX_STATEMENT_SEC} FOR ${sql}`, params);
}
