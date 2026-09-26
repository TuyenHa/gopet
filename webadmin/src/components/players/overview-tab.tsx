import Link from "next/link";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { AdminToggleButton } from "@/components/players/admin-toggle-button";
import { CurrencyAdjustForm } from "@/components/players/currency-adjust-form";
import { GiftGoldForm } from "@/components/players/gift-gold-form";
import { OptimisticFieldForm } from "@/components/players/optimistic-field-form";
import type { PlayerOverview } from "@/lib/players/player-detail-queries";
import { formatDateTime, formatNumber } from "@/lib/format";

export function OverviewTab({
  data,
  isSuperAdmin,
  isSelf,
}: {
  data: PlayerOverview;
  isSuperAdmin: boolean;
  isSelf: boolean;
}) {
  return (
    <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Thông tin</CardTitle>
        </CardHeader>
        <CardContent className="space-y-1 text-sm">
          <p>Tên: {data.name}</p>
          <p>user_id: {data.user_id}</p>
          <p>Vàng: {formatNumber(data.gold)} · Xu: {formatNumber(data.coin)} · Lúa: {formatNumber(data.lua)}</p>
          <p>Sao: {data.star} · PK point: {data.pkPoint}</p>
          <p>Điểm sự kiện: {data.EventPoint} · Điểm tích luỹ: {data.AccumulatedPoint}</p>
          <p>clanId: {data.clanId < 0 ? "không có bang" : data.clanId} (chỉ xem — bang tự quản lý tên/thành viên)</p>
          <p>Đăng nhập lần đầu phiên: {formatDateTime(data.loginDate)}</p>
          <p>Online lần cuối: {formatDateTime(data.LastTimeOnline)}</p>
          <div className="flex items-center gap-2 pt-2">
            <span>isAdmin: {data.isAdmin ? "Có" : "Không"}</span>
            {isSuperAdmin && <AdminToggleButton playerId={data.ID} isAdmin={!!data.isAdmin} isSelf={isSelf} />}
          </div>
          <Link href={`/logs/history?targetId=${data.user_id}`} className="text-blue-600 hover:underline">
            Xem lịch sử người chơi
          </Link>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Sửa tiền tệ (chỉ khi offline)</CardTitle>
        </CardHeader>
        <CardContent>
          <CurrencyAdjustForm playerId={data.ID} />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Tặng vàng (qua hàng đợi — được cả khi online)</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <GiftGoldForm userId={data.user_id} />
          <div className="flex flex-col gap-1 text-sm">
            <Link href={`/giftcodes/new?forUserId=${data.user_id}`} className="text-blue-600 hover:underline">
              Tặng vật phẩm/pet qua giftcode riêng
            </Link>
            <Link href={`/letters?targetUserId=${data.user_id}`} className="text-blue-600 hover:underline">
              Gửi thư (vật phẩm/pet)
            </Link>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Các cột khác (chỉ khi offline — optimistic)</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <OptimisticFieldForm playerId={data.ID} field="star" label="Sao (star)" currentValue={data.star} />
          <OptimisticFieldForm playerId={data.ID} field="pkPoint" label="PK point" currentValue={data.pkPoint} />
          <OptimisticFieldForm
            playerId={data.ID}
            field="EventPoint"
            label="Điểm sự kiện"
            currentValue={data.EventPoint}
          />
          <OptimisticFieldForm
            playerId={data.ID}
            field="AccumulatedPoint"
            label="Điểm tích luỹ"
            currentValue={data.AccumulatedPoint}
          />
          <OptimisticFieldForm
            playerId={data.ID}
            field="gender"
            label="Giới tính (-1 chưa đặt, 0/1)"
            currentValue={data.gender}
          />
          <OptimisticFieldForm
            playerId={data.ID}
            field="avatarPath"
            label="Đường dẫn avatar"
            currentValue={data.avatarPath}
            type="text"
          />
        </CardContent>
      </Card>
    </div>
  );
}
