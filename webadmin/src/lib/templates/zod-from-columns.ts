import { z } from "zod";
import type { ColumnDef, TableConfig } from "./table-registry";

function isValidJson(s: string): boolean {
  try {
    JSON.parse(s);
    return true;
  } catch {
    return false;
  }
}

/** Sinh zod schema cho 1 cột dựa trên `type`/`nullable`/`maxLength` khai trong registry. */
function baseColumnSchema(c: ColumnDef): z.ZodTypeAny {
  switch (c.type) {
    case "int":
    case "ref":
    case "bool":
      return z.coerce.number().int(`${c.label}: phải là số nguyên`);
    case "float":
      return z.coerce.number(`${c.label}: phải là số`);
    case "bigint":
      return z.string().trim().regex(/^-?\d+$/, `${c.label}: phải là số nguyên`);
    case "json": {
      let s = z.string().trim();
      if (!c.nullable) s = s.min(1, `${c.label}: bắt buộc`);
      if (c.maxLength) s = s.max(c.maxLength, `${c.label}: tối đa ${c.maxLength} ký tự`);
      return s.refine(isValidJson, `${c.label}: JSON không hợp lệ (vd [], {}, [1,2])`);
    }
    default: {
      let s = z.string().trim();
      if (!c.nullable) s = s.min(1, `${c.label}: bắt buộc`);
      if (c.maxLength) s = s.max(c.maxLength, `${c.label}: tối đa ${c.maxLength} ký tự`);
      return s;
    }
  }
}

/** Bọc nullable: chuỗi rỗng/undefined → NULL, bỏ qua validate nội dung khi rỗng. */
function columnFieldSchema(c: ColumnDef): z.ZodTypeAny {
  const base = baseColumnSchema(c);
  if (!c.nullable) return base;
  return z.preprocess((v) => (v === "" || v === undefined || v === null ? null : v), base.nullable());
}

/**
 * Sinh zod object cho toàn bộ cột chỉnh sửa được của 1 bảng.
 * - "create": bỏ cột PK khi PK là 1 cột AUTO_INCREMENT (DB tự sinh).
 * - "update": luôn bỏ mọi cột PK (định danh dòng nằm ở tham số riêng, không phải SET).
 */
export function zodSchemaFromColumns(config: TableConfig, mode: "create" | "update") {
  const shape: Record<string, z.ZodTypeAny> = {};
  for (const c of config.columns) {
    const isPk = config.pk.includes(c.name);
    if (mode === "update" && isPk) continue;
    if (mode === "create" && isPk && config.pk.length === 1 && config.autoIncrement) continue;
    shape[c.name] = columnFieldSchema(c);
  }
  return z.object(shape);
}

/** Danh sách cột (theo thứ tự registry) sẽ xuất hiện trong FormData cho 1 mode. */
export function editableColumnNames(config: TableConfig, mode: "create" | "update"): string[] {
  return config.columns
    .filter((c) => {
      const isPk = config.pk.includes(c.name);
      if (mode === "update" && isPk) return false;
      if (mode === "create" && isPk && config.pk.length === 1 && config.autoIncrement) return false;
      return true;
    })
    .map((c) => c.name);
}

/** Đọc FormData → object thô theo đúng tập cột của mode (bool thiếu key → "0"). */
export function formDataToRecord(form: FormData, config: TableConfig, mode: "create" | "update"): Record<string, string> {
  const out: Record<string, string> = {};
  for (const name of editableColumnNames(config, mode)) {
    const col = config.columns.find((c) => c.name === name)!;
    const v = form.get(name);
    out[name] = v === null ? (col.type === "bool" ? "0" : "") : String(v);
  }
  return out;
}
