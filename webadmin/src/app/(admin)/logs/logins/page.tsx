import { DataTable, type Column } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { SearchBar } from "@/components/data/search-bar";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { formatDateTime } from "@/lib/format";
import { type LoginHistoryRow, listLoginHistory } from "@/lib/logs/login-queries";
import { firstParam, parsePage } from "@/lib/pagination";

const SELECT_CLASS =
  "h-8 rounded-lg border border-input bg-transparent px-2.5 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50";

export default async function LoginHistoryPage({ searchParams }: PageProps<"/logs/logins">) {
  const sp = await searchParams;
  const info = parsePage(sp);

  const username = firstParam(sp.username)?.trim().slice(0, 100) || undefined;
  const ip = firstParam(sp.ip)?.trim().slice(0, 100) || undefined;
  const successRaw = firstParam(sp.success);
  const success = successRaw === "1" ? true : successRaw === "0" ? false : undefined;
  const from = firstParam(sp.from) || undefined;
  const to = firstParam(sp.to) || undefined;

  const { rows, hasNext } = await listLoginHistory({ username, ip, success, from, to }, info);

  const columns: Column<LoginHistoryRow>[] = [
    { key: "UserName", header: "Tài khoản" },
    { key: "IPAddress", header: "IP" },
    { key: "LoginTime", header: "Thời gian", render: (r) => formatDateTime(r.LoginTime) },
    {
      key: "IsSuccess",
      header: "Kết quả",
      render: (r) =>
        r.IsSuccess ? <Badge variant="secondary">Thành công</Badge> : <Badge variant="destructive">Thất bại</Badge>,
    },
    { key: "IsWebLogin", header: "Nguồn", render: (r) => (r.IsWebLogin ? "Web" : "Game client") },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        title="Lịch sử đăng nhập"
        description="gopettae_gopet_web.login_history — mặc định 7 ngày gần nhất khi không lọc theo tài khoản/IP. Web admin không ghi vào bảng này."
      />
      <SearchBar name="username" defaultValue={username} placeholder="Tài khoản">
        <Input type="text" name="ip" defaultValue={ip} placeholder="Địa chỉ IP" maxLength={100} className="h-8 w-40" />
        <select name="success" defaultValue={successRaw ?? ""} className={SELECT_CLASS}>
          <option value="">Tất cả kết quả</option>
          <option value="1">Thành công</option>
          <option value="0">Thất bại</option>
        </select>
        <Input type="date" name="from" defaultValue={from} className="h-8 w-40" />
        <Input type="date" name="to" defaultValue={to} className="h-8 w-40" />
      </SearchBar>
      <DataTable columns={columns} rows={rows} rowKey={(r) => r.Id} />
      <PaginationBar basePath="/logs/logins" searchParams={sp} info={info} total={null} hasNext={hasNext} />
    </div>
  );
}
