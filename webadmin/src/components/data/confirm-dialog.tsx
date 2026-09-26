"use client";

import { useState, useTransition, type ReactNode } from "react";
import { toast } from "sonner";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { ActionResult } from "@/lib/actions/action-result";

/**
 * Nút + hộp thoại xác nhận chạy một Server Action. `action` nhận FormData (có thể chứa
 * `confirmPassword` khi requirePassword — dùng cho reauth thao tác nhạy cảm).
 */
export function ConfirmDialog({
  trigger,
  title,
  description,
  confirmLabel = "Xác nhận",
  destructive = false,
  requirePassword = false,
  hiddenFields,
  action,
}: {
  trigger: ReactNode;
  title: string;
  description?: ReactNode;
  confirmLabel?: string;
  destructive?: boolean;
  requirePassword?: boolean;
  /**
   * Trường ẩn gửi kèm (vd `{ id: "47" }`). Dùng từ Server Component thay cho hàm bọc
   * inline — hàm thường KHÔNG truyền được sang Client Component, chỉ Server Action
   * (hoặc `.bind` của nó) mới được.
   */
  hiddenFields?: Record<string, string>;
  action: (form: FormData) => Promise<ActionResult<unknown>>;
}) {
  const [open, setOpen] = useState(false);
  const [pending, start] = useTransition();

  const submit = (form: FormData) =>
    start(async () => {
      const res = await action(form);
      if (res.ok) {
        toast.success(res.message ?? "Đã thực hiện");
        setOpen(false);
      } else {
        toast.error(res.error);
      }
    });

  return (
    <AlertDialog open={open} onOpenChange={setOpen}>
      <AlertDialogTrigger asChild>{trigger}</AlertDialogTrigger>
      <AlertDialogContent>
        <form action={submit}>
          {hiddenFields &&
            Object.entries(hiddenFields).map(([name, value]) => (
              <input key={name} type="hidden" name={name} value={value} />
            ))}
          <AlertDialogHeader>
            <AlertDialogTitle>{title}</AlertDialogTitle>
            {description && <AlertDialogDescription asChild><div>{description}</div></AlertDialogDescription>}
          </AlertDialogHeader>
          {requirePassword && (
            <div className="mt-4 space-y-2">
              <Label htmlFor="confirmPassword">Nhập lại mật khẩu của bạn</Label>
              <Input id="confirmPassword" name="confirmPassword" type="password" required autoComplete="current-password" />
            </div>
          )}
          <AlertDialogFooter className="mt-4">
            <AlertDialogCancel type="button" disabled={pending}>
              Huỷ
            </AlertDialogCancel>
            <Button type="submit" variant={destructive ? "destructive" : "default"} disabled={pending}>
              {pending ? "Đang xử lý..." : confirmLabel}
            </Button>
          </AlertDialogFooter>
        </form>
      </AlertDialogContent>
    </AlertDialog>
  );
}
