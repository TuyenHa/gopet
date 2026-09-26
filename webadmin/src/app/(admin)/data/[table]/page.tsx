import Link from "next/link";
import { notFound } from "next/navigation";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ConfirmDialog } from "@/components/data/confirm-dialog";
import { DataTable, type Column } from "@/components/data/data-table";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { RestartRequiredBanner } from "@/components/data/restart-required-banner";
import { SearchBar } from "@/components/data/search-bar";
import { formatNumber } from "@/lib/format";
import { parsePage, parseQuery, type SearchParams } from "@/lib/pagination";
import { encodePk } from "@/lib/templates/pk-codec";
import { getTableConfig, type ColumnDef } from "@/lib/templates/table-registry";
import { deleteTemplateRow } from "@/lib/templates/template-actions";
import { countTemplateRows, listTemplateRows, type TemplateRow } from "@/lib/templates/template-queries";

function renderCell(c: ColumnDef, v: unknown) {
  if (v === null || v === undefined || v === "") return "—";
  if (c.type === "bool") return Number(v) === 1 ? "Có" : "Không";
  if (c.type === "int" || c.type === "bigint") return formatNumber(v as string | number);
  const s = String(v);
  return s.length > 60 ? `${s.slice(0, 60)}…` : s;
}

export default async function TemplateListPage({ params, searchParams }: PageProps<"/data/[table]">) {
  const { table } = await params;
  const config = getTableConfig(table);
  if (!config) notFound();

  const sp = (await searchParams) as SearchParams;
  const info = parsePage(sp);
  const q = parseQuery(sp);
  const [rows, total] = await Promise.all([listTemplateRows(config, { q, page: info }), countTemplateRows(config, q)]);

  const columns: Column<TemplateRow>[] = config.columns.map((c) => ({
    key: c.name,
    header: c.label,
    render: (row) => renderCell(c, row[c.name]),
  }));

  if (!config.readOnly) {
    columns.push({
      key: "__actions",
      header: "",
      className: "text-right",
      render: (row) => {
        const pk = encodePk(config, row);
        return (
          <div className="flex justify-end gap-2">
            <Button asChild size="sm" variant="outline">
              <Link href={`/data/${table}/edit?pk=${encodeURIComponent(pk)}`}>Sửa</Link>
            </Button>
            {!config.noDelete && (
              <ConfirmDialog
                trigger={
                  <Button size="sm" variant="destructive">
                    Xoá
                  </Button>
                }
                title={`Xoá dòng khỏi "${config.label}"?`}
                description="Không thể hoàn tác. Nếu dữ liệu đang được bảng khác tham chiếu (khoá ngoại) hoặc người chơi đang sở hữu, thao tác sẽ bị từ chối."
                destructive
                confirmLabel="Xoá"
                action={deleteTemplateRow.bind(null, table, pk)}
              />
            )}
          </div>
        );
      },
    });
  }

  return (
    <div>
      <PageHeader
        title={config.label}
        description={config.note}
        actions={
          !config.readOnly && !config.noCreate ? (
            <Button asChild size="sm">
              <Link href={`/data/${table}/edit?new=1`}>
                <Plus className="size-4" /> Thêm mới
              </Link>
            </Button>
          ) : undefined
        }
      />
      <RestartRequiredBanner />
      <div className="mt-4 mb-3">
        <SearchBar defaultValue={q} placeholder={`Tìm trong ${config.label}...`} />
      </div>
      <DataTable columns={columns} rows={rows} rowKey={(row, i) => (config.pk.length > 0 ? encodePk(config, row) : i)} />
      <PaginationBar basePath={`/data/${table}`} searchParams={sp} info={info} total={total} />
    </div>
  );
}
