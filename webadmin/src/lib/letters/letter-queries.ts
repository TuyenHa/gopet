import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { query, queryOne } from "@/lib/db/query";
import type { LetterRow } from "@/lib/db/types/letter-row";
import { parsePage, type PageInfo, type SearchParams } from "@/lib/pagination";

/**
 * Hàng đợi thư CHƯA giao (`letter`) — chỉ xem, không sửa/huỷ: bảng không có PK nên xoá theo
 * điều kiện tuỳ ý có thể trúng nhầm dòng khác cùng nội dung (RT#12). `time DESC` để thư mới
 * lên đầu; không đếm tổng bằng COUNT lớn, dùng `hasNext` để tránh quét toàn bảng mỗi lần tải trang.
 */
export async function listPendingLetters(sp: SearchParams): Promise<{ rows: LetterRow[]; info: PageInfo; hasNext: boolean }> {
  await requireAdmin();
  const info = parsePage(sp);
  const rows = await query<LetterRow>(gamePool(), "SELECT * FROM letter ORDER BY time DESC, targetId LIMIT ? OFFSET ?", [
    info.size + 1,
    info.offset,
  ]);
  return { rows: rows.slice(0, info.size), info, hasNext: rows.length > info.size };
}

export async function countPendingLetters(): Promise<number> {
  await requireAdmin();
  const row = await queryOne<{ c: number }>(gamePool(), "SELECT COUNT(*) AS c FROM letter");
  return row?.c ?? 0;
}
