"use client";

import { ActionForm } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { adjustCoinAction } from "@/lib/accounts/account-actions";

/** Cộng/trừ `user.coin` (int32) — số âm để trừ. DB tự chặn vượt [0, 2^31-1]. */
export function AdjustCoinForm({ userId }: { userId: number }) {
  return (
    <ActionForm action={adjustCoinAction} submitLabel="Áp dụng" resetOnSuccess>
      <input type="hidden" name="userId" value={userId} />
      <div className="space-y-2">
        <Label htmlFor="delta">Số ngọc cộng/trừ (âm = trừ)</Label>
        <Input id="delta" name="delta" type="number" step={1} required placeholder="vd 1000 hoặc -500" />
      </div>
    </ActionForm>
  );
}
