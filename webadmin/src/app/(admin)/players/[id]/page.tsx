import { notFound } from "next/navigation";
import { PageHeader } from "@/components/data/page-header";
import { PlayerDetailTabs } from "@/components/players/player-detail-tabs";
import { OverviewTab } from "@/components/players/overview-tab";
import { InventoryTab } from "./inventory-tab";
import { PetTab } from "./pet-tab";
import { LettersReadonlyTab } from "@/components/players/letters-readonly-tab";
import { FriendsReadonlyTab } from "@/components/players/friends-readonly-tab";
import { QuestsReadonlyTab } from "@/components/players/quests-readonly-tab";
import { RawJsonTab } from "@/components/players/raw-json-tab";
import {
  getPlayerFriends,
  getPlayerLetters,
  getPlayerOverview,
  getPlayerQuests,
  getPlayerRaw,
} from "@/lib/players/player-detail-queries";
import { requireAdmin } from "@/lib/auth/require-admin";

export default async function PlayerDetailPage({ params }: PageProps<"/players/[id]">) {
  const { id } = await params;
  const playerId = Number.parseInt(id, 10);
  if (!Number.isInteger(playerId) || playerId <= 0) notFound();

  const [ctx, overview] = await Promise.all([requireAdmin(), getPlayerOverview(playerId)]);
  if (!overview) notFound();

  const [letters, friends, quests, raw] = await Promise.all([
    getPlayerLetters(overview.user_id),
    getPlayerFriends(playerId),
    getPlayerQuests(playerId),
    getPlayerRaw(playerId),
  ]);

  return (
    <div className="space-y-4">
      <PageHeader title={`Nhân vật #${overview.ID} — ${overview.name}`} description={`user_id ${overview.user_id}`} />
      <PlayerDetailTabs
        overview={<OverviewTab data={overview} isSuperAdmin={ctx.isSuperAdmin} isSelf={overview.user_id === ctx.userId} />}
        inventory={<InventoryTab playerId={overview.ID} />}
        pets={<PetTab playerId={overview.ID} />}
        letters={<LettersReadonlyTab letters={letters} />}
        friends={friends ? <FriendsReadonlyTab data={friends} /> : null}
        quests={quests ? <QuestsReadonlyTab data={quests} /> : null}
        raw={raw ? <RawJsonTab data={raw} /> : null}
      />
    </div>
  );
}
