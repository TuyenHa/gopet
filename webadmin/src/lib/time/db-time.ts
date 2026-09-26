/**
 * Quy đổi giờ Việt Nam (nhập trên UI) ↔ giờ lưu trong DB.
 *
 * Việt Nam luôn là UTC+7 (không DST). MariaDB chạy `--default-time-zone=+07:00` (docker-compose)
 * nên mặc định DB và VN trùng giờ, không quy đổi. DB nào còn chạy UTC thì đặt
 * `DB_TIMEZONE_OFFSET_MIN=0`. Đây là NƠI DUY NHẤT làm phép quy đổi này; đừng cộng/trừ
 * giờ tay ở chỗ khác.
 *
 * Công thức: DB = VN - (VN_OFFSET_MIN - DB_TIMEZONE_OFFSET_MIN) phút.
 * - DB_TIMEZONE_OFFSET_MIN = 420 (mặc định): lệch = 0, DB giữ nguyên giờ VN.
 * - DB_TIMEZONE_OFFSET_MIN = 0 (DB chạy UTC): lệch = 420, DB = VN - 7 giờ.
 */
const VN_OFFSET_MIN = 420;

/** Đọc trực tiếp từ `process.env` (không qua `lib/env.ts` — file dùng chung, phase khác sở hữu). */
export const DB_TIMEZONE_OFFSET_MIN = (() => {
  const raw = Number.parseInt(process.env.DB_TIMEZONE_OFFSET_MIN ?? "", 10);
  return Number.isFinite(raw) ? raw : 420;
})();

const NAIVE_DATETIME_RE = /^(\d{4})-(\d{2})-(\d{2})[ T](\d{2}):(\d{2})(?::(\d{2}))?/;

/** Parse chuỗi "YYYY-MM-DD HH:mm[:ss]" hoặc "YYYY-MM-DDTHH:mm" thành mốc thời gian "trần"
 * (không gắn múi giờ nào) bằng Date.UTC — tránh phụ thuộc TZ của máy chạy Node. */
function parseNaive(s: string): number {
  const m = NAIVE_DATETIME_RE.exec(s.trim());
  if (!m) throw new Error(`Chuỗi thời gian không hợp lệ: ${s}`);
  const [, y, mo, d, h, mi, se] = m;
  return Date.UTC(Number(y), Number(mo) - 1, Number(d), Number(h), Number(mi), Number(se ?? "0"));
}

function pad(n: number): string {
  return String(n).padStart(2, "0");
}

function formatNaive(ms: number, withSeconds: boolean): string {
  const d = new Date(ms);
  const base = `${d.getUTCFullYear()}-${pad(d.getUTCMonth() + 1)}-${pad(d.getUTCDate())} ${pad(d.getUTCHours())}:${pad(d.getUTCMinutes())}`;
  return withSeconds ? `${base}:${pad(d.getUTCSeconds())}` : base;
}

/** Giờ VN (input form) → chuỗi DATETIME để ghi DB. */
export function vnInputToDbDateTime(vnLocal: string): string {
  const ms = parseNaive(vnLocal);
  const deltaMin = VN_OFFSET_MIN - DB_TIMEZONE_OFFSET_MIN;
  return formatNaive(ms - deltaMin * 60_000, true);
}

/** DATETIME đọc từ DB → giá trị cho `<input type="datetime-local">` (giờ VN, không giây). */
export function dbDateTimeToVnInputValue(db: string): string {
  const ms = parseNaive(db);
  const deltaMin = VN_OFFSET_MIN - DB_TIMEZONE_OFFSET_MIN;
  return formatNaive(ms + deltaMin * 60_000, false).replace(" ", "T");
}
