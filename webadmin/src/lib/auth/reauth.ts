import "server-only";
import bcrypt from "bcryptjs";
import { webPool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { UserFacingError } from "@/lib/actions/action-result";
import { isBcryptHash } from "./account-status";
import type { AdminContext } from "./require-admin";

/** Nhập lại mật khẩu cho thao tác nhạy cảm. Hash chỉ tồn tại trong hàm này. */
export async function reauth(ctx: AdminContext, password: string | null | undefined): Promise<void> {
  if (!password) throw new UserFacingError("Cần nhập lại mật khẩu của bạn để xác nhận.");
  const row = await queryOne<{ password: string }>(webPool(), "SELECT password FROM user WHERE user_id = ?", [
    ctx.userId,
  ]);
  const ok = !!row && isBcryptHash(row.password) && (await bcrypt.compare(password, row.password));
  if (!ok) throw new UserFacingError("Mật khẩu xác nhận không đúng.");
}
