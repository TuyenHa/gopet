# Popup Chợ trời toàn cục

Popup mở từ icon **Chợ trời** ở HUD góc phải-trên, dùng được ở **mọi map**. Hai tab: **Chợ** (xem/mua
hàng người khác), **Gian hàng của tôi** (quản lý listing của mình). Dùng lại dữ liệu kiosk sẵn có
(`MarketPlace.kiosks`), nên NPC ki ốt ở map 22 vẫn thấy cùng hàng.

## Luồng tổng thể

```
Icon HUD ──> UiRoot.OpenMarketPopup()
                 │
                 ├─ MarketPopupView.Create(...) ──> SelectTab(TAB_MARKET)
                 │                                      │
                 │              COMMAND_GUIDER/47 ──────┘──> server (filter, sort, page)
                 │
       LIST_STATE(48) ◄─────────────────────────────────── server
                 │
                 ├─ Bind(): dựng lại danh sách MarketListingRowView
                 └─ Listing của mình có isMine=true → không hiện nút Mua
                 └─ Listing chỉ định cho mình hiện nhãn "Chỉ định cho bạn" + nút Mua

Bấm Mua ──> TryBuy(id) ──────────────────► COMMAND_GUIDER/49 ──► server
                                                                │
                                                    RESULT(56) ◄─┘
                                                    LIST_STATE(48) ← server gửi lại
```

Tab **Gian hàng của tôi**: bấm Gỡ → cancel listing, bấm Đăng bán → popup 2 cột (rương đồ, chi tiết + ô
giá), bấm Chỉ định → nhập tên người mua.

## Lọc, sắp xếp, phân trang

- **Lọc 7 loại:** Tất cả / Vũ khí / Giáp / Mũ / Ngọc / Pet / Vật phẩm (mapping ở `MarketItemCategory`).
- **Sắp xếp:** Mới nhất, Giá ↑, Giá ↓ (ổn định: giá bằng nhau thì mới nhất trước).
- **Phân trang:** 5 dòng/trang, server `clamp` page vượt phạm vi; client cảnh báo danh sách cũ khi
  số trang đổi (nếu user bấm Mua ở trang cuối, trang mới lần lại trang 0 — ngoài lỗi **11** ở review).
- Listing của mình **luôn hiện** ở tab Chợ nhưng row có `isMine=true` → client khóa nút Mua.
- Listing **chỉ định ẩn** với người khác: nếu `AssignedName != ""` và `sellerName ≠ viewerName` và
  `assignedName ≠ viewerName` (so `OrdinalIgnoreCase`), row bị lọc ra trước khi gửi.

## Luồng đăng bán

Popup 2 cột: rương đồ (nguồn EQUIP_PET / GEM / NORMAL / pets), chi tiết bên phải. Đồ khóa
(`canTrade=false`), đồ pet đang mặc, đồ có ngọc khảm đều **hiện mờ** + tooltip "Vật phẩm đang
khóa, bạn không thể bán được."; pet đang dùng cũng từ chối. Nhập **số lượng + giá**, giá: 1..2.000.000.000
ngọc, count ≥ 1.

Bấm Đăng → 55 `(source, id, count, price)` → server → 56 `RESULT(3, ok, msg)` + 51 `MINE_STATE`
(listing của mình).

## Chỉ định người mua

Nút **Chỉ định** ở tab **Gian hàng của tôi**, chỉ hiện **sau khi đã đăng** (riêng lệnh 57). Nhập
tên người mua:
- Rỗng = bỏ chỉ định (miễn phí, lệnh 57 chỉ với tên rỗng);
- Khác rỗng nhưng khác tên hiện tại = trừ **phí vàng**: pet **15.000**, đồ khác **10.000**.
- Lỗi: tên là chính mình (local check + server check), tên không tồn tại (server tra cơ sở dữ liệu), 
  thiếu vàng (toast tại chỗ).

## Quy tắc kinh doanh

- **Thời hạn:** 24 giờ (`GopetManager.HOUR_UPLOAD_ITEM`), hết hạn → listing expire, đồ trả lại người
  bán qua `kiosk_recovery`: đang online → nhận vào túi ở gói tin kế tiếp; offline → nhận lúc đăng nhập.
- **Thuế:** 5% (`GopetManager.KIOSK_PER_SELL`), seller nhận **95%** ở mọi đường: buy/cancel/expire.
  `KioskPayout.SellerShare(long v)` tính chung, không tính ở các chỗ riêng.
- **Giá:** `long` 1..2.000.000.000 ngọc; listing bán lẻ dở (từ NPC mua lẻ `sumVal > 0`) hiển thị
  `RemainingPrice = price - sumVal`, buyer trả vừa đúng.
- **Khóa:** đồ `canTrade=false` hoặc item trong túi `canTrade=false` → TryList từ chối.
  Pet mặc trang bị hay có ngọc khảm → từ chối. Pet đang dùng → từ chối.

## Protocol (COMMAND_GUIDER=122)

Tất cả sub là `sbyte`, dữ liệu UTF dùng `putUTF/readUTF`.

| Sub | Hướng | Payload |
|---|---|---|
| 47 `LIST` | C→S | filter: sbyte (-1=tất cả, 0-5=kiosk type), sort: sbyte (0=mới nhất, 1=giá ↑, 2=giá ↓), page: short |
| 48 `LIST_STATE` | S→C | filter, sort, page, totalPages: short, rowCount: sbyte, rows: n×Row |
| 49 `BUY` | C→S | kioskType: sbyte, listingId: int |
| 50 `MINE` | C→S | (rỗng) |
| 51 `MINE_STATE` | S→C | rowCount: short, rows: n×Row |
| 52 `CANCEL` | C→S | kioskType: sbyte, listingId: int |
| 53 `SELLABLE` | C→S | (rỗng) |
| 54 `SELLABLE_STATE` | S→C | rowCount: short, rows: n×SellableRow |
| 55 `SELL` | C→S | source: sbyte (0=equip, 1=normal, 4=gem, -1=pet), id: int, count: int, price: int |
| 56 `RESULT` | S→C | action: sbyte (1=buy, 2=cancel, 3=sell, 4=assign), ok: bool, msg: UTF |
| 57 `ASSIGN` | C→S | kioskType: sbyte, listingId: int, buyerName: UTF (rỗng=bỏ chỉ định) |

**Row** (48/51): kioskType, listingId: int, isMine: bool, name, iconPath, price: long, count: int,
sellerName, secondsLeft: int, desc, assignedName.

**SellableRow** (54): source, id: int, name, iconPath, count: int, tradable: bool, blockReason, desc.

## Lưu trữ & đồng bộ

- **Player save trước.** Mỗi mutation (list/buy/cancel/assign) lưu **đồng bộ** player liên quan
  (`playerData.save()`) rồi lưu market **ngay** (`GopetManager.SaveMarketNow()`), để crash không tạo
  trạng thái "đồ vừa trong túi vừa trên chợ".
- **Market:** mỗi lần lưu INSERT snapshot JSON vào `market` rồi prune giữ 20 row mới nhất. Cờ dirty chỉ
  hạ sau khi lưu thành công; AutoSave vẫn flush định kỳ nếu còn dirty.
- **Hết hạn:** `MarketExpiryTicker` (Runtime thread) INSERT `kiosk_recovery` (MEDIUMTEXT) **trước**, thành
  công mới gỡ listing (lỗi → giữ listing, tick sau thử lại). Không đụng túi đồ từ Runtime thread.
- **Nhận lại:** `KioskRecovery.Deliver` chạy trên thread của player — lúc đăng nhập, hoặc nếu đang
  online thì ở gói tin kế tiếp (`KioskRecovery.DeliverIfPending` trong `GameController.onMessage`,
  kèm thông báo "Vật phẩm ki ốt đã hết hạn và được trả về túi của bạn."). Chỉ xoá đúng dòng đã nhận.

## Đồng bộ & race condition

- **Mỗi listing lock riêng** (`SellItem.Sync` object, không phải `sellItemMutex` cũ). Buy/cancel/expire
  cùng lúc trên 1 listing → chỉ 1 thành công, 2 cái kia skip (check `!hasSell && !hasRemoved`).
- **`PaySeller` trong lock:** DB I/O khi online (cộng tiền người bán, save player), offline (UPDATE
  player.coin). Nếu timeout hoặc fail → **lỗi #20**: listing bị unlock nhưng tiền không cộng.

## Các file chính

| File | Việc |
|------|------|
| `Runtime/UI/MarketPopupView.cs` | Trạng thái, nuốt gói, đổi tab, luồng mua/cancel/sell |
| `Runtime/UI/MarketPopupView.MarketTab.cs` | Tab Chợ: lọc/sắp/phân trang |
| `Runtime/UI/MarketPopupView.MineTab.cs` | Tab Gian hàng của tôi |
| `Runtime/UI/MarketListingRowView.cs` | Bind 1 row listing (tên, icon, giá, thời hạn, nút) |
| `Runtime/UI/MarketFilterBar.cs` | Thanh lọc 7 loại + sắp xếp |
| `Runtime/UI/MarketSellPopupView.cs` | Popup 2 cột, đăng bán |
| `Runtime/UI/MarketSellDetailPane.cs` | Bên phải: chi tiết, số lượng, giá, nút Đăng |
| `Runtime/UI/UiRoot.Market.cs` | Nối popup vào UiRoot |
| `Net/Market/GopetCMD.cs` | Hằng số 47..57, tự sinh từ `tools/gen-gopet-cmd` |
| `GServer/Data/Market/MarketService.cs` | List/Buy/Mine/Cancel/Sellable/Sell/Assign handlers |
| `GServer/Data/Market/MarketQuery.cs` | Lọc/sắp/phân trang (thuần, testable) |
| `GServer/Data/Market/MarketItemCategory.cs` | Template type → kiosk type, lý do chặn |
| `GServer/Data/Market/MarketPacketWriter.cs` | Ghi Row/SellableRow vào gói |
| `GServer/Data/Map/Kiosk*.cs` | TryList/TryBuyWhole/TryCancel/TrySetAssignedName (partial) |
| `GServer/Data/Map/KioskPayout.cs` | SellerShare(v) / AssignFee(isPet) |
| `GServer/Data/Map/MarketExpiryTicker.cs` | Expire ticker, 1 lần/chu kỳ, thread-safe |

## Giới hạn đã biết

1. **NPC map 22 list hiện listing chỉ định cho tất cả** (lỗi #16, pre-existing): NPC sendMenu không
   kiểm tra `AssignedName`. Sửa có thể làm hỏng flow NPC cũ.
2. **`PaySeller` giữ lock khi I/O DB** (lỗi #20): offline expire + buyer không online → chờ timeout hoặc
   mất tiền. Fallback là `kiosk_recovery`.
3. **Page clamp không thông báo** (lỗi #11): user ở trang cuối, bấm Mua lần cuối cùng → page reset về 0 →
   list lạ lùng. Client cần so `filter/sort` rồi nhận `page` từ server (đang so lẫn `state` toàn bộ).

## Các lỗi sửa từ code review

Server fixes (phase 1): mua/cancel race → lock Sync; seller không lưu tên; phí chỉ định đảo (pet 10k,
đồ 15k → pet 15k, đồ 10k); expire online mất tiền; market bảng phình (upsert hoặc prune); TryList không
check khóa; assign case-sensitive; player save chậm hơn market save (dupe/loss).

Client fixes: MarketFilterBar pixel, MarketSellDetailPane int overflow `price * 95` → `(long)`,
MarketPopupView LIST_STATE clamp.
