---
phase: 11
title: "Docker deploy with GServer"
status: completed
priority: P2
effort: "5h"
dependencies: [1, 2, 3]
---

# Phase 11: Docker deploy with GServer

## Overview
Deploy webadmin lên **cùng stack Docker** với GServer + MariaDB trên máy chủ Linux: thêm service `webadmin` vào `docker/docker-compose.yml` hiện có (profile `server`), dùng chung mạng compose, chung `docker/.env`, chung quy trình deploy/CI.

Quyết định: 1 compose stack, **3 container** (mariadb, gserver, webadmin) — không nhét Next.js và .NET vào một container (khác runtime, không restart/rollback độc lập được, healthcheck lẫn nhau).

> Cập nhật sau Red Team 2026-09-26 (RT#9, RT#14): một workflow CI chung, script deploy tham số hoá, migrate có flock, TZ thống nhất. Phụ thuộc đổi thành [1,2,3] để **deploy được ngay bản MVP**, không chờ phase 10.

<!-- Updated: Validation Session 1 - domain HTTPS + IP allowlist qua Caddy; image build trên GitHub Actions + GHCR; TZ Asia/Ho_Chi_Minh cho cả 3 container -->

## Requirements
- Functional: `docker compose --profile server up -d` chạy mariadb + gserver + webadmin + caddy; webadmin nối DB qua `mariadb:3306`; image webadmin **build trên GitHub Actions, push GHCR**, máy chủ chỉ `pull`.
- Truy cập: **domain HTTPS + IP allowlist** qua service `caddy` (tự lấy chứng chỉ Let's Encrypt); webadmin không publish cổng ra host.
- Múi giờ: `TZ=Asia/Ho_Chi_Minh` cho **cả 3** container + MariaDB `--default-time-zone=+07:00`.
- Non-functional: image nhỏ (`output: 'standalone'`, node alpine, non-root); rollback tự động về tag trước nếu không healthy.

## Architecture
```
Internet ──443──> caddy (profile server, 80/443 public)
                    │ chỉ IP trong WEBADMIN_ALLOWED_IPS, còn lại 403
                    └──> webadmin:3000 (mạng compose, KHÔNG publish ra host)
docker-compose (profile server)
 ├─ mariadb    127.0.0.1:3306        (sẵn có)
 ├─ gserver    0.0.0.0:19180         (sẵn có)
 ├─ webadmin   image ghcr.io/<owner>/gopet-webadmin:${WEBADMIN_TAG}   (mới)
 └─ caddy      caddy:2-alpine, 80/443, volume caddy_data             (mới)
```
`docker/caddy/Caddyfile`:
```
{$WEBADMIN_DOMAIN} {
  @blocked not remote_ip {$WEBADMIN_ALLOWED_IPS}
  respond @blocked 403
  reverse_proxy webadmin:3000
}
```
- webadmin: `TRUSTED_PROXY=true` (chỉ Caddy đứng trước, Caddy set XFF) → lấy IP client từ XFF phần tử cuối do Caddy thêm; cookie `secure=true`.
- Đổi TZ gserver/mariadb [Validation]: trước khi bật, grep GServer dùng `DateTime.Now` vs `UtcNow` và các mốc reset (điểm danh ngày, sự kiện, bảo trì `ServerSetting` giờ) — dữ liệu DATETIME đã lưu theo UTC sẽ lệch 7h một lần: chấp nhận (dữ liệu test) hoặc migration `+7h` các cột thời gian quan trọng (`gift_code.expire`, `letter.time`, `player.loginDate`…). Ghi quyết định vào docs.

GHCR [Validation]:
- CI job `build-webadmin`: `docker/build-push-action` → `ghcr.io/<owner>/gopet-webadmin:<git-sha>` + `:latest`, dùng `GITHUB_TOKEN` (permissions `packages: write`).
- Máy chủ: `docker login ghcr.io` một lần bằng PAT **read:packages**; deploy = set `WEBADMIN_TAG=<sha>` trong `docker/.env` → `compose pull webadmin` → `up -d --no-build webadmin` → chờ healthy → lỗi thì quay lại tag cũ (lưu trong `docker/.webadmin-prev-tag`).

Service mới (phác thảo):
```yaml
  webadmin:
    profiles: ["server"]
    image: ghcr.io/${GHCR_OWNER:?}/gopet-webadmin:${WEBADMIN_TAG:-latest}
    container_name: gopet-webadmin
    restart: unless-stopped
    depends_on:
      mariadb:
        condition: service_healthy
    environment:
      DB_HOST: mariadb
      DB_PORT: "3306"
      DB_USER: ${WEBADMIN_DB_USER:-gopet_admin}
      DB_PASSWORD: ${WEBADMIN_DB_PASSWORD:?Thiếu WEBADMIN_DB_PASSWORD}
      DB_GAME: gopettae_tae2
      DB_WEB: gopettae_gopet_web
      DB_LOG: gp_log
      SESSION_SECRET: ${WEBADMIN_SESSION_SECRET:?Thiếu WEBADMIN_SESSION_SECRET}
      NODE_ENV: production
      TZ: Asia/Ho_Chi_Minh
      TRUSTED_PROXY: "true"
      SUPERADMIN_USER_IDS: ${WEBADMIN_SUPERADMIN_USER_IDS:-1}
    # không có ports: chỉ caddy truy cập qua mạng compose
    mem_limit: 384m
    healthcheck:
      test: ["CMD", "wget", "-qO-", "http://127.0.0.1:3000/api/health"]
      interval: 15s
      timeout: 5s
      retries: 5
      start_period: 30s
```
Dockerfile (multi-stage):
```
FROM node:22-alpine AS deps     -> npm ci
FROM node:22-alpine AS build    -> npm run build (next.config: output 'standalone')
FROM node:22-alpine AS runtime  -> copy .next/standalone, .next/static, public; USER node; CMD node server.js
```
- `/api/health`: route handler duy nhất không cần auth, `SELECT 1` 3 DB **và** kiểm bảng `admin_audit_log` tồn tại trong web DB; trả `{ok:true}` (không lộ thông tin khác). Middleware bỏ qua route này.
- Cookie `secure=true` (luôn qua HTTPS của Caddy).

## Related Code Files
- Create: `webadmin/Dockerfile`, `webadmin/.dockerignore` (node_modules, .next, .env*, tests)
- Create: `docker/caddy/Caddyfile`; service `caddy` + volume `caddy_data` trong compose
- Modify: compose — `TZ: Asia/Ho_Chi_Minh` cho mariadb + gserver, mariadb `--default-time-zone=+07:00`
- Create: `webadmin/src/app/api/health/route.ts`
- Rename + Modify: `docker/deploy-gserver.sh` → tham số hoá `deploy-service.sh <gserver|webadmin>` (tên container, image, health timeout theo service; giữ `deploy-gserver.sh` làm wrapper 1 dòng cho tương thích) — DRY, không copy script [RT#14]
- Modify: `docker/migrate-db.sh` — bọc toàn bộ bằng `flock /tmp/gopet-migrate.lock` (chống 2 deploy chạy migrate cùng lúc) [RT#9]
- Modify: `.github/workflows/gserver-ci-cd.yml` → đổi thành workflow chung `deploy.yml` (hoặc giữ tên): **một** concurrency group `gopet-deploy`; job `changes` (dorny/paths-filter) quyết định deploy `gserver` và/hoặc `webadmin`; job test webadmin `npm ci && npm run lint && npx tsc --noEmit && npm test && npm run build`. Đổi filter gserver từ `docker/**` thành `docker/gserver/**`, `docker/deploy-*.sh`, `docker/migrate-db.sh` và riêng service gserver — sửa compose để thêm webadmin **không** được rebuild/restart gserver [RT#14]
- Modify: `docker/docker-compose.yml` — đặt `TZ` thống nhất cho mariadb, gserver, webadmin (cùng một giá trị, vd `Asia/Ho_Chi_Minh`; kiểm tra ảnh hưởng tới dữ liệu thời gian hiện có trước khi đổi TZ của mariadb/gserver) [RT#9]
- Modify: `webadmin/next.config.ts` (`output: 'standalone'`)
- Modify: `webadmin/src/middleware.ts` (loại trừ `/api/health`)
- Modify: `docker/docker-compose.yml` (thêm service webadmin)
- Modify: `docker/.env.example` (WEBADMIN_DB_PASSWORD, WEBADMIN_SESSION_SECRET, WEBADMIN_DOMAIN, WEBADMIN_ALLOWED_IPS, WEBADMIN_SUPERADMIN_USER_IDS=1, GHCR_OWNER, WEBADMIN_TAG)
- Modify: `docs/deployment-linux-backend.md` (mục mới: webadmin — tạo user DB, biến env, truy cập qua tunnel/proxy, cập nhật)

## Implementation Steps
1. `output: 'standalone'` + health route + loại trừ middleware.
2. Viết Dockerfile + .dockerignore; build thử local; job CI build + push GHCR.
3. Thêm service vào compose; `docker compose --profile server config` hợp lệ.
4. Tạo user `gopet_admin` trên MariaDB container (SQL ở phase 1) + chạy migration `admin_audit_log` (đã theo `migrate-db.sh` vì tên `migration-*.sql`).
5. Chạy cả stack local (Caddy dùng `localhost` + `tls internal`) → gserver + webadmin healthy; IP ngoài allowlist nhận 403.
5b. Đổi TZ: kiểm tra mốc thời gian game (điểm danh, sự kiện, bảo trì) trước/sau đổi; quyết định migration +7h.
6. `deploy-service.sh <service>` với rollback (gserver: build tại chỗ như cũ; webadmin: pull tag GHCR, rollback về tag trước); deploy service này không động tới service kia (`compose up -d --no-build <service>`); luôn gọi `migrate-db.sh` (có flock).
7. Mở rộng workflow hiện có (1 concurrency group, paths-filter theo service), dùng lại secret SSH.
8. Cập nhật docs triển khai: DNS domain → IP máy chủ, mở 80/443, cập nhật allowlist, đăng nhập GHCR, đổi TZ.

## Success Criteria
- [x] Docker compose file ready with webadmin + caddy services
- [x] GServer build ok with heartbeat + player_online; 56/56 perf tests pass
- [x] Migrations applied: admin-audit-log, player-online-heartbeat, lock-legacy-password-accounts
- [x] MariaDB container TZ set to +07:00; app env TZ=Asia/Ho_Chi_Minh
- [x] Dockerfile multi-stage (standalone, alpine, non-root) — code implemented
- [x] Webadmin health check endpoint (/api/health) implemented
- [ ] Production deployment verification (pending: docker build on server, image size check, real docker compose up, IP allowlist validation)

## Risk Assessment
- Migration chung: cả hai deploy đều gọi `migrate-db.sh`, đã có flock + chung concurrency group → không chạy song song. Migration phải có header `-- database:` đúng DB.
- Tài nguyên máy nhỏ: giới hạn `mem_limit` webadmin (~256–384MB) để không tranh RAM với gserver/MariaDB (buffer pool 512M).
- Đổi TZ gserver có thể dịch mốc reset/sự kiện 7h → kiểm trước, ghi docs.
- PAT GHCR trên máy chủ chỉ quyền read:packages.

## Security Considerations
- Chỉ Caddy public 80/443; allowlist IP tại Caddy; webadmin không có cổng host.
- Secret chỉ ở `docker/.env` (gitignore), không bake vào image (`.dockerignore` loại `.env*`).
- DB user `gopet_admin` quyền tối thiểu, khác user của gserver.
