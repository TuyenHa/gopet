"use client";

import { useActionState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { loginAction, type LoginState } from "@/lib/auth/login-action";

export function LoginForm() {
  const [state, action, pending] = useActionState<LoginState, FormData>(loginAction, {});

  return (
    <form action={action} className="mt-6 space-y-4">
      <div className="space-y-2">
        <Label htmlFor="username">Tài khoản</Label>
        <Input id="username" name="username" autoComplete="username" required maxLength={20} autoFocus />
      </div>
      <div className="space-y-2">
        <Label htmlFor="password">Mật khẩu</Label>
        <Input id="password" name="password" type="password" autoComplete="current-password" required maxLength={100} />
      </div>
      {state.error && <p className="text-sm text-red-600">{state.error}</p>}
      <Button type="submit" className="w-full" disabled={pending}>
        {pending ? "Đang đăng nhập..." : "Đăng nhập"}
      </Button>
    </form>
  );
}
