import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { SearchBar } from "@/components/data/search-bar";
import { ServerOffSwitchPanel } from "@/components/accounts/server-off-switch-panel";
import { listAccounts, type AccountListRow } from "@/lib/accounts/account-queries";
import { requireAdmin } from "@/lib/auth/require-admin";
import { isServerOffSwitchActive } from "@/lib/players/server-off-switch";
import { formatDateTime, formatNumber } from "@/lib/format";
import { parsePage, parseQuery, type SearchParams } from "@/lib/pagination";

export default async function AccountsPage({ searchParams }: PageProps<"/accounts">) {
  const sp: SearchParams = await searchParams;
  const [ctx, result] = await Promise.all([requireAdmin(), listAccounts(sp)]);
  const info = parsePage(sp);

  return (
    <div className="space-y-4">
      <PageHeader title="Tài khoản" description="Bảng `user` — quản lý đăng nhập, ban, ngọc, 2FA" />
      {ctx.isSuperAdmin && <ServerOffSwitchPanel active={isServerOffSwitchActive()} />}
      <SearchBar defaultValue={parseQuery(sp)} placeholder="Tìm username / user_id / email" />
      <DataTable<AccountListRow>
        rows={result.rows}
        rowKey={(r) => r.user_id}
        columns={[
          {
            key: "user_id",
            header: "ID",
            render: (r) => (
              <Link href={`/accounts/${r.user_id}`} className="text-blue-600 hover:underline">
                {r.user_id}
              </Link>
            ),
          },
          { key: "username", header: "Username" },
          { key: "role", header: "Role" },
          {
            key: "isBaned",
            header: "Ban",
            render: (r) => (r.isBaned === 2 ? <Badge variant="destructive">Vĩnh viễn</Badge> : r.isBaned === 1 ? <Badge variant="destructive">Có hạn</Badge> : <Badge variant="secondary">Không</Badge>),
          },
          { key: "coin", header: "Ngọc", render: (r) => formatNumber(r.coin) },
          { key: "tongnap", header: "Tổng nạp", render: (r) => formatNumber(r.tongnap) },
          { key: "create_date", header: "Ngày tạo", render: (r) => formatDateTime(r.create_date) },
          {
            key: "isBcrypt",
            header: "Hash",
            render: (r) => (r.isBcrypt ? <Badge variant="secondary">bcrypt</Badge> : <Badge variant="destructive">legacy</Badge>),
          },
          { key: "has2fa", header: "2FA", render: (r) => (r.has2fa ? "Có" : "Không") },
          {
            key: "online",
            header: "Online",
            render: (r) => (r.online ? <Badge>Online</Badge> : <span className="text-neutral-400">—</span>),
          },
        ]}
      />
      <PaginationBar basePath="/accounts" searchParams={sp} info={info} total={result.total} />
    </div>
  );
}
