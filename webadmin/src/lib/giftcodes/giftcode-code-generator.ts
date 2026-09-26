import "server-only";
import { randomInt } from "node:crypto";
import { gamePool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";

// Bỏ ký tự dễ nhầm khi đọc/gõ tay: 0/O, 1/I/L.
const ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
const MAX_ATTEMPTS = 8;

function randomCode(length: number): string {
  let out = "";
  for (let i = 0; i < length; i++) out += ALPHABET[randomInt(ALPHABET.length)];
  return out;
}

/** Sinh code ngẫu nhiên chưa tồn tại trong `gift_code`. Ném lỗi nếu đụng độ liên tục (cực hiếm). */
export async function generateUniqueGiftCode(length = 10): Promise<string> {
  for (let i = 0; i < MAX_ATTEMPTS; i++) {
    const code = randomCode(length);
    const exists = await queryOne(gamePool(), "SELECT 1 FROM gift_code WHERE code = ?", [code]);
    if (!exists) return code;
  }
  throw new Error("Không sinh được mã giftcode duy nhất sau nhiều lần thử.");
}

export function isDuplicateKeyError(err: unknown): boolean {
  return typeof err === "object" && err !== null && (err as { code?: string }).code === "ER_DUP_ENTRY";
}
