import "server-only";
import { cache } from "react";
import { redirect } from "next/navigation";
import { gamePool, webPool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { env } from "@/lib/env";
import { readSession } from "./session";
import { isAccountBlocked } from "./account-status";
import { getClientIp } from "./client-ip";

export interface AdminContext {
  userId: number;
  username: string;
  playerName: string;
  isSuperAdmin: boolean;
  ip: string;
}

/**
 * Lớp bảo vệ THẬT: gọi bên trong mọi hàm đọc/ghi DAL (không dựa vào layout hay proxy).
 * Mỗi request kiểm lại DB: tài khoản còn hợp lệ + còn nhân vật isAdmin=1 → thu quyền có
 * hiệu lực ngay ở request kế tiếp. `cache()` gộp các lần gọi trong cùng một request.
 */
export const requireAdmin = cache(async (): Promise<AdminContext> => {
  const s = await readSession();
  if (!s) redirect("/login");

  const user = await queryOne<{ role: number; isBaned: number; banTime: string }>(
    webPool(),
    "SELECT role, isBaned, banTime FROM user WHERE user_id = ?",
    [s.sub],
  );
  if (!user || isAccountBlocked(user)) redirect("/login?reason=revoked");

  const admin = await queryOne<{ name: string }>(
    gamePool(),
    "SELECT name FROM player WHERE user_id = ? AND isAdmin = 1 LIMIT 1",
    [s.sub],
  );
  if (!admin) redirect("/login?reason=revoked");

  return {
    userId: s.sub,
    username: s.username,
    playerName: admin.name,
    isSuperAdmin: env().SUPERADMIN_USER_IDS.includes(s.sub),
    ip: await getClientIp(),
  };
});
