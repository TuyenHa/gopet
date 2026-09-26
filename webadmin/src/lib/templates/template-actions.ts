"use server";

import { revalidatePath } from "next/cache";
import { runAction, UserFacingError, type ActionResult } from "@/lib/actions/action-result";
import { requireAdmin, type AdminContext } from "@/lib/auth/require-admin";
import { reauth } from "@/lib/auth/reauth";
import { gamePool, webPool } from "@/lib/db/pools";
import { execute, queryOne, type SqlParam } from "@/lib/db/query";
import { audited } from "@/lib/audit/write-audit-log";
import { decodePk, type PkValues } from "./pk-codec";
import { getTableConfig, type TableConfig } from "./table-registry";
import { getTemplateRow } from "./template-queries";
import { formDataToRecord, zodSchemaFromColumns } from "./zod-from-columns";

function poolOf(config: TableConfig) {
  return config.db === "web" ? webPool() : gamePool();
}

/** errno chuẩn MySQL/MariaDB dùng để đổi lỗi kỹ thuật thành thông điệp thân thiện. */
const ER_ROW_IS_REFERENCED = new Set([1451, 1217]);
const ER_DUP_ENTRY = 1062;

function friendlyDbError(err: unknown): never {
  const errno = (err as { errno?: number } | undefined)?.errno;
  if (errno !== undefined && ER_ROW_IS_REFERENCED.has(errno)) {
    throw new UserFacingError("Không thể xoá/sửa: dữ liệu này đang được bảng khác tham chiếu (khoá ngoại). Hãy sửa thay vì xoá.");
  }
  if (errno === ER_DUP_ENTRY) {
    throw new UserFacingError("Dữ liệu bị trùng khoá duy nhất (unique key) — kiểm tra lại giá trị.");
  }
  throw err;
}

function requireEditableConfig(table: string): TableConfig {
  const config = getTableConfig(table);
  if (!config) throw new UserFacingError("Bảng không tồn tại trong registry.");
  if (config.readOnly) throw new UserFacingError("Bảng này chỉ xem, không thể sửa qua khung này.");
  return config;
}

/** Nhóm "Hệ thống" (`field`/`server`, registry-system.ts) đổi được cấu hình toàn server (vd
 * `server.IpAddress` — client kết nối tới đâu) → sửa chỉ super-admin + reauth (M3), không
 * theo luật chung `requireAdmin` của các bảng dữ liệu khác. */
const SYSTEM_GROUP = "Hệ thống";

async function assertSystemTableUpdateAllowed(ctx: AdminContext, config: TableConfig, form: FormData): Promise<void> {
  if (config.group !== SYSTEM_GROUP) return;
  if (!ctx.isSuperAdmin) throw new UserFacingError("Chỉ super-admin được sửa bảng thuộc nhóm Hệ thống.");
  await reauth(ctx, form.get("confirmPassword") as string | null);
}

/** Chuyển giá trị đã zod-parse (number/string/null) thành tham số SQL an toàn. */
function toSqlParam(v: unknown): SqlParam {
  if (v === null || typeof v === "number" || typeof v === "string") return v;
  return String(v);
}

export async function createTemplateRow(table: string, _prev: ActionResult<unknown> | null, form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const config = requireEditableConfig(table);
    if (config.noCreate) throw new UserFacingError("Bảng này không cho thêm dòng mới.");

    const raw = formDataToRecord(form, config, "create");
    const data = zodSchemaFromColumns(config, "create").parse(raw) as Record<string, unknown>;

    const cols = Object.keys(data);
    const sql = `INSERT INTO \`${config.table}\` (${cols.map((c) => `\`${c}\``).join(",")}) VALUES (${cols.map(() => "?").join(",")})`;
    const params = cols.map((c) => toSqlParam(data[c]));

    await audited(ctx, { action: `template.create:${table}`, target: table, detail: { after: data } }, async () => {
      try {
        await execute(poolOf(config), sql, params);
      } catch (err) {
        friendlyDbError(err);
      }
    });

    revalidatePath(`/data/${table}`);
    return { ok: true, message: "Đã lưu — cần restart GServer" };
  });
}

export async function updateTemplateRow(
  table: string,
  pkJson: string,
  _prev: ActionResult<unknown> | null,
  form: FormData,
): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const config = requireEditableConfig(table);
    await assertSystemTableUpdateAllowed(ctx, config, form);
    const pkValues = decodePk(pkJson, config);
    if (!pkValues) throw new UserFacingError("Khoá chính không hợp lệ.");

    const before = await getTemplateRow(config, pkValues);
    if (!before) throw new UserFacingError("Dòng dữ liệu không còn tồn tại (có thể đã bị xoá).");

    const raw = formDataToRecord(form, config, "update");
    const data = zodSchemaFromColumns(config, "update").parse(raw) as Record<string, unknown>;

    const cols = Object.keys(data);
    const sql = `UPDATE \`${config.table}\` SET ${cols.map((c) => `\`${c}\`=?`).join(",")} WHERE ${config.pk.map((p) => `\`${p}\`=?`).join(" AND ")}`;
    const params = [...cols.map((c) => toSqlParam(data[c])), ...config.pk.map((p) => pkValues[p])];

    await audited(
      ctx,
      { action: `template.update:${table}`, target: `${table}:${pkJson}`, detail: { before, after: data } },
      async () => {
        try {
          await execute(poolOf(config), sql, params);
        } catch (err) {
          friendlyDbError(err);
        }
      },
    );

    revalidatePath(`/data/${table}`);
    return { ok: true, message: "Đã lưu — cần restart GServer" };
  });
}

/** Đếm dòng player còn tham chiếu item/pet trước khi xoá (Risk Assessment phase 7) — chặn nếu > 0. */
async function assertNoPlayerReference(config: TableConfig, pkValues: PkValues): Promise<void> {
  if (!config.refCheck) return;
  const id = pkValues[config.pk[0]];
  const pattern = `%"${config.refCheck.jsonKey}":${id},%`;
  const row = await queryOne<{ cnt: number }>(
    gamePool(),
    `SELECT COUNT(*) AS cnt FROM player WHERE \`${config.refCheck.playerColumn}\` LIKE ?`,
    [pattern],
  );
  const cnt = row?.cnt ?? 0;
  if (cnt > 0) {
    throw new UserFacingError(
      `Không thể xoá: đang được ${cnt} người chơi sở hữu trong ${config.refCheck.playerColumn === "items" ? "hành trang" : "pet"}. Hãy sửa thay vì xoá.`,
    );
  }
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars -- chữ ký cố định để ConfirmDialog bind (table, pk) => (form) => ...
export async function deleteTemplateRow(table: string, pkJson: string, _form: FormData): Promise<ActionResult<unknown>> {
  return runAction(async () => {
    const ctx = await requireAdmin();
    const config = requireEditableConfig(table);
    if (config.noDelete) throw new UserFacingError("Bảng này không cho xoá dòng.");
    const pkValues = decodePk(pkJson, config);
    if (!pkValues) throw new UserFacingError("Khoá chính không hợp lệ.");

    const before = await getTemplateRow(config, pkValues);
    if (!before) throw new UserFacingError("Dòng dữ liệu không còn tồn tại (có thể đã bị xoá).");

    await assertNoPlayerReference(config, pkValues);

    const sql = `DELETE FROM \`${config.table}\` WHERE ${config.pk.map((p) => `\`${p}\`=?`).join(" AND ")}`;
    const params = config.pk.map((p) => pkValues[p]);

    await audited(ctx, { action: `template.delete:${table}`, target: `${table}:${pkJson}`, detail: { before } }, async () => {
      try {
        await execute(poolOf(config), sql, params);
      } catch (err) {
        friendlyDbError(err);
      }
    });

    revalidatePath(`/data/${table}`);
    return { ok: true, message: "Đã xoá — cần restart GServer" };
  });
}
