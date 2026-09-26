import { DataTable } from "@/components/data/data-table";
import type { PlayerLetterRow } from "@/lib/players/player-detail-queries";
import { formatDateTime } from "@/lib/format";

const TYPE_LABEL: Record<number, string> = { 1: "Bạn bè", 2: "Admin", 3: "Sự kiện" };

export function LettersReadonlyTab({ letters }: { letters: PlayerLetterRow[] }) {
  return (
    <DataTable<PlayerLetterRow>
      rows={letters}
      rowKey={(r, i) => `${r.time}-${i}`}
      empty="Không có thư"
      columns={[
        { key: "time", header: "Thời gian", render: (r) => formatDateTime(r.time) },
        { key: "Type", header: "Loại", render: (r) => TYPE_LABEL[r.Type] ?? r.Type },
        { key: "Title", header: "Tiêu đề" },
        { key: "ShortContent", header: "Tóm tắt" },
      ]}
    />
  );
}
