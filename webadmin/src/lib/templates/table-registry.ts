import type { RefKind } from "./ref-sources";
import { ITEM_SHOP_TABLES } from "./registry-item-shop";
import { PET_TABLES } from "./registry-pet";
import { SKILL_TABLES } from "./registry-skill";
import { WORLD_TABLES } from "./registry-world";
import { PROGRESSION_TABLES } from "./registry-progression";
import { CLAN_TABLES } from "./registry-clan";
import { SYSTEM_TABLES } from "./registry-system";

export type ColumnType = "int" | "bigint" | "float" | "string" | "json" | "bool" | "ref";

/** Cấu hình 1 cột. `name`/`label` bắt buộc; các cờ còn lại quyết định input + validate. */
export interface ColumnDef {
  name: string;
  label: string;
  type: ColumnType;
  /** Cho phép NULL (theo `SHOW CREATE TABLE`) → input rỗng lưu NULL thay vì lỗi bắt buộc. */
  nullable?: boolean;
  /** Hiện textarea thay vì input 1 dòng (mô tả dài, không cần validate JSON). */
  long?: boolean;
  /** Loại tham chiếu khi type === "ref" → hiện ref-picker thay vì input số. */
  ref?: RefKind;
  /** Giới hạn độ dài giống cột DB (chỉ áp cho string/json). */
  maxLength?: number;
  /** Ghi chú hiển thị cạnh label (ngữ nghĩa ẩn, đơn vị, quan hệ không có FK...). */
  note?: string;
}

export interface TableConfig {
  db: "game" | "web";
  table: string;
  label: string;
  group: string;
  /** Cột khoá chính (PK ghép nếu > 1 phần tử). Rỗng ([]) chỉ khi readOnly (không PK). */
  pk: string[];
  /** true khi PK là 1 cột AUTO_INCREMENT → ẩn khỏi form tạo mới, DB tự sinh. */
  autoIncrement?: boolean;
  columns: ColumnDef[];
  /** Cột dùng cho tìm kiếm LIKE (OR); luôn là tên cột trong registry, không lấy từ input. */
  searchCols: string[];
  /** true = chỉ xem (vd gopet_mob không có PK) — ẩn nút sửa/xoá/thêm. */
  readOnly?: boolean;
  /** true = không cho thêm dòng mới (vd field/server chỉ sửa 1 số dòng có sẵn). */
  noCreate?: boolean;
  /** true = không cho xoá dòng. */
  noDelete?: boolean;
  /** Ghi chú hiển thị đầu trang danh sách (ngữ nghĩa đặc thù của bảng). */
  note?: string;
  /** Cảnh báo tham chiếu trước khi xoá (đếm player.items/pets chứa template id này). */
  refCheck?: { playerColumn: "items" | "pets"; jsonKey: string };
}

/** Nguồn DUY NHẤT của registry — gộp từ các file theo nhóm để mỗi file < 200 dòng. */
const ALL_TABLES: TableConfig[] = [
  ...ITEM_SHOP_TABLES,
  ...PET_TABLES,
  ...SKILL_TABLES,
  ...WORLD_TABLES,
  ...PROGRESSION_TABLES,
  ...CLAN_TABLES,
  ...SYSTEM_TABLES,
];

const REGISTRY_MAP: ReadonlyMap<string, TableConfig> = new Map(ALL_TABLES.map((t) => [t.table, t]));

/** [table] trong URL phải khớp đúng 1 bảng trong registry, ngược lại notFound(). */
export function getTableConfig(table: string): TableConfig | undefined {
  return REGISTRY_MAP.get(table);
}

export interface TableGroup {
  group: string;
  tables: TableConfig[];
}

/** Thứ tự nhóm cố định cho trang /data (đi từ hay sửa → ít sửa). */
const GROUP_ORDER = [
  "Vật phẩm",
  "Shop",
  "Pet",
  "Kỹ năng",
  "Bản đồ & NPC",
  "Quái",
  "Nhiệm vụ / Thành tựu / Xăm",
  "Nâng cấp",
  "Bang hội",
  "Hệ thống",
];

/** Danh sách bảng theo nhóm cho trang chỉ mục `/data`. */
export const TABLE_GROUPS: TableGroup[] = GROUP_ORDER.map((group) => ({
  group,
  tables: ALL_TABLES.filter((t) => t.group === group),
})).filter((g) => g.tables.length > 0);
