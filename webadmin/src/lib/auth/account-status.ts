/** Luật chặn tài khoản giống GServer (`Player.cs` lúc login). Hàm thuần — dùng chung + test được. */
export interface AccountStatusInput {
  role: number;
  isBaned: number;
  banTime: string | number;
}

export function isAccountBlocked(u: AccountStatusInput, nowMs = Date.now()): boolean {
  if (Number(u.role) === 0) return true;
  if (Number(u.isBaned) === 2) return true;
  if (Number(u.isBaned) === 1 && BigInt(u.banTime) > BigInt(nowMs)) return true;
  return false;
}

/** Hash bcrypt ($2a$/$2b$/$2y$). Còn lại là legacy (plaintext/SHA256) — không chấp nhận ở web. */
export const isBcryptHash = (hash: string) => /^\$2[aby]\$\d{2}\$/.test(hash);
