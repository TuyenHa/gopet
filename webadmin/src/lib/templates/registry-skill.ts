import type { TableConfig } from "./table-registry";

/** Nhóm "Kỹ năng" — soát cột/PK từ `SHOW CREATE TABLE` (2026-09-26). */
export const SKILL_TABLES: TableConfig[] = [
  {
    db: "game",
    table: "skill",
    label: "Kỹ năng",
    group: "Kỹ năng",
    pk: ["skillID"],
    searchCols: ["skillID", "name"],
    columns: [
      { name: "skillID", label: "ID", type: "int" },
      { name: "name", label: "Tên", type: "string" },
      { name: "description", label: "Mô tả", type: "string", long: true },
      { name: "nClass", label: "Phái", type: "int" },
      { name: "IsNeedCard", label: "Cần thẻ học", type: "bool" },
    ],
  },
  {
    db: "game",
    table: "skilllv",
    label: "Cấp độ kỹ năng",
    group: "Kỹ năng",
    pk: ["ID"],
    autoIncrement: true,
    searchCols: ["ID", "skillID", "lv"],
    columns: [
      { name: "ID", label: "ID", type: "int" },
      { name: "skillID", label: "Kỹ năng", type: "ref", ref: "skill" },
      { name: "skillInfo", label: "Thông tin hiệu ứng (JSON)", type: "json" },
      { name: "mpLost", label: "MP tiêu hao", type: "int" },
      { name: "lv", label: "Cấp", type: "int" },
    ],
  },
];
