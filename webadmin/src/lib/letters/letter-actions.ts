"use server";

import { revalidatePath } from "next/cache";
import { z } from "zod";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { requireAdmin } from "@/lib/auth/require-admin";
import { reauth } from "@/lib/auth/reauth";
import { audited } from "@/lib/audit/write-audit-log";
import { gamePool } from "@/lib/db/pools";
import { execute } from "@/lib/db/query";
import { insertSystemLetter, resolvePlayerTarget } from "./send-system-letter";

const LETTER_TYPES = [2, 3] as const; // Letter.ADMIN=2, Letter.EVENT=3 (Data/User/Letter.cs)
const letterTypeSchema = z.coerce.number().refine((v): v is 2 | 3 => (LETTER_TYPES as readonly number[]).includes(v), {
  message: "Loại thư không hợp lệ (chỉ 2=admin hoặc 3=sự kiện)",
});

const SendOneSchema = z.object({
  target: z.string().trim().min(1, "Cần nhập username / tên nhân vật / user_id"),
  type: letterTypeSchema,
  title: z.string().trim().min(1, "Cần nhập tiêu đề").max(1000),
  shortContent: z.string().trim().min(1, "Cần nhập nội dung ngắn").max(1000),
  content: z.string().trim().min(1, "Cần nhập nội dung"),
});

/** Gửi 1 người: khoá `login_lock_<username>` bọc quanh INSERT (trong `insertSystemLetter`)
 * để không chèn đúng lúc server đang nạp+xoá hàng đợi lúc người đó login (RT#12). */
export async function sendLetterToOneAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = SendOneSchema.parse(Object.fromEntries(form));
    const target = await resolvePlayerTarget(input.target);
    if (!target) throw new UserFacingError(`Không tìm thấy người chơi "${input.target}".`);

    await audited(
      ctx,
      { action: "letter.send_one", target: `user:${target.userId}`, detail: { type: input.type, title: input.title } },
      () => insertSystemLetter(target, input.type, input.title, input.shortContent, input.content),
    );

    revalidatePath("/letters");
    return { ok: true, message: `Đã gửi thư cho ${target.playerName || target.username} (user_id ${target.userId}).` };
  });
}

const SendAllSchema = z.object({
  type: letterTypeSchema,
  title: z.string().trim().min(1, "Cần nhập tiêu đề").max(1000),
  shortContent: z.string().trim().min(1, "Cần nhập nội dung ngắn").max(1000),
  content: z.string().trim().min(1, "Cần nhập nội dung"),
});

/**
 * Gửi TOÀN BỘ người chơi bằng 1 câu `INSERT ... SELECT user_id FROM player`. Rủi ro còn lại
 * (đã cảnh báo trên UI, xem `send-all-letter-form.tsx`): người đang login ĐÚNG khoảnh khắc
 * chèn có thể mất thư vì không transaction với `Player.cs:495-502` — không khoá được vì
 * không biết trước tập user_id nào sẽ login. Thao tác ảnh hưởng toàn server → bắt nhập lại
 * mật khẩu như thao tác xoá giftcode.
 */
export async function sendLetterToAllAction(form: FormData): Promise<ActionResult<{ count: number }>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    await reauth(ctx, form.get("confirmPassword") as string | null);
    const input = SendAllSchema.parse(Object.fromEntries(form));

    const result = await audited(ctx, { action: "letter.send_all", detail: { type: input.type, title: input.title } }, () =>
      execute(
        gamePool(),
        "INSERT INTO letter (userId, targetId, time, Type, Title, ShortContent, Content) SELECT 0, user_id, NOW(), ?, ?, ?, ? FROM player",
        [input.type, input.title, input.shortContent, input.content],
      ),
    );

    revalidatePath("/letters");
    return { ok: true, message: `Đã gửi cho ${result.affectedRows} nhân vật.`, data: { count: result.affectedRows } };
  });
}
