import "server-only";
import type { Pool, PoolConnection, ResultSetHeader, RowDataPacket } from "mysql2/promise";

type Executor = Pool | PoolConnection;
export type SqlParam = string | number | bigint | boolean | null | Date;

/** SELECT có kiểu. Luôn dùng tham số `?`, không nội suy giá trị vào chuỗi SQL. */
export async function query<T>(db: Executor, sql: string, params: SqlParam[] = []): Promise<T[]> {
  const [rows] = await db.query<RowDataPacket[]>(sql, params);
  return rows as unknown as T[];
}

export async function queryOne<T>(db: Executor, sql: string, params: SqlParam[] = []): Promise<T | null> {
  const rows = await query<T>(db, sql, params);
  return rows[0] ?? null;
}

/** INSERT/UPDATE/DELETE; trả về affectedRows + insertId. */
export async function execute(db: Executor, sql: string, params: SqlParam[] = []): Promise<ResultSetHeader> {
  const [res] = await db.query<ResultSetHeader>(sql, params);
  return res;
}

/** Mượn 1 connection riêng (cần cho GET_LOCK — khoá gắn với connection) và luôn trả lại pool. */
export async function withConnection<T>(pool: Pool, fn: (conn: PoolConnection) => Promise<T>): Promise<T> {
  const conn = await pool.getConnection();
  try {
    return await fn(conn);
  } finally {
    conn.release();
  }
}
