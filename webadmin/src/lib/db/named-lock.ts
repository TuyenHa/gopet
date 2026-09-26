import "server-only";
import type { Pool, PoolConnection } from "mysql2/promise";
import { queryOne } from "./query";

export class LockBusyError extends Error {
  constructor(public readonly lockName: string) {
    super("Dữ liệu đang được máy chủ game xử lý, thử lại sau.");
    this.name = "LockBusyError";
  }
}

/**
 * GET_LOCK cùng tên với GServer (vd `login_lock_<username>`, `gift_code_lock_<code>`) trên
 * MỘT connection riêng, chạy fn, rồi luôn RELEASE_LOCK trước khi trả connection về pool —
 * không để khoá kẹt trên connection được tái sử dụng. Nhả khoá lỗi → huỷ connection để
 * MariaDB tự nhả theo session.
 */
export async function withNamedLock<T>(
  pool: Pool,
  name: string,
  timeoutSec: number,
  fn: (conn: PoolConnection) => Promise<T>,
): Promise<T> {
  const conn = await pool.getConnection();
  let healthy = true;
  try {
    const row = await queryOne<{ got: number | null }>(conn, "SELECT GET_LOCK(?, ?) AS got", [name, timeoutSec]);
    // GET_LOCK trả 1 = có khoá, 0 = hết giờ, NULL = lỗi. Chỉ chấp nhận đúng 1.
    if (Number(row?.got) !== 1) throw new LockBusyError(name);
    try {
      return await fn(conn);
    } finally {
      try {
        await conn.query("SELECT RELEASE_LOCK(?)", [name]);
      } catch {
        healthy = false;
      }
    }
  } finally {
    if (healthy) conn.release();
    else conn.destroy();
  }
}
