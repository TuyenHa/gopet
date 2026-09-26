"use client";

import { Button } from "@/components/ui/button";
import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { lockAccountAction, unlockAccountAction } from "@/lib/accounts/account-actions";

/** Khoá (role 1→0) / Mở khoá (role 0→1). Tài khoản role>1 (admin) không khoá được qua đây. */
export function LockToggleButton({ userId, role }: { userId: number; role: number }) {
  if (role > 1) {
    return (
      <p className="text-xs text-neutral-500">
        role={role} — tài khoản admin, không khoá được qua nút này.
      </p>
    );
  }

  const locked = role === 0;
  const action = locked ? unlockAccountAction : lockAccountAction;

  return (
    <ConfirmDialog
      trigger={
        <Button variant={locked ? "default" : "outline"} size="sm">
          {locked ? "Mở khoá" : "Khoá tài khoản"}
        </Button>
      }
      title={locked ? "Mở khoá tài khoản?" : "Khoá tài khoản?"}
      description={
        locked
          ? "Tài khoản sẽ đăng nhập lại được (role=1)."
          : "Tài khoản sẽ không đăng nhập được (role=0) cho tới khi mở khoá."
      }
      destructive={!locked}
      action={(form) => {
        form.set("userId", String(userId));
        return action(null, form);
      }}
    />
  );
}
