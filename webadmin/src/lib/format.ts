/** Định dạng số (kể cả bigint dạng chuỗi) theo kiểu Việt Nam: 1.234.567. */
export function formatNumber(v: string | number | bigint | null | undefined): string {
  if (v === null || v === undefined || v === "") return "—";
  try {
    return BigInt(v).toLocaleString("vi-VN");
  } catch {
    return String(v);
  }
}

/** DATETIME của MariaDB (chuỗi "YYYY-MM-DD HH:mm:ss", dateStrings) → "DD/MM/YYYY HH:mm". */
export function formatDateTime(v: string | null | undefined): string {
  if (!v) return "—";
  const m = /^(\d{4})-(\d{2})-(\d{2})[ T](\d{2}):(\d{2})/.exec(v);
  return m ? `${m[3]}/${m[2]}/${m[1]} ${m[4]}:${m[5]}` : v;
}

/** Epoch ms (bigint dạng chuỗi) → ngày giờ theo giờ Việt Nam. */
export function formatEpochMs(v: string | number | null | undefined): string {
  if (v === null || v === undefined) return "—";
  const n = Number(v);
  if (!Number.isFinite(n) || n <= 0) return "—";
  return new Date(n).toLocaleString("vi-VN", { timeZone: "Asia/Ho_Chi_Minh", hour12: false });
}
