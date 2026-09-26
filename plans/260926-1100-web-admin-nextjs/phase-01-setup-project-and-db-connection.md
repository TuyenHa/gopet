---
phase: 1
title: "Setup project and DB connection"
status: completed
priority: P1
effort: "3h"
dependencies: []
---

# Phase 1: Setup project and DB connection

> Cập nhật sau Red Team 2026-09-26 (RT#7, RT#15): bỏ Drizzle (hai nguồn metadata, phần lớn thao tác ghi cần raw SQL/GET_LOCK); grant theo bảng thay vì `db.*`.

## Overview
Khởi tạo `D:\game\webadmin` (Next.js App Router + TS + Tailwind + shadcn/ui), kết nối 3 DB MariaDB 10.4 bằng `mysql2/promise` + SQL tham số hoá + type TS viết tay cho các bảng dùng tới. Không bao giờ đổi schema game từ app (schema chỉ đổi qua file `migration-*.sql`).

## Requirements
- Functional: 3 pool (game/web/log), helper query có kiểu, `.env.example`.
- Non-functional: DB chỉ truy cập server-side, utf8mb4, pool nhỏ (5/DB), build + typecheck sạch.

## Architecture
```
webadmin/
  src/app/                  # routes (App Router)
  src/components/           # shadcn + component dùng chung
  src/lib/env.ts            # zod validate env
  src/lib/db/pools.ts       # gamePool, webPool, logPool (import 'server-only')
  src/lib/db/query.ts       # query<T>(pool, sql, params), withConnection(pool, fn)
  src/lib/db/types/*.ts     # type viết tay: UserRow, PlayerRow, GiftCodeRow... (chỉ bảng dùng tới)
  .env.example
```
- Stack: Next.js bản mới nhất (App Router, Server Components, Server Actions), TypeScript strict, Tailwind v4, shadcn/ui, TanStack Table, zod, react-hook-form, `mysql2`, `bcryptjs`, `jose`, `sonner`.
- Đọc = Server Components; ghi = Server Actions. Không dựng REST API (trừ `/api/health` ở phase 11).
- Metadata bảng template: registry phase 7 là nguồn duy nhất.
- Kiểu số: `bigint(20)` (gold, coin, lua của player, banTime) đọc dạng string, tính bằng `BigInt`; `web.user.coin` là **int(11)** [RT#15].

DB user `gopet_admin` — grant **theo bảng**, quyền tối thiểu [RT#7]:
```sql
CREATE USER 'gopet_admin'@'%' IDENTIFIED BY '<mật khẩu>';
-- game DB: đọc toàn bộ, ghi đúng bảng cần
GRANT SELECT ON gopettae_tae2.* TO 'gopet_admin'@'%';
GRANT INSERT,UPDATE,DELETE ON gopettae_tae2.gift_code TO 'gopet_admin'@'%';
GRANT INSERT ON gopettae_tae2.letter TO 'gopet_admin'@'%';
GRANT INSERT ON gopettae_tae2.exchange_gold TO 'gopet_admin'@'%';
GRANT UPDATE ON gopettae_tae2.player TO 'gopet_admin'@'%';
-- + INSERT,UPDATE,DELETE cho từng bảng template khi thêm vào registry phase 7
-- web DB
GRANT SELECT ON gopettae_gopet_web.* TO 'gopet_admin'@'%';
GRANT UPDATE ON gopettae_gopet_web.user TO 'gopet_admin'@'%';
GRANT UPDATE ON gopettae_gopet_web.server TO 'gopet_admin'@'%';
GRANT INSERT ON gopettae_gopet_web.admin_audit_log TO 'gopet_admin'@'%';   -- KHÔNG UPDATE/DELETE
-- log DB
GRANT SELECT ON gp_log.* TO 'gopet_admin'@'%';
```
Danh sách grant sống trong một file SQL (`docker/webadmin-grants.sql`) để cập nhật khi mở rộng registry.

## Related Code Files
- Create: toàn bộ `webadmin/` (package.json, next.config.ts, tsconfig.json, components.json, `src/lib/**`, `.env.example`, `.gitignore` có `.env*.local`)
- Create: `docker/webadmin-grants.sql`
- Modify: `docker/README.md` (cách tạo user `gopet_admin`)

## Implementation Steps
1. Trong `D:\game`: `npx create-next-app@latest webadmin --ts --tailwind --eslint --app --src-dir --use-npm --import-alias "@/*"`.
2. `npx shadcn@latest init` (neutral, nền trắng). Add: button input label card table dialog alert-dialog dropdown-menu form select badge tabs sonner separator sheet tooltip skeleton textarea switch.
3. `npm i mysql2 zod bcryptjs jose server-only @tanstack/react-table react-hook-form @hookform/resolvers` ; `npm i -D @types/bcryptjs vitest`.
4. `.env.example`: `DB_HOST DB_PORT DB_USER DB_PASSWORD DB_GAME=gopettae_tae2 DB_WEB=gopettae_gopet_web DB_LOG=gp_log SESSION_SECRET SUPERADMIN_USER_IDS TRUSTED_PROXY=false`.
5. `env.ts` zod; `pools.ts` (`charset:'utf8mb4', connectionLimit:5, dateStrings:true, supportBigNumbers:true, bigNumberStrings:true`, cache `globalThis`); `query.ts`.
6. Type viết tay cho `user`, `player`, `gift_code`, `letter` từ `SHOW CREATE TABLE` trên DB thật.
7. `docker/webadmin-grants.sql` + README.
8. Trang tạm `/health` (xoá ở phase 3). `npm run build`, `npx tsc --noEmit` pass.

## Success Criteria
- [x] `/health` báo 3 DB OK với user `gopet_admin` (không root)
- [x] `gopet_admin` không UPDATE/DELETE được `admin_audit_log`
- [x] `.env.local` bị gitignore; build + typecheck pass

## Risk Assessment
- Type viết tay lệch khi có migration mới → khi thêm migration đụng bảng web dùng, cập nhật type cùng PR (ghi vào docs phase 10).
- Không nâng MariaDB (game phụ thuộc hành vi 10.4).

## Security Considerations
- `import 'server-only'` trong mọi file `lib/db`, `lib/auth`.
- Không root; grant theo bảng; audit chỉ INSERT.
