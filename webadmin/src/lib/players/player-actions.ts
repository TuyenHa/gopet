"use server";

import { randomUUID } from "node:crypto";
import { z } from "zod";
import { revalidatePath } from "next/cache";
import { requireAdmin } from "@/lib/auth/require-admin";
import { requireSuperAdmin } from "@/lib/auth/require-superadmin";
import { reauth } from "@/lib/auth/reauth";
import { gamePool } from "@/lib/db/pools";
import { execute, queryOne } from "@/lib/db/query";
import { audited } from "@/lib/audit/write-audit-log";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { isValidInt32Amount } from "@/lib/accounts/int32-range";
import { executeGuardedUpdate, withOfflinePlayer } from "./offline-guard";
import { OPTIMISTIC_FIELD_NAMES, parseOptimisticFieldValue, type OptimisticField } from "./optimistic-field-parse";

// --- Tiền tệ: CHỈ delta, atomic ở SQL (server tự cộng khi bán ki ốt cho người offline) [RT#4]. ---
const CURRENCY_FIELDS = ["gold", "coin", "lua"] as const;
type CurrencyField = (typeof CURRENCY_FIELDS)[number];

const CURRENCY_SCHEMA = z.object({
  playerId: z.coerce.number().int().positive(),
  field: z.enum(CURRENCY_FIELDS),
  delta: z.coerce.number().int().refine(Number.isSafeInteger, "Số quá lớn"),
});

export async function adjustPlayerCurrencyAction(
  _prev: ActionResult<unknown> | null,
  form: FormData,
): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = CURRENCY_SCHEMA.parse(Object.fromEntries(form));
    const field: CurrencyField = input.field; // enum-checked — an toàn nội suy tên cột

    await audited(
      ctx,
      { action: "player.adjust_currency", target: `player:${input.playerId}`, detail: { field, delta: input.delta } },
      () =>
        withOfflinePlayer(input.playerId, async (conn, userId) => {
          const res = await executeGuardedUpdate(
            conn,
            `UPDATE player SET ${field} = ${field} + ? WHERE ID = ? AND user_id = ? AND ${field} + ? >= 0`,
            [input.delta, input.playerId, userId, input.delta],
          );
          if (res.affectedRows === 0) throw new UserFacingError("Không đổi: số dư sẽ âm hoặc không tìm thấy nhân vật.");
        }),
    );
    revalidatePath(`/players/${input.playerId}`);
    return { ok: true, message: "Đã cập nhật." };
  });
}

// --- Các cột khác: optimistic concurrency (WHERE ID=? AND col=<giá trị cũ>) [RT#4]. ---
const OPTIMISTIC_SCHEMA = z.object({
  playerId: z.coerce.number().int().positive(),
  field: z.enum(OPTIMISTIC_FIELD_NAMES),
  oldValue: z.string(),
  newValue: z.string().max(1000),
});

export async function updatePlayerFieldAction(
  _prev: ActionResult<unknown> | null,
  form: FormData,
): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = OPTIMISTIC_SCHEMA.parse(Object.fromEntries(form));
    const field: OptimisticField = input.field; // enum-checked — an toàn nội suy tên cột

    const parse = (v: string): string | number => {
      try {
        return parseOptimisticFieldValue(field, v);
      } catch (err) {
        throw new UserFacingError(err instanceof Error ? err.message : "Giá trị không hợp lệ.");
      }
    };
    const oldParam = parse(input.oldValue);
    const newParam = parse(input.newValue);

    await audited(
      ctx,
      { action: "player.update_field", target: `player:${input.playerId}`, detail: { field } },
      () =>
        withOfflinePlayer(input.playerId, async (conn, userId) => {
          const res = await executeGuardedUpdate(
            conn,
            `UPDATE player SET ${field} = ? WHERE ID = ? AND user_id = ? AND ${field} = ?`,
            [newParam, input.playerId, userId, oldParam],
          );
          if (res.affectedRows === 0) {
            throw new UserFacingError("Dữ liệu đã thay đổi từ nơi khác — tải lại trang rồi thử lại.");
          }
        }),
    );
    revalidatePath(`/players/${input.playerId}`);
    return { ok: true, message: "Đã lưu." };
  });
}

// --- isAdmin: chỉ super-admin, cần reauth, KHÔNG có hiệu lực ngay nếu đang online (RAM) [RT#15]. ---
const SET_ADMIN_SCHEMA = z.object({
  playerId: z.coerce.number().int().positive(),
  mode: z.enum(["grant", "revoke"]),
  confirmPassword: z.string().optional(),
});

export async function setPlayerAdminAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireSuperAdmin();
    const input = SET_ADMIN_SCHEMA.parse(Object.fromEntries(form));

    // user_id thật của playerId — tra ở server (H3), không tin field nào khác từ client.
    const target = await queryOne<{ user_id: number }>(gamePool(), "SELECT user_id FROM player WHERE ID = ?", [
      input.playerId,
    ]);
    if (!target) throw new UserFacingError("Không tìm thấy nhân vật này.");
    if (input.mode === "revoke" && target.user_id === ctx.userId) {
      throw new UserFacingError("Không thể tự thu quyền admin của chính mình.");
    }
    await reauth(ctx, input.confirmPassword);

    await audited(
      ctx,
      { action: `player.${input.mode}_admin`, target: `player:${input.playerId}` },
      () =>
        withOfflinePlayer(input.playerId, async (conn, userId) => {
          await executeGuardedUpdate(conn, "UPDATE player SET isAdmin = ? WHERE ID = ? AND user_id = ?", [
            input.mode === "grant" ? 1 : 0,
            input.playerId,
            userId,
          ]);
        }),
    );
    revalidatePath(`/players/${input.playerId}`);
    return {
      ok: true,
      message: "Đã lưu. Chỉ có hiệu lực từ lần đăng nhập sau (isAdmin nằm trong RAM khi đang online).",
    };
  });
}

// --- Tặng vàng qua hàng đợi `exchange_gold` — hiệu lực cả khi online, server tự cộng lúc login. ---
const GIFT_GOLD_SCHEMA = z.object({
  userId: z.coerce.number().int().positive(),
  amount: z.coerce.number().int().refine(isValidInt32Amount, "Số vàng phải trong khoảng 0..2147483647"),
});

export async function giftGoldAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = GIFT_GOLD_SCHEMA.parse(Object.fromEntries(form));
    const id = `webadmin-${randomUUID()}`;
    await audited(
      ctx,
      { action: "player.gift_gold", target: `user:${input.userId}`, detail: { amount: input.amount, id } },
      async () => {
        await execute(gamePool(), "INSERT INTO exchange_gold (id, user_id, gold) VALUES (?, ?, ?)", [
          id,
          input.userId,
          input.amount,
        ]);
      },
    );
    return { ok: true, message: "Đã xếp hàng tặng vàng — người chơi nhận khi đăng nhập lần sau." };
  });
}
