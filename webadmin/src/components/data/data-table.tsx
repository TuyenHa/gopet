import type { ReactNode } from "react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { cn } from "@/lib/utils";

export interface Column<T> {
  key: string;
  header: ReactNode;
  className?: string;
  /** Mặc định hiển thị row[key]. Hàm render chạy phía server (Server Component). */
  render?: (row: T) => ReactNode;
}

/**
 * Bảng dữ liệu dùng chung, render ở server — dữ liệu đã được phân trang bằng SQL
 * (LIMIT/OFFSET), không bao giờ tải cả bảng xuống client.
 */
export function DataTable<T>({
  columns,
  rows,
  rowKey,
  empty = "Không có dữ liệu",
}: {
  columns: Column<T>[];
  rows: T[];
  rowKey: (row: T, index: number) => string | number;
  empty?: ReactNode;
}) {
  return (
    <div className="overflow-x-auto rounded-lg border bg-white">
      <Table>
        <TableHeader>
          <TableRow className="bg-neutral-50">
            {columns.map((c) => (
              <TableHead key={c.key} className={cn("whitespace-nowrap", c.className)}>
                {c.header}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableRow>
              <TableCell colSpan={columns.length} className="py-8 text-center text-neutral-500">
                {empty}
              </TableCell>
            </TableRow>
          ) : (
            rows.map((row, i) => (
              <TableRow key={rowKey(row, i)}>
                {columns.map((c) => (
                  <TableCell key={c.key} className={c.className}>
                    {c.render ? c.render(row) : String((row as Record<string, unknown>)[c.key] ?? "")}
                  </TableCell>
                ))}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
