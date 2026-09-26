"use client";

import { Button } from "@/components/ui/button";
import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { clear2faAction } from "@/lib/accounts/account-actions";

/** `requirePassword` chỉ khi mục tiêu là tài khoản admin khác (trang cha quyết định có hiện nút hay không). */
export function Clear2faButton({ userId, requirePassword }: { userId: number; requirePassword: boolean }) {
  return (
    <ConfirmDialog
      trigger={
        <Button variant="outline" size="sm">
          Xoá 2FA
        </Button>
      }
      title="Xoá 2FA của tài khoản này?"
      description="Tài khoản sẽ đăng nhập không cần mã OTP nữa."
      requirePassword={requirePassword}
      action={(form) => {
        form.set("userId", String(userId));
        return clear2faAction(null, form);
      }}
    />
  );
}
