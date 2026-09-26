"use server";

import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { query } from "@/lib/db/query";
import { likeContains } from "@/lib/pagination";

export interface SearchHit {
  id: number;
  name: string;
}

/** Tìm nhanh vật phẩm theo tên/id cho ô chọn trong gift-data-builder (client component gọi
 * trực tiếp Server Action này, không qua route API riêng). */
export async function searchItems(q: string): Promise<SearchHit[]> {
  await requireAdmin();
  const term = q.trim().slice(0, 100);
  if (!term) return [];
  const byId = /^\d+$/.test(term);
  const rows = await query<{ itemId: number; name: string }>(
    gamePool(),
    byId
      ? "SELECT itemId, name FROM item WHERE itemId = ? LIMIT 20"
      : "SELECT itemId, name FROM item WHERE name LIKE ? ESCAPE '\\\\' ORDER BY itemId LIMIT 20",
    byId ? [Number(term)] : [likeContains(term)],
  );
  return rows.map((r) => ({ id: r.itemId, name: r.name }));
}

export async function searchPets(q: string): Promise<SearchHit[]> {
  await requireAdmin();
  const term = q.trim().slice(0, 100);
  if (!term) return [];
  const byId = /^\d+$/.test(term);
  const rows = await query<{ petId: number; name: string | null }>(
    gamePool(),
    byId
      ? "SELECT petId, name FROM gopet_pet WHERE petId = ? LIMIT 20"
      : "SELECT petId, name FROM gopet_pet WHERE name LIKE ? ESCAPE '\\\\' ORDER BY petId LIMIT 20",
    byId ? [Number(term)] : [likeContains(term)],
  );
  return rows.map((r) => ({ id: r.petId, name: r.name ?? `Pet #${r.petId}` }));
}
