import "server-only";
import { SignJWT, jwtVerify } from "jose";
import { cookies } from "next/headers";
import { env } from "@/lib/env";

export const SESSION_COOKIE = "gopet_admin_session";
const SESSION_TTL_SEC = 8 * 60 * 60;

export interface SessionPayload {
  sub: number; // user_id
  username: string;
  playerName: string;
}

const key = () => new TextEncoder().encode(env().SESSION_SECRET);

export async function signSession(p: SessionPayload): Promise<string> {
  return new SignJWT({ username: p.username, playerName: p.playerName })
    .setProtectedHeader({ alg: "HS256" })
    .setSubject(String(p.sub))
    .setIssuedAt()
    .setExpirationTime(`${SESSION_TTL_SEC}s`)
    .sign(key());
}

export async function verifySession(token: string | undefined): Promise<SessionPayload | null> {
  if (!token) return null;
  try {
    const { payload } = await jwtVerify(token, key(), { algorithms: ["HS256"] });
    const sub = Number(payload.sub);
    if (!Number.isInteger(sub) || sub <= 0) return null;
    return {
      sub,
      username: String(payload.username ?? ""),
      playerName: String(payload.playerName ?? ""),
    };
  } catch {
    return null;
  }
}

function cookieSecure(): boolean {
  return env().COOKIE_SECURE ?? process.env.NODE_ENV === "production";
}

export async function setSessionCookie(p: SessionPayload): Promise<void> {
  const token = await signSession(p);
  (await cookies()).set(SESSION_COOKIE, token, {
    httpOnly: true,
    secure: cookieSecure(),
    sameSite: "lax",
    path: "/",
    maxAge: SESSION_TTL_SEC,
  });
}

export async function clearSessionCookie(): Promise<void> {
  (await cookies()).delete(SESSION_COOKIE);
}

export async function readSession(): Promise<SessionPayload | null> {
  return verifySession((await cookies()).get(SESSION_COOKIE)?.value);
}
