"use client";

import { useState } from "react";
import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { sendLetterToAllAction } from "@/lib/letters/letter-actions";

/** Gửi thư cho TOÀN BỘ người chơi — hai bước: soạn (compose) rồi xem lại + cảnh báo (review)
 * trước khi bấm nút xác nhận cuối (ConfirmDialog + nhập lại mật khẩu). */
export function SendAllLetterForm() {
  const [step, setStep] = useState<"compose" | "review">("compose");
  const [type, setType] = useState(2);
  const [title, setTitle] = useState("");
  const [shortContent, setShortContent] = useState("");
  const [content, setContent] = useState("");
  const canReview = title.trim() && shortContent.trim() && content.trim();

  if (step === "review") {
    return (
      <div className="space-y-4">
        <div className="rounded-lg border border-amber-300 bg-amber-50 p-3 text-sm text-amber-900">
          <p className="font-medium">Cảnh báo trước khi gửi cho TOÀN BỘ người chơi:</p>
          <ul className="mt-1 list-disc pl-5">
            <li>Không thể huỷ sau khi gửi (hàng đợi thư không có khoá chính để xoá an toàn).</li>
            <li>Người đang đăng nhập ĐÚNG lúc gửi có thể không nhận được thư (rủi ro đã biết, xem phase 8).</li>
            <li>Nên gửi lúc ít người online để giảm rủi ro trên.</li>
          </ul>
        </div>
        <div className="space-y-1 rounded-lg border p-3 text-sm">
          <p>
            <span className="font-medium">Loại:</span> {type === 2 ? "Admin" : "Sự kiện"}
          </p>
          <p>
            <span className="font-medium">Tiêu đề:</span> {title}
          </p>
          <p>
            <span className="font-medium">Nội dung ngắn:</span> {shortContent}
          </p>
          <p className="whitespace-pre-wrap">
            <span className="font-medium">Nội dung:</span> {content}
          </p>
        </div>
        <div className="flex gap-2">
          <Button type="button" variant="outline" onClick={() => setStep("compose")}>
            Quay lại sửa
          </Button>
          <ConfirmDialog
            trigger={<Button variant="destructive">Gửi cho toàn bộ người chơi</Button>}
            title="Xác nhận gửi cho toàn bộ người chơi?"
            description="Thao tác không thể hoàn tác. Cần nhập lại mật khẩu để xác nhận."
            destructive
            requirePassword
            action={(form) => {
              form.set("type", String(type));
              form.set("title", title);
              form.set("shortContent", shortContent);
              form.set("content", content);
              return sendLetterToAllAction(form);
            }}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div>
        <Label htmlFor="all-type">Loại thư</Label>
        <select
          id="all-type"
          className="h-9 w-full rounded border px-2 text-sm"
          value={type}
          onChange={(e) => setType(Number(e.target.value))}
        >
          <option value={2}>Admin</option>
          <option value={3}>Sự kiện</option>
        </select>
      </div>
      <div>
        <Label htmlFor="all-title">Tiêu đề</Label>
        <Input id="all-title" maxLength={1000} value={title} onChange={(e) => setTitle(e.target.value)} required />
      </div>
      <div>
        <Label htmlFor="all-short">Nội dung ngắn</Label>
        <Input id="all-short" maxLength={1000} value={shortContent} onChange={(e) => setShortContent(e.target.value)} required />
      </div>
      <div>
        <Label htmlFor="all-content">Nội dung đầy đủ</Label>
        <Textarea id="all-content" rows={5} value={content} onChange={(e) => setContent(e.target.value)} required />
      </div>
      <Button type="button" disabled={!canReview} onClick={() => canReview && setStep("review")}>
        Xem trước &amp; gửi
      </Button>
    </div>
  );
}
