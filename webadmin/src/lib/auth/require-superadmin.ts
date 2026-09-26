import "server-only";
import { UserFacingError } from "@/lib/actions/action-result";
import { requireAdmin, type AdminContext } from "./require-admin";

/** Super-admin = user_id trong SUPERADMIN_USER_IDS (mặc định 1 = tài khoản admin) + vẫn là admin hợp lệ. */
export async function requireSuperAdmin(): Promise<AdminContext> {
  const ctx = await requireAdmin();
  if (!ctx.isSuperAdmin) throw new UserFacingError("Chỉ super-admin được làm thao tác này.");
  return ctx;
}
