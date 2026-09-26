import { TriangleAlert } from "lucide-react";

/** GServer chỉ nạp template lúc khởi động (GopetManager.init) → sửa xong phải restart. */
export function RestartRequiredBanner() {
  return (
    <div className="flex items-start gap-2 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
      <TriangleAlert className="mt-0.5 size-4 shrink-0" />
      <span>Dữ liệu template chỉ áp dụng sau khi khởi động lại GServer.</span>
    </div>
  );
}
