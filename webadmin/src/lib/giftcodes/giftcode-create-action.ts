"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { requireAdmin } from "@/lib/auth/require-admin";
import { audited } from "@/lib/audit/write-audit-log";
import { gamePool } from "@/lib/db/pools";
import { execute } from "@/lib/db/query";
import { vnInputToDbDateTime } from "@/lib/time/db-time";
import { insertSystemLetter, resolvePlayerTarget } from "@/lib/letters/send-system-letter";
import { giftDataSchema, giftDataToJson } from "./gift-data-schema";
import { generateUniqueGiftCode, isDuplicateKeyError } from "./giftcode-code-generator";

const LETTER_TYPE_ADMIN = 2; // Letter.ADMIN (Data/User/Letter.cs)
const CODE_RE = /^[a-zA-Z0-9]{4,100}$/; // khớp regex server chấp nhận khi đổi code (MenuController.inputDialog.cs:76)

const CreateSchema = z.object({
  codeMode: z.enum(["manual", "random"]),
  code: z.string().trim().optional().default(""),
  maxUser: z.coerce.number().int().positive().max(1_000_000),
  expire: z.string().min(1, "Cần nhập hạn sử dụng"),
  giftData: z.string().min(1),
  forUserId: z.coerce.number().int().positive().optional(),
});

/** Server Action tạo giftcode mới. Hỗ trợ `?forUserId=` (phase 5B): mã riêng cho 1 người,
 * kèm tuỳ chọn gửi thư chứa mã. Insert không cần khoá `gift_code_lock_` — code chưa tồn tại
 * nên không có ai (kể cả server) đang thao tác cùng tên đó. */
export async function createGiftcodeAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<{ code: string }>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = CreateSchema.parse({
      codeMode: form.get("codeMode"),
      code: form.get("code"),
      maxUser: form.get("maxUser"),
      expire: form.get("expire"),
      giftData: form.get("giftData"),
      forUserId: form.get("forUserId") || undefined,
    });
    const isClanCode = form.get("isClanCode") === "on";
    const sendLetter = form.get("sendLetter") === "on";

    let entries: z.infer<typeof giftDataSchema>;
    try {
      entries = giftDataSchema.parse(JSON.parse(input.giftData));
    } catch {
      throw new UserFacingError("Danh sách quà không hợp lệ.");
    }

    let code: string;
    if (input.codeMode === "random") {
      code = await generateUniqueGiftCode();
    } else {
      if (!CODE_RE.test(input.code)) throw new UserFacingError("Code chỉ gồm chữ/số, 4-100 ký tự.");
      code = input.code;
    }

    const dbExpire = vnInputToDbDateTime(input.expire);
    let giftDataJson: string;
    try {
      giftDataJson = giftDataToJson(entries);
    } catch (err) {
      throw new UserFacingError(err instanceof Error ? err.message : "Danh sách quà không hợp lệ.");
    }

    await audited(
      ctx,
      { action: "giftcode.create", target: `giftcode:${code}`, detail: { maxUser: input.maxUser, expire: input.expire, isClanCode, forUserId: input.forUserId } },
      async () => {
        try {
          await execute(
            gamePool(),
            "INSERT INTO gift_code (code, currentUser, maxUser, gift_data, expire, usersOfUseThis, isClanCode) VALUES (?,0,?,?,?,'[]',?)",
            [code, input.maxUser, giftDataJson, dbExpire, isClanCode ? 1 : 0],
          );
        } catch (err) {
          if (isDuplicateKeyError(err)) throw new UserFacingError("Mã code đã tồn tại, chọn mã khác.");
          throw err;
        }

        if (input.forUserId && sendLetter) {
          const target = await resolvePlayerTarget(String(input.forUserId));
          if (target) {
            await insertSystemLetter(
              target,
              LETTER_TYPE_ADMIN,
              "Quà tặng riêng dành cho bạn",
              `Bạn nhận được giftcode: ${code}`,
              `Ban quản trị gửi tặng bạn giftcode riêng: ${code}\n\nVào game, mở menu Giftcode và nhập mã trên để nhận quà (không phân biệt hoa/thường).`,
            );
          }
        }
      },
    );

    revalidatePath("/giftcodes");
    return { ok: true, message: `Đã tạo giftcode ${code}`, data: { code } };
  });
}
