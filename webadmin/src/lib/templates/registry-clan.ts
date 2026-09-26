import type { TableConfig } from "./table-registry";

/** Nhóm "Bang hội" (template, KHÔNG phải bảng `clan` runtime) — soát 2026-09-26. */
export const CLAN_TABLES: TableConfig[] = [
  {
    db: "game",
    table: "clan_template",
    label: "Cấp độ bang hội",
    group: "Bang hội",
    pk: ["clanLvl"],
    autoIncrement: true,
    searchCols: ["clanLvl"],
    columns: [
      { name: "clanLvl", label: "Cấp bang", type: "int" },
      { name: "maxMember", label: "Số thành viên tối đa", type: "int" },
      { name: "tiemnangPoint", label: "Điểm tiềm năng thưởng", type: "int" },
      { name: "fundNeed", label: "Quỹ cần để lên cấp", type: "bigint" },
    ],
  },
  {
    db: "game",
    table: "clan_skill",
    label: "Kỹ năng bang hội",
    group: "Bang hội",
    pk: ["id"],
    autoIncrement: true,
    searchCols: ["id", "name"],
    columns: [
      { name: "id", label: "ID", type: "int" },
      { name: "name", label: "Tên", type: "string" },
      { name: "description", label: "Mô tả", type: "string", long: true },
      { name: "expire", label: "Hạn hiệu lực (ms)", type: "bigint" },
      { name: "moneyType", label: "Loại tiền (JSON)", type: "json" },
      { name: "price", label: "Giá (JSON)", type: "json" },
      { name: "lvlClanRequire", label: "Cấp bang tối thiểu", type: "int" },
    ],
  },
  {
    db: "game",
    table: "clan_skill_lvl",
    label: "Cấp độ kỹ năng bang hội",
    group: "Bang hội",
    pk: ["id"],
    autoIncrement: true,
    searchCols: ["id", "skillId", "lvl"],
    columns: [
      { name: "id", label: "ID", type: "int" },
      { name: "skillId", label: "Kỹ năng bang hội", type: "int", note: "Xem clan_skill.id, không có FK ràng buộc" },
      { name: "lvl", label: "Cấp", type: "int" },
      { name: "skillInfo", label: "Thông tin hiệu ứng (JSON)", type: "json" },
    ],
  },
];
