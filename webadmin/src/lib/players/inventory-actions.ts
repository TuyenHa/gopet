"use server";

import type { PoolConnection } from "mysql2/promise";
import { z } from "zod";
import { revalidatePath } from "next/cache";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { audited } from "@/lib/audit/write-audit-log";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { INT32_MAX } from "@/lib/accounts/int32-range";
import { executeGuardedUpdate, withOfflinePlayer } from "./offline-guard";
import {
  applyItemEdit,
  deleteItem,
  findItem,
  itemPetEquipId,
  parsePlayerItems,
  sortAllInvTypes,
  stringifyPlayerItems,
} from "@/lib/game-json/item-json";
import { parsePetList, parseSinglePet, removeFromEquip, sortPetList, stringifyPetList, stringifySinglePet } from "@/lib/game-json/pet-json";

/**
 * Sửa/xoá vật phẩm có sẵn trong `player.items` (JSON). KHÔNG tạo item mới (xem bất biến
 * trong `@/lib/game-json/item-json.ts`) — tặng đồ đi qua giftcode/thư (phase 8).
 * Optimistic concurrency: client gửi kèm `expectedMd5` (đọc từ `MD5(items)` lúc tải tab),
 * ghi bằng `UPDATE ... WHERE ID=? AND MD5(items)=?` — 0 dòng ảnh hưởng = dữ liệu đã đổi.
 */

const MD5_HEX = /^[0-9a-f]{32}$/i;

const ITEM_EDIT_SCHEMA = z.object({
  playerId: z.coerce.number().int().positive(),
  itemId: z.coerce.number().int(),
  expectedMd5: z.string().regex(MD5_HEX),
  count: z.coerce.number().int().min(0).max(INT32_MAX),
  lvl: z.coerce.number().int().min(0).max(INT32_MAX),
  expire: z.coerce.number().int().refine((v) => v === -1 || v >= 0, "expire phải là -1 (không hết hạn) hoặc mốc thời gian (ms) >= 0"),
  canTrade: z.enum(["true", "false"]).transform((v) => v === "true"),
  // Rỗng = item chưa từng có durability (không phải trang bị pet) → không gửi field này.
  durability: z
    .union([z.literal(""), z.coerce.number().int().min(0).max(80)])
    .optional()
    .transform((v) => (v === "" || v === undefined ? undefined : v)),
});

async function loadItemsForWrite(conn: PoolConnection, playerId: number, expectedMd5: string) {
  const row = await queryOne<{ items: string | null; md5: string | null }>(
    conn,
    "SELECT items, MD5(items) AS md5 FROM player WHERE ID = ?",
    [playerId],
  );
  if (!row?.items) throw new UserFacingError("Không tìm thấy hành trang của nhân vật này.");
  if (row.md5 !== expectedMd5) {
    throw new UserFacingError("Hành trang đã thay đổi từ nơi khác — tải lại trang rồi thử lại.");
  }
  return parsePlayerItems(row.items);
}

export async function updateItemFieldsAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = ITEM_EDIT_SCHEMA.parse(Object.fromEntries(form));

    // Đọc trước (không khoá) chỉ để lưu bản cũ vào audit — điểm chốt an toàn thật là
    // `WHERE MD5(items)=?` bên trong withOfflinePlayer.
    const before = await queryOne<{ items: string | null }>(gamePool(), "SELECT items FROM player WHERE ID = ?", [input.playerId]);
    if (!before?.items) throw new UserFacingError("Không tìm thấy hành trang của nhân vật này.");

    await audited(
      ctx,
      { action: "player.item.update", target: `player:${input.playerId}`, detail: { itemId: input.itemId, before: before.items } },
      () =>
        withOfflinePlayer(input.playerId, async (conn, userId) => {
          const tree = await loadItemsForWrite(conn, input.playerId, input.expectedMd5);
          const found = findItem(tree, input.itemId);
          if (!found) throw new UserFacingError("Không tìm thấy vật phẩm (có thể đã bị xoá).");

          applyItemEdit(found.item, {
            count: input.count,
            lvl: input.lvl,
            expire: input.expire,
            canTrade: input.canTrade,
            durability: input.durability,
          });
          sortAllInvTypes(tree);

          const res = await executeGuardedUpdate(
            conn,
            "UPDATE player SET items = ? WHERE ID = ? AND user_id = ? AND MD5(items) = ?",
            [stringifyPlayerItems(tree), input.playerId, userId, input.expectedMd5],
          );
          if (res.affectedRows === 0) throw new UserFacingError("Ghi thất bại — dữ liệu vừa đổi từ nơi khác, thử lại.");
        }),
    );
    revalidatePath(`/players/${input.playerId}`);
    return { ok: true, message: "Đã lưu vật phẩm." };
  });
}

const ITEM_DELETE_SCHEMA = z.object({
  playerId: z.coerce.number().int().positive(),
  itemId: z.coerce.number().int(),
  expectedMd5: z.string().regex(MD5_HEX),
});

/** Tìm pet đang đeo `itemId` trên 3 nguồn pets/petSelected/PetDefLeague rồi gỡ khỏi `equip`.
 * Ném lỗi khi UPDATE ghi 0 dòng dù đã tìm thấy thay đổi cần lưu (M8) — caller (trong
 * transaction) sẽ rollback thay vì âm thầm để lại tham chiếu equip lệch. */
async function clearEquipReference(
  conn: PoolConnection,
  playerId: number,
  userId: number,
  petId: number,
  itemId: number,
): Promise<void> {
  const petsRow = await queryOne<{ pets: string | null; md5: string | null }>(
    conn,
    "SELECT pets, MD5(pets) AS md5 FROM player WHERE ID = ?",
    [playerId],
  );
  if (petsRow?.pets) {
    const list = parsePetList(petsRow.pets);
    const pet = list.find((p) => Number(p.petId) === petId);
    if (pet) {
      if (removeFromEquip(pet, itemId)) {
        sortPetList(list);
        const res = await executeGuardedUpdate(
          conn,
          "UPDATE player SET pets = ? WHERE ID = ? AND user_id = ? AND MD5(pets) = ?",
          [stringifyPetList(list), playerId, userId, petsRow.md5],
        );
        if (res.affectedRows === 0) throw new UserFacingError("Ghi thất bại khi gỡ trang bị pet — dữ liệu vừa đổi từ nơi khác, thử lại.");
      }
      return;
    }
  }

  const singleColumns = ["petSelected", "PetDefLeague"] as const;
  for (const col of singleColumns) {
    // col chỉ nhận 2 giá trị cố định ở trên (không phải input người dùng) — an toàn để nội suy tên cột.
    const row = await queryOne<{ val: string | null; md5: string | null }>(
      conn,
      `SELECT ${col} AS val, MD5(${col}) AS md5 FROM player WHERE ID = ?`,
      [playerId],
    );
    const pet = parseSinglePet(row?.val ?? null);
    if (pet && Number(pet.petId) === petId) {
      if (removeFromEquip(pet, itemId) && row?.md5) {
        const res = await executeGuardedUpdate(
          conn,
          `UPDATE player SET ${col} = ? WHERE ID = ? AND user_id = ? AND MD5(${col}) = ?`,
          [stringifySinglePet(pet), playerId, userId, row.md5],
        );
        if (res.affectedRows === 0) throw new UserFacingError("Ghi thất bại khi gỡ trang bị pet — dữ liệu vừa đổi từ nơi khác, thử lại.");
      }
      return;
    }
  }
  // Không tìm thấy pet sở hữu (dữ liệu có thể đã lệch từ trước) — vẫn tiếp tục xoá item (best-effort).
}

export async function deleteItemAction(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = ITEM_DELETE_SCHEMA.parse(Object.fromEntries(form));

    const before = await queryOne<{ items: string | null }>(gamePool(), "SELECT items FROM player WHERE ID = ?", [input.playerId]);
    if (!before?.items) throw new UserFacingError("Không tìm thấy hành trang của nhân vật này.");

    await audited(
      ctx,
      { action: "player.item.delete", target: `player:${input.playerId}`, detail: { itemId: input.itemId, before: before.items } },
      () =>
        withOfflinePlayer(input.playerId, async (conn, userId) => {
          const tree = await loadItemsForWrite(conn, input.playerId, input.expectedMd5);
          const removed = deleteItem(tree, input.itemId);
          if (!removed) throw new UserFacingError("Không tìm thấy vật phẩm (có thể đã bị xoá).");

          await conn.beginTransaction();
          try {
            const res = await executeGuardedUpdate(
              conn,
              "UPDATE player SET items = ? WHERE ID = ? AND user_id = ? AND MD5(items) = ?",
              [stringifyPlayerItems(tree), input.playerId, userId, input.expectedMd5],
            );
            if (res.affectedRows === 0) throw new UserFacingError("Ghi thất bại — dữ liệu vừa đổi từ nơi khác, thử lại.");

            const petEuipId = itemPetEquipId(removed);
            if (petEuipId !== null && petEuipId !== -1) {
              await clearEquipReference(conn, input.playerId, userId, petEuipId, input.itemId);
            }
            await conn.commit();
          } catch (err) {
            await conn.rollback();
            throw err;
          }
        }),
    );
    revalidatePath(`/players/${input.playerId}`);
    return { ok: true, message: "Đã xoá vật phẩm." };
  });
}
