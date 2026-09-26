"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ActionForm } from "@/components/data/action-form";
import { updateItemFieldsAction } from "@/lib/players/inventory-actions";

/** Form sửa 5 field có sẵn của 1 item (count/lvl/expire/canTrade/durability) — không tạo field mới. */
export function InventoryItemEditForm({
  playerId,
  itemId,
  expectedMd5,
  count,
  lvl,
  expire,
  canTrade,
  durability,
}: {
  playerId: number;
  itemId: number;
  expectedMd5: string;
  count: number;
  lvl: number;
  expire: number;
  canTrade: boolean;
  /** null = item chưa từng có độ bền (không phải trang bị pet) → không hiện field này. */
  durability: number | null;
}) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          Sửa
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Sửa vật phẩm #{itemId}</DialogTitle>
        </DialogHeader>
        <ActionForm action={updateItemFieldsAction} submitLabel="Lưu" onSuccess={() => setOpen(false)}>
          <input type="hidden" name="playerId" value={playerId} />
          <input type="hidden" name="itemId" value={itemId} />
          <input type="hidden" name="expectedMd5" value={expectedMd5} />
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor={`count-${itemId}`}>Số lượng</Label>
              <Input id={`count-${itemId}`} name="count" type="number" min={0} defaultValue={count} required />
            </div>
            <div className="space-y-1">
              <Label htmlFor={`lvl-${itemId}`}>Cấp cường hoá (lvl)</Label>
              <Input id={`lvl-${itemId}`} name="lvl" type="number" min={0} defaultValue={lvl} required />
            </div>
            <div className="space-y-1">
              <Label htmlFor={`expire-${itemId}`}>Hết hạn (ms, -1 = vô hạn)</Label>
              <Input id={`expire-${itemId}`} name="expire" type="number" defaultValue={expire} required />
            </div>
            <div className="space-y-1">
              <Label htmlFor={`canTrade-${itemId}`}>Giao dịch</Label>
              <select
                id={`canTrade-${itemId}`}
                name="canTrade"
                defaultValue={String(canTrade)}
                className="h-9 w-full rounded-lg border border-input bg-transparent px-2.5 text-sm"
              >
                <option value="true">Được phép</option>
                <option value="false">Đã khoá</option>
              </select>
            </div>
            {durability !== null && (
              <div className="space-y-1">
                <Label htmlFor={`durability-${itemId}`}>Độ bền (0-80)</Label>
                <Input
                  id={`durability-${itemId}`}
                  name="durability"
                  type="number"
                  min={0}
                  max={80}
                  defaultValue={durability}
                  required
                />
              </div>
            )}
          </div>
        </ActionForm>
      </DialogContent>
    </Dialog>
  );
}
