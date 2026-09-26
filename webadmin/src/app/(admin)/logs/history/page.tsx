import { DataTable, type Column } from "@/components/data/data-table";
import { DetailToggle } from "@/components/logs/detail-toggle";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { SearchBar } from "@/components/data/search-bar";
import { Input } from "@/components/ui/input";
import { formatDateTime } from "@/lib/format";
import { type HistoryRow, listHistory } from "@/lib/logs/history-queries";
import { firstParam, parsePage } from "@/lib/pagination";

// targetId có thể được link từ trang chi tiết nhân vật qua ?targetId=<user_id>.
export default async function HistoryLogPage({ searchParams }: PageProps<"/logs/history">) {
  const sp = await searchParams;
  const info = parsePage(sp);

  const targetIdRaw = firstParam(sp.targetId);
  const targetIdNum = targetIdRaw ? Number.parseInt(targetIdRaw, 10) : NaN;
  const targetId = Number.isFinite(targetIdNum) ? targetIdNum : undefined;
  const charname = firstParam(sp.charname)?.trim().slice(0, 50) || undefined;
  const keyword = firstParam(sp.keyword)?.trim().slice(0, 100) || undefined;
  const from = firstParam(sp.from) || undefined;
  const to = firstParam(sp.to) || undefined;

  const { rows, hasNext } = await listHistory({ targetId, charname, keyword, from, to }, info);

  const columns: Column<HistoryRow>[] = [
    { key: "targetId", header: "Mã người chơi" },
    { key: "charname", header: "Nhân vật" },
    { key: "timeDB", header: "Thời gian", render: (r) => formatDateTime(r.timeDB) },
    { key: "log", header: "Log" },
    { key: "obj", header: "Chi tiết (obj)", render: (r) => <DetailToggle value={r.obj} /> },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        title="Lịch sử người chơi"
        description="gp_log.history — mặc định 7 ngày gần nhất khi không lọc theo mã người chơi/nhân vật."
      />
      <SearchBar name="charname" defaultValue={charname} placeholder="Tên nhân vật (charname)">
        <Input type="number" name="targetId" defaultValue={targetIdRaw ?? ""} placeholder="Mã người chơi" className="h-8 w-36" />
        <Input type="text" name="keyword" defaultValue={keyword} placeholder="Từ khoá trong log" maxLength={100} className="h-8 w-48" />
        <Input type="date" name="from" defaultValue={from} className="h-8 w-40" />
        <Input type="date" name="to" defaultValue={to} className="h-8 w-40" />
      </SearchBar>
      <DataTable columns={columns} rows={rows} rowKey={(r, i) => r.eventId ?? `${r.targetId}-${r.timeDB}-${i}`} />
      <PaginationBar basePath="/logs/history" searchParams={sp} info={info} total={null} hasNext={hasNext} />
    </div>
  );
}
