"use server";

import type { Pool, PoolConnection } from "mysql2/promise";
import { z } from "zod";
import { revalidatePath } from "next/cache";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { audited } from "@/lib/audit/write-audit-log";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { executeGuardedUpdate, withOfflinePlayer } from "./offline-guard";
import { INT32_MAX } from "@/lib/accounts/int32-range";
import {
  applyPetEdit,
  parsePetList,
  parseSinglePet,
  sortPetList,
  stringifyPetList,
  stringifySinglePet,
  type PetSource,
} from "@/lib/game-json/pet-json";

/**
 * Sửa "chỉ số cơ bản" của 1 pet có sẵn (KHÔNG tạo pet mới, không sửa equip/tatto/skill).
 * Pet có thể nằm ở 1 trong 3 cột `pets`/`petSelected`/`PetDefLeague` (xem bất biến trong
 * `@/lib/game-json/pet-json.ts`) — client phải gửi kèm `source` xác định đúng cột, vì mỗi
 * cột ghi optimistic bằng MD5 riêng của chính nó.
 */

const MD5_HEX = /^[0-9a-f]{32}$/i;
const SOURCES = ["pets", "petSelected", "PetDefLeague"] as const;

const PET_EDIT_SCHEMA = z
  .object({
    playerId: z.coerce.number().int().positive(),
    source: z.enum(SOURCES),
    petId: z.coerce.number().int(),
    expectedMd5: z.string().regex(MD5_HEX),
    name: z.string().max(64).optional(),
    lvl: z.coerce.number().int().min(1).max(INT32_MAX),
    star: z.coerce.number().int().min(0).max(5),
    // exp là `long` phía server (không phải int) — không cần chặn INT32_MAX (H6).
    exp: z.coerce.number().int().min(0),
    str: z.coerce.number().int().min(0).max(INT32_MAX),
    agi: z.coerce.number().int().min(0).max(INT32_MAX),
    intStat: z.coerce.number().int().min(0).max(INT32_MAX),
    hp: z.coerce.number().int().min(0).max(INT32_MAX),
    mp: z.coerce.number().int().min(0).max(INT32_MAX),
    maxHp: z.coerce.number().int().min(1).max(INT32_MAX),
    maxMp: z.coerce.number().int().min(1).max(INT32_MAX),
    tiemnang_point: z.coerce.number().int().min(0).max(INT32_MAX),
    skillPoint: z.coerce.number().int().min(0).max(INT32_MAX),
  })
  .refine((v) => v.hp <= v.maxHp, { message: "hp không được lớn hơn maxHp", path: ["hp"] })
  .refine((v) => v.mp <= v.maxMp, { message: "mp không được lớn hơn maxMp", path: ["mp"] });

async function readColumn(conn: Pool | PoolConnection, playerId: number, source: PetSource) {
  // `source` chỉ nhận 1 trong 3 giá trị cố định (SOURCES) — không phải input tự do, an toàn để nội suy tên cột.
  return queryOne<{ val: string | null; md5: string | null }>(
    conn,
    `SELECT ${source} AS val, MD5(${source}) AS md5 FROM player WHERE ID = ?`,
    [playerId],
  );
}

export async function updatePetFieldsAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const raw = Object.fromEntries(form);
    // "_int" không phải tên field FormData hợp lệ để dùng làm identifier JS thoải mái — map riêng.
    const input = PET_EDIT_SCHEMA.parse({ ...raw, intStat: raw._int });

    const before = await readColumn(gamePool(), input.playerId, input.source);
    if (!before?.val) throw new UserFacingError("Không tìm thấy pet ở cột này.");

    await audited(
      ctx,
      {
        action: "player.pet.update",
        target: `player:${input.playerId}`,
        detail: { petId: input.petId, source: input.source, before: before.val },
      },
      () =>
        withOfflinePlayer(input.playerId, async (conn, userId) => {
          const row = await readColumn(conn, input.playerId, input.source);
          if (!row?.val) throw new UserFacingError("Không tìm thấy pet ở cột này.");
          if (row.md5 !== input.expectedMd5) {
            throw new UserFacingError("Dữ liệu pet đã thay đổi từ nơi khác — tải lại trang rồi thử lại.");
          }

          const patch = {
            name: input.name && input.name.length > 0 ? input.name : null,
            lvl: input.lvl,
            star: input.star,
            exp: input.exp,
            str: input.str,
            agi: input.agi,
            _int: input.intStat,
            hp: input.hp,
            mp: input.mp,
            maxHp: input.maxHp,
            maxMp: input.maxMp,
            tiemnang_point: input.tiemnang_point,
            skillPoint: input.skillPoint,
          };

          let newText: string;
          if (input.source === "pets") {
            const list = parsePetList(row.val);
            const pet = list.find((p) => Number(p.petId) === input.petId);
            if (!pet) throw new UserFacingError("Không tìm thấy pet trong danh sách (có thể đã chuyển cột khác).");
            applyPetEdit(pet, patch);
            sortPetList(list);
            newText = stringifyPetList(list);
          } else {
            const pet = parseSinglePet(row.val);
            if (!pet || Number(pet.petId) !== input.petId) {
              throw new UserFacingError("Pet ở cột này đã đổi (có thể admin khác vừa sửa) — tải lại trang rồi thử lại.");
            }
            applyPetEdit(pet, patch);
            newText = stringifySinglePet(pet);
          }

          const res = await executeGuardedUpdate(
            conn,
            `UPDATE player SET ${input.source} = ? WHERE ID = ? AND user_id = ? AND MD5(${input.source}) = ?`,
            [newText, input.playerId, userId, input.expectedMd5],
          );
          if (res.affectedRows === 0) throw new UserFacingError("Ghi thất bại — dữ liệu vừa đổi từ nơi khác, thử lại.");
        }),
    );
    revalidatePath(`/players/${input.playerId}`);
    return { ok: true, message: "Đã lưu pet." };
  });
}
