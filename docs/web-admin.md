# Web Admin — Quản trị game Gopet qua trang web

**Next.js 16** (App Router + React 19, TypeScript) trong `D:\game\webadmin`.
Kết nối **trực tiếp** 3 database MariaDB 10.4 qua 3 connection pool riêng.
Cung cấp UI quản trị tài khoản, nhân vật, kho đồ, thú cưng, giftcode, thư, cài đặt game, log.

---

## Kiến trúc

### 3 Database Pools

Webadmin kết nối 3 database MariaDB 10.4 riêng biệt qua `src/lib/db/pools.ts`:

```
gamePool()   → gopettae_tae2     (dữ liệu game: player, item, pet, clan, market, template, gift_code, letter, player_online, server_heartbeat)
webPool()    → gopettae_gopet_web (tài khoản: user, admin_audit_log, bank)
logPool()    → gp_log             (log: login_history)
```

Mỗi pool dùng `utf8mb4`, `dateStrings: true` (DATETIME trả về string), `bigNumberStrings: true` (int64→string để tính BigInt).
Pool size: 5 kết nối/database. Cached trên `globalThis` để hot-reload khi dev không mở thêm pool mới.

### DAL Pattern — requireAdmin trong mọi query/action

Mọi hàm DAL (data access layer) ở `src/lib/{domain}/*-queries.ts` và `*-actions.ts` **phải gọi `requireAdmin()` trước tiên**.

`requireAdmin()` mô phỏng lớp xác thực **thật**: mỗi request kiểm lại database
- User còn tồn tại trên web DB (`user.role > 0`, không bị ban)
- User có ít nhất 1 nhân vật với `isAdmin = 1` trên game DB
- Nếu không đủ điều kiện → redirect `/login`
- Kết quả được cache trong request để tránh N+1 query

```typescript
// src/lib/{domain}/{action}-actions.ts
"use server";
export async function doSomething(...): Promise<ActionResult<T>> {
  return runAction(async () => {
    const ctx = await requireAdmin();  // ← PHẢI gọi đầu tiên
    // ctx.userId, ctx.username, ctx.playerName, ctx.isSuperAdmin, ctx.ip
    
    // Mọi SQL dùng ? params, tránh injection
    await audited(ctx, { action: "...", target: "player:123", detail: {...} }, async () => {
      // UPDATE player SET ... WHERE ID = ? AND user_id = ?
    });
    return { ok: true };
  });
}
```

**Super-admin** = chỉ những user trong `env().SUPERADMIN_USER_IDS` (mặc định `user_id=1` admin). 
Admin bình thường (`user.role=1`) không có quyền sensitive như: ban/unlock tài khoản khác, xóa template, chuyển đổi cờ server OFF.

### Audit Log — Fail-Closed + INSERT-Only

Mọi thay đổi được ghi vào `admin_audit_log` (game DB, table append-only):

```sql
INSERT INTO admin_audit_log 
  (admin_id, action, target, before, after, ip, created_at, status)
VALUES (?, ?, ?, ..., NOW(), 'pending');
-- Thực thi sửa (UPDATE/DELETE)
UPDATE admin_audit_log SET status = 'done' WHERE id = ?;
-- Nếu exception → UPDATE ... status = 'failed'
```

**Fail-closed**: nếu INSERT audit thất bại → rollback toàn bộ sửa, không cho thay đổi được ghi lại.
Audit log bị **khóa DELETE** từ phía permissions (grant chỉ SELECT + INSERT).

### proxy.ts — Tiện ích (không là lớp bảo vệ)

File `src/proxy.ts` xử lý:
- Redirect `/` → `/accounts` (giới thiệu)
- Validation IP allowlist (nếu có Caddy)
- Không phải lớp xác thực chính (lớp chính là `requireAdmin` ở DAL)

---

## Mô hình Đăng nhập

### Yêu cầu

1. Tài khoản game phải tồn tại trên web DB (`gopettae_gopet_web.user`)
2. Mật khẩu phải dùng bcrypt hash (`password` LIKE `$2a$` hoặc `$2b$`)
3. Tài khoản phải không bị ban (`isBaned=0`)
4. Phải có 1 nhân vật trên game DB với `isAdmin=1` (`player.isAdmin=1`)

### Tài khoản mật khẩu cũ (legacy hash)

Các tài khoản với hash không phải bcrypt (vd SaltHash, MD5) được **khóa tự động** 
trong migration `migration-260926-lock-legacy-password-accounts.sql` (đặt `role=0`).

Để mở lại: super-admin hoặc admin dùng **"Reset mật khẩu"** → gửi link/mã reset → user đặt mật khẩu mới bcrypt.

Không dùng "Mở khóa" cho tài khoản legacy — sẽ mở lại plaintext login (nguy hiểm).

### Session & Reauth

- JWT token có hiệu lực **8 giờ**. Không revocable trong 8h → nếu revoke (ban/reset) thì phải chờ token hết hạn.
- Cookie session xóa khi logout → nhưng JWT vẫn có thể dùng nếu bị lấy cắp.
- Các thao tác sensitive (ban, unlock, reset password, xóa template) yêu cầu **reauth** — nhập lại mật khẩu hiện tại.

---

## Giao thức Online-Lock với GServer (Phase 4)

### Bảng Phase 4

GServer ghi 2 bảng vào game DB:

**`player_online`** (PK: `user_id`)
```sql
CREATE TABLE player_online (
  user_id INT PRIMARY KEY,
  since DATETIME NOT NULL
);
```
Ghi khi login (trong vùng GET_LOCK), xóa khi disconnect.

**`server_heartbeat`** (PK: `id`=1)
```sql
CREATE TABLE server_heartbeat (
  id TINYINT PRIMARY KEY,
  protocol_version INT NOT NULL,
  beat_at DATETIME NOT NULL
);
```
GServer UPDATE mỗi 30 giây. Webadmin kiểm `beat_at > NOW()-90s` để biết server còn sống.

### Giao thức Webadmin ↔ GServer

Khi webadmin sửa dữ liệu player **offline**:

```
1. Lấy connection riêng từ gamePool
2. GET_LOCK('login_lock_<username>', 5 giây)
   - Nếu không lấy được (=0) → báo "Tài khoản đang đăng nhập, thử lại"
3. Kiểm heartbeat: beat_at > NOW()-90s AND protocol_version >= 1
   - Nếu heartbeat cũ/không tìm → khoá sửa (fail-closed) → báo "Server không respond"
4. Kiểm NOT EXISTS player_online WHERE user_id = ?
   - Nếu online → báo "Nhân vật đang online, máy chủ giữ dữ liệu"
5. Thực thi UPDATE ... SET max_statement_time=8 FOR ...
6. RELEASE_LOCK (luôn, dù success hay error)
```

Fail-closed: nếu bảng `player_online` chưa tồn tại (schema cũ / lỗi query) → coi như online → khoá sửa.

### Công tắc "Server đã TẮT" (super-admin)

Nếu bảo trì GServer (tắt hẳn, không có heartbeat), super-admin có thể bật công tắc 
"Server đã TẮT" (ghi audit) → webadmin bỏ kiểm heartbeat, chỉ kiểm `player_online`.

Khi GServer restart, công tắc **tự động tắt** (cơ chế tự động chưa implement, cần manual).

---

## Bảng — Cái nào an toàn sửa khi server chạy?

| Bảng | Khi server chạy | Ghi chú |
|---|---|---|
| **`user`** | ✅ CHỈ ĐỌC lúc login | Webadmin không sửa user (giành cho khác) |
| **`player`** | ❌ KHOÁ nếu online | Dùng `withOfflinePlayer` + lock |
| **`player_online`** | ❌ CHỈ ĐỌC | GServer quản lý (web chỉ SELECT) |
| **`server_heartbeat`** | ✅ CHỈ ĐỌC | GServer cập nhật |
| **`gift_code`** | ✅ (với lock) | Phải GET_LOCK('gift_code_lock_<code_lowercase>') |
| **`letter`** | ✅ (với lock) | Ghi khi gửi thư cho người chơi; targetId=user_id; lấy login_lock_<username> |
| **`exchange_gold`** | ✅ (hàng đợi) | Đơn trao đổi vàng (cơ chế hàng đợi, không ghi trực tiếp coin) |
| **`market` (kiosk)** | ❌ CHỈ ĐỌC | Server snapshot/save đè |
| **`clan`** | ❌ CHỈ ĐỌC | Server save đè |
| **`item`, `shop`, `pet`, ... (template)** | ❌ RESTART CẦN | Máy chủ chỉ nạp lúc khởi động → sửa xong cần restart |

**Coin/Gold**: sửa chỉ bằng **delta** (UPDATE player SET coin = coin + ?) với ràng buộc `BETWEEN 0 AND 2147483647`.
Không bao giờ SET coin = ? trực tiếp (GServer overwrite full-row).

---

## Giới hạn

1. **Không kick player** — không có lệnh disconnect từ web; player phải thoát game để mở khóa.
2. **Revoking isAdmin không tức thì** — phải chờ player reload nhân vật (hoặc logout/login).
3. **Gửi thư cho toàn server** — race condition nếu khi đó có người login/logout (web không thấy mọi người).
4. **JWT không revocable trong 8h** — nếu ban/reset password, token cũ vẫn dùng được đến hết hạn.
5. **Mật khẩu legacy** — chỉ unlock qua reset (không dùng "mở khóa" trực tiếp).

---

## Cài đặt phát triển

### 1. Biến môi trường (`.env.local`)

```bash
# Database
DB_HOST=127.0.0.1
DB_PORT=3306
DB_USER=gopet_admin
DB_PASSWORD=<mật khẩu đặt khi CREATE USER>
DB_GAME=gopettae_tae2
DB_WEB=gopettae_gopet_web
DB_LOG=gp_log

# Super-admin user IDs (comma-separated)
SUPERADMIN_USER_IDS=1

# Phiên đăng nhập (bắt buộc, >= 32 ký tự): openssl rand -base64 48
SESSION_SECRET=<chuỗi ngẫu nhiên>
# true CHỈ khi đứng sau Caddy (mới tin X-Forwarded-For)
TRUSTED_PROXY=false
# Lệch múi giờ DB (phút), mặc định 420 = +07:00
# DB_TIMEZONE_OFFSET_MIN=420
```

### 2. Database — tài khoản gopet_admin

Tạo user + grant từ `docker/webadmin-grants.sql` (chạy qua docker):

```bash
PASS=$(grep '^MARIADB_ROOT_PASSWORD=' D:/game/docker/.env | cut -d= -f2-)
# Tạo user trước (grant cần bảng admin_audit_log đã có → chạy migration trước)
docker exec -i -e MYSQL_PWD="$PASS" gopet-mariadb mysql -uroot   -e "CREATE USER IF NOT EXISTS 'gopet_admin'@'%' IDENTIFIED BY '<mật khẩu>'"
docker exec -i -e MYSQL_PWD="$PASS" gopet-mariadb mysql -uroot --default-character-set=utf8mb4 \
  < D:/game/docker/webadmin-grants.sql
```

Tài khoản test local: `wadtest` (user_id 1467, player isAdmin=1) — lead xóa trước deploy.

### 3. Chạy dev server

```bash
cd D:\game\webadmin
npm install
npm run dev
# Tìm http://localhost:3000 → /login
```

### 4. Chạy tests

```bash
# Unit tests (không cần DB)
npm run test
# hoặc
npx vitest run tests/unit

# Integration tests (cần local DB + RUN_DB_TESTS=1)
RUN_DB_TESTS=1 npx vitest run tests/integration

# Cả hai
RUN_DB_TESTS=1 npm run test
```

Test configuration: `webadmin/vitest.config.mts`.
Alises: `@` → `src/`, `server-only` → stub để test chạy.

### 5. Type checking + Linting

```bash
npx tsc --noEmit
npm run lint
npm audit --omit=dev
```

### 6. Build

```bash
npm run build
npm start  # production server
```

---

## Deployment

Chi tiết: `docs/deployment-linux-backend.md` § 11 (trang quản trị webadmin).

- Docker image: `webadmin:latest` (built từ `Dockerfile`)
- Caddy reverse proxy: IP allowlist + HTTPS (tự ký hoặc Let's Encrypt)
- Database grants: `docker/webadmin-grants.sql`
- CI/CD: `.github/workflows/gserver-ci-cd.yml` (build → GHCR → deploy)

---

## Tham khảo thêm

- **Kế hoạch**: `plans/260926-1100-web-admin-nextjs/plan.md` + phase files
- **Code review**: `plans/reports/code-reviewer-260926-webadmin-review.md`
- **Test report**: `plans/reports/tester-260926-webadmin-tests.md`
- **Quy ước code**: `plans/260926-1100-web-admin-nextjs/reports/webadmin-foundation-conventions.md`
- **Deployment backend**: `docs/deployment-linux-backend.md` § 1–10
