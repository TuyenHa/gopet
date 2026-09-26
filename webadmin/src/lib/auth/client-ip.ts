import "server-only";
import { headers } from "next/headers";
import { env } from "@/lib/env";

/**
 * IP client dùng cho rate-limit + audit. Chỉ tin `X-Forwarded-For` khi TRUSTED_PROXY=true
 * (đứng sau Caddy do mình cấu hình): lấy phần tử CUỐI — do proxy tự thêm, client không
 * giả được. Không có proxy tin cậy → không đọc header nào (client tự đặt được) và trả "direct".
 */
export async function getClientIp(): Promise<string> {
  if (!env().TRUSTED_PROXY) return "direct";
  const xff = (await headers()).get("x-forwarded-for");
  const last = xff?.split(",").map((s) => s.trim()).filter(Boolean).pop();
  return (last ?? "unknown").slice(0, 64);
}
