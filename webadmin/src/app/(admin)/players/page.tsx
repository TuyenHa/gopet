import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { SearchBar } from "@/components/data/search-bar";
import { listPlayers, type PlayerListRow } from "@/lib/players/player-queries";
import { formatDateTime, formatNumber } from "@/lib/format";
import { parsePage, parseQuery, type SearchParams } from "@/lib/pagination";

export default async function PlayersPage({ searchParams }: PageProps<"/players">) {
  const sp: SearchParams = await searchParams;
  const result = await listPlayers(sp);
  const info = parsePage(sp);

  return (
    <div className="space-y-4">
      <PageHeader title="Nhân vật" description="Bảng `player` — chỉ sửa trực tiếp được khi nhân vật offline" />
      <SearchBar defaultValue={parseQuery(sp)} placeholder="Tìm tên nhân vật / user_id / ID" />
      <DataTable<PlayerListRow>
        rows={result.rows}
        rowKey={(r) => r.ID}
        columns={[
          {
            key: "ID",
            header: "ID",
            render: (r) => (
              <Link href={`/players/${r.ID}`} className="text-blue-600 hover:underline">
                {r.ID}
              </Link>
            ),
          },
          { key: "user_id", header: "user_id" },
          { key: "name", header: "Tên" },
          { key: "isAdmin", header: "Admin", render: (r) => (r.isAdmin ? <Badge variant="outline">Admin</Badge> : "—") },
          { key: "gold", header: "Vàng", render: (r) => formatNumber(r.gold) },
          { key: "coin", header: "Xu", render: (r) => formatNumber(r.coin) },
          { key: "star", header: "Sao" },
          { key: "LastTimeOnline", header: "Online lần cuối", render: (r) => formatDateTime(r.LastTimeOnline) },
          {
            key: "online",
            header: "Trạng thái",
            render: (r) => (r.online ? <Badge>Online</Badge> : <span className="text-neutral-400">Offline</span>),
          },
        ]}
      />
      <PaginationBar basePath="/players" searchParams={sp} info={info} total={result.total} />
    </div>
  );
}
