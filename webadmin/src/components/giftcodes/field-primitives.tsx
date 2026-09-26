"use client";

import type { ChangeEvent } from "react";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ItemPetPicker } from "./item-pet-picker";
import { DurationFields, type Duration } from "./duration-fields";

export function AmountField({ label, value, onChange }: { label: string; value: number; onChange: (v: number) => void }) {
  return (
    <div>
      <Label className="text-xs">{label}</Label>
      <Input type="number" value={value} onChange={(e: ChangeEvent<HTMLInputElement>) => onChange(Number(e.target.value) || 0)} />
    </div>
  );
}

/** Ô ID; nếu `kind` được truyền thì hiện thêm ô tìm nhanh vật phẩm/pet phía dưới. */
export function IdField({
  label,
  value,
  onChange,
  kind,
}: {
  label: string;
  value: number;
  onChange: (v: number) => void;
  kind?: "item" | "pet";
}) {
  return (
    <div className="space-y-1">
      <Label className="text-xs">{label} (ID)</Label>
      <Input type="number" value={value} onChange={(e) => onChange(Number(e.target.value) || 0)} />
      {kind && <ItemPetPicker kind={kind} onPick={(hit) => onChange(hit.id)} />}
    </div>
  );
}

/** Bố cục dùng chung cho danh hiệu/skin/pet thử nghiệm: 1 ID + cờ vĩnh viễn + thời hạn. */
export function TimedFields({
  idLabel,
  idValue,
  idKind,
  onIdChange,
  isInfinite,
  onInfiniteChange,
  duration,
  onDurationChange,
  withMonthYear = false,
}: {
  idLabel: string;
  idValue: number;
  idKind?: "item" | "pet";
  onIdChange: (v: number) => void;
  isInfinite: boolean;
  onInfiniteChange: (v: boolean) => void;
  duration: Duration;
  onDurationChange: (d: Duration) => void;
  withMonthYear?: boolean;
}) {
  return (
    <div className="space-y-2">
      <IdField label={idLabel} value={idValue} onChange={onIdChange} kind={idKind} />
      <label className="flex items-center gap-2 text-sm">
        <Checkbox checked={isInfinite} onCheckedChange={(c) => onInfiniteChange(c === true)} /> Vĩnh viễn
      </label>
      {!isInfinite && (
        <div>
          <Label className="text-xs">Thời hạn</Label>
          <DurationFields value={duration} onChange={onDurationChange} withMonthYear={withMonthYear} />
        </div>
      )}
    </div>
  );
}
