/**
 * Nguồn dữ liệu cho ref-picker (combobox tìm theo tên/ID). Chỉ 5 loại tham chiếu theo
 * plan (item/pet/npc/map/skill) — danh sách đóng, KHÔNG suy diễn từ input người dùng.
 */
export type RefKind = "item" | "pet" | "npc" | "map" | "skill";

export interface RefSource {
  table: string;
  pkCol: string;
  labelCol: string;
}

export const REF_SOURCES: Record<RefKind, RefSource> = {
  item: { table: "item", pkCol: "itemId", labelCol: "name" },
  pet: { table: "gopet_pet", pkCol: "petId", labelCol: "name" },
  npc: { table: "npc", pkCol: "npcId", labelCol: "name" },
  map: { table: "map", pkCol: "mapId", labelCol: "name" },
  skill: { table: "skill", pkCol: "skillID", labelCol: "name" },
};
