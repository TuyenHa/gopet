"use client";

import { ActionForm } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { adjustPlayerCurrencyAction } from "@/lib/players/player-actions";

/** Chỉ delta (gold/coin/lua) — không sửa trực tiếp giá trị vì server tự cộng khi bán ki ốt [RT#4]. */
export function CurrencyAdjustForm({ playerId }: { playerId: number }) {
  return (
    <ActionForm action={adjustPlayerCurrencyAction} submitLabel="Áp dụng" resetOnSuccess>
      <input type="hidden" name="playerId" value={playerId} />
      <div className="flex flex-wrap items-end gap-2">
        <div className="space-y-2">
          <Label htmlFor="field">Loại tiền</Label>
          <select id="field" name="field" className="h-9 rounded-lg border border-input bg-transparent px-2.5 text-sm">
            <option value="gold">Vàng (gold)</option>
            <option value="coin">Xu (coin)</option>
            <option value="lua">Lúa (lua)</option>
          </select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="delta">Số lượng (âm = trừ)</Label>
          <Input id="delta" name="delta" type="number" step={1} required className="w-40" />
        </div>
      </div>
      <p className="text-xs text-neutral-500">Chỉ áp dụng được khi nhân vật đang OFFLINE.</p>
    </ActionForm>
  );
}
