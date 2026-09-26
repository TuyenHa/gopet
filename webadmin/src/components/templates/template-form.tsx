import { ActionForm, type FormAction } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import type { ColumnDef, TableConfig } from "@/lib/templates/table-registry";
import { RefPicker } from "./ref-picker";

/**
 * Form tạo/sửa dùng chung cho mọi bảng registry — sinh input từ `config.columns`.
 * Cột PK: ẩn khi tạo mới nếu là AUTO_INCREMENT đơn, hiện read-only khi sửa.
 */
export function TemplateForm({
  config,
  mode,
  row,
  action,
  submitLabel = "Lưu",
  requirePassword = false,
}: {
  config: TableConfig;
  mode: "create" | "edit";
  row?: Record<string, unknown>;
  action: FormAction;
  submitLabel?: string;
  /** true khi bảng thuộc nhóm "Hệ thống" (M3) — super-admin + reauth ở server, form phải gửi kèm `confirmPassword`. */
  requirePassword?: boolean;
}) {
  const isPk = (name: string) => config.pk.includes(name);
  const singleAutoIncPk = config.pk.length === 1 && config.autoIncrement;

  const readOnlyPkCols = mode === "edit" ? config.columns.filter((c) => isPk(c.name)) : [];
  const editableCols = config.columns.filter((c) => {
    if (mode === "edit") return !isPk(c.name);
    if (isPk(c.name) && singleAutoIncPk) return false;
    return true;
  });

  return (
    <ActionForm action={action} submitLabel={submitLabel} className="space-y-4">
      {readOnlyPkCols.map((c) => (
        <div key={c.name} className="space-y-1">
          <Label>{c.label}</Label>
          <Input value={displayValue(row?.[c.name])} disabled />
        </div>
      ))}
      {editableCols.map((c) => (
        <div key={c.name} className="space-y-1">
          <Label htmlFor={c.name}>
            {c.label}
            {c.note && <span className="ml-1 text-xs font-normal text-neutral-400">({c.note})</span>}
          </Label>
          {renderInput(c, mode === "edit" ? row?.[c.name] : undefined)}
        </div>
      ))}
      {requirePassword && (
        <div className="space-y-1">
          <Label htmlFor="confirmPassword">Nhập lại mật khẩu của bạn (bảng Hệ thống — cần super-admin)</Label>
          <Input id="confirmPassword" name="confirmPassword" type="password" required autoComplete="current-password" />
        </div>
      )}
    </ActionForm>
  );
}

function displayValue(v: unknown): string {
  return v === null || v === undefined ? "" : String(v);
}

function renderInput(column: ColumnDef, value: unknown) {
  const dv = displayValue(value);

  if (column.type === "ref" && column.ref) {
    return <RefPicker name={column.name} kind={column.ref} defaultValue={dv || null} />;
  }

  if (column.type === "bool") {
    const checked = dv === "1" || dv === "true";
    return (
      <select
        id={column.name}
        name={column.name}
        defaultValue={checked ? "1" : "0"}
        className="h-8 w-full rounded-lg border border-input bg-transparent px-2.5 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
      >
        <option value="1">Có</option>
        <option value="0">Không</option>
      </select>
    );
  }

  if (column.type === "json" || column.long) {
    return (
      <Textarea
        id={column.name}
        name={column.name}
        defaultValue={dv}
        rows={column.type === "json" ? 4 : 3}
        className="font-mono text-xs"
      />
    );
  }

  if (column.type === "int" || column.type === "float") {
    return <Input id={column.name} name={column.name} type="number" step={column.type === "float" ? "any" : "1"} defaultValue={dv} />;
  }

  return <Input id={column.name} name={column.name} type="text" defaultValue={dv} maxLength={column.maxLength} />;
}
