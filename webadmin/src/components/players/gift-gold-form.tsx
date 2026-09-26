"use client";

import { ActionForm } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { giftGoldAction } from "@/lib/players/player-actions";

/** Tặng vàng qua hàng đợi `exchange_gold` — hiệu lực cả khi online, server cộng lúc đăng nhập. */
export function GiftGoldForm({ userId }: { userId: number }) {
  return (
    <ActionForm action={giftGoldAction} submitLabel="Tặng vàng" resetOnSuccess>
      <input type="hidden" name="userId" value={userId} />
      <div className="space-y-2">
        <Label htmlFor="amount">Số vàng</Label>
        <Input id="amount" name="amount" type="number" min={0} step={1} required className="w-40" />
      </div>
    </ActionForm>
  );
}
