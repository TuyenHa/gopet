# Phase 9 — Logs and monitoring — implementation report (2026-09-26)

## Phase
- `D:\game\plans\260926-1100-web-admin-nextjs\phase-09-logs-and-monitoring.md`
- Status: completed

## Files created (all new, no shared files touched)
- `webadmin/src/lib/logs/history-queries.ts` — `listHistory` (gp_log.history), filter targetId/charname/keyword/date, default 7-day window unless targetId/charname given, LIMIT size+1 hasNext.
- `webadmin/src/lib/logs/login-queries.ts` — `listLoginHistory` (gopettae_gopet_web.login_history), filter username/IP/success/date.
- `webadmin/src/lib/logs/audit-queries.ts` — `listAuditLog` (admin_audit_log), filter admin/action/target/date.
- `webadmin/src/lib/market/kiosk-parser.ts` — zod schema + `parseKioskData()` pure function (no DB), `kioskTypeLabel()`.
- `webadmin/src/lib/market/market-queries.ts` — `getLatestMarketSnapshot`, `getItemNames` (batched IN lookup), `listKioskRecovery`.
- `webadmin/src/lib/clans/clan-queries.ts` — `listClans` (LEFT JOIN player for leader name, member count from JSON.parse(members).length).
- `webadmin/src/components/logs/detail-toggle.tsx` — shared `<details>`+JsonViewer toggle (obj/detail/raw fallback), no client JS.
- Pages: `src/app/(admin)/logs/history/page.tsx`, `.../logs/logins/page.tsx`, `.../logs/audit/page.tsx`, `.../market/page.tsx`, `.../clans/page.tsx`.
- `webadmin/tests/fixtures/market-sample.json` — trimmed real row from `gopettae_tae2.market` (Id 675, kzhd9x pet listing).
- `webadmin/tests/unit/market-kiosk-parser.test.ts` — 7 tests (fixture parse, kioskTypeLabel, ItemSell path, hasRemoved filter, SellerName fallback, malformed JSON throws, non-JSON throws).

Did NOT create/edit vitest.config.mts (already existed, per your notice) or any shared file (sidebar already links all 5 routes, no changes needed there).

## Kiosk JSON shape (verified against source, not just plan's line hint)
Read `Manager/GopetManager.cs` `loadMarket()` (near line ~1392, `SELECT ... FROM market ORDER BY TimeSave DESC` → `JsonConvert.DeserializeObject<Kiosk[]>`), `Data/map/Kiosk.cs` (class Kiosk: `kioskType` + `kioskItems: SellItem[]`), `Data/item/SellItem.cs` (ItemSell/pet/price/sumVal/RemainingPrice/TotalCount/itemId/user_id/SellerName/hasSell/hasRemoved), `KIOSK_HAT..KIOSK_OTHER` constants (GopetManager.cs:517-537). Real DB row (Id 675) confirmed exact shape: `[{kioskItems:[],kioskType:0}, ..., {kioskItems:[{ItemSell:null,pet:{...},...}],kioskType:4}, ...]`. Parser uses zod `.passthrough()` (only validates fields the UI needs), filters `hasRemoved` listings, falls back `RemainingPrice ?? price-sumVal`, `SellerName || "???"`. Item template name resolved via `item.name` (template table, NOT `iteminfo` which is stat-name lookup, unrelated) batched by distinct `itemTemplateId`.

## Query timing (local Docker DB, `SET profiling=1`, wall time excl. client startup)
- history (7-day window, no filter): 7168 rows total, 2681 in window → **6.5ms**, but `EXPLAIN` shows `type: ALL` (full table scan, no filesort index) — table has no index on `timeDB`, only `targetId`/`charname`/`eventId`. Fine today; will degrade linearly as `history` grows (per plan's own warning, this table has no PK and can get very large).
- login_history (7-day window): **15ms**. audit_log (7-day window, 2 rows): **4ms**. clan list + LEFT JOIN player (1 row): **6ms**. kiosk_recovery (0 rows): instant.
- All well under the 1s threshold on local data → **did not create** `migration-260926-history-index.sql` (task explicitly gates file creation on >1s local measurement). Flagging for lead anyway: proposed index if/when it becomes needed —
  ```sql
  -- database: gp_log
  ALTER TABLE history ADD INDEX ix_history_time (timeDB);
  -- or, if targetId lookups dominate: ADD INDEX ix_history_target_time (targetId, timeDB);
  ```
  Recommend re-measuring after the table has grown organically in production before applying.

## Verification
- `npx next typegen` — OK, `PageProps<"/logs/history"|"/logs/logins"|"/logs/audit"|"/market"|"/clans">` all resolved.
- `npx tsc --noEmit` — clean, 0 errors.
- `npm run lint` (eslint) — clean, 0 errors/warnings.
- `npx vitest run` — 3 files / 26 tests pass (7 new + 19 pre-existing, none broken).
- Manual SQL smoke test of every generated query against live `gopet-mariadb` container (docker exec) — all return expected rows/shape (login_history has `gopettest` real rows, audit_log has the 2 wadtest rows, clan `UnityGuild` with 5 members via `members` JSON array, market Id 675 pet listing).

## Design notes / deviations
- Read-only pages: zero mutate buttons/forms anywhere (Success Criteria #2). Confirmed by re-reading all 5 page files — only `<form method="get">` (SearchBar) present.
- `admin_audit_log` action filter is `LIKE %term%`, so filtering by `action=player.ban` also matches `player.ban:done`/`player.ban:failed` (lifecycle suffixes from `audited()` helper) — intentional, lets admin see the full outcome chain in one filter.
- `history`/`login_history`/`audit_log` all use `total=null` + `hasNext` (LIMIT size+1) pattern per `PaginationBar` contract — no `COUNT(*)`.
- `clan`/`kiosk_recovery` tables are tiny (AUTO_INCREMENT ~100, 0 rows) so no default time window needed there (no timestamp column on kiosk_recovery anyway); still use LIMIT size+1 for consistency/future-proofing.
- Native `<select>`/`<input type=date>` used instead of shadcn `Select` (client component) — filters are plain GET-form fields, no client JS needed, matches `SearchBar`'s progressive-enhancement style already used elsewhere.
- `?targetId=` query param supported on `/logs/history` for a future link from the player detail page (that page is owned by a different phase/agent — not edited here).

## Concerns
- None blocking. Only soft note: `history` full-table-scan risk noted above for the lead to revisit as data grows (index migration file intentionally NOT created, per >1s gate not being met).

**Status:** DONE
**Summary:** All 5 read-only logs/monitoring pages implemented with DAL (requireAdmin, parameterized SQL, 7-day default windows, LIMIT size+1/hasNext, no COUNT(*)); Kiosk[] parser verified against GServer source + real DB row, zod-validated with try/catch→raw JSON fallback; unit tests (7) pass against a real trimmed DB fixture; typegen/tsc/lint all clean; all local query timings <20ms so history-index migration was not created (gate is >1s) but proposed SQL is in this report for the lead.
**Concerns/Blockers:** none.
