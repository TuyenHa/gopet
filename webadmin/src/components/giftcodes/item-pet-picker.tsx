"use client";

import { useState, useTransition } from "react";
import { Input } from "@/components/ui/input";
import { searchItems, searchPets, type SearchHit } from "@/lib/giftcodes/item-pet-search-actions";

/** Ô tìm nhanh vật phẩm/pet theo tên hoặc id, chọn xong tự điền vào ô ID cạnh nó. */
export function ItemPetPicker({ kind, onPick }: { kind: "item" | "pet"; onPick: (hit: SearchHit) => void }) {
  const [q, setQ] = useState("");
  const [hits, setHits] = useState<SearchHit[]>([]);
  const [pending, start] = useTransition();

  function search(value: string) {
    setQ(value);
    start(async () => {
      const fn = kind === "item" ? searchItems : searchPets;
      setHits(value.trim() ? await fn(value) : []);
    });
  }

  return (
    <div className="space-y-1">
      <Input
        placeholder={kind === "item" ? "Tìm vật phẩm theo tên/id..." : "Tìm pet theo tên/id..."}
        value={q}
        onChange={(e) => search(e.target.value)}
      />
      {pending && <p className="text-xs text-neutral-400">Đang tìm...</p>}
      {hits.length > 0 && (
        <div className="max-h-40 overflow-auto rounded border bg-white">
          {hits.map((h) => (
            <button
              type="button"
              key={h.id}
              className="block w-full px-2 py-1 text-left text-sm hover:bg-neutral-100"
              onClick={() => {
                onPick(h);
                setHits([]);
                setQ(`${h.name} (#${h.id})`);
              }}
            >
              #{h.id} — {h.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
