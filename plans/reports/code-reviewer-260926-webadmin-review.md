# Code Review — webadmin (Next.js 16) + GServer phase-4 patch + deploy

Date: 2026-09-26 · Reviewer: code-reviewer · Branch: fix/performance (uncommitted)

## Scope
- webadmin/src/** (~10.4k LOC), Dockerfile, .dockerignore
- GServer: Server/Player.cs, Server/MenuController.inputDialog.cs, Data/User/PlayerData.cs, App/Main.cs, Manager/PlayerOnlineRegistry.cs, Runtime/ServerHeartbeat.cs
- docker/docker-compose.yml, deploy-service.sh, migrate-db.sh, caddy/Caddyfile, webadmin-grants.sql, .github/workflows/gserver-ci-cd.yml, migration-260926-*.sql
- Checked: `tsc --noEmit` clean; vitest 104 passed / 19 skipped. Local DB checks: GET_LOCK names are **case-sensitive** (`GET_LOCK('login_lock_Ab')` → `IS_USED_LOCK('login_lock_ab')` = NULL); `gift_code.code` is `utf8_unicode_ci` (lookup is case-insensitive); sql_mode STRICT_TRANS_TABLES.

## Overall
Solid foundation. Every exported DAL/"use server" function calls requireAdmin/requireSuperAdmin before touching the DB (verified per file). All SQL values go through `?` params; the only identifiers built into SQL strings come from registries or enums. LIKE escaping is correct. `withNamedLock` always releases the lock, and fails closed on a missing heartbeat or `player_online` table. The audit write fails closed, and secrets stay out of it. The main risks are in the **GServer online-flag lifecycle**, which can silently turn off the web guard, and a few **authZ gaps vs RT#7 / phase 5**.

**Score: 7/10** (8.5 once C1, H1–H4 are fixed)

---

## Critical

### C1. Any extra session for the same account deletes the live session's `player_online` flag
`GServer/Server/Player.cs:748-754` (onDisconnected) calls `MarkOffline(user.user_id)` whenever `user != null`, even when **this** session never called `MarkOnline` (line 476).
The session only gets past the flag at line 476 if it clears the 2FA, ban and duplicate checks. The 2FA check (448) comes **before** the duplicate check (453).
- Scenario: player P is online (session S1, flag set). Someone logs in to P's account with 2FA enabled (session S2). S2 sits at the OTP prompt, then the app is closed. S2's onDisconnected runs with `user` set and `playerData == null`, so it deletes P's flag. The heartbeat is still fresh, so the web now sees P as offline. An admin edits gold or items. S1's AutoSave or logout then **overwrites the edit** (for example, a duped item comes back or a gold deduction is undone).
- Same effect from: a login rejected by ban (while S1 is online after being banned), the ROLE_NON_ACTIVE branch at 412 (account locked from the web while online, then a new login), a duplicate login where S2's disconnect gets the lock before S1's final save, and the 2FA "Máy chủ bận" path.
- Fix: add `bool ownsOnlineFlag` on Player. Set it only after `MarkOnline` succeeds, and call `MarkOffline` only when it is true. Defense in depth: `DELETE ... WHERE user_id=? AND since=?`, or add a `session_token` column and delete by token.

## High

### H1. `MarkOnline` fails open
`Manager/PlayerOnlineRegistry.cs:18-30` swallows errors. If the REPLACE fails (missing table, grant, deadlock, or a transient error), the player loads with no flag. The web sees a fresh heartbeat and no flag, so it allows edits on an online player. This contradicts the plan's "fail-closed".
Fix: return a bool from `MarkOnline`. In `ProcessingUser`, if it returns false, show "Máy chủ bận" and close the session before `SELECT * FROM player`.

### H2. Kiosk payout is lost after the `disposed` change
`Data/map/KioskPayout.cs:40-45` and `Data/User/PlayerData.cs:198-205`.
- `PaySeller` gets `sellPlayer` from `PlayerManager` before the seller's disconnect. onDisconnected then saves and sets `disposed=true` (Player.cs:761). `PaySeller`'s `addCoin(share)` + `save()` is now a no-op, so the seller's 95% share disappears. The buyer has already been charged.
- There is also a remaining window: a `save()` that passes the `disposed` check just before line 761 can finish after the lock is released, which is the original RT#1 overwrite. `disposed` is also not volatile.
- Fix: guard with `lock(playerData)` in `save()`, in the dispose block, and in `PaySeller`. Inside `PaySeller`: `lock(pd){ if (pd.disposed) → SQL "coin = coin + @share"; else { addCoin; save(); } }`. Set `disposed=true` in the same critical section as the final save.

### H3. Web edits trust the client's `userId` to pick the lock and online check, but write by `playerId`
`webadmin/src/lib/players/offline-guard.ts:61-70` and the callers `player-actions.ts:40,97,134`, `inventory-actions.ts:76,168`, `pet-actions.ts:83`.
`withOfflinePlayer(input.userId)` locks `login_lock_<username of userId>` and checks `player_online` for that userId. The UPDATE then targets `WHERE ID = input.playerId`. A crafted or edited form (hidden inputs) with the userId of an offline account plus the playerId of an online player bypasses the guard completely.
Fix: take only `playerId` from the client. Resolve `SELECT user_id FROM player WHERE ID=?`, then the username, on the server. Optionally add `AND user_id = ?` to every UPDATE.

### H4. Any admin can unlock legacy-password accounts, re-enabling plaintext login
`webadmin/src/lib/accounts/account-actions.ts:73-85` (`unlockAccountAction`).
The migration `migration-260926-lock-legacy-password-accounts.sql` sets role=0 on non-bcrypt accounts, and phase 5 says they can only be reopened through a super-admin forced reset. The unlock action instead sets role 0→1 for any account, and `GopetHashHelper.VerifyHash` still accepts plaintext (SaltParseException fallback). A normal admin can reopen tester/admi11/adm111/adm000 with their old plaintext passwords.
Fix: `UPDATE user SET role=1 WHERE user_id=? AND role=0 AND password LIKE '$2%'`, and tell the admin to use reset for legacy accounts.

### H5. Giftcode lock name is case-sensitive but the code lookup is case-insensitive
`webadmin/src/lib/giftcodes/giftcode-update-actions.ts:20-22` and `GServer/Server/MenuController.inputDialog.cs:85,173`.
Lock names are case-sensitive (verified), while `gift_code.code` uses a `_ci` collation.
- A player who types `abcd` for code `ABCD` locks `gift_code_lock_abcd`. The web's reset/update locks `gift_code_lock_ABCD`, so there is no mutual exclusion (RT#11/12 is not met).
- Two players, or one player on two sessions, using different casings can also redeem concurrently. That can exceed `maxUser` or give a double reward. This is a pre-existing server bug, but the web now depends on this lock.
- The letter text at `giftcode-create-action.ts:85` ("phân biệt hoa/thường") is wrong: lookup ignores case.
- Fix: lowercase on both sides: `"gift_code_lock_" + code.ToLowerInvariant()` and `lockName(code.toLowerCase())`.

### H6. Pet stat edits have no int32 bound, which can make the account unable to log in
`webadmin/src/lib/players/pet-actions.ts:40-51`.
`lvl/str/agi/_int/hp/mp/maxHp/maxMp/tiemnang_point/skillPoint` only have `.min()`. The C# fields are `int` (Pet.cs:21-41), so a value above 2147483647 is written into the JSON. Newtonsoft then throws when the player data loads at next login.
Fix: `.max(INT32_MAX)` on every int field (`exp` is `long` and is fine). Same class of issue in `gift-entry-schema.ts`, see M1.

## Medium

- **M1. gift_data values can break a code for everyone.** `lib/giftcodes/gift-entry-schema.ts:4,27`: `posInt` has no upper bound, and `percent` accepts floats (`12.5`). The server deserializes into `int[][]` (GiftCodeData.cs:15), so one bad value makes the whole code unreadable at redeem time. Fix: `.int().max(INT32_MAX)` everywhere, `percent` as an int from 0 to 100, and cap `items` in RANDOM_ITEM so the JSON stays within `varchar(15000)`.
- **M2. Normal admins can lock other admins (RT#7).** `account-actions.ts:54-70`: `lockAccountAction` only refuses role>1. An admin with role=1 who owns an `isAdmin=1` player can be locked by another regular admin, which cuts off their web and game access. Unban (`:30`) also skips the guard, so a regular admin can undo a super-admin's ban on an admin. Fix: call `assertCanTargetSensitive` for lock and unban.
- **M3. Any admin can edit the System tables.** `lib/templates/template-actions.ts:46-157` and `registry-system.ts:24-41`. The `server` table (IpAddress/Port that clients connect to) and `field` can be edited by any admin. Changing `server.IpAddress` redirects every client, including plaintext login credentials, to another host. Fix: `requireSuperAdmin` + reauth for `group === "Hệ thống"` (and arguably for all template deletes).
- **M4. Reset password does not match the phase-5 spec.** `account-actions.ts:96-121`: phase 5 line 22 says reopening legacy accounts is super-admin only. The code lets any admin reset non-admin accounts and also flips role 0→1. Decide which is intended and make the code and docs agree.
- **M5. No session revocation.** `lib/auth/session.ts` + `logout-action.ts`: the JWT stays valid for 8h after logout or password reset; only the cookie is cleared. `requireAdmin` re-checks the role on every call, but a stolen cookie survives both actions. Fix: add a `pwd_changed_at` or session-epoch claim, compared in `requireAdmin` (one extra column or an in-memory denylist).
- **M6. Deploy leaves a bad tag in `.env` on pull failure.** `docker/deploy-service.sh:97-115`: `.env` is rewritten with the new tag before `compose pull`. If the pull fails, `set -e` exits: the old container keeps running, but `.env` points to a broken tag, so the next `compose up` or reboot fails. `.webadmin-prev-tag` is written but never read. Fix: pull with an env override first, and update `.env` only after the service is healthy (or add a trap to restore it).
- **M7. No clickjacking or security headers.** Neither `next.config.ts` nor the Caddyfile sets `X-Frame-Options`/CSP `frame-ancestors 'none'`, `Referrer-Policy` or HSTS. Admin forms could be framed. Fix: add a `header` block in Caddy.
- **M8. Delete-item leaves a stale equip reference.** `inventory-actions.ts:123-128,144-148`: the `clearEquipReference` UPDATE result is ignored. With 0 rows (the MD5 changed in between, which is unlikely under the lock), the pet keeps the stale itemId. It self-heals in `Pet.applyInfo`, but should at least be logged or thrown.

## Low
- `offline-guard.ts` / `send-system-letter.ts`: lock names use the DB `username` while the server uses the typed name. They are all lowercase today (0 rows with uppercase), which is fine as long as registration stays `^[a-z0-9]+$`.
- `ban-calc.ts:12-18`: `durationHours` has no max. A huge value gives `String(1e+300)` for a bigint column, which is a strict-mode error surfaced as a generic message. Cap it at, say, 87600 h.
- `player-actions.ts:84-91`: `parseInt("12abc")` is accepted as 12, and `gender`/`avatarPath` are unvalidated. Use `/^-?\d+$/` plus an int32 max, and an enum for gender.
- `lossless-json-parser.ts:59`: a `"__proto__"` key would set the prototype and drop the key. Use `Object.create(null)` or `Object.defineProperty`.
- `giftcode-update-actions.ts:54-73`: `current.code` is read outside the lock. A concurrent rename by another admin means locking a stale name (the UPDATE is by id, so the damage is limited). Toggling `isClanCode` on a used code mixes user_id and clanId in `usersOfUseThis`.
- `MenuController.inputDialog.cs:722-755`: the 2FA branch does not re-read `user` after taking the lock (ban/role may be stale). The `finally` RELEASE can throw on a broken connection.
- `onDisconnected` blocks a thread-pool thread for up to 20s on GET_LOCK while holding `dispatchGate`. A mass disconnect while the web holds locks could starve the pool (the web holds locks for ≤13s, so this is acceptable).
- CI deploy `if:` treats a `cancelled` test/build as not-failure, so the deploy runs with an empty `WEBADMIN_TAG`. Add `!= 'cancelled'`.
- `docker/webadmin-grants.sql` is not applied by the deploy. Document it as a manual step whenever the registry grows.
- Caddy `remote_ip` sees the docker-proxy gateway IP for IPv6 clients. Do not "fix" this by allowlisting 172.16/12.

## Edge cases from scouting
- A GET_LOCK spans the whole MariaDB instance. The web locks on the game pool and the server locks on the web-DB connection, so this works only while both DBs share one instance (they do in compose). Document it for RT#5.
- After a crash, stale flags plus a stale heartbeat keep everyone who was online at crash time read-only even with the off-switch on, until GServer restarts (`ResetAllForThisServer`). This is fail-closed and acceptable.
- `saveFailed` keeps the flag, which is correct. But `disposed=true` is still set, so RAM state can't be retried. Accepted.

## Positive observations
- `withNamedLock`: accepts only `got === 1`, always releases, and destroys the connection if the release fails. `queryOrFailClosed` treats a missing table as online or not-alive.
- Currency uses delta-only atomic SQL. Optimistic MD5 guards on JSON columns. Transaction plus rollback on item delete.
- A single "server-only" read of `password`. `SAFE_COLUMNS` for accounts. The audit sanitizer. An INSERT-only audit grant.
- The in-memory rate limiter avoids `login_history` (RT#8). The XFF last-hop read is correct behind Caddy, which overwrites the header from untrusted clients.
- Lossless JSON codec: RawNumber round-trip, sorted lists, `durability` never added.
- Dockerfile/.dockerignore exclude `.env*`. The image runs as non-root. The migration runner has a flock, a filename whitelist and a backup first.

## Recommended actions (priority)
1. C1 + H1 + H2 (GServer flag ownership, fail-closed MarkOnline, disposed/PaySeller lock). These block production use of player editing.
2. H3: derive userId on the server in `withOfflinePlayer`.
3. H4 / M2 / M3 / M4: close the RT#7 gaps.
4. H5: lowercase giftcode lock names on both sides.
5. H6 / M1: int32 bounds for pet and gift_data.
6. M5–M7: session epoch, deploy `.env` rollback, security headers.

## Metrics
- Type check: clean. Lint: not run (CI runs it). Tests: 104 pass / 19 skipped (DB-integration skips).
- AuthZ coverage: 100% of exported DAL/action functions guarded.

## Unresolved questions
- Is reset-password by a normal admin on non-admin accounts intended (code) or not (phase 5 line 22)?
- Should template writes in general require super-admin, or only the System group?
- Will any production deployment put the game DB and web DB on separate MariaDB instances? If so, the login_lock protocol breaks.
