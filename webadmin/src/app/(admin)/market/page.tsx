import { DataTable, type Column } from "@/components/data/data-table";
import { JsonViewer } from "@/components/data/json-viewer";
import { PageHeader } from "@/components/data/page-header";
import { PaginationBar } from "@/components/data/pagination-bar";
import { Badge } from "@/components/ui/badge";
import { formatDateTime, formatNumber } from "@/lib/format";
import { kioskTypeLabel, parseKioskData, type KioskListing } from "@/lib/market/kiosk-parser";
import { getItemNames, getLatestMarketSnapshot, listKioskRecovery, type KioskRecoveryRow } from "@/lib/market/market-queries";
import { parsePage } from "@/lib/pagination";

interface DisplayListing extends KioskListing {
  displayName: string;
}

export default async function MarketPage({ searchParams }: PageProps<"/market">) {
  const sp = await searchParams;
  const recoveryInfo = parsePage(sp, 50);

  const [snapshot, recovery] = await Promise.all([getLatestMarketSnapshot(), listKioskRecovery(recoveryInfo)]);

  let listings: DisplayListing[] = [];
  let parseError: string | null = null;
  if (snapshot) {
    try {
      const raw = parseKioskData(snapshot.Data);
      const itemNames = await getItemNames(raw.map((l) => l.itemTemplateId).filter((id): id is number => id !== null));
      listings = raw.map((l) => ({
        ...l,
        displayName: l.petName ?? (l.itemTemplateId !== null ? (itemNames.get(l.itemTemplateId) ?? `Mẫu vật phẩm #${l.itemTemplateId}`) : "???"),
      }));
    } catch (err) {
      parseError = err instanceof Error ? err.message : String(err);
    }
  }

  const listingColumns: Column<DisplayListing>[] = [
    { key: "kioskType", header: "Loại ki ốt", render: (r) => <Badge variant="outline">{kioskTypeLabel(r.kioskType)}</Badge> },
    { key: "sellerName", header: "Người bán" },
    { key: "displayName", header: "Vật phẩm / pet" },
    { key: "count", header: "SL", render: (r) => formatNumber(r.count) },
    { key: "remainingPrice", header: "Giá (ngọc)", render: (r) => formatNumber(r.remainingPrice) },
  ];

  const recoveryColumns: Column<KioskRecoveryRow>[] = [
    { key: "kioskType", header: "Loại ki ốt", render: (r) => kioskTypeLabel(r.kioskType) },
    { key: "user_id", header: "Mã người dùng" },
    {
      key: "item",
      header: "Item (JSON — SellItem)",
      render: (r) => <span className="line-clamp-2 max-w-md font-mono text-xs">{r.item}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Chợ trời"
        description="Snapshot mới nhất của bảng `market` (server ghi đè khi lưu) — CHỈ ĐỌC, không phản ánh giao dịch tức thời trong game."
      />

      {!snapshot && <p className="text-sm text-neutral-500">Chưa có dữ liệu chợ trời.</p>}

      {snapshot && (
        <div className="space-y-2">
          <p className="text-sm text-neutral-500">
            Bản lưu #{snapshot.Id} · {formatDateTime(snapshot.TimeSave)} · {listings.length} vật phẩm đang bán
          </p>
          {parseError ? (
            <div className="space-y-2">
              <p className="text-sm text-red-600">Không parse được `Data` theo khuôn Kiosk[] đã biết: {parseError}</p>
              <JsonViewer value={snapshot.Data} />
            </div>
          ) : (
            <DataTable columns={listingColumns} rows={listings} rowKey={(r) => r.itemId} />
          )}
        </div>
      )}

      <div className="space-y-2">
        <h2 className="text-lg font-semibold">Ki ốt chờ hoàn trả (kiosk_recovery)</h2>
        <p className="text-sm text-neutral-500">
          Vật phẩm/tiền của ki ốt hết hạn, chờ giao lại cho người bán lúc họ online — CHỈ ĐỌC.
        </p>
        <DataTable columns={recoveryColumns} rows={recovery.rows} rowKey={(r, i) => `${r.user_id}-${r.kioskType}-${i}`} />
        <PaginationBar basePath="/market" searchParams={sp} info={recoveryInfo} total={null} hasNext={recovery.hasNext} />
      </div>
    </div>
  );
}
