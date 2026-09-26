/** Hiển thị JSON (chuỗi hoặc object) dạng đã format; chuỗi không phải JSON thì in nguyên văn. */
export function JsonViewer({ value, maxHeight = 480 }: { value: unknown; maxHeight?: number }) {
  let text: string;
  if (typeof value === "string") {
    try {
      text = JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      text = value;
    }
  } else {
    text = JSON.stringify(value, null, 2) ?? "";
  }
  return (
    <pre
      className="overflow-auto rounded-lg border bg-neutral-50 p-3 font-mono text-xs leading-relaxed whitespace-pre-wrap break-all"
      style={{ maxHeight }}
    >
      {text}
    </pre>
  );
}
