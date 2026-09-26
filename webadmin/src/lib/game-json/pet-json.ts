/**
 * Bất biến server phải giữ khi sửa pet qua web (đọc GServer trước khi đổi file này):
 * - `pets` (`Data/User/PlayerData.cs:23`) là mảng `Pet`, phải SẮP TĂNG DẦN theo `petId` —
 *   `PlayerData.addPet` luôn `BinaryObjectAdd` + `.Sort(new BinaryCompare<Pet>())`
 *   (`Util/Utilities.cs:346-390`, `Util/BinaryCompare.cs`).
 * - `petSelected`/`PetDefLeague` (`PlayerData.cs:28,52`) là 1 Pet DUY NHẤT (không phải
 *   mảng, có thể null) — pet đang theo/đang thủ đài bị GỠ khỏi `pets`
 *   (`Server/MenuController.selectMenu.cs`, case `MENU_SELECT_PET_TO_DEF_LEAGUE` /
 *   `MENU_PET_INVENTORY` ~dòng 451-489: `pets.remove(pet)` rồi gán cột tương ứng; pet cũ ở
 *   cột đó — nếu có — được add lại vào `pets`). Vì vậy phải xem cả 3 nguồn khi hiển thị/tìm
 *   pet theo id.
 * - `petId` phải DUY NHẤT trên `pets` ∪ `petSelected` ∪ `PetDefLeague`. Web không tự cấp id
 *   mới (không tạo pet, chỉ sửa field có sẵn) nên không thể TỰ gây trùng, nhưng vẫn kiểm tra
 *   khi đọc để phát hiện dữ liệu đã lệch từ trước (`findDuplicatePetIds`).
 * - Web chỉ sửa "chỉ số cơ bản" — KHÔNG sửa `equip`/`tatto`/`skill`/`petIdTemplate` (đổi
 *   giống loài) hay tạo pet mới. Ngoại lệ: `removeFromEquip` được item-json gọi khi XOÁ 1
 *   item đang đeo (dọn tham chiếu mồ côi, không phải "sửa chỉ số").
 * - Field không biết (property tính toán không có `[JsonIgnore]` lúc ghi như `SkipPercent`/
 *   `AccuracyPercent`/`IsCrit`/`CritPercent`, hoặc rác cũ như `Template`) PHẢI giữ nguyên —
 *   dùng `lossless-json.ts` (RawNumber) để không đổi định dạng số gốc.
 * - Big-number safety: xem `lossless-json.ts` — `petId` là `int` C#, `exp`/`TimeDieZ` là
 *   `long` nhưng dữ liệu thật (đã quét DB) không có số ≥16 chữ số → an toàn với `Number`.
 */
import { RawNumber, parseLossless, stringifyLossless, toNumber, type JsonNode } from "./lossless-json";

export type PetNode = { [key: string]: JsonNode };
export type PetSource = "pets" | "petSelected" | "PetDefLeague";

function isPetNode(v: JsonNode): v is PetNode {
  return typeof v === "object" && v !== null && !Array.isArray(v) && !(v instanceof RawNumber);
}

export function parsePetList(raw: string): PetNode[] {
  const tree = parseLossless(raw);
  if (!Array.isArray(tree) || !tree.every(isPetNode)) throw new Error("Cột pets không phải mảng Pet hợp lệ.");
  return tree;
}

export function stringifyPetList(list: PetNode[]): string {
  return stringifyLossless(list as JsonNode);
}

/** `petSelected`/`PetDefLeague`: cột có thể NULL (chưa chọn pet nào). */
export function parseSinglePet(raw: string | null): PetNode | null {
  if (raw === null || raw === "" || raw === "null") return null;
  const node = parseLossless(raw);
  if (!isPetNode(node)) throw new Error("Cột petSelected/PetDefLeague không phải object Pet hợp lệ.");
  return node;
}

export function stringifySinglePet(pet: PetNode | null): string {
  return pet === null ? "null" : stringifyLossless(pet as JsonNode);
}

/** Sắp lại mảng `pets` tăng dần theo petId — gọi sau mọi lần sửa (bất biến server). */
export function sortPetList(list: PetNode[]): void {
  list.sort((a, b) => toNumber(a.petId) - toNumber(b.petId));
}

export interface CollectedPet {
  source: PetSource;
  pet: PetNode;
}

/** Gộp cả 3 nguồn để hiển thị/tìm kiếm — KHÔNG dùng để ghi (ghi phải theo đúng cột nguồn). */
export function collectAllPets(
  pets: PetNode[],
  petSelected: PetNode | null,
  petDefLeague: PetNode | null,
): CollectedPet[] {
  const all: CollectedPet[] = pets.map((pet) => ({ source: "pets" as const, pet }));
  if (petSelected) all.push({ source: "petSelected", pet: petSelected });
  if (petDefLeague) all.push({ source: "PetDefLeague", pet: petDefLeague });
  return all;
}

/** petId bị trùng trên cả 3 nguồn (rỗng = hợp lệ) — chỉ để CẢNH BÁO, web không tự sửa dữ liệu lệch. */
export function findDuplicatePetIds(all: CollectedPet[]): number[] {
  const seen = new Map<number, number>();
  for (const { pet } of all) {
    const id = toNumber(pet.petId);
    seen.set(id, (seen.get(id) ?? 0) + 1);
  }
  return [...seen.entries()].filter(([, count]) => count > 1).map(([id]) => id);
}

export function findPetById(all: CollectedPet[], petId: number): CollectedPet | null {
  return all.find(({ pet }) => toNumber(pet.petId) === petId) ?? null;
}

export interface PetEditPatch {
  name?: string | null;
  lvl?: number;
  star?: number;
  exp?: number;
  str?: number;
  agi?: number;
  _int?: number;
  hp?: number;
  mp?: number;
  maxHp?: number;
  maxMp?: number;
  tiemnang_point?: number;
  skillPoint?: number;
}

/** Sửa "chỉ số cơ bản" có sẵn của 1 pet — không đụng equip/tatto/skill/petIdTemplate. */
export function applyPetEdit(pet: PetNode, patch: PetEditPatch): void {
  const entries = Object.entries(patch) as [keyof PetEditPatch, PetEditPatch[keyof PetEditPatch]][];
  for (const [key, value] of entries) {
    if (value !== undefined) pet[key] = value as JsonNode;
  }
}

/**
 * Gỡ itemId khỏi `equip` của pet — dùng khi item-json xoá 1 item đang được pet này đeo.
 * Trả về true nếu có thay đổi thật (để caller biết có cần ghi lại cột hay không).
 */
export function removeFromEquip(pet: PetNode, itemId: number): boolean {
  const equip = pet.equip;
  if (!Array.isArray(equip)) return false;
  const next = equip.filter((v) => toNumber(v) !== itemId);
  if (next.length === equip.length) return false;
  pet.equip = next;
  return true;
}
