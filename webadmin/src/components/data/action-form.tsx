"use client";

import { useActionState, useEffect, useRef, type ReactNode } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import type { ActionResult } from "@/lib/actions/action-result";

export type FormAction = (prev: ActionResult<unknown> | null, form: FormData) => Promise<ActionResult<unknown>>;

/**
 * Form gọi Server Action + toast kết quả. Dùng chung cho mọi form ghi (DRY).
 * Action phải có chữ ký `(prev, formData) => Promise<ActionResult>`.
 */
export function ActionForm({
  action,
  children,
  submitLabel = "Lưu",
  submitVariant = "default",
  resetOnSuccess = false,
  confirmMessage,
  onSuccess,
  className = "space-y-4",
}: {
  action: FormAction;
  children: ReactNode;
  submitLabel?: string;
  submitVariant?: "default" | "destructive" | "outline";
  resetOnSuccess?: boolean;
  /** Hỏi xác nhận (window.confirm) trước khi gửi — cho thao tác nguy hiểm. */
  confirmMessage?: string;
  onSuccess?: (result: ActionResult<unknown>) => void;
  className?: string;
}) {
  const [state, formAction, pending] = useActionState(action, null);
  const formRef = useRef<HTMLFormElement>(null);

  useEffect(() => {
    if (!state) return;
    if (state.ok) {
      toast.success(state.message ?? "Đã lưu");
      if (resetOnSuccess) formRef.current?.reset();
      onSuccess?.(state);
    } else {
      toast.error(state.error);
    }
    // Chỉ phản ứng khi có kết quả mới.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  return (
    <form
      ref={formRef}
      action={formAction}
      className={className}
      onSubmit={(e) => {
        if (confirmMessage && !window.confirm(confirmMessage)) e.preventDefault();
      }}
    >
      {children}
      {state && !state.ok && <p className="text-sm text-red-600">{state.error}</p>}
      <Button type="submit" variant={submitVariant} disabled={pending}>
        {pending ? "Đang xử lý..." : submitLabel}
      </Button>
    </form>
  );
}
