import Link from "next/link";
import { PageHeader } from "@/components/data/page-header";
import { RestartRequiredBanner } from "@/components/data/restart-required-banner";
import { requireAdmin } from "@/lib/auth/require-admin";
import { TABLE_GROUPS } from "@/lib/templates/table-registry";

/** Trang chỉ mục — liệt kê toàn bộ bảng template trong registry, gộp theo nhóm. */
export default async function DataIndexPage() {
  await requireAdmin();

  return (
    <div>
      <PageHeader title="Dữ liệu template" description="Cấu hình game — GServer chỉ nạp lúc khởi động." />
      <RestartRequiredBanner />
      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {TABLE_GROUPS.map((g) => (
          <div key={g.group} className="rounded-lg border bg-white p-4">
            <h2 className="mb-2 text-sm font-semibold text-neutral-700">{g.group}</h2>
            <ul className="space-y-1 text-sm">
              {g.tables.map((t) => (
                <li key={t.table}>
                  <Link href={`/data/${t.table}`} className="text-blue-600 hover:underline">
                    {t.label}
                  </Link>
                  {t.readOnly && <span className="ml-1.5 text-xs text-neutral-400">(chỉ xem)</span>}
                  <span className="ml-1.5 text-xs text-neutral-300">· {t.table}</span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </div>
  );
}
