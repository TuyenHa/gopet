import { notFound } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { PageHeader } from "@/components/data/page-header";
import { GiftcodeEditForm } from "@/components/giftcodes/giftcode-edit-form";
import { formatNumber } from "@/lib/format";
import { parseGiftDataArrays, parseGiftDataJson } from "@/lib/giftcodes/gift-data-schema";
import { getGiftcodeById, resolveGiftUsers } from "@/lib/giftcodes/giftcode-queries";
import { deleteGiftcodeAction, resetGiftcodeUsesAction } from "@/lib/giftcodes/giftcode-update-actions";
import { dbDateTimeToVnInputValue } from "@/lib/time/db-time";

export default async function GiftcodeDetailPage({ params }: PageProps<"/giftcodes/[id]">) {
  const { id: idParam } = await params;
  const id = Number(idParam);
  if (!Number.isInteger(id) || id <= 0) notFound();

  const row = await getGiftcodeById(id);
  if (!row) notFound();

  const users = await resolveGiftUsers(row);
  const giftEntries = parseGiftDataArrays(parseGiftDataJson(row.gift_data));

  return (
    <div className="max-w-2xl space-y-6">
      <PageHeader
        title={`Giftcode: ${row.code}`}
        description={`Đã dùng ${formatNumber(row.currentUser)} / ${formatNumber(row.maxUser)} lượt`}
        actions={
          <>
            <ConfirmDialog
              trigger={<Button variant="outline">Reset lượt dùng</Button>}
              title="Reset lượt dùng giftcode?"
              description="Xoá danh sách người/bang đã đổi và đưa số lượt đã dùng về 0 — mọi người có thể đổi lại."
              hiddenFields={{ id: String(id) }}
              action={resetGiftcodeUsesAction.bind(null, null)}
            />
            <ConfirmDialog
              trigger={<Button variant="destructive">Xoá</Button>}
              title="Xoá giftcode?"
              description="Không thể hoàn tác. Người chơi sẽ không đổi được mã này nữa."
              destructive
              requirePassword
              hiddenFields={{ id: String(id) }}
              action={deleteGiftcodeAction}
            />
          </>
        }
      />

      <GiftcodeEditForm
        id={id}
        code={row.code}
        maxUser={row.maxUser}
        expireVnInputValue={dbDateTimeToVnInputValue(row.expire)}
        isClanCode={!!row.isClanCode}
        giftEntries={giftEntries}
      />

      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium">Đã dùng bởi ({users.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {users.length === 0 ? (
            <p className="text-sm text-neutral-500">Chưa ai dùng.</p>
          ) : (
            <ul className="space-y-1 text-sm">
              {users.map((u) => (
                <li key={u.id}>{u.label}</li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
