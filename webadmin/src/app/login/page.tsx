import { redirect } from "next/navigation";
import { readSession } from "@/lib/auth/session";
import { LoginForm } from "./login-form";

const REASONS: Record<string, string> = {
  revoked: "Phiên không còn hiệu lực (tài khoản bị khoá hoặc mất quyền quản trị).",
};

export default async function LoginPage({ searchParams }: PageProps<"/login">) {
  const { reason } = await searchParams;
  // Đã có phiên hợp lệ và không bị đá ra → vào thẳng dashboard.
  if (!reason && (await readSession())) redirect("/");

  return (
    <main className="flex min-h-screen items-center justify-center bg-neutral-50 px-4">
      <div className="w-full max-w-sm rounded-xl border bg-white p-8 shadow-sm">
        <h1 className="text-center text-2xl font-semibold">Gopet Admin</h1>
        <p className="mt-1 text-center text-sm text-neutral-500">Đăng nhập bằng tài khoản game có quyền quản trị</p>
        {typeof reason === "string" && REASONS[reason] && (
          <p className="mt-4 rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-800">{REASONS[reason]}</p>
        )}
        <LoginForm />
      </div>
    </main>
  );
}
