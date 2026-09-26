import { describe, expect, it } from "vitest";
import { buildHref, likeContains, parsePage, parseQuery } from "@/lib/pagination";
import { formatDateTime, formatNumber } from "@/lib/format";

describe("parsePage", () => {
  it("mặc định trang 1, size 20", () => expect(parsePage({})).toEqual({ page: 1, size: 20, offset: 0 }));
  it("size ngoài danh sách cho phép → mặc định", () => expect(parsePage({ size: "99999" }).size).toBe(20));
  it("page rác/âm → 1", () => {
    expect(parsePage({ page: "abc" }).page).toBe(1);
    expect(parsePage({ page: "-3" }).page).toBe(1);
  });
  it("offset đúng", () => expect(parsePage({ page: "3", size: "50" }).offset).toBe(100));
});

describe("likeContains / parseQuery", () => {
  it("escape ký tự LIKE", () => expect(likeContains("a%b_c\\")).toBe("%a\\%b\\_c\\\\%"));
  it("trim + giới hạn 100 ký tự", () => expect(parseQuery({ q: `  ${"x".repeat(150)} ` }).length).toBe(100));
});

describe("buildHref", () => {
  it("giữ tham số cũ, ghi đè + xoá tham số rỗng", () =>
    expect(buildHref("/accounts", { q: "abc", page: "2" }, { page: undefined, size: 50 })).toBe(
      "/accounts?q=abc&size=50",
    ));
});

describe("format", () => {
  it("bigint dạng chuỗi", () => expect(formatNumber("9007199254740993")).toBe("9.007.199.254.740.993"));
  it("DATETIME", () => expect(formatDateTime("2026-09-26 13:05:09")).toBe("26/09/2026 13:05"));
});
