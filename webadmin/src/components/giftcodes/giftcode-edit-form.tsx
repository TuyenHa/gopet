"use client";

import { ActionForm } from "@/components/data/action-form";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { updateGiftcodeAction } from "@/lib/giftcodes/giftcode-update-actions";
import type { GiftEntry } from "@/lib/giftcodes/gift-data-schema";
import { GiftDataBuilder } from "./gift-data-builder";

export interface GiftcodeEditFormProps {
  id: number;
  code: string;
  maxUser: number;
  expireVnInputValue: string;
  isClanCode: boolean;
  giftEntries: GiftEntry[];
}

/** Sửa giftcode: đổi code (khoá theo tên CŨ ở server action), maxUser, hạn, quà. */
export function GiftcodeEditForm({ id, code, maxUser, expireVnInputValue, isClanCode, giftEntries }: GiftcodeEditFormProps) {
  return (
    <ActionForm action={updateGiftcodeAction} submitLabel="Lưu thay đổi">
      <input type="hidden" name="id" value={id} />
      <div>
        <Label htmlFor="code">Code</Label>
        <Input id="code" name="code" defaultValue={code} maxLength={100} required />
      </div>
      <div>
        <Label htmlFor="maxUser">Số lượt tối đa</Label>
        <Input id="maxUser" name="maxUser" type="number" min={1} defaultValue={maxUser} required />
      </div>
      <div>
        <Label htmlFor="expire">Hạn sử dụng (giờ Việt Nam)</Label>
        <Input id="expire" name="expire" type="datetime-local" defaultValue={expireVnInputValue} required />
      </div>
      <label className="flex items-center gap-2 text-sm">
        <Checkbox name="isClanCode" defaultChecked={isClanCode} /> Code dành cho bang hội
      </label>
      <div>
        <Label>Quà</Label>
        <GiftDataBuilder initialEntries={giftEntries} />
      </div>
    </ActionForm>
  );
}
