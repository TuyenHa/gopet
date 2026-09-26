"use server";

import bcrypt from "bcryptjs";
import { redirect } from "next/navigation";
import { z } from "zod";
import { gamePool, webPool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { writeAuditLog } from "@/lib/audit/write-audit-log";
import { getClientIp } from "./client-ip";
import { isAccountBlocked, isBcryptHash } from "./account-status";
import { loginBlockedFor, recordLoginFailure, recordLoginSuccess } from "./rate-limit";
import { setSessionCookie } from "./session";

export interface LoginState {
  error?: string;
}

const LoginSchema = z.object({
  username: z.string().trim().toLowerCase().regex(/^[a-z0-9]{1,20}$/),
  password: z.string().min(1).max(100),
});

const GENERIC_ERROR = "Sai tài khoản/mật khẩu hoặc tài khoản không có quyền quản trị.";

// Hash giả để user không tồn tại vẫn tốn thời gian bcrypt như user thật (chống dò username).
let dummyHash: string | undefined;
const getDummyHash = () => (dummyHash ??= bcrypt.hashSync("gopet-dummy-password", 12));

export async function loginAction(_prev: LoginState, form: FormData): Promise<LoginState> {
  const parsed = LoginSchema.safeParse({ username: form.get("username"), password: form.get("password") });
  if (!parsed.success) return { error: GENERIC_ERROR };
  const { username, password } = parsed.data;
  const ip = await getClientIp();

  const wait = loginBlockedFor(username, ip);
  if (wait > 0) return { error: `Sai quá nhiều lần. Thử lại sau ${Math.ceil(wait / 60)} phút.` };

  const fail = async (reason: string, userId = 0): Promise<LoginState> => {
    recordLoginFailure(username, ip);
    await writeAuditLog({ userId, username, ip }, { action: "auth.login:failed", target: username, detail: { reason } }).catch(
      (err) => console.error("[login] ghi audit lỗi", err),
    );
    return { error: GENERIC_ERROR };
  };

  let session: { sub: number; username: string; playerName: string };
  try {
    // Chỗ DUY NHẤT đọc cột password; hash không rời khỏi hàm này.
    const user = await queryOne<{ user_id: number; password: string; role: number; isBaned: number; banTime: string }>(
      webPool(),
      "SELECT user_id, password, role, isBaned, banTime FROM user WHERE username = ? LIMIT 1",
      [username],
    );

    if (!user) {
      await bcrypt.compare(password, getDummyHash());
      return await fail("no_user");
    }
    if (!isBcryptHash(user.password)) {
      await bcrypt.compare(password, getDummyHash());
      recordLoginFailure(username, ip);
      return { error: "Tài khoản dùng mật khẩu kiểu cũ — nhờ super-admin buộc đặt lại mật khẩu." };
    }
    if (!(await bcrypt.compare(password, user.password))) return await fail("bad_password", user.user_id);
    if (isAccountBlocked(user)) return await fail("blocked", user.user_id);

    const admin = await queryOne<{ name: string }>(
      gamePool(),
      "SELECT name FROM player WHERE user_id = ? AND isAdmin = 1 LIMIT 1",
      [user.user_id],
    );
    if (!admin) return await fail("not_admin", user.user_id);

    session = { sub: user.user_id, username, playerName: admin.name };
    // Không ghi được audit → không cho đăng nhập (fail-closed).
    await writeAuditLog({ userId: user.user_id, username, ip }, { action: "auth.login", target: username });
  } catch (err) {
    console.error("[login] lỗi", err);
    return { error: "Không kết nối được cơ sở dữ liệu." };
  }

  recordLoginSuccess(username, ip);
  await setSessionCookie(session);
  redirect("/");
}
