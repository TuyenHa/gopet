"use client";

import { banAccountAction } from "@/lib/accounts/account-actions";
import { ActionForm } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

/**
 * Ban có hạn/vĩnh viễn/unban trong 1 form (chọn chế độ bằng `<select>` thuần — không cần
 * JS phức tạp, submit qua Server Action như mọi form khác trong dự án).
 */
export function BanAccountForm({ userId, banReason }: { userId: number; banReason: string }) {
  return (
    <ActionForm action={banAccountAction} submitLabel="Áp dụng" submitVariant="destructive">
      <input type="hidden" name="userId" value={userId} />
      <div className="space-y-2">
        <Label htmlFor="mode">Chế độ</Label>
        <select
          id="mode"
          name="mode"
          defaultValue="temporary"
          className="h-9 w-full rounded-lg border border-input bg-transparent px-2.5 text-sm"
        >
          <option value="temporary">Ban có hạn</option>
          <option value="permanent">Ban vĩnh viễn</option>
          <option value="unban">Gỡ ban</option>
        </select>
      </div>
      <div className="space-y-2">
        <Label htmlFor="durationHours">Số giờ (chỉ dùng khi &quot;Ban có hạn&quot;)</Label>
        <Input id="durationHours" name="durationHours" type="number" min={1} step={1} placeholder="24" />
      </div>
      <div className="space-y-2">
        <Label htmlFor="banReason">Lý do</Label>
        <Textarea id="banReason" name="banReason" defaultValue={banReason} maxLength={1000} rows={2} />
      </div>
      <p className="text-xs text-neutral-500">
        Người đang online chỉ bị chặn từ lần đăng nhập sau (server chỉ đọc `user` lúc login).
      </p>
    </ActionForm>
  );
}
