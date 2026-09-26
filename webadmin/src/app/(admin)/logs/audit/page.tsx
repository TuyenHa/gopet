import { DataTable, type Column } from "@/components/data/data-table";
import { DetailToggle } from "@/components/logs/detail-toggle";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { SearchBar } from "@/components/data/search-bar";
import { Input } from "@/components/ui/input";
import { formatDateTime } from "@/lib/format";
import { type AuditLogRow, listAuditLog } from "@/lib/logs/audit-queries";
import { firstParam, parsePage } from "@/lib/pagination";

export default async function AuditLogPage({ searchParams }: PageProps<"/logs/audit">) {
  const sp = await searchParams;
  const info = parsePage(sp);

  const admin = firstParam(sp.admin)?.trim().slice(0, 20) || undefined;
  const action = firstParam(sp.action)?.trim().slice(0, 64) || undefined;
  const target = firstParam(sp.target)?.trim().slice(0, 128) || undefined;
  const from = firstParam(sp.from) || undefined;
  const to = firstParam(sp.to) || undefined;

  const { rows, hasNext } = await listAuditLog({ admin, action, target, from, to }, info);

  const columns: Column<AuditLogRow>[] = [
    { key: "created_at", header: "Thời gian", render: (r) => formatDateTime(r.created_at) },
    { key: "admin_username", header: "Admin" },
    { key: "action", header: "Hành động" },
    { key: "target", header: "Đối tượng" },
    { key: "ip", header: "IP" },
    { key: "detail", header: "Chi tiết (before/after)", render: (r) => <DetailToggle value={r.detail} /> },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        title="Audit log"
        description="admin_audit_log — mặc định 7 ngày gần nhất khi không lọc theo admin/hành động/đối tượng. Mỗi thao tác ghi thêm dòng `:done`/`:failed` (xem cột Hành động)."
      />
      <SearchBar name="admin" defaultValue={admin} placeholder="Admin (tài khoản)">
        <Input type="text" name="action" defaultValue={action} placeholder="Hành động (vd: player.ban)" maxLength={64} className="h-8 w-52" />
        <Input type="text" name="target" defaultValue={target} placeholder="Đối tượng (vd: user:12)" maxLength={128} className="h-8 w-44" />
        <Input type="date" name="from" defaultValue={from} className="h-8 w-40" />
        <Input type="date" name="to" defaultValue={to} className="h-8 w-40" />
      </SearchBar>
      <DataTable columns={columns} rows={rows} rowKey={(r) => r.id} />
      <PaginationBar basePath="/logs/audit" searchParams={sp} info={info} total={null} hasNext={hasNext} />
    </div>
  );
}
