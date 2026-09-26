import "server-only";
import { gamePool, webPool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { UserFacingError } from "@/lib/actions/action-result";
import type { AdminContext } from "@/lib/auth/require-admin";

/**
 * "Tài khoản admin khác" = user_id có role=3 (admin portal cũ) HOẶC có nhân vật isAdmin=1
 * (đúng điều kiện `requireAdmin` dùng để cấp quyền vào trang này). Thao tác nhạy cảm lên
 * tài khoản dạng này (reset mật khẩu/2FA/ban) chỉ super-admin được làm [RT#7].
 */
export async function isProtectedAdminAccount(userId: number): Promise<boolean> {
  const [role3, hasAdminPlayer] = await Promise.all([
    queryOne<{ role: number }>(webPool(), "SELECT role FROM user WHERE user_id = ? AND role = 3", [userId]),
    queryOne<{ x: number }>(gamePool(), "SELECT 1 AS x FROM player WHERE user_id = ? AND isAdmin = 1 LIMIT 1", [
      userId,
    ]),
  ]);
  return !!role3 || !!hasAdminPlayer;
}

/**
 * Chặn admin thường thao tác nhạy cảm lên tài khoản admin khác. Super-admin luôn được phép
 * (trừ khi tự chặn ở nơi gọi, vd không tự thu quyền chính mình).
 */
export async function assertCanTargetSensitive(ctx: AdminContext, targetUserId: number): Promise<void> {
  if (ctx.isSuperAdmin) return;
  if (await isProtectedAdminAccount(targetUserId)) {
    throw new UserFacingError("Chỉ super-admin được thao tác này trên tài khoản admin khác.");
  }
}
