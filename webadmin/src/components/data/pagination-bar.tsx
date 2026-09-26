import Link from "next/link";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { buildHref, type PageInfo, type SearchParams } from "@/lib/pagination";

/**
 * Phân trang qua query string. `total` = null khi không đếm (bảng log lớn) — khi đó dựa
 * vào `hasNext` (truy vấn LIMIT size+1).
 */
export function PaginationBar({
  basePath,
  searchParams,
  info,
  total,
  hasNext,
}: {
  basePath: string;
  searchParams: SearchParams;
  info: PageInfo;
  total: number | null;
  hasNext?: boolean;
}) {
  const lastPage = total === null ? null : Math.max(1, Math.ceil(total / info.size));
  const canNext = lastPage === null ? !!hasNext : info.page < lastPage;
  const href = (page: number) => buildHref(basePath, searchParams, { page: page === 1 ? undefined : page });

  return (
    <div className="flex items-center justify-between gap-2 py-3 text-sm text-neutral-500">
      <span>
        Trang {info.page}
        {lastPage !== null && ` / ${lastPage}`}
        {total !== null && ` · ${total.toLocaleString("vi-VN")} dòng`}
      </span>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" asChild>
          {info.page > 1 ? (
            <Link href={href(info.page - 1)}>
              <ChevronLeft className="size-4" /> Trước
            </Link>
          ) : (
            <span aria-disabled className="pointer-events-none opacity-50">
              <ChevronLeft className="size-4" /> Trước
            </span>
          )}
        </Button>
        <Button variant="outline" size="sm" asChild>
          {canNext ? (
            <Link href={href(info.page + 1)}>
              Sau <ChevronRight className="size-4" />
            </Link>
          ) : (
            <span aria-disabled className="pointer-events-none opacity-50">
              Sau <ChevronRight className="size-4" />
            </span>
          )}
        </Button>
      </div>
    </div>
  );
}
