"use client";

import { useState } from "react";
import { Plus, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { GIFT_TYPE, GIFT_TYPE_OPTIONS, giftDataToArrays, type GiftEntry, type GiftTypeValue } from "@/lib/giftcodes/gift-data-schema";
import { GiftEntryFields } from "./gift-entry-fields";
import { defaultEntry } from "./gift-entry-defaults";

/**
 * Trình dựng danh sách quà cho 1 giftcode. Ghi kết quả vào `<input type="hidden" name={name}>`
 * dạng JSON của `GiftEntry[]` — action phía server tự validate lại bằng `giftDataSchema`
 * trước khi chuyển sang `int[][]` để ghi cột `gift_data` (KHÔNG tin dữ liệu từ client).
 */
export function GiftDataBuilder({ name = "giftData", initialEntries = [] }: { name?: string; initialEntries?: GiftEntry[] }) {
  const [entries, setEntries] = useState<GiftEntry[]>(initialEntries.length ? initialEntries : [defaultEntry(GIFT_TYPE.GOLD)]);

  function updateAt(i: number, entry: GiftEntry) {
    setEntries((prev) => prev.map((e, idx) => (idx === i ? entry : e)));
  }
  function removeAt(i: number) {
    setEntries((prev) => prev.filter((_, idx) => idx !== i));
  }

  const preview = JSON.stringify(giftDataToArrays(entries));

  return (
    <div className="space-y-3">
      {entries.map((entry, i) => (
        <div key={i} className="space-y-2 rounded-lg border p-3">
          <div className="flex items-center justify-between gap-2">
            <select
              className="h-9 flex-1 rounded border px-2 text-sm"
              value={entry.type}
              onChange={(e) => updateAt(i, defaultEntry(Number(e.target.value) as GiftTypeValue))}
            >
              {GIFT_TYPE_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
            <Button type="button" variant="outline" size="sm" onClick={() => removeAt(i)} disabled={entries.length <= 1}>
              <X className="size-4" />
            </Button>
          </div>
          <GiftEntryFields entry={entry} onChange={(e) => updateAt(i, e)} />
        </div>
      ))}
      <Button type="button" variant="outline" size="sm" onClick={() => setEntries((prev) => [...prev, defaultEntry(GIFT_TYPE.GOLD)])}>
        <Plus className="size-4" /> Thêm mục quà
      </Button>
      <details className="text-xs text-neutral-500">
        <summary className="cursor-pointer">Xem trước dữ liệu sẽ lưu (gift_data, int[][])</summary>
        <pre className="mt-1 overflow-auto rounded bg-neutral-50 p-2">{preview}</pre>
      </details>
      <input type="hidden" name={name} value={JSON.stringify(entries)} />
    </div>
  );
}
