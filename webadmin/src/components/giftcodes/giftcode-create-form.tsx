"use client";

import { useState } from "react";
import { ActionForm } from "@/components/data/action-form";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { createGiftcodeAction } from "@/lib/giftcodes/giftcode-create-action";
import { GiftDataBuilder } from "./gift-data-builder";

/** Form tạo giftcode mới. Khi có `forUserId` (đến từ `/giftcodes/new?forUserId=`, phase 5B):
 * ẩn tuỳ chọn "code bang hội", mặc định sinh code ngẫu nhiên + maxUser=1 + gửi kèm thư. */
export function GiftcodeCreateForm({ forUserId }: { forUserId?: number }) {
  const [codeMode, setCodeMode] = useState<"manual" | "random">(forUserId ? "random" : "manual");

  return (
    <ActionForm action={createGiftcodeAction} submitLabel="Tạo giftcode" resetOnSuccess>
      {forUserId !== undefined && <input type="hidden" name="forUserId" value={forUserId} />}

      <div className="flex items-center gap-4">
        <label className="flex items-center gap-2 text-sm">
          <input type="radio" name="codeMode" value="manual" checked={codeMode === "manual"} onChange={() => setCodeMode("manual")} />
          Tự nhập code
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input type="radio" name="codeMode" value="random" checked={codeMode === "random"} onChange={() => setCodeMode("random")} />
          Sinh ngẫu nhiên
        </label>
      </div>

      {codeMode === "manual" && (
        <div>
          <Label htmlFor="code">Code</Label>
          <Input id="code" name="code" maxLength={100} required />
        </div>
      )}

      <div>
        <Label htmlFor="maxUser">Số lượt tối đa</Label>
        <Input id="maxUser" name="maxUser" type="number" min={1} defaultValue={forUserId ? 1 : 100} required />
      </div>

      <div>
        <Label htmlFor="expire">Hạn sử dụng (giờ Việt Nam)</Label>
        <Input id="expire" name="expire" type="datetime-local" required />
      </div>

      {forUserId === undefined && (
        <label className="flex items-center gap-2 text-sm">
          <Checkbox name="isClanCode" /> Code dành cho bang hội (id trong danh sách dùng là clanId, không phải user_id)
        </label>
      )}
      {forUserId !== undefined && (
        <label className="flex items-center gap-2 text-sm">
          <Checkbox name="sendLetter" defaultChecked /> Gửi kèm thư chứa mã cho người dùng này
        </label>
      )}

      <div>
        <Label>Quà</Label>
        <GiftDataBuilder />
      </div>
    </ActionForm>
  );
}
