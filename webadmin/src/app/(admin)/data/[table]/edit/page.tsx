import { notFound } from "next/navigation";
import { PageHeader } from "@/components/data/page-header";
import { RestartRequiredBanner } from "@/components/data/restart-required-banner";
import { TemplateForm } from "@/components/templates/template-form";
import { firstParam } from "@/lib/pagination";
import { decodePk } from "@/lib/templates/pk-codec";
import { getTableConfig } from "@/lib/templates/table-registry";
import { createTemplateRow, updateTemplateRow } from "@/lib/templates/template-actions";
import { getTemplateRow } from "@/lib/templates/template-queries";

export default async function TemplateEditPage({ params, searchParams }: PageProps<"/data/[table]/edit">) {
  const { table } = await params;
  const config = getTableConfig(table);
  if (!config) notFound();
  if (config.readOnly) notFound();

  const sp = await searchParams;
  const isNew = firstParam(sp.new) === "1";

  if (isNew) {
    if (config.noCreate) notFound();
    return (
      <div>
        <PageHeader title={`Thêm ${config.label}`} />
        <RestartRequiredBanner />
        <div className="mt-4 max-w-2xl">
          <TemplateForm config={config} mode="create" action={createTemplateRow.bind(null, table)} />
        </div>
      </div>
    );
  }

  const pkRaw = firstParam(sp.pk);
  const pkValues = pkRaw ? decodePk(pkRaw, config) : null;
  if (!pkRaw || !pkValues) notFound();

  const row = await getTemplateRow(config, pkValues);
  if (!row) notFound();

  return (
    <div>
      <PageHeader title={`Sửa ${config.label}`} />
      <RestartRequiredBanner />
      <div className="mt-4 max-w-2xl">
        <TemplateForm
          config={config}
          mode="edit"
          row={row}
          action={updateTemplateRow.bind(null, table, pkRaw)}
          requirePassword={config.group === "Hệ thống"}
        />
      </div>
    </div>
  );
}
