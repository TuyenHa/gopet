"use client";

import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RANDOM_ITEM_POOL_CODES } from "@/lib/giftcodes/random-item-pools";

export interface RandomItemEntry {
  count: number;
  itemId: number;
}

/** Danh sách (số lượng, itemId) cho loại quà "vật phẩm ngẫu nhiên" — mỗi lần rút, server chọn
 * NGẪU NHIÊN 1 dòng trong danh sách này (GameController.cs:4193-4279), rút `Số lần rút` lần. */
export function RandomItemListEditor({ items, onChange }: { items: RandomItemEntry[]; onChange: (items: RandomItemEntry[]) => void }) {
  function update(i: number, patch: Partial<RandomItemEntry>) {
    onChange(items.map((it, idx) => (idx === i ? { ...it, ...patch } : it)));
  }

  return (
    <div className="space-y-2 rounded border p-2">
      <Label className="text-xs">Danh sách vật phẩm ngẫu nhiên</Label>
      {items.map((it, i) => (
        <div key={i} className="flex flex-wrap items-end gap-2">
          <div>
            <Label className="text-xs">Số lượng</Label>
            <Input type="number" className="w-24" value={it.count} onChange={(e) => update(i, { count: Number(e.target.value) || 0 })} />
          </div>
          <div>
            <Label className="text-xs">itemId (âm = nhóm đặc biệt)</Label>
            <Input type="number" className="w-28" value={it.itemId} onChange={(e) => update(i, { itemId: Number(e.target.value) || 0 })} />
          </div>
          <select
            className="h-9 rounded border px-2 text-sm"
            value=""
            onChange={(e) => e.target.value && update(i, { itemId: Number(e.target.value) })}
          >
            <option value="">— chọn nhóm đặc biệt —</option>
            {RANDOM_ITEM_POOL_CODES.map((p) => (
              <option key={p.value} value={p.value}>
                {p.label}
              </option>
            ))}
          </select>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => onChange(items.filter((_, idx) => idx !== i))}
            disabled={items.length <= 1}
          >
            <X className="size-4" />
          </Button>
        </div>
      ))}
      <Button type="button" variant="outline" size="sm" onClick={() => onChange([...items, { count: 1, itemId: 0 }])}>
        + Thêm dòng
      </Button>
    </div>
  );
}
