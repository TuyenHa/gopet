/** searchParams của App Router (Next 16: đã await). */
export type SearchParams = Record<string, string | string[] | undefined>;

export const firstParam = (v: string | string[] | undefined): string | undefined => (Array.isArray(v) ? v[0] : v);

export interface PageInfo {
  page: number; // bắt đầu từ 1
  size: number;
  offset: number;
}

const ALLOWED_SIZES = [20, 50, 100];

/** Đọc ?page=&size= an toàn; size chỉ nhận giá trị trong danh sách cho phép. */
export function parsePage(sp: SearchParams, defaultSize = 20): PageInfo {
  const page = Math.max(1, Math.min(100_000, Number.parseInt(firstParam(sp.page) ?? "1", 10) || 1));
  const rawSize = Number.parseInt(firstParam(sp.size) ?? "", 10);
  const size = ALLOWED_SIZES.includes(rawSize) ? rawSize : defaultSize;
  return { page, size, offset: (page - 1) * size };
}

/** Chuỗi tìm kiếm ?q= đã trim, giới hạn độ dài. */
export const parseQuery = (sp: SearchParams, key = "q"): string => (firstParam(sp[key]) ?? "").trim().slice(0, 100);

/** Escape ký tự đặc biệt của LIKE; dùng với `LIKE ? ESCAPE '\\'`. */
export const likeContains = (s: string) => `%${s.replace(/[\\%_]/g, (c) => `\\${c}`)}%`;

/** Tạo href giữ nguyên các tham số khác, ghi đè một số tham số. */
export function buildHref(basePath: string, sp: SearchParams, patch: Record<string, string | number | undefined>): string {
  const params = new URLSearchParams();
  for (const [k, v] of Object.entries(sp)) {
    const val = firstParam(v);
    if (val !== undefined && val !== "") params.set(k, val);
  }
  for (const [k, v] of Object.entries(patch)) {
    if (v === undefined || v === "") params.delete(k);
    else params.set(k, String(v));
  }
  const qs = params.toString();
  return qs ? `${basePath}?${qs}` : basePath;
}
