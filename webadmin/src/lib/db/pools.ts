import "server-only";
import mysql, { type Pool } from "mysql2/promise";
import { env } from "@/lib/env";

type DbName = "game" | "web" | "log";

// Cache trên globalThis để hot-reload khi dev không mở thêm pool mới mỗi lần sửa file.
const g = globalThis as unknown as { __gopetPools?: Partial<Record<DbName, Pool>> };

function createPool(database: string): Pool {
  const e = env();
  return mysql.createPool({
    host: e.DB_HOST,
    port: e.DB_PORT,
    user: e.DB_USER,
    password: e.DB_PASSWORD,
    database,
    charset: "utf8mb4",
    connectionLimit: 5,
    // DATETIME trả về chuỗi nguyên văn — không để driver tự đổi múi giờ.
    dateStrings: true,
    // bigint(20) (gold, coin, banTime...) trả về chuỗi, tính bằng BigInt.
    supportBigNumbers: true,
    bigNumberStrings: true,
    waitForConnections: true,
  });
}

function getPool(name: DbName): Pool {
  const pools = (g.__gopetPools ??= {});
  let pool = pools[name];
  if (!pool) {
    const e = env();
    pool = createPool(name === "game" ? e.DB_GAME : name === "web" ? e.DB_WEB : e.DB_LOG);
    pools[name] = pool;
  }
  return pool;
}

export const gamePool = () => getPool("game");
export const webPool = () => getPool("web");
export const logPool = () => getPool("log");
