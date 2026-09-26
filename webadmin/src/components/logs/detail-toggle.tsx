import { JsonViewer } from "@/components/data/json-viewer";

/**
 * `<details>` gấp/mở JSON trong 1 ô bảng — không cần JS client, dùng chung cho `obj`
 * (history), `detail` (audit log), raw JSON fallback (market parse lỗi).
 */
export function DetailToggle({ label = "Xem chi tiết", value }: { label?: string; value: unknown }) {
  if (value === null || value === undefined || value === "") {
    return <span className="text-neutral-400">—</span>;
  }
  return (
    <details>
      <summary className="cursor-pointer text-sm text-blue-600 select-none hover:underline">{label}</summary>
      <div className="mt-2 max-w-xl">
        <JsonViewer value={value} maxHeight={320} />
      </div>
    </details>
  );
}
