---
phase: 10
title: "Testing and docs"
status: completed
priority: P2
effort: "6h"
dependencies: [5, 6, 7, 8, 9]
---

# Phase 10: Testing and docs

## Overview
Kiểm thử tự động phần logic rủi ro cao, kiểm thử thật với GServer + client Unity, review code, cập nhật tài liệu.

## Requirements
- Unit test (Vitest): JSON round-trip item/pet (dùng mẫu thật trích từ dump, không dữ liệu giả), sinh id không trùng, zod schema gift_data, registry allowlist, parse bigint, logic ban (banTime), bcrypt verify với hash thật từ DB.
- Integration test (Vitest + DB MariaDB docker local, DB test riêng nạp từ dump): offline-guard (online → từ chối; lock đang giữ → từ chối), action cộng coin nguyên tử, giftcode tạo/đọc.
- E2E thủ công có checklist (hoặc Playwright cho login + điều hướng sidebar).
- Bổ sung sau Red Team 2026-09-26:
  - Chạy `docker/migrate-db.sh` trên DB docker **sạch** → `admin_audit_log` ở web DB, `player_online`/`server_heartbeat` ở game DB.
  - Khoá online: login thường, login **2FA**, createChar, disconnect có giao dịch ki ốt đang chạy, rollback GServer về image cũ → web phải khoá sửa (heartbeat).
  - `offline-guard`: lock luôn được nhả kể cả khi throw (kiểm `IS_USED_LOCK` = NULL sau đó).
  - Grep response HTML/RSC các trang tài khoản: không chứa `$2a$`/`$2b$`/giá trị `secretKey`.
  - Login web sai 10 lần → vẫn đăng nhập game bình thường.
  - Giftcode: đổi code song song với reset từ web.
- Docs.

## Architecture
```
webadmin/vitest.config.ts
webadmin/tests/unit/*.test.ts
webadmin/tests/integration/*.test.ts   # cần DB_TEST_* env, skip nếu thiếu
webadmin/tests/fixtures/               # JSON items/pets trích từ dump thật
```

## Related Code Files
- Create: test files, `webadmin/README.md` (cài đặt, env, chạy)
- Create: `docs/web-admin.md` (kiến trúc, giao thức khoá online, bảng nào an toàn sửa khi server chạy, cấp quyền admin)
- Triển khai Docker: xem phase 11 (không làm ở đây)
- Modify: `docs/project-changelog.md`, `docs/development-roadmap.md` nếu tồn tại

## Implementation Steps
1. Trích fixtures từ `SRCGOPETGOC/MariaDB_SQL/server_db.sql` (vài player có items/pets đa dạng).
2. Viết unit + integration test; `npm test` pass.
3. Checklist E2E thật: login web → ban/unban → reset mk → sửa vàng offline → thêm item/pet → giftcode → thư → sửa shop + restart → xác nhận trong client Unity.
4. `code-reviewer` agent review toàn bộ `webadmin/` + patch GServer.
5. `docs-manager` cập nhật docs.

## Success Criteria
- [x] `npm run build`, `npx tsc --noEmit` — passed (tsc/lint clean, 20 routes compiled)
- [x] `npm test` — 140/140 vitest pass (121 unit + 19 DB integration)
- [x] Smoke test all pages 200 with real session
- [x] Grep response: no password hash/secretKey revealed
- [x] Review C1/H1-H6/M1-M3/M6-M8 fixed; M4/M5 accepted (code review complete)
- [ ] Checklist E2E full (pending manual QA: login with client, ban/unban effect, giftcode/letter receipt, template edit + restart, inventory item operations)

## Risk Assessment
- Integration test chạm DB → bắt buộc DB test riêng, test tự dọn dữ liệu; không chạy trên DB thật.

## Security Considerations
- Triển khai: webadmin KHÔNG public Internet trần; đặt sau VPN/IP allowlist + HTTPS. DB vẫn chỉ bind loopback/mạng nội bộ.
- `npm audit` trước khi deploy.
