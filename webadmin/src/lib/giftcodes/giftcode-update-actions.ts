"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { requireAdmin } from "@/lib/auth/require-admin";
import { reauth } from "@/lib/auth/reauth";
import { audited } from "@/lib/audit/write-audit-log";
import { gamePool } from "@/lib/db/pools";
import { execute } from "@/lib/db/query";
import { withNamedLock } from "@/lib/db/named-lock";
import { vnInputToDbDateTime } from "@/lib/time/db-time";
import { giftDataSchema, giftDataToJson } from "./gift-data-schema";
import { getGiftcodeById } from "./giftcode-queries";
import { isDuplicateKeyError } from "./giftcode-code-generator";

const CODE_RE = /^[a-zA-Z0-9]{4,100}$/;
const LOCK_TIMEOUT_SEC = 10; // khớp GET_LOCK(..., 10) server dùng (MenuController.inputDialog.cs:84)

/** `gift_code.code` là `utf8_unicode_ci` (lookup không phân biệt hoa/thường) nhưng GET_LOCK
 * lại phân biệt hoa/thường — phải lowercase tên khoá cả 2 phía để cùng khoá đúng 1 tên
 * (H5, khớp `"gift_code_lock_" + code.ToLowerInvariant()` phía GServer MenuController.inputDialog.cs). */
function lockName(code: string): string {
  return `gift_code_lock_${code.toLowerCase()}`;
}

async function loadOrThrow(id: number) {
  const row = await getGiftcodeById(id);
  if (!row) throw new UserFacingError("Không tìm thấy giftcode.");
  return row;
}

const UpdateSchema = z.object({
  id: z.coerce.number().int().positive(),
  code: z.string().trim().regex(CODE_RE, "Code chỉ gồm chữ/số, 4-100 ký tự"),
  maxUser: z.coerce.number().int().positive().max(1_000_000),
  expire: z.string().min(1, "Cần nhập hạn sử dụng"),
  giftData: z.string().min(1),
});

/** Sửa giftcode: đổi code (nếu khác), maxUser, hạn, quà. Khoá theo TÊN CŨ trong suốt thao
 * tác (RT#11) — nếu server đang xử lý người chơi đổi code này thì admin đợi tới khi xong,
 * và ngược lại, không ai ghi đè mất lượt dùng của người kia. */
export async function updateGiftcodeAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = UpdateSchema.parse(Object.fromEntries(form));
    const isClanCode = form.get("isClanCode") === "on";

    let entries: z.infer<typeof giftDataSchema>;
    try {
      entries = giftDataSchema.parse(JSON.parse(input.giftData));
    } catch {
      throw new UserFacingError("Danh sách quà không hợp lệ.");
    }

    const current = await loadOrThrow(input.id);
    const dbExpire = vnInputToDbDateTime(input.expire);
    let giftDataJson: string;
    try {
      giftDataJson = giftDataToJson(entries);
    } catch (err) {
      throw new UserFacingError(err instanceof Error ? err.message : "Danh sách quà không hợp lệ.");
    }

    await audited(
      ctx,
      { action: "giftcode.update", target: `giftcode:${current.code}`, detail: { newCode: input.code, maxUser: input.maxUser, expire: input.expire, isClanCode } },
      () =>
        withNamedLock(gamePool(), lockName(current.code), LOCK_TIMEOUT_SEC, async (conn) => {
          try {
            await execute(
              conn,
              "UPDATE gift_code SET code = ?, maxUser = ?, expire = ?, isClanCode = ?, gift_data = ? WHERE id = ?",
              [input.code, input.maxUser, dbExpire, isClanCode ? 1 : 0, giftDataJson, input.id],
            );
          } catch (err) {
            if (isDuplicateKeyError(err)) throw new UserFacingError("Mã code mới đã tồn tại, chọn mã khác.");
            throw err;
          }
        }),
    );

    revalidatePath("/giftcodes");
    revalidatePath(`/giftcodes/${input.id}`);
    return { ok: true, message: "Đã lưu giftcode." };
  });
}

/** Reset lượt dùng: xoá `usersOfUseThis`, đưa `currentUser` về 0. Cùng khoá với update/redeem. */
export async function resetGiftcodeUsesAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const id = z.coerce.number().int().positive().parse(form.get("id"));
    const current = await loadOrThrow(id);

    await audited(ctx, { action: "giftcode.reset_uses", target: `giftcode:${current.code}` }, () =>
      withNamedLock(gamePool(), lockName(current.code), LOCK_TIMEOUT_SEC, (conn) =>
        execute(conn, "UPDATE gift_code SET currentUser = 0, usersOfUseThis = '[]' WHERE id = ?", [id]),
      ),
    );

    revalidatePath("/giftcodes");
    revalidatePath(`/giftcodes/${id}`);
    return { ok: true, message: "Đã reset lượt dùng." };
  });
}

/** Xoá giftcode. Thao tác nguy hiểm nhất trong domain này → bắt nhập lại mật khẩu. */
export async function deleteGiftcodeAction(form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const id = z.coerce.number().int().positive().parse(form.get("id"));
    await reauth(ctx, form.get("confirmPassword") as string | null);
    const current = await loadOrThrow(id);

    await audited(ctx, { action: "giftcode.delete", target: `giftcode:${current.code}` }, () =>
      withNamedLock(gamePool(), lockName(current.code), LOCK_TIMEOUT_SEC, (conn) => execute(conn, "DELETE FROM gift_code WHERE id = ?", [id])),
    );

    revalidatePath("/giftcodes");
    return { ok: true, message: "Đã xoá giftcode." };
  });
}
