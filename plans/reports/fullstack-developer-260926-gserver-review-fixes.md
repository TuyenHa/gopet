# GServer code-review fixes — C1, H1, H2, H5

Date: 2026-09-26 · Agent: fullstack-developer · Scope: `SRCGOPETGOC/GServer/**` only (phase-4 patch)

Source: `plans/reports/code-reviewer-260926-webadmin-review.md` (C1, H1, H2, H5) + `plans/260926-1100-web-admin-nextjs/phase-04-gserver-isonline-patch.md`.

## Files changed
- `SRCGOPETGOC/GServer/Manager/PlayerOnlineRegistry.cs` — `MarkOnline` now returns `DateTime?` (null = failed), `MarkOffline` takes the `since` it wrote.
- `SRCGOPETGOC/GServer/Server/Player.cs` — new `ownsOnlineFlag` + `onlineFlagSince` fields; `ProcessingUser` fail-closed on `MarkOnline` failure; `onDisconnected` gates `MarkOffline` on `ownsOnlineFlag`, and locks `playerData` around save+disposed.
- `SRCGOPETGOC/GServer/Data/User/PlayerData.cs` — `save()` wrapped in `lock (this)`.
- `SRCGOPETGOC/GServer/Data/map/KioskPayout.cs` — `PaySeller` locks the seller's `PlayerData`, checks `disposed` inside the lock, falls back to the offline SQL delta.
- `SRCGOPETGOC/GServer/Server/MenuController.inputDialog.cs` — gift-code `GET_LOCK`/`RELEASE_LOCK` name lowercased (`code.ToLowerInvariant()`), same name reused for both calls (hoisted above the `try` so `finally` sees it).

Not touched (excluded per instructions / not owned): `NpcTemplate.cs`, `GopetPlace.cs`, `assets/npcs/Tho_Ren.png`, `App/Main.cs`, `Runtime/ServerHeartbeat.cs` — pre-existing changes from other work, untouched by this session.

## C1 — cross-session MarkOffline
Root cause confirmed: `onDisconnected` called `MarkOffline` whenever `user != null`, even for sessions that returned before ever calling `MarkOnline` (2FA-OTP-abandoned, banned, `ROLE_NON_ACTIVE`, duplicate-login-rejected, lock-busy).

Fix:
- `Player.ownsOnlineFlag` (bool, default false) — set to `true` only in `ProcessingUser`, immediately after `PlayerOnlineRegistry.MarkOnline` returns non-null.
- `Player.onlineFlagSince` — the `since` value that this session's `MarkOnline` wrote.
- `onDisconnected`: `if (!saveFailed && ownsOnlineFlag) MarkOffline(gameconn, user.user_id, onlineFlagSince);`
- `MarkOffline` now does `DELETE ... WHERE user_id=@userId AND since=@since` — second line of defense: even if `ownsOnlineFlag` were ever wrong, the delete cannot touch a row a newer `REPLACE` already overwrote. `since` is truncated to whole seconds in C# before use (`player_online.since` is `DATETIME` with no fractional part) so the round-trip equality check is exact.
- Verified the duplicate-login kick path (`Player.cs` `ProcessingUser`, `player2.session.Close(); this.session.Close();`): the *new* session (`this`) returns before reaching `MarkOnline`, so it never sets `ownsOnlineFlag` and its own `onDisconnected` cannot touch the old session's row. The *old* session (`player2`) already owns its flag from its own earlier `MarkOnline` and clears it correctly on its own disconnect. No new migration needed — same `since` column, just a stricter WHERE.

## H1 — MarkOnline fail-closed
`MarkOnline` signature changed `void` → `DateTime? ` (null on any exception, logged via `e.printStackTrace()` same as before). `ProcessingUser`: if null, `redDialog("Máy chủ bận, thử lại"); Thread.Sleep(500); session.Close(); return;` before ever reaching `SELECT * FROM player`. Runs inside the existing `using (gameconn)` block so cleanup is automatic.

## H2 — kiosk payout vs. disposed
- `PlayerData.save()`: `lock (this) { if (disposed) return; saveStatic(this); }`.
- `Player.onDisconnected`: the final `save()` call and `disposed = true` now happen inside one `lock (playerData) { ... }` block (previously two statements with `PlayerManager.remove` + `MarkOffline` logic in between — the exact gap RT#1/H2 flagged).
- `KioskPayout.PaySeller`: takes `sellPlayer?.playerData`, then `lock (sellerData) { if (!sellerData.disposed) { addCoin + save(); return; } }`, falls through to the existing offline `UPDATE player SET coin = coin + @share` path if disposed (either because the player was already gone, or disconnected while `PaySeller` waited for the lock).
- All three sites lock the same `PlayerData` instance, so `PaySeller` and `onDisconnected` are now mutually exclusive — no interleaving where a stale save clobbers the reloaded state, and no window where `PaySeller` deposits into a `PlayerData` that will never be saved again. `save()`'s internal `lock(this)` is reentrant with `PaySeller`'s outer `lock(sellerData)` (same thread, same object) — safe, no deadlock.
- `disposed` reads/writes all happen under the same lock now, so the review's "not volatile" note is moot (Monitor entry/exit is already a full memory barrier).

## H5 — gift-code lock case
`MenuController.inputDialog.cs`: `giftCodeLockName = "gift_code_lock_" + code.ToLowerInvariant()` used for both `GET_LOCK` and `RELEASE_LOCK` (was two separate string literals built from the raw, case-preserved `code`). Matches `gift_code.code`'s `utf8_unicode_ci` lookup collation. Web side is a separate agent's change (`giftcode-update-actions.ts`), not touched here.

## Build / test verification
- `dotnet build -c Release` (SRCGOPETGOC/GServer/Gopet.csproj): **0 errors**, 475 pre-existing warnings (nullable/analyzer, unrelated to this patch).
- `dotnet run -c Release --project tests/GServer.Performance.Tests`: **56/56 PASS**, exit code 0 (includes `market kiosk payout calculates seller share`, `market kiosk sell item locking prevents race conditions`, `market FlushMarketSaveIfDirty keeps dirty flag on failed save`). The `ConfigurationErrorsException` trace printed near the end is expected test output for the deliberately-induced DB failure in that last test, not a real failure.
- Did not touch or kill any running `Gopet.exe`.
- No commits made.

## Deviations from the review's literal wording
- Review's C1 fix suggested `DELETE ... WHERE user_id=? AND since=?` *or* a session-token column, "only if needed." Implemented the `since`-based delete as defense-in-depth alongside `ownsOnlineFlag` (the actual fix) — no session-token column, no new migration (existing `since` column is reused, just made part of the WHERE clause).
- Did not touch `Runtime/AutoSave.cs`, which calls `PlayerData.saveStatic(player.playerData, conn)` directly (bypassing the instance `save()`/`disposed`/lock path entirely). This is a separate, pre-existing gap not mentioned in C1/H1/H2/H5 and not in the owned file list — flagging it here for a follow-up, not fixed in this pass.

## Concerns
- None blocking. The `AutoSave.cs` bypass above is worth a follow-up ticket since it could theoretically race with the new `lock(playerData)` in the same way `PaySeller` used to, but it's outside this task's scope (H2 only named `KioskPayout.PaySeller`).

**Status:** DONE
**Summary:** C1/H1/H2/H5 fixed in GServer per review; build clean (0 errors), performance tests 56/56 pass; no other files touched, no commits made.
**Concerns/Blockers:** `Runtime/AutoSave.cs` calls `PlayerData.saveStatic` directly, bypassing the new `save()`/disposed lock — pre-existing, out of scope, flagged for follow-up.
