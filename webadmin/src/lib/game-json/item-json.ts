/**
 * Bất biến server phải giữ khi sửa `player.items` qua web (đọc GServer trước khi đổi file
 * này — `SRCGOPETGOC/GServer`):
 * - Serialize bằng Newtonsoft, `DefaultValueHandling.Include` + `NullValueHandling.Include`
 *   (`Adapter/JsonAdapter.cs`) — giữ nguyên tên field C# (không camelCase), field mặc định
 *   vẫn được ghi ra.
 * - `items` là `HashMap<sbyte invType, CopyOnWriteArrayList<Item>>` (`Data/User/
 *   PlayerData.cs:22`). Mỗi danh sách theo invType phải SẮP TĂNG DẦN theo `itemId` — server
 *   tìm bằng `BinarySearch` và luôn `.Sort(new BinaryCompare<Item>())` sau khi thêm
 *   (`Util/Utilities.cs:346-390`, `PlayerData.addItem` ~dòng 326-330, `Util/
 *   BinaryCompare.cs`). Web PHẢI sort lại sau mọi lần sửa (id không đổi nên thực chất là
 *   no-op, nhưng vẫn gọi để phòng thủ).
 * - Gộp stack theo `itemTemplateId + canTrade` (`PlayerData.addItem`: `p.Template.itemId ==
 *   item.Template.itemId && p.canTrade == item.canTrade`), KHÔNG chỉ theo templateId — web
 *   không tạo/gộp item nên chỉ cần biết để không phá vỡ giả định này khi sửa `canTrade`.
 * - `Item` có chỉ số (atk/def/hp/mp) random lúc khởi tạo (`Data/item/Item.cs:83-97`) → web
 *   KHÔNG được tạo item mới (chỉ giftcode/thư mới tạo — phase 8). Web chỉ sửa field có sẵn:
 *   `count`, `lvl`, `expire`, `canTrade`, `durability`.
 * - `durability` (độ bền trang bị pet, `Util/EquipDurability.cs`, Max=80) chỉ được server
 *   ghi ra khi `ShouldSerializedurability()` = true (`EquipDurability.Applies` — item là
 *   trang bị pet). Item cũ/không phải trang bị KHÔNG có field này trong JSON — web KHÔNG
 *   được tự thêm field mới vào item chưa từng có (giữ đúng "chỉ sửa field có sẵn").
 * - Xoá item: mỗi Item có `petEuipId` trỏ tới `Pet.petId` đang đeo nó; pet giữ `equip:
 *   int[]` chứa các itemId đã đeo (`Data/pet/Pet.cs:20,64`; `Pet.applyInfo` tự gỡ đồ mồ côi
 *   ở dòng ~309-323 nếu lệch). Xoá item xong PHẢI gỡ itemId khỏi `equip` của đúng pet (tìm
 *   trên `pets` ∪ `petSelected` ∪ `PetDefLeague` — xem `pet-json.ts`).
 * - Field không biết (rác code cũ như `Template`/`HpItem`/`MpItem`... không có [JsonIgnore]
 *   lúc ghi, hoặc field mới hơn) PHẢI giữ nguyên giá trị/định dạng — dùng `lossless-json.ts`
 *   (RawNumber) thay vì `JSON.parse` thường để không biến `5.0` (gemOptionValue, kiểu
 *   `float`) thành `5`.
 * - Big-number safety: xem `lossless-json.ts` — itemId/petEuipId là `int` C#, an toàn với
 *   `Number`; đã quét thật DB không có số ≥16 chữ số.
 */
import { RawNumber, parseLossless, stringifyLossless, toNumber, type JsonNode } from "./lossless-json";

export const EQUIP_DURABILITY_MAX = 80;

export type ItemNode = { [key: string]: JsonNode };
export type PlayerItemsTree = { [invType: string]: ItemNode[] };

function isItemNode(v: JsonNode): v is ItemNode {
  return typeof v === "object" && v !== null && !Array.isArray(v) && !(v instanceof RawNumber);
}

/** Parse cột `player.items`. Ném lỗi nếu không đúng khuôn `{ [invType]: Item[] }`. */
export function parsePlayerItems(raw: string): PlayerItemsTree {
  const tree = parseLossless(raw);
  if (typeof tree !== "object" || tree === null || Array.isArray(tree) || tree instanceof RawNumber) {
    throw new Error("Cột items không phải object JSON hợp lệ.");
  }
  for (const [invType, list] of Object.entries(tree)) {
    if (!Array.isArray(list) || !list.every(isItemNode)) {
      throw new Error(`invType ${invType} trong items không phải mảng Item hợp lệ.`);
    }
  }
  return tree as PlayerItemsTree;
}

export function stringifyPlayerItems(tree: PlayerItemsTree): string {
  return stringifyLossless(tree as JsonNode);
}

/** Tìm item theo `itemId` trên toàn bộ các danh sách invType. */
export function findItem(
  tree: PlayerItemsTree,
  itemId: number,
): { invType: string; index: number; item: ItemNode } | null {
  for (const [invType, list] of Object.entries(tree)) {
    const index = list.findIndex((it) => toNumber(it.itemId) === itemId);
    if (index !== -1) return { invType, index, item: list[index] };
  }
  return null;
}

/** Sắp lại mọi danh sách invType tăng dần theo itemId — gọi sau MỌI lần sửa (bất biến server). */
export function sortAllInvTypes(tree: PlayerItemsTree): void {
  for (const list of Object.values(tree)) {
    list.sort((a, b) => toNumber(a.itemId) - toNumber(b.itemId));
  }
}

export interface ItemEditPatch {
  count?: number;
  lvl?: number;
  expire?: number;
  canTrade?: boolean;
  durability?: number;
}

/** Sửa field có sẵn của 1 item — KHÔNG tạo field `durability` mới nếu item chưa từng có. */
export function applyItemEdit(item: ItemNode, patch: ItemEditPatch): void {
  if (patch.count !== undefined) item.count = patch.count;
  if (patch.lvl !== undefined) item.lvl = patch.lvl;
  if (patch.expire !== undefined) item.expire = patch.expire;
  if (patch.canTrade !== undefined) item.canTrade = patch.canTrade;
  if (patch.durability !== undefined) {
    if (!("durability" in item)) {
      throw new Error("Vật phẩm này chưa từng có độ bền (không phải trang bị pet) — không thể sửa.");
    }
    item.durability = patch.durability;
  }
}

/** Xoá item khỏi danh sách invType chứa nó. Trả về node đã xoá (để dọn `equip` của pet) hoặc null nếu không thấy. */
export function deleteItem(tree: PlayerItemsTree, itemId: number): ItemNode | null {
  const found = findItem(tree, itemId);
  if (!found) return null;
  tree[found.invType].splice(found.index, 1);
  return found.item;
}

/** `petEuipId` của item (-1 = chưa đeo pet nào). Trả null nếu field thiếu/không phải số. */
export function itemPetEquipId(item: ItemNode): number | null {
  const v = toNumber(item.petEuipId);
  return Number.isFinite(v) ? v : null;
}
