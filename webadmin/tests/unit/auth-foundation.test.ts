import bcrypt from "bcryptjs";
import { describe, expect, it } from "vitest";
import { isAccountBlocked, isBcryptHash } from "@/lib/auth/account-status";
import { loginBlockedFor, recordLoginFailure, recordLoginSuccess } from "@/lib/auth/rate-limit";

describe("isAccountBlocked (luật ban giống GServer)", () => {
  const now = 1_800_000_000_000;
  it("role=0 bị chặn", () => expect(isAccountBlocked({ role: 0, isBaned: 0, banTime: "0" }, now)).toBe(true));
  it("role=3 (admin portal cũ) không bị chặn", () =>
    expect(isAccountBlocked({ role: 3, isBaned: 0, banTime: "0" }, now)).toBe(false));
  it("ban vĩnh viễn", () => expect(isAccountBlocked({ role: 1, isBaned: 2, banTime: "0" }, now)).toBe(true));
  it("ban có hạn còn hiệu lực", () =>
    expect(isAccountBlocked({ role: 1, isBaned: 1, banTime: String(now + 1000) }, now)).toBe(true));
  it("ban có hạn đã hết", () =>
    expect(isAccountBlocked({ role: 1, isBaned: 1, banTime: String(now - 1) }, now)).toBe(false));
  it("banTime bigint vượt Number.MAX_SAFE_INTEGER", () =>
    expect(isAccountBlocked({ role: 1, isBaned: 1, banTime: "9223372036854775807" }, now)).toBe(true));
});

describe("isBcryptHash", () => {
  it("nhận hash bcryptjs ($2b$) — verify được như BCrypt.Net", async () => {
    const h = await bcrypt.hash("x", 4);
    expect(isBcryptHash(h)).toBe(true);
    expect(await bcrypt.compare("x", h.replace(/^\$2b\$/, "$2a$"))).toBe(true);
  });
  it("từ chối plaintext / SHA256 legacy", () => {
    expect(isBcryptHash("123456")).toBe(false);
    expect(isBcryptHash("5c48a8f0e2e0c8c1c3f6a7c0b1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0")).toBe(false);
  });
});

describe("rate-limit đăng nhập in-memory", () => {
  it("chặn sau 5 lần sai của cùng user+IP, user khác vẫn thử được", () => {
    const t = 1_000_000;
    for (let i = 0; i < 5; i++) recordLoginFailure("rluser", "1.1.1.1", t + i);
    expect(loginBlockedFor("rluser", "1.1.1.1", t + 10)).toBeGreaterThan(0);
    expect(loginBlockedFor("other", "1.1.1.1", t + 10)).toBe(0);
  });
  it("thành công xoá bộ đếm theo user", () => {
    const t = 2_000_000;
    for (let i = 0; i < 4; i++) recordLoginFailure("okuser", "2.2.2.2", t + i);
    recordLoginSuccess("okuser", "2.2.2.2");
    recordLoginFailure("okuser", "2.2.2.2", t + 10);
    expect(loginBlockedFor("okuser", "2.2.2.2", t + 11)).toBe(0);
  });
});
