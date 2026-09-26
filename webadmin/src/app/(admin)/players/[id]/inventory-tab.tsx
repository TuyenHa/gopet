import "server-only";
import { requireAdmin } from "@/lib/auth/require-admin";
import { gamePool } from "@/lib/db/pools";
import { queryOne } from "@/lib/db/query";
import { parsePlayerItems, itemPetEquipId, type ItemNode } from "@/lib/game-json/item-json";
import { toNumber } from "@/lib/game-json/lossless-json";
import { formatEpochMs } from "@/lib/format";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { JsonViewer } from "@/components/data/json-viewer";
import { InventoryItemEditForm } from "@/components/players/inventory-item-edit-form";
import { InventoryItemDeleteButton } from "@/components/players/inventory-item-delete-button";

/** invType (`GopetManager.cs:444-453`) — chỉ để hiển thị nhãn, không ảnh hưởng logic. */
const INV_TYPE_LABEL: Record<string, string> = {
  "0": "Trang bị pet",
  "1": "Vật phẩm thường",
  "2": "Skin",
  "3": "Cánh (wing)",
  "4": "Ngọc",
  "5": "Tiền/khác",
};

interface InventoryRow {
  invType: string;
  itemId: number;
  itemTemplateId: number;
  count: number;
  lvl: number;
  expire: number;
  canTrade: boolean;
  durability: number | null;
  equippedByPet: number | null;
}

function toRows(tree: Record<string, ItemNode[]>): InventoryRow[] {
  const rows: InventoryRow[] = [];
  for (const [invType, list] of Object.entries(tree)) {
    for (const item of list) {
      rows.push({
        invType,
        itemId: toNumber(item.itemId),
        itemTemplateId: toNumber(item.itemTemplateId),
        count: toNumber(item.count),
        lvl: toNumber(item.lvl),
        expire: toNumber(item.expire),
        canTrade: item.canTrade === true,
        durability: "durability" in item ? toNumber(item.durability) : null,
        equippedByPet: itemPetEquipId(item),
      });
    }
  }
  return rows;
}

export async function InventoryTab({ playerId }: { playerId: number }) {
  await requireAdmin();
  const row = await queryOne<{
    items: string | null;
    md5: string | null;
    favouriteList: string | null;
    skin: string | null;
    wing: string | null;
  }>(gamePool(), "SELECT items, MD5(items) AS md5, favouriteList, skin, wing FROM player WHERE ID = ?", [playerId]);

  if (!row) return <p className="text-sm text-neutral-500">Không có dữ liệu.</p>;

  let rows: InventoryRow[] = [];
  let parseError: string | null = null;
  if (row.items && row.md5) {
    try {
      rows = toRows(parsePlayerItems(row.items));
    } catch (err) {
      parseError = err instanceof Error ? err.message : String(err);
    }
  }

  const byInvType = new Map<string, InventoryRow[]>();
  for (const r of rows) {
    const list = byInvType.get(r.invType) ?? [];
    list.push(r);
    byInvType.set(r.invType, list);
  }

  return (
    <div className="space-y-4">
      {parseError && (
        <Card className="border-destructive/50">
          <CardContent className="pt-6 text-sm text-destructive">
            Không đọc được cột `items` ({parseError}) — xem Raw JSON để kiểm tra thủ công.
          </CardContent>
        </Card>
      )}
      {[...byInvType.entries()].map(([invType, list]) => (
        <Card key={invType}>
          <CardHeader>
            <CardTitle>
              {INV_TYPE_LABEL[invType] ?? `Loại ${invType}`} ({list.length})
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>ID</TableHead>
                  <TableHead>Template</TableHead>
                  <TableHead>SL</TableHead>
                  <TableHead>Lvl</TableHead>
                  <TableHead>Hết hạn</TableHead>
                  <TableHead>Giao dịch</TableHead>
                  <TableHead>Độ bền</TableHead>
                  <TableHead>Pet đeo</TableHead>
                  <TableHead>Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {list.map((it) => (
                  <TableRow key={it.itemId}>
                    <TableCell>{it.itemId}</TableCell>
                    <TableCell>{it.itemTemplateId}</TableCell>
                    <TableCell>{it.count}</TableCell>
                    <TableCell>{it.lvl}</TableCell>
                    <TableCell>{it.expire === -1 ? "Vô hạn" : formatEpochMs(it.expire)}</TableCell>
                    <TableCell>
                      <Badge variant={it.canTrade ? "secondary" : "outline"}>{it.canTrade ? "Được" : "Khoá"}</Badge>
                    </TableCell>
                    <TableCell>{it.durability === null ? "—" : `${it.durability}/80`}</TableCell>
                    <TableCell>{it.equippedByPet && it.equippedByPet !== -1 ? it.equippedByPet : "—"}</TableCell>
                    <TableCell className="space-x-2">
                      <InventoryItemEditForm
                        playerId={playerId}
                        itemId={it.itemId}
                        expectedMd5={row.md5!}
                        count={it.count}
                        lvl={it.lvl}
                        expire={it.expire}
                        canTrade={it.canTrade}
                        durability={it.durability}
                      />
                      <InventoryItemDeleteButton playerId={playerId} itemId={it.itemId} expectedMd5={row.md5!} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ))}

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Yêu thích (favouriteList)</CardTitle>
          </CardHeader>
          <CardContent>
            <JsonViewer value={row.favouriteList} maxHeight={160} />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Skin đang mặc</CardTitle>
          </CardHeader>
          <CardContent>
            <JsonViewer value={row.skin} maxHeight={160} />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Cánh đang mặc (wing)</CardTitle>
          </CardHeader>
          <CardContent>
            <JsonViewer value={row.wing} maxHeight={160} />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
