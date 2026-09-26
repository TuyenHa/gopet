import { AppHeader } from "@/components/layout/app-header";
import { AppSidebar } from "@/components/layout/app-sidebar";
import { requireAdmin } from "@/lib/auth/require-admin";

// Layout chỉ gọi requireAdmin() để hiển thị tên admin; bảo vệ thật nằm trong từng hàm DAL.
export default async function AdminLayout({ children }: LayoutProps<"/">) {
  const admin = await requireAdmin();
  return (
    <div className="flex min-h-screen bg-white">
      <AppSidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <AppHeader playerName={admin.playerName} username={admin.username} isSuperAdmin={admin.isSuperAdmin} />
        <main className="min-w-0 flex-1 p-4 md:p-6">{children}</main>
      </div>
    </div>
  );
}
