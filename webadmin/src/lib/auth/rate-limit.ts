import "server-only";

/**
 * Rate-limit đăng nhập IN-MEMORY (1 process webadmin). KHÔNG ghi `login_history` — GServer
 * đếm mọi dòng theo IP+UserName để khoá đăng nhập game, ghi vào sẽ khoá người chơi.
 * 5 lần sai trong 5 phút → chặn, thời gian chặn tăng gấp đôi mỗi lần tái phạm (tối đa 1 giờ).
 */
const WINDOW_MS = 5 * 60_000;
const MAX_FAILS = 5;
const BASE_BLOCK_MS = 5 * 60_000;
const MAX_BLOCK_MS = 60 * 60_000;

interface Entry {
  fails: number[];
  blockedUntil: number;
  strikes: number;
}

const g = globalThis as unknown as { __gopetLoginLimiter?: Map<string, Entry> };
const store = (g.__gopetLoginLimiter ??= new Map<string, Entry>());

const keysFor = (username: string, ip: string) => [`u:${username}|ip:${ip}`, `ip:${ip}`];

/** Số giây còn bị chặn (0 = được thử). */
export function loginBlockedFor(username: string, ip: string, now = Date.now()): number {
  let wait = 0;
  for (const k of keysFor(username, ip)) {
    const e = store.get(k);
    if (e && e.blockedUntil > now) wait = Math.max(wait, Math.ceil((e.blockedUntil - now) / 1000));
  }
  return wait;
}

export function recordLoginFailure(username: string, ip: string, now = Date.now()): void {
  // Theo IP được phép nhiều hơn (nhiều admin chung NAT) — gấp 4 ngưỡng.
  keysFor(username, ip).forEach((k, i) => {
    const e = store.get(k) ?? { fails: [], blockedUntil: 0, strikes: 0 };
    e.fails = e.fails.filter((t) => now - t < WINDOW_MS);
    e.fails.push(now);
    const limit = i === 0 ? MAX_FAILS : MAX_FAILS * 4;
    if (e.fails.length >= limit) {
      e.strikes += 1;
      e.blockedUntil = now + Math.min(BASE_BLOCK_MS * 2 ** (e.strikes - 1), MAX_BLOCK_MS);
      e.fails = [];
    }
    store.set(k, e);
  });
  // Dọn định kỳ để map không phình.
  if (store.size > 5000) {
    for (const [k, e] of store) if (e.blockedUntil < now && e.fails.every((t) => now - t >= WINDOW_MS)) store.delete(k);
  }
}

export function recordLoginSuccess(username: string, ip: string): void {
  store.delete(keysFor(username, ip)[0]);
}
