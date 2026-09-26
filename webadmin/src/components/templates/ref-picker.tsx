"use client";

import { useEffect, useState, useTransition } from "react";
import { Input } from "@/components/ui/input";
import { searchRefOptions, type RefOption } from "@/lib/templates/ref-search-action";
import type { RefKind } from "@/lib/templates/ref-sources";

/**
 * Combobox tìm đơn giản cho cột tham chiếu (item/pet/npc/map/skill). Gõ để tìm theo
 * tên hoặc ID qua Server Action `searchRefOptions`; giá trị thật gửi lên form là ID
 * (hidden input tên `name`), không phải nhãn hiển thị.
 */
export function RefPicker({
  name,
  kind,
  defaultValue,
  placeholder = "Tìm theo tên hoặc ID...",
}: {
  name: string;
  kind: RefKind;
  defaultValue?: string | number | null;
  placeholder?: string;
}) {
  const initial = defaultValue === null || defaultValue === undefined || defaultValue === "" ? "" : String(defaultValue);
  const [text, setText] = useState(initial);
  const [value, setValue] = useState(initial);
  const [options, setOptions] = useState<RefOption[]>([]);
  const [open, setOpen] = useState(false);
  const [, startTransition] = useTransition();

  useEffect(() => {
    const timer = setTimeout(() => {
      startTransition(() => {
        searchRefOptions(kind, text).then(setOptions).catch(() => setOptions([]));
      });
    }, 250);
    return () => clearTimeout(timer);
  }, [text, kind]);

  return (
    <div className="relative">
      <input type="hidden" name={name} value={value} />
      <Input
        placeholder={placeholder}
        value={text}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
        onChange={(e) => {
          setText(e.target.value);
          setValue(e.target.value);
        }}
      />
      {open && options.length > 0 && (
        <ul className="absolute z-10 mt-1 max-h-56 w-full overflow-auto rounded-lg border bg-white text-sm shadow-md">
          {options.map((o) => (
            <li
              key={o.value}
              className="cursor-pointer px-2.5 py-1.5 hover:bg-neutral-100"
              onMouseDown={() => {
                setValue(o.value);
                setText(o.label);
                setOpen(false);
              }}
            >
              {o.label}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
