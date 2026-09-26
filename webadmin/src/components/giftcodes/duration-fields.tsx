"use client";

import type { ChangeEvent } from "react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export interface Duration {
  minutes: number;
  hours: number;
  days: number;
  months: number;
  years: number;
}

/** min/giờ/ngày(/tháng/năm) — thứ tự & tập trường phải khớp `giftEntryToArray`
 * (gift-data-serialize.ts) vì server đọc theo VỊ TRÍ trong mảng, không theo tên. */
export function DurationFields({
  value,
  onChange,
  withMonthYear = false,
}: {
  value: Duration;
  onChange: (d: Duration) => void;
  withMonthYear?: boolean;
}) {
  const set = (k: keyof Duration) => (e: ChangeEvent<HTMLInputElement>) => onChange({ ...value, [k]: Number(e.target.value) || 0 });
  return (
    <div className="grid grid-cols-3 gap-2 sm:grid-cols-5">
      <Field label="Phút" value={value.minutes} onChange={set("minutes")} />
      <Field label="Giờ" value={value.hours} onChange={set("hours")} />
      <Field label="Ngày" value={value.days} onChange={set("days")} />
      {withMonthYear && <Field label="Tháng" value={value.months ?? 0} onChange={set("months")} />}
      {withMonthYear && <Field label="Năm" value={value.years ?? 0} onChange={set("years")} />}
    </div>
  );
}

function Field({ label, value, onChange }: { label: string; value: number; onChange: (e: ChangeEvent<HTMLInputElement>) => void }) {
  return (
    <div>
      <Label className="text-xs">{label}</Label>
      <Input type="number" min={0} value={value} onChange={onChange} />
    </div>
  );
}
