import { DataTable, type Column } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { type ClanRow, listClans } from "@/lib/clans/clan-queries";
import { formatDateTime, formatNumber } from "@/lib/format";
import { parsePage } from "@/lib/pagination";

export default async function ClansPage({ searchParams }: PageProps<"/clans">) {
  const sp = await searchParams;
  const info = parsePage(sp);
  const { rows, hasNext } = await listClans(info);

  const columns: Column<ClanRow>[] = [
    { key: "clanId", header: "Mã" },
    { key: "name", header: "Tên bang hội" },
    { key: "lvl", header: "Cấp" },
    { key: "leaderName", header: "Bang chủ" },
    { key: "memberCount", header: "Thành viên" },
    { key: "fund", header: "Quỹ", render: (r) => formatNumber(r.fund) },
    { key: "timeCreate", header: "Ngày tạo", render: (r) => formatDateTime(r.timeCreate) },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        title="Bang hội"
        description="Dữ liệu runtime trong RAM của GServer, server ghi đè khi lưu — CHỈ ĐỌC, không có thao tác chỉnh sửa ở đây."
      />
      <DataTable columns={columns} rows={rows} rowKey={(r) => r.clanId} />
      <PaginationBar basePath="/clans" searchParams={sp} info={info} total={null} hasNext={hasNext} />
    </div>
  );
}
