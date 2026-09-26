"use server";

import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { query } from "@/lib/db/query";
import { likeContains } from "@/lib/pagination";
import { REF_SOURCES, type RefKind } from "./ref-sources";

export interface RefOption {
  value: string;
  label: string;
}

/**
 * Server Action gọi trực tiếp từ ref-picker (client component). `kind` chỉ nhận 1 trong
 * các khoá của REF_SOURCES (allowlist cố định) → tên bảng/cột SQL không bao giờ từ input.
 */
export async function searchRefOptions(kind: RefKind, q: string): Promise<RefOption[]> {
  await requireAdmin();
  const src = REF_SOURCES[kind];
  if (!src) return [];

  const trimmed = q.trim().slice(0, 100);
  const isNumeric = /^\d+$/.test(trimmed);
  const idCol = `\`${src.pkCol}\``;
  const labelCol = `\`${src.labelCol}\``;
  const table = `\`${src.table}\``;

  if (!trimmed) {
    const rows = await query<{ id: number | string; label: string | null }>(
      gamePool(),
      `SELECT ${idCol} AS id, ${labelCol} AS label FROM ${table} ORDER BY ${idCol} LIMIT 20`,
    );
    return rows.map((r) => ({ value: String(r.id), label: `${r.id} — ${r.label ?? ""}` }));
  }

  const where = isNumeric ? `${labelCol} LIKE ? ESCAPE '\\\\' OR ${idCol} = ?` : `${labelCol} LIKE ? ESCAPE '\\\\'`;
  const params = isNumeric ? [likeContains(trimmed), Number(trimmed)] : [likeContains(trimmed)];
  const rows = await query<{ id: number | string; label: string | null }>(
    gamePool(),
    `SELECT ${idCol} AS id, ${labelCol} AS label FROM ${table} WHERE ${where} ORDER BY ${idCol} LIMIT 20`,
    params,
  );
  return rows.map((r) => ({ value: String(r.id), label: `${r.id} — ${r.label ?? ""}` }));
}
