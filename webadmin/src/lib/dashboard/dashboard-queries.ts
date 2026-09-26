import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool, webPool } from "@/lib/db/pools";
import { query, queryOne } from "@/lib/db/query";

export interface DashboardStats {
  totalUsers: number;
  totalPlayers: number;
  /** null = bảng player_online chưa có (GServer chưa vá phase 4). */
  onlinePlayers: number | null;
  /** null = chưa có bảng heartbeat; số giây kể từ lần beat cuối. */
  heartbeatAgeSec: number | null;
  activeGiftcodes: number;
  recentAudit: { id: string; admin_username: string; action: string; target: string | null; created_at: string }[];
}

// Bảng của phase 4 có thể chưa tồn tại → trả null thay vì làm hỏng cả dashboard.
async function optional<T>(fn: () => Promise<T>): Promise<T | null> {
  try {
    return await fn();
  } catch {
    return null;
  }
}

export async function getDashboardStats(): Promise<DashboardStats> {
  await requireAdmin();
  const [users, players, online, beat, gifts, audit] = await Promise.all([
    queryOne<{ n: number }>(webPool(), "SELECT COUNT(*) AS n FROM user"),
    queryOne<{ n: number }>(gamePool(), "SELECT COUNT(*) AS n FROM player"),
    optional(() => queryOne<{ n: number }>(gamePool(), "SELECT COUNT(*) AS n FROM player_online")),
    optional(() =>
      queryOne<{ age: number }>(
        gamePool(),
        "SELECT TIMESTAMPDIFF(SECOND, beat_at, NOW()) AS age FROM server_heartbeat WHERE id = 1",
      ),
    ),
    queryOne<{ n: number }>(
      gamePool(),
      "SELECT COUNT(*) AS n FROM gift_code WHERE expire > NOW() AND currentUser < maxUser",
    ),
    query<DashboardStats["recentAudit"][number]>(
      webPool(),
      "SELECT id, admin_username, action, target, created_at FROM admin_audit_log ORDER BY id DESC LIMIT 10",
    ),
  ]);
  return {
    totalUsers: Number(users?.n ?? 0),
    totalPlayers: Number(players?.n ?? 0),
    onlinePlayers: online ? Number(online.n) : null,
    heartbeatAgeSec: beat ? Number(beat.age) : null,
    activeGiftcodes: Number(gifts?.n ?? 0),
    recentAudit: audit,
  };
}
