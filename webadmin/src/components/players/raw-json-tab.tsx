import { JsonViewer } from "@/components/data/json-viewer";

export function RawJsonTab({ data }: { data: Record<string, unknown> }) {
  return <JsonViewer value={data} maxHeight={720} />;
}
