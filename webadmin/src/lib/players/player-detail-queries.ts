import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { query, queryOne } from "@/lib/db/query";

/** Tab "Tổng quan". `clanId`/`ArenaPoint`/`KioskFund` chỉ hiện thông tin, KHÔNG có trong form sửa [RT#4]. */
export interface PlayerOverview {
  ID: number;
  user_id: number;
  name: string;
  gender: number;
  isAdmin: number;
  gold: string;
  coin: string;
  lua: string;
  star: number;
  pkPoint: number;
  clanId: number;
  avatarPath: string;
  AccumulatedPoint: number;
  EventPoint: number;
  loginDate: string;
  LastTimeOnline: string;
}

export async function getPlayerOverview(id: number): Promise<PlayerOverview | null> {
  await requireAdmin();
  return queryOne<PlayerOverview>(
    gamePool(),
    `SELECT ID, user_id, name, gender, isAdmin, gold, coin, lua, star, pkPoint, clanId, avatarPath,
            AccumulatedPoint, EventPoint, loginDate, LastTimeOnline
     FROM player WHERE ID = ?`,
    [id],
  );
}

/** Tab "Thư" — `letter.targetId` = `player.user_id` (không phải `player.ID`). */
export interface PlayerLetterRow {
  time: string;
  Type: number;
  Title: string;
  ShortContent: string;
  Content: string;
}

export async function getPlayerLetters(userId: number): Promise<PlayerLetterRow[]> {
  await requireAdmin();
  return query<PlayerLetterRow>(
    gamePool(),
    "SELECT time, Type, Title, ShortContent, Content FROM letter WHERE targetId = ? ORDER BY time DESC LIMIT 100",
    [userId],
  );
}

/** Tab "Bạn bè" — JSON lưu ngay trên row `player`, không có bảng riêng. */
export interface PlayerFriendsSnapshot {
  ListFriends: string;
  RequestAddFriends: string;
  BlockFriendLists: string;
}

export async function getPlayerFriends(id: number): Promise<PlayerFriendsSnapshot | null> {
  await requireAdmin();
  return queryOne<PlayerFriendsSnapshot>(
    gamePool(),
    "SELECT ListFriends, RequestAddFriends, BlockFriendLists FROM player WHERE ID = ?",
    [id],
  );
}

/** Tab "Nhiệm vụ/thành tựu". */
export interface PlayerQuestsSnapshot {
  task: string | null;
  tasking: string | null;
  wasTask: string | null;
  achievements: string;
  CurrentAchievementId: number;
  AccumulatedPoint: number;
}

export async function getPlayerQuests(id: number): Promise<PlayerQuestsSnapshot | null> {
  await requireAdmin();
  return queryOne<PlayerQuestsSnapshot>(
    gamePool(),
    "SELECT task, tasking, wasTask, achievements, CurrentAchievementId, AccumulatedPoint FROM player WHERE ID = ?",
    [id],
  );
}

/** Tab "Raw JSON" — nguyên hàng `player` (bảng này không có cột mật khẩu/secret nào). */
export async function getPlayerRaw(id: number): Promise<Record<string, unknown> | null> {
  await requireAdmin();
  return queryOne<Record<string, unknown>>(gamePool(), "SELECT * FROM player WHERE ID = ?", [id]);
}
