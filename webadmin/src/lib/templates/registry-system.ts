import type { TableConfig } from "./table-registry";

/**
 * Nhóm "Hệ thống" — 2 bảng cài đặt gộp vào registry template theo yêu cầu Red Team
 * (RT#10) thay vì làm trang riêng. `field`/`server` chỉ có vài dòng cố định → không cho
 * thêm/xoá, chỉ sửa giá trị (khớp GRANT UPDATE-only trong `webadmin-grants.sql`).
 */
export const SYSTEM_TABLES: TableConfig[] = [
  {
    db: "game",
    table: "field",
    label: "Cài đặt server (field)",
    group: "Hệ thống",
    pk: ["FieldName"],
    noCreate: true,
    noDelete: true,
    searchCols: ["FieldName", "Description"],
    columns: [
      { name: "FieldName", label: "Tên cấu hình", type: "string" },
      { name: "Description", label: "Mô tả", type: "string", long: true },
      { name: "Value", label: "Giá trị", type: "string" },
    ],
  },
  {
    db: "web",
    table: "server",
    label: "Máy chủ (danh sách server client thấy)",
    group: "Hệ thống",
    pk: ["Id"],
    noCreate: true,
    noDelete: true,
    searchCols: ["Id", "Name", "IpAddress"],
    columns: [
      { name: "Id", label: "ID", type: "int" },
      { name: "Name", label: "Tên server", type: "string", maxLength: 11 },
      { name: "IpAddress", label: "Địa chỉ IP", type: "string" },
      { name: "Port", label: "Port", type: "int" },
      { name: "NeedAdmin", label: "Chỉ admin vào được", type: "bool" },
      { name: "GreaterThanEquals", label: "Yêu cầu bản client ≥", type: "string", nullable: true },
      { name: "LessThan", label: "Yêu cầu bản client <", type: "string", nullable: true },
    ],
  },
];
