import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import {
  collectAllPets,
  findDuplicatePetIds,
  parsePetList,
  parseSinglePet,
  type CollectedPet,
  type PetNode,
  type PetSource,
} from "@/lib/game-json/pet-json";
import { toNumber } from "@/lib/game-json/lossless-json";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { PetEditForm, type PetEditableStats } from "@/components/players/pet-edit-form";

const SOURCE_LABEL: Record<PetSource, string> = {
  petSelected: "Đang theo",
  PetDefLeague: "Thủ đài",
  pets: "Trong kho",
};

function readStats(pet: PetNode): PetEditableStats {
  return {
    name: typeof pet.name === "string" ? pet.name : null,
    lvl: toNumber(pet.lvl),
    star: toNumber(pet.star),
    exp: toNumber(pet.exp),
    str: toNumber(pet.str),
    agi: toNumber(pet.agi),
    _int: toNumber(pet._int),
    hp: toNumber(pet.hp),
    mp: toNumber(pet.mp),
    maxHp: toNumber(pet.maxHp),
    maxMp: toNumber(pet.maxMp),
    tiemnang_point: toNumber(pet.tiemnang_point),
    skillPoint: toNumber(pet.skillPoint),
  };
}

function PetCard({
  playerId,
  entry,
  md5,
}: {
  playerId: number;
  entry: CollectedPet;
  md5: string;
}) {
  const petId = toNumber(entry.pet.petId);
  const petIdTemplate = toNumber(entry.pet.petIdTemplate);
  const stats = readStats(entry.pet);
  const equip = Array.isArray(entry.pet.equip) ? entry.pet.equip.length : 0;

  return (
    <div className="flex items-center justify-between gap-3 rounded-lg border p-3">
      <div className="min-w-0 space-y-1 text-sm">
        <p className="font-medium">
          {stats.name ?? `Loài #${petIdTemplate}`} <span className="text-neutral-400">#{petId}</span>
        </p>
        <p className="text-neutral-500">
          Lvl {stats.lvl} · {stats.star}★ · exp {stats.exp} · str/agi/int {stats.str}/{stats.agi}/{stats._int}
        </p>
        <p className="text-neutral-500">
          HP {stats.hp}/{stats.maxHp} · MP {stats.mp}/{stats.maxMp} · điểm tiềm năng {stats.tiemnang_point} · điểm kỹ năng{" "}
          {stats.skillPoint} · trang bị: {equip}
        </p>
      </div>
      <PetEditForm playerId={playerId} petId={petId} source={entry.source} expectedMd5={md5} stats={stats} />
    </div>
  );
}

export async function PetTab({ playerId }: { playerId: number }) {
  await requireAdmin();
  const row = await queryOne<{
    pets: string | null;
    petsMd5: string | null;
    petSelected: string | null;
    selMd5: string | null;
    PetDefLeague: string | null;
    defMd5: string | null;
  }>(
    gamePool(),
    `SELECT pets, MD5(pets) AS petsMd5, petSelected, MD5(petSelected) AS selMd5,
            PetDefLeague, MD5(PetDefLeague) AS defMd5
     FROM player WHERE ID = ?`,
    [playerId],
  );
  if (!row) return <p className="text-sm text-neutral-500">Không có dữ liệu.</p>;

  let parseError: string | null = null;
  let all: CollectedPet[] = [];
  try {
    const pets = row.pets ? parsePetList(row.pets) : [];
    const petSelected = parseSinglePet(row.petSelected);
    const petDefLeague = parseSinglePet(row.PetDefLeague);
    all = collectAllPets(pets, petSelected, petDefLeague);
  } catch (err) {
    parseError = err instanceof Error ? err.message : String(err);
  }

  const duplicates = findDuplicatePetIds(all);
  const bySource = new Map<PetSource, CollectedPet[]>();
  for (const entry of all) {
    const list = bySource.get(entry.source) ?? [];
    list.push(entry);
    bySource.set(entry.source, list);
  }
  const md5For = (source: PetSource): string =>
    (source === "pets" ? row.petsMd5 : source === "petSelected" ? row.selMd5 : row.defMd5) ?? "";

  return (
    <div className="space-y-4">
      {parseError && (
        <Card className="border-destructive/50">
          <CardContent className="pt-6 text-sm text-destructive">
            Không đọc được dữ liệu pet ({parseError}) — xem Raw JSON để kiểm tra thủ công.
          </CardContent>
        </Card>
      )}
      {duplicates.length > 0 && (
        <Card className="border-destructive/50">
          <CardContent className="pt-6 text-sm text-destructive">
            Phát hiện petId trùng trên nhiều cột (dữ liệu gốc đã lệch, không phải do web gây ra):{" "}
            {duplicates.join(", ")}. <Badge variant="destructive">Cần kiểm tra thủ công</Badge>
          </CardContent>
        </Card>
      )}
      {(["petSelected", "PetDefLeague", "pets"] as const).map((source) => (
        <Card key={source}>
          <CardHeader>
            <CardTitle>
              {SOURCE_LABEL[source]} ({bySource.get(source)?.length ?? 0})
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {(bySource.get(source) ?? []).length === 0 && <p className="text-sm text-neutral-500">Không có pet.</p>}
            {(bySource.get(source) ?? []).map((entry) => (
              <PetCard key={toNumber(entry.pet.petId)} playerId={playerId} entry={entry} md5={md5For(source)} />
            ))}
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
