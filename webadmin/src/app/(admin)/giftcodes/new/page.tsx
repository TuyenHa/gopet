import { notFound } from "next/navigation";
import { PageHeader } from "@/components/data/page-header";
import { GiftcodeCreateForm } from "@/components/giftcodes/giftcode-create-form";
import { requireAdmin } from "@/lib/auth/require-admin";
import { resolvePlayerTarget } from "@/lib/letters/send-system-letter";
import { firstParam } from "@/lib/pagination";

/** `?forUserId=<user_id>` (đến từ phase 5B — tặng item/pet an toàn): tạo code riêng cho 1
 * người, dựng sẵn quà đúng chỉ số qua server thay vì admin chỉnh JSON tay trên player. */
export default async function NewGiftcodePage({ searchParams }: PageProps<"/giftcodes/new">) {
  await requireAdmin();
  const sp = await searchParams;
  const rawForUserId = firstParam(sp.forUserId);
  const forUserId = rawForUserId ? Number(rawForUserId) : undefined;

  let targetLabel: string | undefined;
  if (forUserId !== undefined) {
    if (!Number.isInteger(forUserId) || forUserId <= 0) notFound();
    const target = await resolvePlayerTarget(String(forUserId));
    if (!target) notFound();
    targetLabel = `${target.playerName || target.username} (user_id ${target.userId})`;
  }

  return (
    <div className="max-w-2xl space-y-4">
      <PageHeader
        title="Tạo giftcode"
        description={targetLabel ? `Code riêng cho: ${targetLabel}` : "Tạo giftcode dùng chung hoặc cho bang hội."}
      />
      <GiftcodeCreateForm forUserId={forUserId} />
    </div>
  );
}
