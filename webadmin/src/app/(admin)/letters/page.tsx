import Link from "next/link";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { formatDateTime } from "@/lib/format";
import { listPendingLetters } from "@/lib/letters/letter-queries";

const TYPE_LABEL: Record<number, string> = { 1: "Bạn bè", 2: "Admin", 3: "Sự kiện" };

/** Hàng đợi thư CHƯA giao — chỉ xem. Xem `letter-queries.ts` vì sao không có nút huỷ. */
export default async function LettersPage({ searchParams }: PageProps<"/letters">) {
  const sp = await searchParams;
  const { rows, info, hasNext } = await listPendingLetters(sp);

  return (
    <div className="space-y-4">
      <PageHeader
        title="Thư hệ thống"
        description="Hàng đợi thư chưa giao — người chơi nhận ở lần đăng nhập kế tiếp. Chỉ xem, không huỷ được (bảng letter không có khoá chính)."
        actions={
          <Button asChild>
            <Link href="/letters/send">Gửi thư</Link>
          </Button>
        }
      />
      <DataTable
        rows={rows}
        rowKey={(r, i) => `${r.targetId}-${r.time}-${i}`}
        columns={[
          { key: "targetId", header: "user_id nhận" },
          { key: "Type", header: "Loại", render: (r) => TYPE_LABEL[r.Type] ?? r.Type },
          { key: "Title", header: "Tiêu đề" },
          { key: "time", header: "Thời gian tạo (giờ DB)", render: (r) => formatDateTime(r.time) },
        ]}
      />
      <PaginationBar basePath="/letters" searchParams={sp} info={info} total={null} hasNext={hasNext} />
    </div>
  );
}
