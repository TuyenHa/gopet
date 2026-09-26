# Webadmin code-review fixes — H3–H8/M1–M8/Low + deploy/CI

Date: 2026-09-26 · Agent: fullstack-developer · Branch: fix/performance (uncommitted)
Source review: `plans/reports/code-reviewer-260926-webadmin-review.md`
Scope: webadmin/src/**, webadmin/tests/**, docker/deploy-service.sh, docker/caddy/Caddyfile,
.github/workflows/gserver-ci-cd.yml. GServer (C1/H1/H2/H5-server-side) handled by a different
agent (`fullstack-developer-260926-gserver-review-fixes.md`) — not touched here.

## H3 — offline guard trusted client `userId`

- `src/lib/players/offline-guard.ts`: `withOfflinePlayer` now takes **only `playerId`**.
  Resolves `user_id` via `SELECT user_id FROM player WHERE ID=?` (gamePool), then `username`
  via `webPool`, both server-side. `fn` callback now receives `(conn, userId)` so callers can
  add `AND user_id=?` to their UPDATEs (defense in depth per review's "optionally").
- Callers updated to pass `playerId` and drop `userId` from their zod schemas + UPDATE SQL
  now includes `AND user_id = ?` using the resolved id:
  - `player-actions.ts`: `adjustPlayerCurrencyAction`, `updatePlayerFieldAction`,
    `setPlayerAdminAction` (self-revoke check now resolves target's `user_id` server-side
    from `playerId` before the check, instead of trusting client `userId`).
  - `inventory-actions.ts`: `updateItemFieldsAction`, `deleteItemAction` (+ `clearEquipReference`
    now takes/uses `userId` too, see M8 below).
  - `pet-actions.ts`: `updatePetFieldsAction`.
  - `giftGoldAction` (player-actions.ts) intentionally **unchanged** — it writes to
    `exchange_gold` by `user_id` directly (queue, not player-row-scoped, no lock/online check
    to bypass).
- Removed the now-unnecessary `userId` hidden inputs / props all the way up the component
  tree: `currency-adjust-form.tsx`, `optimistic-field-form.tsx`, `admin-toggle-button.tsx`,
  `inventory-item-delete-button.tsx`, `inventory-item-edit-form.tsx`, `pet-edit-form.tsx`,
  `overview-tab.tsx`, `players/[id]/inventory-tab.tsx`, `players/[id]/pet-tab.tsx` (+ its
  `PetCard`), `players/[id]/page.tsx`. `GiftGoldForm`/`gift-gold-form.tsx` left untouched
  (different action, see above).
- `tests/integration/offline-guard-protocol.test.ts` (DB-gated): resolves `wadtest`'s
  `player.ID` in `beforeAll` and calls `withOfflinePlayer(testPlayerId, ...)` — verified green
  against the local MariaDB (`RUN_DB_TESTS=1`).

## H4 — unlockAccountAction reopened legacy (non-bcrypt) accounts

`src/lib/accounts/account-actions.ts`: `UPDATE user SET role=1 WHERE user_id=? AND role=0 AND
password LIKE '$2%'`. 0 affected rows (either not locked, or locked-legacy) → error message
telling the admin to use "Đặt lại mật khẩu" instead. `resetPasswordAction`'s existing
role 0→1-on-legacy-hash behavior kept as-is (already correct per lead decision).

## H5 (web side) — giftcode lock name case mismatch

`src/lib/giftcodes/giftcode-update-actions.ts`: `lockName` now lowercases
(`gift_code_lock_${code.toLowerCase()}`), matching `gift_code.code`'s `_ci` collation and the
GServer-side fix (handled by the other agent). Fixed the incorrect "phân biệt hoa/thường" copy
in `giftcode-create-action.ts`'s system-letter body → now says "không phân biệt hoa/thường".

## H6 / M1 — int32 bounds on pet stats and gift_data

- `src/lib/players/pet-actions.ts`: added `.max(INT32_MAX)` to `lvl/star(unchanged, 0-5)/str/
  agi/intStat/hp/mp/maxHp/maxMp/tiemnang_point/skillPoint`. `exp` left unbounded (server field
  is `long`, per review).
- `src/lib/giftcodes/gift-entry-schema.ts`: `posInt`/`nonNegInt` now `.max(INT32_MAX)`;
  `itemPercentEntry.percent` is `.int()` (rejects `12.5`); `randomItemEntry.items[].itemId`
  bounded to `[-INT32_MAX, INT32_MAX]`; added `MAX_RANDOM_ITEMS = 300` cap on
  `randomItemEntry.items.length`.
- `src/lib/giftcodes/gift-data-serialize.ts`: `giftDataToJson` now throws (plain `Error` — see
  note below) when the serialized JSON exceeds `gift_code.gift_data varchar(15000)`, as a
  second line of defense beyond the per-entry cap (multiple `RANDOM_ITEM` entries can still add
  up). **Note:** deliberately throws a plain `Error`, not `UserFacingError` — this module is
  also imported by the client component `gift-data-builder.tsx` (JSON preview), and importing
  `@/lib/actions/action-result` (which pulls in `named-lock.ts` → `"server-only"`) broke the
  Next.js build ("cannot be imported from a Client Component"). The two call sites
  (`giftcode-create-action.ts`, `giftcode-update-actions.ts`) now wrap `giftDataToJson(...)` in
  try/catch and rethrow as `UserFacingError` there (server-only files, safe).

## M2 — normal admin could lock/unban a protected admin account

`src/lib/accounts/account-actions.ts`:
- `banAccountAction`: `assertCanTargetSensitive` now called for **all** modes including
  `"unban"` (was skipped before, letting a regular admin undo a super-admin's ban on another
  admin).
- `lockAccountAction`: added `assertCanTargetSensitive(ctx, userId)` — previously only checked
  `role > 1`, missing the "role=1 but has an `isAdmin=1` player" case from `isProtectedAdminAccount`.

## M3 — template registry "Hệ thống" tables (`field`, `server`)

`src/lib/templates/template-actions.ts`: new `assertSystemTableUpdateAllowed(ctx, config, form)`
— no-op for non-system groups; for `config.group === "Hệ thống"` requires `ctx.isSuperAdmin`
and calls `reauth(ctx, form.get("confirmPassword"))`. Wired into `updateTemplateRow` only
(create/delete on these 2 tables are already blocked by `noCreate`/`noDelete` in
`registry-system.ts`, so no separate guard needed there).
`src/components/templates/template-form.tsx`: new `requirePassword` prop renders a
"confirmPassword" password input (same pattern as `ConfirmDialog`/`reset-password-dialog.tsx`).
`src/app/(admin)/data/[table]/edit/page.tsx`: passes `requirePassword={config.group === "Hệ
thống"}` to the edit-mode form.

## M4 — no change (per lead decision)

`resetPasswordAction` unchanged — normal admin may reset non-admin passwords by design.

## M5 — session revocation: accepted risk, not implemented (per lead decision)

No `pwd_changed_at`/session-epoch claim added. `requireAdmin()` already re-checks `role`/`ban`
on every request (revokes access on next request after lock/ban), and the JWT TTL is 8h. A
stolen cookie for an account that gets its password reset (not banned/locked) would remain
valid until natural expiry — documented here as an accepted risk, no code change (KISS, per
lead instruction to skip).

## M6 — deploy-service.sh writes `.env` before pull succeeds

`docker/deploy-service.sh` `deploy_webadmin()`: no longer sed's `.env` up front. Reads current
`WEBADMIN_TAG` from `.env` into `env_tag`, computes `target_tag = new_tag or env_tag`, then
pulls/ups with `WEBADMIN_TAG="$target_tag" compose pull|up ...` (shell env var overrides the
`.env`-sourced compose interpolation without touching the file). `.env` is only rewritten
**after** `wait_healthy` succeeds. Pull failure → early return, `.env` and running container
untouched. Rollback path (`compose up --force-recreate` with `prev_tag`) also uses the env-var
override, never touching `.env` (it was never changed on the failed attempt). `bash -n` clean.

## M7 — Caddy security headers

`docker/caddy/Caddyfile`: added `header { Strict-Transport-Security; X-Frame-Options DENY;
X-Content-Type-Options nosniff; Referrer-Policy same-origin; Content-Security-Policy
"frame-ancestors 'none'"; -Server }` inside the site block (applies to all responses,
including the 403 from `@blocked`).

## M8 — inventory delete: unchecked equip-strip UPDATE result

`src/lib/players/inventory-actions.ts` `clearEquipReference`: both UPDATE branches (the `pets`
array path and the `petSelected`/`PetDefLeague` single-pet path) now check `affectedRows` and
throw `UserFacingError` on 0 rows. Since this runs inside `deleteItemAction`'s transaction, the
throw triggers the existing `catch { rollback(); throw }` — the item delete itself rolls back
too rather than silently leaving a stale `equip` reference.

## Low findings

- **`lossless-json-parser.ts`**: `parseObject` now builds on `Object.create(null)` instead of
  `{}` — a `"__proto__"` key in the source JSON is stored as an ordinary own property instead
  of reassigning the object's prototype (which would silently drop the key). Verified
  `Object.keys`/`node[k]` access elsewhere in `lossless-json.ts` don't rely on inherited
  `Object.prototype` methods, so this is a safe swap.
- **`ban-calc.ts`**: added `MAX_BAN_HOURS = 87_600` (~10y); `computeBanUpdate` throws above it.
  `account-actions.ts`'s `BAN_SCHEMA.durationHours` also gets `.max(MAX_BAN_HOURS, ...)` at the
  zod layer for a friendlier error before it ever reaches `computeBanUpdate`.
- **`player-actions.ts` parseInt leniency / gender**: extracted validation into new
  `src/lib/players/optimistic-field-parse.ts` (pure, no `"use server"` — needed since a
  "use server" file may only export async functions, so the schema/parse logic couldn't stay
  exported from `player-actions.ts` itself for unit testing). Strict `/^-?\d+$/` regex (no
  trailing garbage like `"12abc"`), int32 bound via `INT32_MAX`, and `gender` restricted to
  `{-1, 0, 1}` (`-1` = unset per DB default, `0`/`1` per `GameController.cs:4944`). `avatarPath`
  capped at 255 chars.
- **CI cancelled deploy**: `.github/workflows/gserver-ci-cd.yml` `deploy` job condition changed
  `always()` → `!cancelled()`. `always()` ran the deploy even when the whole workflow run was
  cancelled (a cancelled dependency's `result` is `'cancelled'`, which is `!= 'failure'`, so the
  old condition let it through with a possibly-empty `WEBADMIN_TAG`). `!cancelled()` keeps the
  "skipped deps don't block deploy" behavior but stops on an actual cancellation.
- Not touched (out of scope per lead instructions): `giftcode-update-actions.ts`'s
  `current.code`-read-outside-lock race, `MenuController.inputDialog.cs` re-read-after-lock,
  `docker/webadmin-grants.sql` not being applied by deploy (documentation-only issue).

## Files changed

```
webadmin/src/lib/players/offline-guard.ts
webadmin/src/lib/players/player-actions.ts
webadmin/src/lib/players/inventory-actions.ts
webadmin/src/lib/players/pet-actions.ts
webadmin/src/lib/players/optimistic-field-parse.ts        (new)
webadmin/src/lib/accounts/account-actions.ts
webadmin/src/lib/accounts/ban-calc.ts
webadmin/src/lib/giftcodes/giftcode-update-actions.ts
webadmin/src/lib/giftcodes/giftcode-create-action.ts
webadmin/src/lib/giftcodes/gift-entry-schema.ts
webadmin/src/lib/giftcodes/gift-data-serialize.ts
webadmin/src/lib/templates/template-actions.ts
webadmin/src/lib/game-json/lossless-json-parser.ts
webadmin/src/components/templates/template-form.tsx
webadmin/src/components/players/currency-adjust-form.tsx
webadmin/src/components/players/optimistic-field-form.tsx
webadmin/src/components/players/admin-toggle-button.tsx
webadmin/src/components/players/inventory-item-delete-button.tsx
webadmin/src/components/players/inventory-item-edit-form.tsx
webadmin/src/components/players/pet-edit-form.tsx
webadmin/src/components/players/overview-tab.tsx
webadmin/src/app/(admin)/players/[id]/inventory-tab.tsx
webadmin/src/app/(admin)/players/[id]/pet-tab.tsx
webadmin/src/app/(admin)/players/[id]/page.tsx
webadmin/src/app/(admin)/data/[table]/edit/page.tsx
webadmin/tests/integration/offline-guard-protocol.test.ts
webadmin/tests/unit/account-ban-calc.test.ts
webadmin/tests/unit/optimistic-field-parse.test.ts         (new)
webadmin/tests/unit/lossless-json-parser.test.ts           (new)
webadmin/tests/unit/giftcode-gift-data-schema.test.ts
docker/deploy-service.sh
docker/caddy/Caddyfile
.github/workflows/gserver-ci-cd.yml
```

## Verify (all green, Windows/Git Bash, local MariaDB `gopet-mariadb` running)

- `npx next typegen` — OK
- `npx tsc --noEmit` — clean
- `npm run lint` — clean
- `npx vitest run` — 116 passed / 19 skipped
- `RUN_DB_TESTS=1 npx vitest run` — 140 passed / 0 skipped
- `npm run build` — compiled successfully, all 20 routes generated
- `bash -n docker/deploy-service.sh` / `bash -n docker/migrate-db.sh` — OK

Not committed (per instructions).

## Unresolved / accepted risk

- M5 (session revocation) intentionally skipped — accepted risk, see above.
- `assertSystemTableUpdateAllowed` (M3) and the M2 `assertCanTargetSensitive` additions aren't
  unit-tested directly: both require a live DB session (`requireAdmin`/`reauth`/`isProtectedAdminAccount`
  all query DB) and, for the template one, exporting the helper from a `"use server"` file isn't
  possible for a non-function binding — consistent with how the rest of the authz-gated action
  files in this codebase are verified (tsc/lint/build + the DB-gated integration test for the
  offline-guard, which is the one with the clearest attack-surface regression risk).
