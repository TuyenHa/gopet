"use client";

import { GIFT_TYPE, type GiftEntry } from "@/lib/giftcodes/gift-data-schema";
import { AmountField, IdField, TimedFields } from "./field-primitives";
import { RandomItemListEditor } from "./random-item-list-editor";
import { Checkbox } from "@/components/ui/checkbox";

/** Các ô nhập riêng cho từng loại quà, tuỳ theo `entry.type`. */
export function GiftEntryFields({ entry, onChange }: { entry: GiftEntry; onChange: (e: GiftEntry) => void }) {
  switch (entry.type) {
    case GIFT_TYPE.GOLD:
    case GIFT_TYPE.COIN:
    case GIFT_TYPE.EXP:
    case GIFT_TYPE.ENERGY:
    case GIFT_TYPE.EVENT_POINT:
    case GIFT_TYPE.FUND_CLAN:
      return <AmountField label="Số lượng" value={entry.amount} onChange={(amount) => onChange({ ...entry, amount })} />;

    case GIFT_TYPE.ITEM:
      return (
        <div className="space-y-2">
          <IdField label="Vật phẩm" value={entry.itemId} kind="item" onChange={(itemId) => onChange({ ...entry, itemId })} />
          <AmountField label="Số lượng" value={entry.count} onChange={(count) => onChange({ ...entry, count })} />
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={entry.canTrade} onCheckedChange={(c) => onChange({ ...entry, canTrade: c === true })} /> Cho phép giao dịch
          </label>
        </div>
      );

    case GIFT_TYPE.ITEM_PERCENT_NO_DROP_MORE:
      return (
        <div className="space-y-2">
          <IdField label="Vật phẩm" value={entry.itemId} kind="item" onChange={(itemId) => onChange({ ...entry, itemId })} />
          <AmountField label="Tỉ lệ % trúng (0-100)" value={entry.percent} onChange={(percent) => onChange({ ...entry, percent })} />
          <AmountField label="Số lượng khi trúng" value={entry.count} onChange={(count) => onChange({ ...entry, count })} />
        </div>
      );

    case GIFT_TYPE.RANDOM_ITEM:
      return (
        <div className="space-y-2">
          <AmountField label="Số lần rút" value={entry.numGift} onChange={(numGift) => onChange({ ...entry, numGift })} />
          <RandomItemListEditor items={entry.items} onChange={(items) => onChange({ ...entry, items })} />
        </div>
      );

    case GIFT_TYPE.ITEM_MAX_OPTION:
      return (
        <div className="space-y-2">
          <IdField label="Vật phẩm" value={entry.itemId} kind="item" onChange={(itemId) => onChange({ ...entry, itemId })} />
          <AmountField label="Số lượng" value={entry.count} onChange={(count) => onChange({ ...entry, count })} />
        </div>
      );

    case GIFT_TYPE.TITLE:
      return (
        <TimedFields
          idLabel="Danh hiệu (titleId)"
          idValue={entry.titleId}
          onIdChange={(titleId) => onChange({ ...entry, titleId })}
          isInfinite={entry.isInfinite}
          onInfiniteChange={(isInfinite) => onChange({ ...entry, isInfinite })}
          duration={entry.duration}
          onDurationChange={(duration) => onChange({ ...entry, duration })}
          withMonthYear
        />
      );

    case GIFT_TYPE.SKIN:
      return (
        <TimedFields
          idLabel="Vật phẩm skin"
          idValue={entry.itemId}
          idKind="item"
          onIdChange={(itemId) => onChange({ ...entry, itemId })}
          isInfinite={entry.isInfinite}
          onInfiniteChange={(isInfinite) => onChange({ ...entry, isInfinite })}
          duration={entry.duration}
          onDurationChange={(duration) => onChange({ ...entry, duration })}
        />
      );

    case GIFT_TYPE.PET_TRIAL:
      return (
        <TimedFields
          idLabel="Pet"
          idValue={entry.petId}
          idKind="pet"
          onIdChange={(petId) => onChange({ ...entry, petId })}
          isInfinite={entry.isInfinite}
          onInfiniteChange={(isInfinite) => onChange({ ...entry, isInfinite })}
          duration={entry.duration}
          onDurationChange={(duration) => onChange({ ...entry, duration })}
          withMonthYear
        />
      );
  }
}
