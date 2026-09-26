# Webadmin foundation — conventions for phase agents (2026-09-26)

Phase 1–3 done in `D:\game\webadmin` (Next.js **16.3** App Router, React 19.2, Tailwind v4, shadcn radix-nova, zod 4.6, mysql2, jose, bcryptjs, vitest).

## Next 16 gotchas
- `middleware.ts` → `src/proxy.ts` (done). `params`/`searchParams`/`cookies()`/`headers()` are **async**.
- Page typing: `export default async function P({ searchParams, params }: PageProps<"/accounts/[id]">)` — global helper types; run `npx next typegen` after adding routes so `PageProps<"...">` knows new routes.
- When unsure read `webadmin/node_modules/next/dist/docs/`.
- Do NOT run `next build` / `next dev` in parallel agents (lockfile). Use `npx tsc --noEmit` + `npm run lint`.

## Helpers (use them, do not duplicate)
| Path | What |
|---|---|
| `src/lib/db/pools.ts` | `gamePool()`, `webPool()`, `logPool()` (utf8mb4, dateStrings, bigint → string) |
| `src/lib/db/query.ts` | `query<T>(db, sql, params)`, `queryOne<T>`, `execute` (→ ResultSetHeader), `withConnection(pool, fn)`, type `SqlParam` |
| `src/lib/db/named-lock.ts` | `withNamedLock(pool, name, timeoutSec, fn(conn))` — GET_LOCK===1 else `LockBusyError`; always RELEASE + release/destroy conn |
| `src/lib/db/types/*.ts` | `UserRow` (no password/secretKey), `PlayerRow`, `GiftCodeRow`, `LetterRow` |
| `src/lib/auth/require-admin.ts` | `requireAdmin(): AdminContext {userId, username, playerName, isSuperAdmin, ip}` — call FIRST inside every DAL query/action |
| `src/lib/auth/require-superadmin.ts` | `requireSuperAdmin()` |
| `src/lib/auth/reauth.ts` | `reauth(ctx, password)` — throws UserFacingError if wrong |
| `src/lib/auth/account-status.ts` | `isAccountBlocked`, `isBcryptHash` |
| `src/lib/actions/action-result.ts` | `ActionResult<T>`, `UserFacingError`, `runAction(fn)` (maps errors, rethrows redirect) |
| `src/lib/audit/write-audit-log.ts` | `audited(ctx, {action, target, detail}, mutate)` = fail-closed pending row → mutate → `:done`/`:failed`; `writeAuditLog` |
| `src/lib/pagination.ts` | `SearchParams`, `firstParam`, `parsePage(sp)` → {page,size,offset}, `parseQuery(sp)`, `likeContains(s)` (use `LIKE ? ESCAPE '\\\\'`), `buildHref` |
| `src/lib/format.ts` | `formatNumber` (bigint strings), `formatDateTime`, `formatEpochMs` |
| `src/components/data/data-table.tsx` | Server `DataTable<T>({columns:[{key,header,render?}], rows, rowKey})` |
| `src/components/data/pagination-bar.tsx` | `PaginationBar({basePath, searchParams, info, total|null, hasNext?})` |
| `src/components/data/search-bar.tsx` | GET form `?q=`, children = extra filter inputs |
| `src/components/data/page-header.tsx` | `PageHeader({title, description, actions})` |
| `src/components/data/action-form.tsx` | client `ActionForm({action:(prev, FormData)=>Promise<ActionResult>, submitLabel, confirmMessage?, resetOnSuccess?})` + toast |
| `src/components/data/confirm-dialog.tsx` | client `ConfirmDialog({trigger, title, description, action:(FormData)=>Promise<ActionResult>, destructive?, requirePassword?})` → password field name `confirmPassword` |
| `src/components/data/json-viewer.tsx` | `JsonViewer({value})` |
| `src/components/data/restart-required-banner.tsx` | yellow banner for template pages |
| `src/components/ui/*` | shadcn: button input label card table dialog alert-dialog dropdown-menu select badge tabs sonner separator sheet tooltip skeleton textarea switch checkbox |

Sidebar menu (`src/components/layout/sidebar-nav-config.ts`, owned by lead) already links: `/accounts`, `/players`, `/data/{item,shop,shoparena,drop_item,boss,map,field,server}`, `/data`, `/giftcodes`, `/letters`, `/logs/history`, `/market`, `/clans`, `/logs/logins`, `/logs/audit`. All admin pages go under `src/app/(admin)/...` (layout with sidebar already applied).

## DAL pattern
```ts
// src/lib/<domain>/<x>-queries.ts
import "server-only";
export async function listX(...) { await requireAdmin(); ... }

// src/lib/<domain>/<x>-actions.ts
"use server";
export async function doX(_prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const input = Schema.parse(Object.fromEntries(form));   // zod
    await audited(ctx, { action: "domain.verb", target: "user:12", detail: {...} }, async () => { /* UPDATE */ });
    revalidatePath("/x");
    return { ok: true, message: "Đã ..." };
  });
}
```
- Every SQL value parameterised (`?`). Identifiers never from user input.
- Never SELECT `password`/`secretKey` outside `src/lib/auth/*`.
- Files kebab-case, < 200 lines, Vietnamese UI text, comments where logic is non-obvious.

## Local environment
- MariaDB 10.4 docker `gopet-mariadb` on 127.0.0.1:3306. App creds in `webadmin/.env.local` (user `gopet_admin`, table-level grants from `docker/webadmin-grants.sql`). Root pw: `docker/.env` `MARIADB_ROOT_PASSWORD`.
  Query example: `PASS=$(grep '^MARIADB_ROOT_PASSWORD=' D:/game/docker/.env | cut -d= -f2-); docker exec -i -e MYSQL_PWD="$PASS" gopet-mariadb mysql -uroot --default-character-set=utf8mb4 -e "..."`
- Migrations: `SRCGOPETGOC/MariaDB_SQL/migration-YYMMDD-name.sql`, first line `-- database: <db>`; apply with `bash docker/migrate-db.sh`.
- Temporary local admin test account: `wadtest` (user_id 1467, player isAdmin=1) — lead removes it at the end.
- Smoke-testing a page without a browser: lead runs the server; agents rely on tsc/lint + SQL checks against the DB.

## Tests
- `webadmin/vitest.config.mts` EXISTS (lead): alias `@`→src, `server-only`→stub, include `tests/**/*.test.ts`. Do NOT create another vitest config. Run `npx vitest run tests/unit/<file>`.
