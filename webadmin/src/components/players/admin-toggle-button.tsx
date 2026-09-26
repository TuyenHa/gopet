"use client";

import { Button } from "@/components/ui/button";
import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { setPlayerAdminAction } from "@/lib/players/player-actions";

/** Cấp/thu quyền admin nhân vật — super-admin only, reauth, chỉ hiệu lực từ lần đăng nhập sau [RT#15]. */
export function AdminToggleButton({
  playerId,
  isAdmin,
  isSelf,
}: {
  playerId: number;
  isAdmin: boolean;
  isSelf: boolean;
}) {
  if (isAdmin && isSelf) {
    return <p className="text-xs text-neutral-500">Không thể tự thu quyền admin của chính mình.</p>;
  }

  return (
    <ConfirmDialog
      trigger={
        <Button variant={isAdmin ? "outline" : "destructive"} size="sm">
          {isAdmin ? "Thu quyền admin" : "Cấp quyền admin"}
        </Button>
      }
      title={isAdmin ? "Thu quyền admin nhân vật này?" : "Cấp quyền admin cho nhân vật này?"}
      description="Chỉ có hiệu lực từ lần đăng nhập sau (isAdmin nạp vào RAM khi vào game, không lưu lại khi đang online)."
      destructive={!isAdmin}
      requirePassword
      action={(form) => {
        form.set("playerId", String(playerId));
        form.set("mode", isAdmin ? "revoke" : "grant");
        return setPlayerAdminAction(null, form);
      }}
    />
  );
}
