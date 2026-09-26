import { z } from "zod";

/**
 * Khuôn JSON cột `market.Data` — mảng 6 `Kiosk` (nón/vũ khí/giáp/ngọc/pet/khác), mỗi cái chứa
 * `kioskItems: SellItem[]`. Theo `GServer/Data/map/Kiosk.cs` (class Kiosk) +
 * `GServer/Data/item/SellItem.cs` (class SellItem) + `Manager/GopetManager.cs` (hàm loadMarket,
 * đọc dòng `market` mới nhất). Dùng `.passthrough()` vì object C# có nhiều trường không cần
 * hiển thị (Sync, IsRetail...) — chỉ validate các trường web admin dùng.
 */
const ItemSellSchema = z
  .object({
    itemTemplateId: z.number().int(),
    count: z.number().int(),
  })
  .passthrough();

const PetSchema = z
  .object({
    petIdTemplate: z.number().int(),
    name: z.string().nullable().optional(),
  })
  .passthrough();

const SellItemSchema = z
  .object({
    ItemSell: ItemSellSchema.nullable(),
    pet: PetSchema.nullable(),
    price: z.number(),
    sumVal: z.number().default(0),
    RemainingPrice: z.number().optional(),
    TotalCount: z.number().int().default(1),
    itemId: z.number().int(),
    user_id: z.number().int(),
    SellerName: z.string().nullable().optional(),
    hasSell: z.boolean().default(false),
    hasRemoved: z.boolean().default(false),
  })
  .passthrough();

const KioskSchema = z
  .object({
    kioskType: z.number().int(),
    kioskItems: z.array(SellItemSchema),
  })
  .passthrough();

export const KioskArraySchema = z.array(KioskSchema);

export type Kiosk = z.infer<typeof KioskSchema>;
export type SellItem = z.infer<typeof SellItemSchema>;

/** Nhãn tiếng Việt cho `kioskType` (`GopetManager.KIOSK_*`, GopetManager.cs). */
export const KIOSK_TYPE_LABELS: Record<number, string> = {
  0: "Nón",
  1: "Vũ khí",
  2: "Giáp",
  3: "Ngọc",
  4: "Thú cưng",
  5: "Khác",
};

export function kioskTypeLabel(t: number): string {
  return KIOSK_TYPE_LABELS[t] ?? `Loại ${t}`;
}

/** 1 dòng bảng hiển thị — đã dẹp phẳng từ Kiosk[] để render, chưa gắn tên item template. */
export interface KioskListing {
  kioskType: number;
  itemId: number;
  sellerName: string;
  sellerUserId: number;
  itemTemplateId: number | null;
  petName: string | null;
  count: number;
  /** Số ngọc còn phải trả (đã trừ phần bán lẻ dở); fallback price - sumVal nếu thiếu. */
  remainingPrice: number;
}

/**
 * Parse `market.Data` (chuỗi JSON) → danh sách listing đang bán (bỏ `hasRemoved`). Ném lỗi khi
 * JSON không đúng khuôn — người gọi bọc try/catch và hiển thị JSON gốc khi lỗi.
 */
export function parseKioskData(raw: string): KioskListing[] {
  const parsed = KioskArraySchema.parse(JSON.parse(raw));
  const listings: KioskListing[] = [];
  for (const kiosk of parsed) {
    for (const item of kiosk.kioskItems) {
      if (item.hasRemoved) continue;
      listings.push({
        kioskType: kiosk.kioskType,
        itemId: item.itemId,
        sellerName: item.SellerName || "???",
        sellerUserId: item.user_id,
        itemTemplateId: item.ItemSell?.itemTemplateId ?? null,
        petName: item.pet?.name ?? (item.pet ? "(pet không tên)" : null),
        count: item.ItemSell?.count ?? item.TotalCount,
        remainingPrice: item.RemainingPrice ?? Math.max(0, item.price - item.sumVal),
      });
    }
  }
  return listings;
}
