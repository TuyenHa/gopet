import { gamePool, logPool, webPool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";

export const dynamic = "force-dynamic";

/**
 * Health check cho Docker: SELECT 1 cả 3 DB + bảng admin_audit_log tồn tại (migration đã
 * chạy). Không cần đăng nhập nên KHÔNG trả thông tin gì ngoài ok.
 */
export async function GET() {
  try {
    await Promise.all([
      queryOne(gamePool(), "SELECT 1"),
      queryOne(logPool(), "SELECT 1"),
      queryOne(webPool(), "SELECT 1 FROM admin_audit_log LIMIT 1"),
    ]);
    return Response.json({ ok: true });
  } catch (err) {
    console.error("[health] lỗi", err);
    return Response.json({ ok: false }, { status: 503 });
  }
}
