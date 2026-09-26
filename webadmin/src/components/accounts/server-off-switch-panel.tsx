"use client";

import { useState, useTransition } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { activateServerOffSwitchAction, deactivateServerOffSwitchAction } from "@/lib/accounts/account-actions";

/**
 * Chỉ super-admin thấy. Bật/tắt việc coi máy chủ game là đã TẮT (bỏ qua kiểm tra heartbeat khi
 * sửa `player` trực tiếp) — dùng khi bảo trì. Luôn yêu cầu nhập lại mật khẩu [RT#7].
 */
export function ServerOffSwitchPanel({ active }: { active: boolean }) {
  const [pending, start] = useTransition();
  const [password, setPassword] = useState("");

  const run = (action: typeof activateServerOffSwitchAction) =>
    start(async () => {
      const form = new FormData();
      form.set("confirmPassword", password);
      const res = await action(null, form);
      if (res.ok) {
        toast.success(res.message ?? "Đã cập nhật");
        setPassword("");
      } else {
        toast.error(res.error);
      }
    });

  return (
    <div className="flex flex-wrap items-end gap-3 rounded-lg border border-amber-300 bg-amber-50 p-3">
      <div className="space-y-2">
        <Label htmlFor="switchPassword">Mật khẩu của bạn (bắt buộc)</Label>
        <Input
          id="switchPassword"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="w-56"
        />
      </div>
      <Button
        type="button"
        variant={active ? "outline" : "destructive"}
        disabled={pending || !password}
        onClick={() => run(active ? deactivateServerOffSwitchAction : activateServerOffSwitchAction)}
      >
        {active ? "Tắt công tắc (server đã chạy lại)" : "Xác nhận: server đã TẮT (2 giờ)"}
      </Button>
      <p className="w-full text-xs text-amber-700">
        Đang {active ? "BẬT" : "TẮT"} — khi bật, sửa dữ liệu `player` trực tiếp bỏ qua kiểm tra heartbeat GServer.
      </p>
    </div>
  );
}
