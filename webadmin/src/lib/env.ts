import "server-only";
import { z } from "zod";

// Biến môi trường chỉ đọc phía server; parse một lần, lỗi thì dừng sớm với thông báo rõ.
const EnvSchema = z.object({
  DB_HOST: z.string().min(1),
  DB_PORT: z.coerce.number().int().positive().default(3306),
  DB_USER: z.string().min(1),
  DB_PASSWORD: z.string(),
  DB_GAME: z.string().min(1).default("gopettae_tae2"),
  DB_WEB: z.string().min(1).default("gopettae_gopet_web"),
  DB_LOG: z.string().min(1).default("gp_log"),
  SESSION_SECRET: z.string().min(32, "SESSION_SECRET phải ≥ 32 ký tự"),
  SUPERADMIN_USER_IDS: z
    .string()
    .default("1")
    .transform((s) =>
      s
        .split(",")
        .map((x) => Number(x.trim()))
        .filter((n) => Number.isInteger(n) && n > 0),
    ),
  TRUSTED_PROXY: z
    .string()
    .default("false")
    .transform((s) => s === "true"),
  COOKIE_SECURE: z
    .string()
    .optional()
    .transform((s) => (s === undefined ? undefined : s === "true")),
});

export type Env = z.infer<typeof EnvSchema>;

let cached: Env | undefined;

export function env(): Env {
  if (cached) return cached;
  const parsed = EnvSchema.safeParse(process.env);
  if (!parsed.success) {
    const issues = parsed.error.issues.map((i) => `${i.path.join(".")}: ${i.message}`).join("; ");
    throw new Error(`Cấu hình env không hợp lệ: ${issues}`);
  }
  cached = parsed.data;
  return cached;
}
