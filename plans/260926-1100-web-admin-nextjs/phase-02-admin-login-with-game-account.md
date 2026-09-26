---
phase: 2
title: "Admin login with game account"
status: completed
priority: P1
effort: "4h"
dependencies: [1]
---

# Phase 2: Admin login with game account

> Cập nhật sau Red Team 2026-09-26 (RT#6, RT#7, RT#8, RT#9): không ghi `login_history`; không tin `x-forwarded-for`; `requireAdmin()` trong mọi hàm DAL; migration có header DB; super-admin.
<!-- Updated: Validation Session 1 - SUPERADMIN_USER_IDS=1 (tài khoản admin); TRUSTED_PROXY=true sau Caddy; migration khoá tài khoản hash legacy -->

## Overview
Đăng nhập bằng tài khoản game (`gopettae_gopet_web.user`), chỉ cho vào khi user có nhân vật `gopettae_tae2.player.isAdmin = 1`. Session = JWT (jose) trong cookie httpOnly. Audit log mọi thao tác ghi.

## Requirements
- Functional: `/login`, logout, chặn user bị ban / role=0, rate-limit, audit log, super-admin.
- Non-functional: cookie httpOnly, `secure` khi HTTPS, sameSite=lax, hạn 8h. Không bao giờ log password/hash/secretKey.

## Architecture
Luồng `loginAction`:
1. zod: username `^[a-z0-9]+$`, password 1..100.
2. Rate-limit **in-memory** theo cặp (username, IP) + theo IP: 5 lần sai/5 phút → backoff tăng dần. **Không INSERT `login_history`** — server đếm MỌI dòng theo `IPAddress+UserName` (không lọc IsSuccess/IsWebLogin, `Player.cs:604-630`), ghi vào sẽ khoá người chơi khỏi game [RT#8].
3. IP: lấy từ socket; chỉ đọc `x-forwarded-for` khi `TRUSTED_PROXY=true` (reverse proxy do mình cấu hình) [RT#8].
4. `SELECT user_id, password, role, isBaned, banTime FROM user WHERE username=?` (chỉ ở đây mới đọc `password`, không trả ra ngoài hàm).
5. `bcryptjs.compare` nếu hash bắt đầu `$2`; hash legacy (plaintext/SHA256) → từ chối, báo "cần đổi mật khẩu". User không tồn tại → vẫn chạy compare với hash giả.
6. Chặn: `role=0`; `isBaned=2`; `isBaned=1 AND banTime > nowMs`.
7. `SELECT ID,name FROM player WHERE user_id=? AND isAdmin=1 LIMIT 1` → rỗng thì từ chối. Lỗi hiển thị chung.
8. JWT `{sub:user_id, username, playerName}` HS256, cookie `gopet_admin_session`. Ghi audit `auth.login` (thành công/thất bại, IP).

Bảo vệ [RT#6]:
- `middleware.ts`: chỉ là lớp tiện lợi (redirect `/login`), **không** phải lớp bảo mật (tiền lệ bypass CVE-2025-29927).
- `requireAdmin()` được gọi **bên trong mọi hàm đọc/ghi DAL** (`lib/**/…-queries.ts`, `…-actions.ts`), không dựa vào layout. Mỗi lần gọi query lại `isAdmin` + ban (rẻ, ít admin) → thu quyền có hiệu lực ngay trên web.
- `requireSuperAdmin()`: `sub ∈ SUPERADMIN_USER_IDS` + `requireAdmin()` [RT#7]. `reauth(password)` cho thao tác nhạy cảm.

Audit log — migration `SRCGOPETGOC/MariaDB_SQL/migration-260926-admin-audit-log.sql`, **dòng đầu `-- database: gopettae_gopet_web`** (không có thì `docker/migrate-db.sh:79-81` chạy vào `gopettae_tae2`) [RT#9]:
```sql
-- database: gopettae_gopet_web
CREATE TABLE IF NOT EXISTS admin_audit_log (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  admin_user_id INT NOT NULL,
  admin_username VARCHAR(20) NOT NULL,
  action VARCHAR(64) NOT NULL,
  target VARCHAR(128) NULL,
  detail LONGTEXT NULL,          -- JSON {before, after}; không chứa password/secretKey
  ip VARCHAR(64) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY idx_created (created_at), KEY idx_target (target)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```
- Thứ tự ghi: audit trước (status `pending`) → mutate → không UPDATE được audit (chỉ INSERT) nên ghi thêm dòng `…:done`/`…:failed`. Audit lỗi → **không** mutate (fail-closed) [RT#9].

## Related Code Files
- Create: `SRCGOPETGOC/MariaDB_SQL/migration-260926-admin-audit-log.sql`
- Create: `webadmin/src/app/login/page.tsx`, `login-form.tsx`
- Create: `webadmin/src/lib/auth/session.ts`, `login-action.ts`, `logout-action.ts`, `require-admin.ts`, `require-superadmin.ts`, `reauth.ts`, `rate-limit.ts`, `client-ip.ts`
- Create: `webadmin/src/lib/audit/write-audit-log.ts`
- Create: `webadmin/src/middleware.ts`

## Implementation Steps
1. Migration audit (chạy qua `docker/migrate-db.sh`, không tạo tay) — kiểm bảng nằm ở web DB.
2. session / rate-limit / client-ip.
3. login-action + trang `/login` (nền trắng, card giữa, "Gopet Admin").
4. require-admin / require-superadmin / reauth; middleware.
5. write-audit-log (fail-closed).

## Success Criteria
- [x] Tài khoản không có player isAdmin=1 không vào được
- [x] Login web sai nhiều lần KHÔNG làm game khoá đăng nhập của user đó
- [x] Gửi `X-Forwarded-For` giả không đổi IP ghi nhận (khi TRUSTED_PROXY=false)
- [x] Gọi action/đọc DAL không session → lỗi, không truy cập DB
- [x] Bỏ isAdmin → request kế tiếp bị từ chối
- [x] `admin_audit_log` nằm trong `gopettae_gopet_web`

## Risk Assessment
- Admin còn hash legacy → không login web; super-admin dùng "Buộc reset" (phase 5A).
- Quyền web = quyền game (lựa chọn của user) — ghi rõ trong docs.

## Security Considerations
- SESSION_SECRET ≥ 32 ký tự; Server Actions kiểm Origin; sameSite=lax.
- Audit không chứa hash/secret; `gopet_admin` không xoá/sửa được audit.
