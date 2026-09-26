"use server";

import { z } from "zod";
import bcrypt from "bcryptjs";
import { requireAdmin } from "@/lib/auth/require-admin";
import { requireSuperAdmin } from "@/lib/auth/require-superadmin";
import { reauth } from "@/lib/auth/reauth";
import { webPool } from "@/lib/db/pools";
import { execute, queryOne } from "@/lib/db/query";
import { audited } from "@/lib/audit/write-audit-log";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { revalidatePath } from "next/cache";
import { assertCanTargetSensitive, isProtectedAdminAccount } from "./admin-account-guard";
import { computeBanUpdate, MAX_BAN_HOURS, type BanMode } from "./ban-calc";
import { INT32_MAX, isValidInt32Delta } from "./int32-range";
import { activateServerOffSwitch, deactivateServerOffSwitch } from "@/lib/players/server-off-switch";

const BAN_SCHEMA = z.object({
  userId: z.coerce.number().int().positive(),
  mode: z.enum(["temporary", "permanent", "unban"]),
  durationHours: z.coerce.number().positive().max(MAX_BAN_HOURS, `Thời hạn ban tối đa ${MAX_BAN_HOURS} giờ`).optional(),
  banReason: z.string().trim().max(1000).default(""),
});

/** Ban có hạn/vĩnh viễn/unban. Ban HOẶC gỡ ban tài khoản admin khác chỉ super-admin [RT#7]
 * (M2: unban trước đây bỏ qua guard này — 1 admin thường có thể tự gỡ ban admin khác mà
 * 1 super-admin vừa đặt). */
export async function banAccountAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = BAN_SCHEMA.parse(Object.fromEntries(form));
    await assertCanTargetSensitive(ctx, input.userId);
    const update = computeBanUpdate(input.mode as BanMode, input.durationHours);

    await audited(
      ctx,
      { action: "account.ban", target: `user:${input.userId}`, detail: { mode: input.mode, reason: input.banReason } },
      async () => {
        const res = await execute(
          webPool(),
          "UPDATE user SET isBaned = ?, banTime = ?, banReason = ? WHERE user_id = ?",
          [update.isBaned, update.banTime, input.banReason, input.userId],
        );
        if (res.affectedRows === 0) throw new UserFacingError("Không tìm thấy tài khoản.");
      },
    );
    revalidatePath(`/accounts/${input.userId}`);
    revalidatePath("/accounts");
    return { ok: true, message: "Đã cập nhật trạng thái ban. Người đang online chỉ bị chặn từ lần đăng nhập sau." };
  });
}

const TOGGLE_SCHEMA = z.object({ userId: z.coerce.number().int().positive() });

/** Khoá: chỉ áp dụng role=1 → 0. Từ chối role>1 (tài khoản admin) — không có đường mở khoá nào lỡ trả về role=3.
 * M2: role=1 nhưng có nhân vật isAdmin=1 vẫn là "tài khoản admin khác" theo RT#7 → cũng cần
 * `assertCanTargetSensitive` (chỉ check role>1 không đủ, admin thường có thể khoá 1 admin
 * khác chỉ vì role của họ chưa được nâng lên >1). */
export async function lockAccountAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const { userId } = TOGGLE_SCHEMA.parse(Object.fromEntries(form));
    const row = await queryOne<{ role: number }>(webPool(), "SELECT role FROM user WHERE user_id = ?", [userId]);
    if (!row) throw new UserFacingError("Không tìm thấy tài khoản.");
    if (row.role > 1) {
      throw new UserFacingError("Không thể khoá tài khoản admin qua đây (role>1). Dùng thu quyền/ban nếu cần.");
    }
    await assertCanTargetSensitive(ctx, userId);
    await audited(ctx, { action: "account.lock", target: `user:${userId}` }, async () => {
      await execute(webPool(), "UPDATE user SET role = 0 WHERE user_id = ? AND role = 1", [userId]);
    });
    revalidatePath(`/accounts/${userId}`);
    revalidatePath("/accounts");
    return { ok: true, message: "Đã khoá tài khoản." };
  });
}

/** Mở khoá: role=0 → 1 (luôn về 1, không bao giờ khôi phục role cũ >1 — vì không thể khoá role>1 từ đầu).
 * H4: CHỈ mở được tài khoản có hash bcrypt (`$2...`) — tài khoản bị migration khoá vì hash cũ
 * (`migration-260926-lock-legacy-password-accounts.sql`, không phải bcrypt) chỉ được mở lại
 * qua `resetPasswordAction` (buộc đặt mật khẩu mới), không cho mở nguyên trạng để tránh khôi
 * phục đăng nhập bằng plaintext cũ. */
export async function unlockAccountAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const { userId } = TOGGLE_SCHEMA.parse(Object.fromEntries(form));
    await audited(ctx, { action: "account.unlock", target: `user:${userId}` }, async () => {
      const res = await execute(
        webPool(),
        "UPDATE user SET role = 1 WHERE user_id = ? AND role = 0 AND password LIKE '$2%'",
        [userId],
      );
      if (res.affectedRows === 0) {
        throw new UserFacingError(
          "Không thể mở khoá: tài khoản không ở trạng thái khoá (role=0), hoặc đang dùng mật khẩu cũ " +
            "(không phải bcrypt) — dùng \"Đặt lại mật khẩu\" để mở lại loại tài khoản này.",
        );
      }
    });
    revalidatePath(`/accounts/${userId}`);
    revalidatePath("/accounts");
    return { ok: true, message: "Đã mở khoá tài khoản (role=1)." };
  });
}

const RESET_PW_SCHEMA = z.object({ userId: z.coerce.number().int().positive(), confirmPassword: z.string().optional() });

function generateStrongPassword(): string {
  const bytes = new Uint8Array(18);
  crypto.getRandomValues(bytes);
  return Buffer.from(bytes).toString("base64url");
}

/** Buộc reset mật khẩu (bcrypt cost 12). Mật khẩu mới CHỈ trả về 1 lần trong response, không vào audit. */
export async function resetPasswordAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<{ newPassword: string }>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = RESET_PW_SCHEMA.parse(Object.fromEntries(form));
    const isTarget = await isProtectedAdminAccount(input.userId);
    if (isTarget && !ctx.isSuperAdmin) throw new UserFacingError("Chỉ super-admin được reset mật khẩu tài khoản admin khác.");
    if (isTarget) await reauth(ctx, input.confirmPassword);

    const newPassword = generateStrongPassword();
    const hash = await bcrypt.hash(newPassword, 12);
    // Không đưa `newPassword`/hash vào detail audit — sanitize() cũng chặn theo tên khoá nhưng không dựa vào đó.
    await audited(ctx, { action: "account.reset_password", target: `user:${input.userId}` }, async () => {
      // Tài khoản bị migration khoá vì hash cũ (role=0 + hash không phải bcrypt) → mở lại role=1.
      // Khoá thủ công (hash bcrypt) giữ nguyên role. `role` đứng TRƯỚC `password` vì MariaDB gán
      // SET từ trái sang phải — phải đọc hash cũ trước khi bị ghi đè.
      const res = await execute(
        webPool(),
        "UPDATE user SET role = IF(role = 0 AND password NOT LIKE '$2%', 1, role), password = ? WHERE user_id = ?",
        [hash, input.userId],
      );
      if (res.affectedRows === 0) throw new UserFacingError("Không tìm thấy tài khoản.");
    });
    revalidatePath(`/accounts/${input.userId}`);
    return { ok: true, message: "Đã đặt lại mật khẩu.", data: { newPassword } };
  });
}

const CLEAR_2FA_SCHEMA = z.object({ userId: z.coerce.number().int().positive(), confirmPassword: z.string().optional() });

export async function clear2faAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = CLEAR_2FA_SCHEMA.parse(Object.fromEntries(form));
    const isTarget = await isProtectedAdminAccount(input.userId);
    if (isTarget && !ctx.isSuperAdmin) throw new UserFacingError("Chỉ super-admin được xoá 2FA tài khoản admin khác.");
    if (isTarget) await reauth(ctx, input.confirmPassword);

    await audited(ctx, { action: "account.clear_2fa", target: `user:${input.userId}` }, async () => {
      await execute(webPool(), "UPDATE user SET secretKey = NULL WHERE user_id = ?", [input.userId]);
    });
    revalidatePath(`/accounts/${input.userId}`);
    return { ok: true, message: "Đã xoá 2FA." };
  });
}

const COIN_SCHEMA = z.object({
  userId: z.coerce.number().int().positive(),
  delta: z.coerce.number().int().refine(isValidInt32Delta, `|delta| phải ≤ ${INT32_MAX}`),
});

/** Cộng/trừ `user.coin` (int32) nguyên tử — không vượt [0, 2^31-1]. */
export async function adjustCoinAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = COIN_SCHEMA.parse(Object.fromEntries(form));
    await audited(ctx, { action: "account.adjust_coin", target: `user:${input.userId}`, detail: { delta: input.delta } }, async () => {
      const res = await execute(
        webPool(),
        "UPDATE user SET coin = coin + ? WHERE user_id = ? AND coin + ? BETWEEN 0 AND 2147483647",
        [input.delta, input.userId, input.delta],
      );
      if (res.affectedRows === 0) throw new UserFacingError("Không đổi: vượt giới hạn ngọc hoặc tài khoản không tồn tại.");
    });
    revalidatePath(`/accounts/${input.userId}`);
    return { ok: true, message: "Đã cập nhật ngọc." };
  });
}

const SWITCH_SCHEMA = z.object({ confirmPassword: z.string().optional() });

/** Công tắc "server đã TẮT" — super-admin, reauth, audit [RT#2]. */
export async function activateServerOffSwitchAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireSuperAdmin();
    const input = SWITCH_SCHEMA.parse(Object.fromEntries(form));
    await reauth(ctx, input.confirmPassword);
    await audited(ctx, { action: "server_off_switch.activate" }, async () => activateServerOffSwitch());
    revalidatePath("/accounts");
    revalidatePath("/players");
    return { ok: true, message: "Đã bật công tắc: coi máy chủ game là đã TẮT (2 giờ)." };
  });
}

export async function deactivateServerOffSwitchAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireSuperAdmin();
    const input = SWITCH_SCHEMA.parse(Object.fromEntries(form));
    await reauth(ctx, input.confirmPassword);
    await audited(ctx, { action: "server_off_switch.deactivate" }, async () => deactivateServerOffSwitch());
    revalidatePath("/accounts");
    revalidatePath("/players");
    return { ok: true, message: "Đã tắt công tắc." };
  });
}
