# Webadmin — Trang quản trị game Gopet

Next.js 16 (App Router + React 19, TypeScript, Tailwind v4) kết nối 3 database MariaDB 10.4.

## Cài đặt nhanh

### 1. Biến môi trường

Tạo `webadmin/.env.local`:

```bash
DB_HOST=127.0.0.1
DB_PORT=3306
DB_USER=gopet_admin
DB_PASSWORD=<mật khẩu>
DB_GAME=gopettae_tae2
DB_WEB=gopettae_gopet_web
DB_LOG=gp_log

SUPERADMIN_USER_IDS=1
# Phiên đăng nhập (bắt buộc, >= 32 ký tự): openssl rand -base64 48
SESSION_SECRET=<chuỗi ngẫu nhiên>
# true CHỈ khi đứng sau Caddy (mới tin X-Forwarded-For)
TRUSTED_PROXY=false
# Lệch múi giờ DB (phút), mặc định 420 = +07:00
# DB_TIMEZONE_OFFSET_MIN=420
```

### 2. Cài đặt dependencies

```bash
npm install
```

### 3. Chạy dev server

```bash
npm run dev
# Mở http://localhost:3000 → /login
```

## Testing

```bash
# Unit tests
npm test

# Integration tests (cần DB)
RUN_DB_TESTS=1 npm test

# Type check
npx tsc --noEmit

# Lint
npm run lint

# Audit
npm audit --omit=dev
```

## Build & Production

```bash
npm run build
npm start
```

## Cấu trúc dự án

```
webadmin/
├── src/
│   ├── app/          # Routes & pages (Next.js App Router)
│   ├── lib/          # DAL, utilities, auth
│   │   ├── db/       # Pools, queries, named locks
│   │   ├── auth/     # requireAdmin, session, rate-limit
│   │   └── {domain}/ # Query/action files per domain
│   └── components/   # Reusable UI components
├── tests/            # Unit + integration tests
├── vitest.config.mts # Test configuration
└── docker/           # Dockerfile, deploy scripts
```

## Khóa học

Xem chi tiết: `docs/web-admin.md`

- **Kiến trúc**: 3 database pool, DAL pattern với requireAdmin
- **Audit**: Fail-closed, INSERT-only log
- **Online-lock**: Giao thức với GServer (player_online, server_heartbeat)
- **Bảng an toàn**: Cái nào sửa được khi server chạy
- **Phát triển**: Setup, testing, deploy

## Deployment

Xem: `docs/deployment-linux-backend.md` § 11 (webadmin + Caddy + GitHub Actions)

---

**Version**: Next.js 16  
**Database**: MariaDB 10.4  
**Node**: 18+  
**License**: Internal
