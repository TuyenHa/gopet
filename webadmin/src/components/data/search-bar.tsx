import type { ReactNode } from "react";
import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

/**
 * Form GET thuần (không cần JS): gửi ?q= về chính trang, reset về trang 1.
 * `children` = các ô lọc thêm (select/input có name) nằm cùng form.
 */
export function SearchBar({
  defaultValue,
  placeholder = "Tìm kiếm...",
  name = "q",
  children,
}: {
  defaultValue?: string;
  placeholder?: string;
  name?: string;
  children?: ReactNode;
}) {
  return (
    <form method="get" className="flex flex-wrap items-center gap-2">
      <div className="relative w-full max-w-xs">
        <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-neutral-400" />
        <Input name={name} defaultValue={defaultValue} placeholder={placeholder} className="pl-8" maxLength={100} />
      </div>
      {children}
      <Button type="submit" variant="outline">
        Lọc
      </Button>
    </form>
  );
}
