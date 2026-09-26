"use client";

import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { Button } from "@/components/ui/button";
import { deleteItemAction } from "@/lib/players/inventory-actions";

/** Xoá vĩnh viễn 1 item — nếu đang được pet đeo, server tự gỡ khỏi `equip` của pet đó. */
export function InventoryItemDeleteButton({
  playerId,
  itemId,
  expectedMd5,
}: {
  playerId: number;
  itemId: number;
  expectedMd5: string;
}) {
  return (
    <ConfirmDialog
      trigger={
        <Button variant="destructive" size="sm">
          Xoá
        </Button>
      }
      title={`Xoá vật phẩm #${itemId}?`}
      description="Không hoàn tác được qua giao diện — bản cũ đã lưu trong audit log (khôi phục thủ công bằng SQL nếu cần). Nếu vật phẩm đang được pet đeo, hệ thống tự gỡ khỏi pet đó."
      confirmLabel="Xoá"
      destructive
      action={async (form) => {
        form.set("playerId", String(playerId));
        form.set("itemId", String(itemId));
        form.set("expectedMd5", expectedMd5);
        return deleteItemAction(null, form);
      }}
    />
  );
}
