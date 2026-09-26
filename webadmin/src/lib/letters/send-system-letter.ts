import "server-only";
import { gamePool, webPool } from "@/lib/db/pools";
import { execute, queryOne } from "@/lib/db/query";
import { withNamedLock } from "@/lib/db/named-lock";

export interface ResolvedTarget {
  userId: number;
  username: string;
  playerName: string;
}

/**
 * Tra người nhận theo 1 trong 3 cách: `user_id` số, `username` (bảng `user`), hoặc tên nhân
 * vật (bảng `player.name`). Ưu tiên user_id số trước (rõ ràng nhất, tránh trùng với username
 * toàn số), rồi username, cuối cùng tên nhân vật.
 */
export async function resolvePlayerTarget(input: string): Promise<ResolvedTarget | null> {
  const term = input.trim();
  if (!term) return null;

  if (/^\d+$/.test(term)) {
    const byId = await queryOne<{ user_id: number; username: string }>(
      webPool(),
      "SELECT user_id, username FROM user WHERE user_id = ?",
      [Number(term)],
    );
    if (byId) return withPlayerName(byId.user_id, byId.username);
  }

  const byUsername = await queryOne<{ user_id: number; username: string }>(
    webPool(),
    "SELECT user_id, username FROM user WHERE username = ?",
    [term],
  );
  if (byUsername) return withPlayerName(byUsername.user_id, byUsername.username);

  const byPlayerName = await queryOne<{ user_id: number; name: string }>(
    gamePool(),
    "SELECT user_id, name FROM player WHERE name = ? LIMIT 1",
    [term],
  );
  if (byPlayerName) {
    const user = await queryOne<{ username: string }>(webPool(), "SELECT username FROM user WHERE user_id = ?", [
      byPlayerName.user_id,
    ]);
    if (user) return { userId: byPlayerName.user_id, username: user.username, playerName: byPlayerName.name };
  }
  return null;
}

async function withPlayerName(userId: number, username: string): Promise<ResolvedTarget> {
  const player = await queryOne<{ name: string }>(gamePool(), "SELECT name FROM player WHERE user_id = ? LIMIT 1", [userId]);
  return { userId, username, playerName: player?.name ?? "" };
}

/**
 * Chèn 1 thư hệ thống (`userId=0`) vào hàng đợi `letter`, khoá `login_lock_<username>` —
 * ĐÚNG tên khoá server dùng lúc login để nạp + xoá hàng đợi (`Player.cs:495-502`), tránh chèn
 * thư đúng lúc server đang giữa `SELECT`/`DELETE` (RT#12) làm mất thư.
 */
export async function insertSystemLetter(
  target: Pick<ResolvedTarget, "userId" | "username">,
  type: number,
  title: string,
  shortContent: string,
  content: string,
): Promise<void> {
  await withNamedLock(gamePool(), `login_lock_${target.username}`, 5, async (conn) => {
    await execute(
      conn,
      "INSERT INTO letter (userId, targetId, time, Type, Title, ShortContent, Content) VALUES (0, ?, NOW(), ?, ?, ?, ?)",
      [target.userId, type, title, shortContent, content],
    );
  });
}
