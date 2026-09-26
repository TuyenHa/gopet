import Link from "next/link";
import { notFound } from "next/navigation";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { PageHeader } from "@/components/data/page-header";
import { AdjustCoinForm } from "@/components/accounts/adjust-coin-form";
import { BanAccountForm } from "@/components/accounts/ban-account-form";
import { Clear2faButton } from "@/components/accounts/clear-2fa-button";
import { LockToggleButton } from "@/components/accounts/lock-toggle-button";
import { ResetPasswordDialog } from "@/components/accounts/reset-password-dialog";
import { getAccountDetail } from "@/lib/accounts/account-queries";
import { requireAdmin } from "@/lib/auth/require-admin";
import { formatDateTime, formatEpochMs, formatNumber } from "@/lib/format";

export default async function AccountDetailPage({ params }: PageProps<"/accounts/[id]">) {
  const { id } = await params;
  const userId = Number.parseInt(id, 10);
  if (!Number.isInteger(userId) || userId <= 0) notFound();

  const [ctx, account] = await Promise.all([requireAdmin(), getAccountDetail(userId)]);
  if (!account) notFound();

  const canActSensitive = ctx.isSuperAdmin || !account.isProtectedAdmin;
  const requirePassword = account.isProtectedAdmin; // luôn super-admin khi tới đây nên phải reauth

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Tài khoản #${account.user_id} — ${account.username}`}
        description={
          <span className="flex flex-wrap items-center gap-2">
            {account.isBcrypt ? (
              <Badge variant="secondary">bcrypt</Badge>
            ) : (
              <Badge variant="destructive">Mật khẩu legacy — cần buộc reset</Badge>
            )}
            {account.has2fa && <Badge variant="secondary">2FA bật</Badge>}
            {account.online && <Badge>Online</Badge>}
            {account.isProtectedAdmin && <Badge variant="outline">Tài khoản admin</Badge>}
          </span>
        }
        actions={
          <Link href={`/logs/history?targetId=${account.user_id}`} className="text-sm text-blue-600 hover:underline">
            Lịch sử người chơi
          </Link>
        }
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Thông tin</CardTitle>
          </CardHeader>
          <CardContent className="space-y-1 text-sm">
            <p>Role: {account.role}</p>
            <p>Email: {account.email || "—"}</p>
            <p>Điện thoại: {account.phone || "—"}</p>
            <p>Ngọc: {formatNumber(account.coin)}</p>
            <p>Tổng nạp: {formatNumber(account.tongnap)}</p>
            <p>Ngày tạo: {formatDateTime(account.create_date)}</p>
            <p>
              Ban: {account.isBaned === 0 ? "Không" : account.isBaned === 2 ? "Vĩnh viễn" : `Đến ${formatEpochMs(account.banTime)}`}
            </p>
            {account.banReason && <p>Lý do ban: {account.banReason}</p>}
            <div className="flex gap-2 pt-2">
              <LockToggleButton userId={account.user_id} role={account.role} />
              {canActSensitive && <Clear2faButton userId={account.user_id} requirePassword={requirePassword} />}
              {canActSensitive && <ResetPasswordDialog userId={account.user_id} requirePassword={requirePassword} />}
            </div>
            {!canActSensitive && (
              <p className="pt-2 text-xs text-amber-600">
                Đây là tài khoản admin khác — chỉ super-admin được reset mật khẩu/2FA/ban.
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Ban / Unban</CardTitle>
          </CardHeader>
          <CardContent>
            {canActSensitive ? (
              <BanAccountForm userId={account.user_id} banReason={account.banReason} />
            ) : (
              <p className="text-xs text-amber-600">Chỉ super-admin được ban/unban tài khoản admin khác.</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Cộng/trừ ngọc</CardTitle>
          </CardHeader>
          <CardContent>
            <AdjustCoinForm userId={account.user_id} />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
