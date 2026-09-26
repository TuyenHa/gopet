"use client";

import { useState, useTransition } from "react";
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
import { resetPasswordAction } from "@/lib/accounts/account-actions";

/**
 * Không dùng `ConfirmDialog` dùng chung vì cần GIỮ mật khẩu mới hiển thị 1 lần trên màn hình
 * (không phải toast tự biến mất) — mật khẩu này không bao giờ vào audit log.
 */
export function ResetPasswordDialog({ userId, requirePassword }: { userId: number; requirePassword: boolean }) {
  const [open, setOpen] = useState(false);
  const [pending, start] = useTransition();
  const [newPassword, setNewPassword] = useState<string | null>(null);

  const submit = (form: FormData) =>
    start(async () => {
      form.set("userId", String(userId));
      const res = await resetPasswordAction(null, form);
      if (res.ok) {
        setNewPassword(res.data?.newPassword ?? null);
        toast.success("Đã đặt lại mật khẩu.");
      } else {
        toast.error(res.error);
      }
    });

  return (
    <AlertDialog
      open={open}
      onOpenChange={(v) => {
        setOpen(v);
        if (!v) setNewPassword(null);
      }}
    >
      <AlertDialogTrigger asChild>
        <Button variant="destructive" size="sm">
          Buộc reset mật khẩu
        </Button>
      </AlertDialogTrigger>
      <AlertDialogContent>
        {newPassword ? (
          <>
            <AlertDialogHeader>
              <AlertDialogTitle>Mật khẩu mới (chỉ hiện 1 lần)</AlertDialogTitle>
              <AlertDialogDescription asChild>
                <div className="space-y-2">
                  <p>Sao chép ngay — mật khẩu này không được lưu lại ở bất kỳ đâu, kể cả audit log.</p>
                  <code className="block rounded-lg border bg-neutral-50 p-2 text-sm break-all select-all">
                    {newPassword}
                  </code>
                </div>
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter className="mt-4">
              <Button type="button" onClick={() => setOpen(false)}>
                Đã sao chép, đóng lại
              </Button>
            </AlertDialogFooter>
          </>
        ) : (
          <form action={submit}>
            <AlertDialogHeader>
              <AlertDialogTitle>Buộc reset mật khẩu?</AlertDialogTitle>
              <AlertDialogDescription>
                Tạo mật khẩu ngẫu nhiên mới (bcrypt). Mật khẩu cũ sẽ không dùng được nữa.
              </AlertDialogDescription>
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
              <Button type="submit" variant="destructive" disabled={pending}>
                {pending ? "Đang xử lý..." : "Reset"}
              </Button>
            </AlertDialogFooter>
          </form>
        )}
      </AlertDialogContent>
    </AlertDialog>
  );
}
