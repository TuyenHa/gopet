import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { SearchBar } from "@/components/data/search-bar";
import { firstParam } from "@/lib/pagination";
import { formatDateTime, formatNumber } from "@/lib/format";
import { listGiftcodes, type GiftcodeStatus } from "@/lib/giftcodes/giftcode-queries";

const STATUS_LABEL: Record<GiftcodeStatus, { label: string; variant: "default" | "secondary" | "destructive" }> = {
  active: { label: "Còn hiệu lực", variant: "default" },
  expired: { label: "Hết hạn", variant: "destructive" },
  full: { label: "Hết lượt", variant: "secondary" },
};

export default async function GiftcodesPage({ searchParams }: PageProps<"/giftcodes">) {
  const sp = await searchParams;
  const { rows, total, info } = await listGiftcodes(sp);

  return (
    <div className="space-y-4">
      <PageHeader
        title="Giftcode"
        description="Mã quà tặng — server đọc trực tiếp từ DB khi người chơi nhập mã trong game."
        actions={
          <Button asChild>
            <Link href="/giftcodes/new">Tạo giftcode</Link>
          </Button>
        }
      />
      <SearchBar defaultValue={firstParam(sp.q)} placeholder="Tìm theo code..." />
      <DataTable
        rows={rows}
        rowKey={(r) => r.id}
        columns={[
          {
            key: "code",
            header: "Code",
            render: (r) => (
              <Link href={`/giftcodes/${r.id}`} className="font-medium text-blue-600 hover:underline">
                {r.code}
              </Link>
            ),
          },
          { key: "uses", header: "Đã dùng / tối đa", render: (r) => `${formatNumber(r.currentUser)} / ${formatNumber(r.maxUser)}` },
          { key: "expire", header: "Hạn (giờ DB)", render: (r) => formatDateTime(r.expire) },
          { key: "isClanCode", header: "Loại", render: (r) => (r.isClanCode ? <Badge variant="outline">Bang hội</Badge> : "Cá nhân") },
          {
            key: "status",
            header: "Trạng thái",
            render: (r) => <Badge variant={STATUS_LABEL[r.status].variant}>{STATUS_LABEL[r.status].label}</Badge>,
          },
        ]}
      />
      <PaginationBar basePath="/giftcodes" searchParams={sp} info={info} total={total} />
    </div>
  );
}
