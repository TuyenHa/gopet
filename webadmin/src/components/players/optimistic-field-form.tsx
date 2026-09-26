"use client";

import { ActionForm } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { updatePlayerFieldAction } from "@/lib/players/player-actions";

/**
 * Sửa 1 cột với optimistic concurrency — `currentValue` phải khớp giá trị lúc tải trang, nếu
 * không server báo xung đột (dữ liệu đã đổi từ nơi khác) [RT#4]. Chỉ áp dụng khi OFFLINE.
 */
export function OptimisticFieldForm({
  playerId,
  field,
  label,
  currentValue,
  type = "number",
}: {
  playerId: number;
  field: "star" | "pkPoint" | "EventPoint" | "AccumulatedPoint" | "avatarPath" | "gender";
  label: string;
  currentValue: string | number;
  type?: "number" | "text";
}) {
  return (
    <ActionForm action={updatePlayerFieldAction} submitLabel="Lưu" className="flex items-end gap-2">
      <input type="hidden" name="playerId" value={playerId} />
      <input type="hidden" name="field" value={field} />
      <input type="hidden" name="oldValue" value={String(currentValue)} />
      <div className="space-y-2">
        <Label htmlFor={`field-${field}`}>{label}</Label>
        <Input id={`field-${field}`} name="newValue" type={type} defaultValue={currentValue} className="w-40" />
      </div>
    </ActionForm>
  );
}
