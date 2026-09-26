"use client";

import { ActionForm } from "@/components/data/action-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { sendLetterToOneAction } from "@/lib/letters/letter-actions";

export function SendOneLetterForm({ defaultTarget }: { defaultTarget?: string }) {
  return (
    <ActionForm action={sendLetterToOneAction} submitLabel="Gửi thư" resetOnSuccess>
      <div>
        <Label htmlFor="target">Người nhận (username / tên nhân vật / user_id)</Label>
        <Input id="target" name="target" defaultValue={defaultTarget} required />
      </div>
      <div>
        <Label htmlFor="type">Loại thư</Label>
        <select id="type" name="type" className="h-9 w-full rounded border px-2 text-sm" defaultValue={2}>
          <option value={2}>Admin</option>
          <option value={3}>Sự kiện</option>
        </select>
      </div>
      <div>
        <Label htmlFor="title">Tiêu đề</Label>
        <Input id="title" name="title" maxLength={1000} required />
      </div>
      <div>
        <Label htmlFor="shortContent">Nội dung ngắn (hiện ở danh sách thư)</Label>
        <Input id="shortContent" name="shortContent" maxLength={1000} required />
      </div>
      <div>
        <Label htmlFor="content">Nội dung đầy đủ</Label>
        <Textarea id="content" name="content" rows={5} required />
      </div>
    </ActionForm>
  );
}
