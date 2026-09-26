/** `user.coin` và `exchange_gold.gold` đều là `int(11)` — chặn tràn số trước khi chạm DB. */
export const INT32_MAX = 2147483647;

export function isValidInt32Delta(delta: number): boolean {
  return Number.isInteger(delta) && Math.abs(delta) <= INT32_MAX;
}

export function isValidInt32Amount(amount: number): boolean {
  return Number.isInteger(amount) && amount >= 0 && amount <= INT32_MAX;
}
