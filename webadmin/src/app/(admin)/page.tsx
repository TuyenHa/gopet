import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { getDashboardStats } from "@/lib/dashboard/dashboard-queries";
import { formatDateTime, formatNumber } from "@/lib/format";
import { cn } from "@/lib/utils";

// Web coi server là "sống" nếu heartbeat trong vòng 90s (khớp guard sửa người chơi).
const HEARTBEAT_FRESH_SEC = 90;

function Stat({ label, value, hint }: { label: string; value: React.ReactNode; hint?: React.ReactNode }) {
  return (
    <Card className="gap-2 py-4">
      <CardHeader className="px-4">
        <CardTitle className="text-sm font-normal text-neutral-500">{label}</CardTitle>
      </CardHeader>
      <CardContent className="px-4">
        <div className="text-2xl font-semibold">{value}</div>
        {hint && <div className="mt-1 text-xs text-neutral-500">{hint}</div>}
      </CardContent>
    </Card>
  );
}

export default async function DashboardPage() {
  const s = await getDashboardStats();
  const alive = s.heartbeatAgeSec !== null && s.heartbeatAgeSec <= HEARTBEAT_FRESH_SEC;

  return (
    <div className="space-y-6">
      <PageHeader title="Dashboard" description="Tổng quan máy chủ Gopet" />
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-5">
        <Stat label="Tài khoản" value={formatNumber(s.totalUsers)} />
        <Stat label="Nhân vật" value={formatNumber(s.totalPlayers)} />
        <Stat
          label="Đang online"
          value={s.onlinePlayers === null ? "—" : formatNumber(s.onlinePlayers)}
          hint={s.onlinePlayers === null ? "GServer chưa có bảng player_online" : undefined}
        />
        <Stat
          label="GServer"
          value={
            <span className="flex items-center gap-2">
              <span className={cn("size-3 rounded-full", alive ? "bg-green-500" : "bg-red-500")} />
              {alive ? "Hoạt động" : "Không phản hồi"}
            </span>
          }
          hint={s.heartbeatAgeSec === null ? "Chưa có heartbeat" : `Heartbeat ${s.heartbeatAgeSec}s trước`}
        />
        <Stat label="Giftcode còn hiệu lực" value={formatNumber(s.activeGiftcodes)} />
      </div>

      <section className="space-y-2">
        <div className="flex items-center justify-between">
          <h2 className="font-medium">Thao tác quản trị gần đây</h2>
          <Link href="/logs/audit" className="text-sm text-blue-600 hover:underline">
            Xem tất cả
          </Link>
        </div>
        <DataTable
          rows={s.recentAudit}
          rowKey={(r) => r.id}
          columns={[
            { key: "created_at", header: "Thời gian", render: (r) => formatDateTime(r.created_at) },
            { key: "admin_username", header: "Admin" },
            { key: "action", header: "Hành động", render: (r) => <code className="text-xs">{r.action}</code> },
            { key: "target", header: "Đối tượng", render: (r) => r.target ?? "—" },
          ]}
        />
      </section>
    </div>
  );
}
