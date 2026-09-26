import type { TableConfig } from "./table-registry";

export type PkValues = Record<string, string | number>;

/** Mã hoá giá trị PK của 1 dòng thành JSON dùng trong `?pk=`. */
export function encodePk(config: TableConfig, row: Record<string, unknown>): string {
  const obj: PkValues = {};
  for (const p of config.pk) {
    const v = row[p];
    obj[p] = typeof v === "number" ? v : String(v ?? "");
  }
  return JSON.stringify(obj);
}

/**
 * Giải mã `?pk=` — kiểm đúng bộ khoá của registry (không thừa/thiếu cột), giá trị chỉ
 * nhận string/number (không object/array/null) để dùng an toàn làm tham số SQL.
 */
export function decodePk(raw: string, config: TableConfig): PkValues | null {
  if (config.pk.length === 0) return null;
  let parsed: unknown;
  try {
    parsed = JSON.parse(raw);
  } catch {
    return null;
  }
  if (typeof parsed !== "object" || parsed === null || Array.isArray(parsed)) return null;
  const entries = Object.entries(parsed as Record<string, unknown>);
  if (entries.length !== config.pk.length) return null;
  const out: PkValues = {};
  for (const [k, v] of entries) {
    if (!config.pk.includes(k)) return null;
    if (typeof v !== "string" && typeof v !== "number") return null;
    out[k] = v;
  }
  return out;
}
