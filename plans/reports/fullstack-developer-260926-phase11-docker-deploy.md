# Phase 11 — Docker deploy with GServer + webadmin (2026-09-26)

## Status: DONE_WITH_CONCERNS

## Files created/modified (file ownership respected, nothing under webadmin/src or package.json touched)

- `webadmin/Dockerfile` (new) — 3-stage (deps/build/runtime), node:22-alpine, non-root `USER node`, `HEALTHCHECK`.
- `webadmin/.dockerignore` (new)
- `docker/docker-compose.yml` — mariadb: `TZ=Asia/Ho_Chi_Minh` + `--default-time-zone=+07:00`; gserver: `TZ=Asia/Ho_Chi_Minh` env (documents existing Dockerfile ENV, no behavior change); new `webadmin` service (profile server, GHCR image, no published port, mem_limit 384m, healthcheck `/api/health`); new `caddy` service (80/443 published, `caddy_data`+`caddy_config` volumes).
- `docker/caddy/Caddyfile` (new) — domain + IP allowlist + reverse_proxy, relies on Caddy's built-in "localhost → internal CA" behavior for local testing (no extra `tls internal` block needed).
- `docker/.env.example` — added `WEBADMIN_DB_USER/PASSWORD`, `WEBADMIN_SESSION_SECRET`, `WEBADMIN_SUPERADMIN_USER_IDS`, `WEBADMIN_DOMAIN`, `WEBADMIN_ALLOWED_IPS`, `GHCR_OWNER`, `WEBADMIN_TAG`.
- `docker/deploy-service.sh` (new) — `<gserver|webadmin> [tag]`. gserver: unchanged build-locally+tag-prev-rollback logic (moved verbatim). webadmin: pulls GHCR tag (optionally set via 2nd arg, updates `docker/.env` WEBADMIN_TAG), captures the **currently-running** container's image tag via `docker inspect --format '{{.Config.Image}}'` (not by diffing `.env`) into `docker/.webadmin-prev-tag` before swapping, rolls back to it on healthcheck failure. Both branches call `migrate-db.sh` first and only touch their own container (`up -d --no-build <service>`).
- `docker/deploy-gserver.sh` — now a 1-line wrapper: `exec bash deploy-service.sh gserver`.
- `docker/migrate-db.sh` — original body moved into `main()`, wrapped with `flock /tmp/gopet-migrate.lock` (non-blocking probe + blocking wait, logs when waiting); falls back to a warning + running unlocked when `flock` isn't installed (verified: not present in this Windows Git Bash — confirmed the fallback path is what actually executes here).
- `docker/.gitignore` — added `.webadmin-prev-tag`.
- `docker/README.md` — short pointer update (webadmin/caddy/deploy-service.sh), heavy detail lives in docs.
- `.github/workflows/gserver-ci-cd.yml` — rewritten as one workflow, `concurrency: gopet-deploy`. Jobs: `changes` (dorny/paths-filter, gserver vs webadmin path lists per spec; `workflow_dispatch` treated as "both changed" via a second conditional step whose outputs are combined with `||`) → `test-gserver` / `test-webadmin` (`npm ci && npm run lint && npx tsc --noEmit && npm test && npm run build`) → `build-push-webadmin` (docker/login-action with `GITHUB_TOKEN`, `packages: write`, pushes `:<12-char-sha>` + `:latest`, lowercases `github.repository_owner` for GHCR) → `deploy` (single job, `if: always() && ...!= 'failure'` pattern so a skipped sibling job never blocks; SSH heredoc runs `deploy-service.sh gserver` and/or `deploy-service.sh webadmin "$TAG"` based on the `changes` outputs — compose changes that only touch webadmin never invoke `deploy-service.sh gserver`, so gserver's container is never recreated).
- `docs/deployment-linux-backend.md` — new `## 11. Trang quản trị webadmin` section (11.1 DNS/firewall/allowlist, 11.2 create `gopet_admin` + grants, 11.3 env var table, 11.4 GHCR login with `read:packages` PAT, 11.5 deploy/rollback, 11.6 TZ decision write-up); updated overview table + firewall section (80/443) + rewrote section 4.6 to describe the shared workflow/jobs.

## TZ investigation (required before touching MariaDB/gserver TZ)

Grepped `SRCGOPETGOC/GServer` for `DateTime.Now`/`UtcNow` and the maintenance/event scheduling code:

- **`SRCGOPETGOC/GServer/Dockerfile` already has `ENV TZ=Asia/Ho_Chi_Minh`** (pre-existing, not added by me). `AutoMaintenance.cs` (`hourMaintenance` from `server.json`, currently `5`) and all daily-reset/event code use `DateTime.Now`, which is governed purely by the .NET process's OS timezone — already VN time in the container. **Adding `TZ:` to compose's gserver `environment:` is a no-op / documentation-only change**, not a behavior change.
- **MariaDB had no timezone configuration before this phase** (defaults to `SYSTEM`, effectively UTC). This is the one real change.
- Grepped `SRCGOPETGOC/MariaDB_SQL/*.sql` for column type `timestamp`: only **2 real SQL `TIMESTAMP` columns** exist (`payment.time_create`, `user.update_date` in `web_db.sql`) — everything else time-related is `datetime` (literal storage, immune to timezone changes; e.g. `gift_code.expire`, `letter.time`, `player.loginDate`). Real `TIMESTAMP` columns store UTC internally and re-display according to session timezone — existing rows in those 2 columns will **display** 7h later after the change (absolute instant unchanged). `datetime` columns populated via `DEFAULT current_timestamp()` (many tables) will show a one-time 7h gap between rows written before vs. after the change, since old defaults were computed under the previous (UTC) session timezone and new ones under `+07:00`; already-written literal values are never rewritten.
- **Decision documented in `docs/deployment-linux-backend.md` §11.6**: accept the one-time discontinuity (test data per `docker/README.md`), do **not** write a `+7h` migration. If real user data is ever loaded, migrate `payment.time_create`, `user.update_date`, and any `DEFAULT current_timestamp()`-populated `datetime` rows created before the cutover.
- Local dev compose also gets `TZ`/`--default-time-zone` on `mariadb` (applies to any profile, not just `server`) — **did not restart** the running `gopet-mariadb` container (other agents use it); validated purely via `docker compose --profile server config` (succeeded, rendered webadmin/caddy correctly) using a temp `--env-file` copy so the real `docker/.env` was never touched.

## Local validation performed

- `docker compose --profile server config` — OK (webadmin/caddy env/healthcheck/volumes render as expected), via temp env file, real `docker/.env` untouched, mariadb container not restarted.
- `bash -n` on `deploy-service.sh`, `deploy-gserver.sh`, `migrate-db.sh` — all pass.
- YAML-parsed `.github/workflows/gserver-ci-cd.yml` with `js-yaml` (from `webadmin/node_modules`) — parses, 5 jobs present as expected (no actionlint available in this environment).
- `docker build -t gopet-webadmin:test webadmin` — **got partway then failed**: deps (`npm ci`) and Next's compile step succeeded; failed at Next's TypeScript check on `src/lib/giftcodes/gift-data-schema.ts:150` (`Property 'isInfinite' does not exist...`). Confirmed via `git status` that `webadmin/src/lib/giftcodes/` is **untracked, another agent's in-progress work**, unrelated to anything in my file ownership (Dockerfile/.dockerignore). No image was tagged (build failed before the final stage), so there is nothing to clean up. Per instructions I did not edit that file or attempt to route around it (e.g., temporarily stashing it) — reporting instead. **This blocks a full end-to-end image-size/non-root/<250MB confirmation** until that TS error is fixed; the Dockerfile mechanics themselves (deps install, build invocation, multi-stage layout) are otherwise verified working up to that point.
- Did **not** run `bash docker/migrate-db.sh` for real (would touch the shared `gopet-mariadb` container / was blocked by the sandbox's "Modify Shared Resources" classifier) — confirmed instead that `flock` is absent in this Git Bash (so the fallback branch is exactly what would execute here) and syntax-checked the script.

## Concerns

1. **Webadmin Dockerfile end-to-end build is unverified** (blocked by unrelated giftcodes TS error above) — recommend re-running `docker build -t gopet-webadmin:test webadmin && docker images gopet-webadmin:test && docker run --rm --user node gopet-webadmin:test id` once that file compiles, to confirm the `<250MB` target and non-root user, then `docker rmi gopet-webadmin:test`.
2. CI `test-webadmin`/`build-push-webadmin` jobs will hit the same giftcodes TS error currently — expected to resolve once that other in-progress phase finishes; not something to fix here.
3. `docker/webadmin-grants.sql` and the `admin_audit_log` migration already existed (lead's prior work, per the task brief) — I did not modify them, only referenced them from the new docs section 11.2.

## Next steps

- Once giftcodes code compiles: re-run local `docker build` to confirm size/non-root, and let CI's `test-webadmin`/`build-push-webadmin` run for real.
- Operator setting up a real server needs to follow `docs/deployment-linux-backend.md` §11 end-to-end once (DNS, firewall, GHCR PAT, `gopet_admin` user, `docker/.env` secrets) before the CI `deploy` job's `webadmin` branch can succeed.
