import { NextResponse, type NextRequest } from "next/server";

/**
 * Lớp TIỆN LỢI: chưa có cookie phiên → chuyển tới /login. KHÔNG phải lớp bảo mật —
 * bảo vệ thật là requireAdmin() bên trong mọi hàm DAL/action.
 */
export function proxy(req: NextRequest) {
  if (!req.cookies.has("gopet_admin_session")) {
    return NextResponse.redirect(new URL("/login", req.url));
  }
  return NextResponse.next();
}

export const config = {
  // Bỏ qua trang login, health check và tài nguyên tĩnh.
  matcher: ["/((?!login|api/health|_next/static|_next/image|favicon.ico).*)"],
};
