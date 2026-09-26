import "server-only";
import { gamePool } from "@/lib/db/pools";
import { query } from "@/lib/db/query";
import { requireAdmin } from "@/lib/auth/require-admin";
import type { PageInfo } from "@/lib/pagination";

interface ClanQueryRow {
  clanId: number;
  name: string;
  lvl: number;
  leaderId: number;
  leaderName: string | null;
  members: string;
  fund: string;
  timeCreate: string;
}

export interface ClanRow {
  clanId: number;
  name: string;
  lvl: number;
  leaderId: number;
  leaderName: string;
  memberCount: number;
  fund: string;
  timeCreate: string;
}

/** Đếm số thành viên từ JSON `members` — rẻ vì đã tải cả cột để hiển thị, không cần truy vấn thêm. */
function countMembers(membersJson: string): number {
  try {
    const arr = JSON.parse(membersJson);
    return Array.isArray(arr) ? arr.length : 0;
  } catch {
    return 0;
  }
}

/**
 * `clan` (gopettae_tae2) — dữ liệu runtime trong RAM của GServer, chỉ ghi đè khi save → CHỈ ĐỌC.
 * Bảng nhỏ (auto_increment ~100) nên không cần khoảng thời gian mặc định, chỉ LIMIT size+1.
 */
export async function listClans(info: PageInfo): Promise<{ rows: ClanRow[]; hasNext: boolean }> {
  await requireAdmin();
  const rows = await query<ClanQueryRow>(
    gamePool(),
    `SELECT c.clanId, c.name, c.lvl, c.leaderId, p.name AS leaderName, c.members, c.fund, c.timeCreate
     FROM clan c
     LEFT JOIN player p ON p.user_id = c.leaderId
     ORDER BY c.clanId ASC LIMIT ? OFFSET ?`,
    [info.size + 1, info.offset],
  );
  const hasNext = rows.length > info.size;
  return {
    rows: rows.slice(0, info.size).map((r) => ({
      clanId: r.clanId,
      name: r.name,
      lvl: r.lvl,
      leaderId: r.leaderId,
      leaderName: r.leaderName ?? "???",
      memberCount: countMembers(r.members),
      fund: r.fund,
      timeCreate: r.timeCreate,
    })),
    hasNext,
  };
}
