---
title: Web admin Nextjs quan tri game Gopet
description: >-
  Web admin Next.js ở webadmin/, nối thẳng MariaDB (không qua API GServer): tài
  khoản, người chơi, hành trang/pet, template, giftcode, thư, cài đặt, log.
status: in-review
priority: P2
branch: fix/performance
tags:
  - webadmin
  - nextjs
  - mariadb
  - admin
blockedBy: []
blocks: []
created: '2026-09-26T04:54:45.208Z'
createdBy: 'ck:plan'
source: skill
---

# Web admin Nextjs quan tri game Gopet

## Overview

App Next.js (App Router, TS) trong `D:\game\webadmin`, giao diện trắng + sidebar trái.
Kết nối **trực tiếp** 3 DB MariaDB 10.4: `gopettae_tae2` (game), `gopettae_gopet_web` (tài khoản), `gp_log` (log).
Đăng nhập bằng **tài khoản game** (bcrypt trong `user.password`) + bắt buộc `player.isAdmin = 1`.

## Ràng buộc then chốt (đã khảo sát GServer)

- Người chơi online: row `player` nằm trong RAM, bị **ghi đè toàn bộ** khi save (15 phút / logout). → Phase 4 vá server: bảng `player_online` + `server_heartbeat` trong game DB, sửa khoá login (hasLock, 2FA, disconnect). Web khoá sửa player khi online **hoặc khi không thấy heartbeat** (fail-closed).
- Server vẫn ghi vào row người **offline** (tiền bán ki ốt `coin=coin+?`, vào bang `clanId`) → web sửa tiền tệ chỉ bằng delta, cột khác dùng optimistic check.
- Template chỉ nạp lúc khởi động → sửa xong **cần restart server** (banner).
- An toàn khi server chạy: `user` (chỉ đọc lúc login), `gift_code` (phải lấy `gift_code_lock_<code>`), `letter` (targetId = user_id; lấy `login_lock_<username>` khi gửi 1 người), `exchange_gold` (hàng đợi vàng).
- Tặng item/pet: qua giftcode/thư để server tự dựng item (chỉ số random, id, thứ tự sort) — không tự dựng JSON.
- `market`, `clan` bị server snapshot/save đè → chỉ xem.
- `web.user` là MyISAM, `user.coin` là int(11) → delta nguyên tử có chặn tràn int32.
- Chi tiết: [scout-report](./reports/scout-report.md).

## Lộ trình (sau Red Team)

| Vòng | Phase | Kết quả |
|---|---|---|
| **1 — MVP (~20h)** | 1, 2, 3, 5A, 8, 11 | Login, tài khoản (ban, reset mk, 2FA, ngọc), giftcode, thư — deploy chung docker-compose |
| 2 | 4, 5B, 7 (2a→2b), 9 | Khoá online + sửa/tặng cho nhân vật, CRUD template, log |
| 3 | 6 | Sửa/xoá hành trang & pet trực tiếp |
| cuối mỗi vòng | 10 | Test + review + docs cho phần vừa làm |

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Setup project and DB connection](./phase-01-setup-project-and-db-connection.md) | Completed |
| 2 | [Admin login with game account](./phase-02-admin-login-with-game-account.md) | Completed |
| 3 | [White layout and sidebar](./phase-03-white-layout-and-sidebar.md) | Completed |
| 4 | [GServer isOnline patch](./phase-04-gserver-isonline-patch.md) | Completed |
| 5 | [Account and player management](./phase-05-account-and-player-management.md) | Completed |
| 6 | [Inventory and pet editor](./phase-06-inventory-and-pet-editor.md) | Completed |
| 7 | [Template data CRUD](./phase-07-template-data-crud.md) | Completed |
| 8 | [Giftcode letters and settings](./phase-08-giftcode-letters-and-settings.md) | Completed |
| 9 | [Logs and monitoring](./phase-09-logs-and-monitoring.md) | Completed |
| 10 | [Testing and docs](./phase-10-testing-and-docs.md) | Completed |
| 11 | [Docker deploy with GServer](./phase-11-docker-deploy-with-gserver.md) | Completed |

## Dependencies

- 1 → 2 → 3 là nền. 5A, 8, 7, 9 chỉ cần 3. 11 cần 1–3 (deploy sớm, CI chung với gserver).
- 4 (server C#) độc lập, làm song song. 5B cần 4. 6 cần 4 + 5.
- 10 chạy cuối mỗi vòng.
- Không chồng file với plan khác. `260922-0025-thu-he-thong-admin-su-kien` phase 05 (quà đính kèm thư) nếu làm sẽ đổi schema `letter` → phase 8 cập nhật theo.

## Red Team Review

### Session — 2026-09-26
**Findings:** 15 (15 accepted, 0 rejected) — gộp từ 36 phát hiện của 4 reviewer (Bảo mật, Lỗi vận hành, Phá giả định, Phạm vi/YAGNI). Người dùng chọn áp dụng tất cả.
**Severity breakdown:** 3 Critical, 8 High, 4 Medium

| # | Finding | Severity | Disposition | Applied To |
|---|---------|----------|-------------|------------|
| 1 | Khoá online hở: server bỏ qua `hasLock`, nhánh 2FA không khoá, disconnect save→remove để ki ốt save đè, lock rò trên pool | Critical | Accept | Phase 4, 5 |
| 2 | Rollback GServer / deploy lệch thứ tự làm mất guard → heartbeat + fail-closed | Critical | Accept | Phase 4, 5, 10 |
| 3 | Sửa JSON hành trang phá BinarySearch (phải sorted), bỏ sót petSelected/PetDefLeague, stack theo canTrade, chỉ số random → tặng qua giftcode, phase 6 xuống vòng 3 | Critical | Accept | Phase 6, 5 |
| 4 | Server ghi row người offline (ki ốt, clan) → delta/optimistic; bỏ clanId, name, ArenaPoint, KioskFund | High | Accept | Phase 5 |
| 5 | `user` dùng chung nhiều server → cờ online theo game DB | High | Accept | Phase 4 |
| 6 | Lộ `password`/`secretKey` (hash legacy = mật khẩu game) → phép chiếu an toàn, requireAdmin trong DAL | High | Accept | Phase 2, 3, 5 |
| 7 | Leo thang quyền, xoá audit → super-admin, reauth, grant theo bảng, audit chỉ INSERT | High | Accept | Phase 1, 2, 5 |
| 8 | Ghi `login_history` khoá người chơi khỏi game, XFF giả mạo → limiter in-memory, không ghi bảng game | High | Accept | Phase 2, 9 |
| 9 | Migration audit rơi vào game DB; migrate chạy song song; lệch TZ | High | Accept | Phase 2, 9, 10, 11 |
| 10 | Phase 7 xếp nhầm bảng runtime (`exchange_gold`, `clan`), có 17 FK, PK ghép, bảng không PK; cắt còn 6 bảng trước | High | Accept | Phase 7, scout-report |
| 11 | Workflow CI thứ hai kích hoạt restart gserver + đua migrate → 1 workflow, script tham số hoá | High | Accept | Phase 11 |
| 12 | Giftcode: thiếu `gift_code_lock_`, loại quà 3/5/6 không có handler | Medium | Accept | Phase 8 |
| 13 | Thư: targetId = user_id, mất thư khi đua login, không PK → bỏ "huỷ thư" | Medium | Accept | Phase 8 |
| 14 | Scope: bỏ `web_config`/`options` (GServer không đọc), trang nạp tiền (bảng rỗng), xếp hạng; bỏ Drizzle; thêm lộ trình MVP; dashboard tối giản | Medium | Accept | Phase 1, 3, 8, 9, plan.md |
| 15 | Schema sai: `user.coin` int32, `role=3` bị hạ; tước `isAdmin` không tức thì trong game | Medium | Accept | Phase 1, 5 |

## Validation Log

### Session 1 — 2026-09-26
**Verification:** bỏ qua (đã có Red Team Review kèm bằng chứng file:line). Failed: 0.
**Questions asked:** 7

| # | Câu hỏi | Quyết định | Áp dụng |
|---|---|---|---|
| 1 | Số server game production | 1 GServer + 1 game DB | Phase 4 |
| 2 | Truy cập web admin | Domain HTTPS + IP allowlist (service Caddy trong compose) | Phase 2, 11 |
| 3 | Múi giờ | Đổi cả 3 container sang Asia/Ho_Chi_Minh (kiểm mốc reset/sự kiện, cân nhắc migration +7h) | Phase 11 |
| 4 | Super-admin | Chỉ `admin` (user_id=1) | Phase 2, 5, 11 |
| 5 | Tài khoản hash cũ | Migration khoá (role=0) mọi tài khoản không phải bcrypt | Phase 5 |
| 6 | Server từ chối login khi khoá bận >20s | Đồng ý | Phase 4 |
| 7 | Nơi build image webadmin | GitHub Actions + GHCR, máy chủ chỉ pull | Phase 11 |
