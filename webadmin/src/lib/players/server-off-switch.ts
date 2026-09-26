import "server-only";

/**
 * Công tắc "server đã TẮT" cho bảo trì — super-admin bật khi biết chắc GServer không chạy,
 * để bỏ qua kiểm tra heartbeat trong `offline-guard.ts` [RT#2]. Chỉ lưu trong RAM tiến trình
 * (không cần bền vững qua restart — restart web coi như tắt công tắc, an toàn hơn là bật nhầm).
 */
const DEFAULT_DURATION_MS = 2 * 60 * 60 * 1000; // 2 giờ

const g = globalThis as unknown as { __gopetServerOffSwitch?: { activeUntil: number } };

export function isServerOffSwitchActive(now = Date.now()): boolean {
  const s = g.__gopetServerOffSwitch;
  return !!s && now < s.activeUntil;
}

export function serverOffSwitchExpiresAt(): number | null {
  const s = g.__gopetServerOffSwitch;
  return s && isServerOffSwitchActive() ? s.activeUntil : null;
}

export function activateServerOffSwitch(durationMs = DEFAULT_DURATION_MS): void {
  g.__gopetServerOffSwitch = { activeUntil: Date.now() + durationMs };
}

export function deactivateServerOffSwitch(): void {
  g.__gopetServerOffSwitch = undefined;
}
