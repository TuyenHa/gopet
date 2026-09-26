---
phase: 2
title: Server market protocol
status: completed
priority: P1
effort: 5h
dependencies:
  - 1
---

# Phase 2: Server market protocol

## Overview
Thêm bộ packet có cấu trúc riêng cho popup Chợ trời, gồm list/lọc/sắp xếp/phân trang, mua, gian hàng của tôi, gỡ, danh sách đồ bán được, đăng bán. Các packet này là sub-command của `COMMAND_GUIDER=122` và dùng được ở mọi map.

## Key Insights (scout)
- Mẫu cần theo là battle background (commit 65551c1):
  - Hằng số nằm ở `Server\GopetCMD.cs:133-136`.
  - Các `case` đặt trong `GameController.guider()` (`GameController.cs:834-849`), có guard `when player.playerData != null`.
  - Service gửi state: `Data\BattleBackground\BattleBackgroundService.cs:15-31`.
- Sub 40-46 đã dùng. Chọn **47..56** cho market. Phải xác minh không trùng `case` nào trong `guider()`. Hằng số 47-49 khác (`SELECT_METERIAL_*`) thuộc envelope `PET_SERVICE`, không xung đột.
- Map loại đồ → kiosk (`GopetManager.cs:142-184`, `MenuController.cs:703-743`):

  | Kiosk | Template type | Túi |
  |---|---|---|
  | HAT 0 | `PET_EQUIP_HAT` (3) | EQUIP_PET (0) |
  | WEAPON 1 | `PET_EQUIP_WEAPON` (1) | EQUIP_PET (0) |
  | AMOUR 2 | `ARMOUR` (2), `GLOVE` (105), `SHOE` (104) | EQUIP_PET (0) |
  | GEM 3 | `ITEM_GEM` (17) | GEM (4) |
  | PET 4 | pet | `playerData.pets` |
  | OTHER 5 | còn lại trong NORMAL (1) | NORMAL (1) |

  Túi Skin (2), Wing (3), Money (5) không bán được.
- Giá hiển thị: `SellItem.MathPrice`. Popup chỉ bán trọn gói, nên giá = `price - sumVal` khi `sumVal > 0` (listing bán dở từ luồng NPC mua lẻ).

## Protocol (COMMAND_GUIDER=122, tất cả sub là sbyte)
| Sub | Hướng | Payload |
|---|---|---|
| 47 `TYPE_MARKET_LIST` | C→S | sbyte filter (-1 = tất cả, 0..5 = kiosk type), sbyte sort (0 mới nhất, 1 giá ↑, 2 giá ↓), short page (0-based) |
| 48 `TYPE_MARKET_LIST_STATE` | S→C | sbyte filter, sbyte sort, short page, short totalPages, sbyte n, n × Row |
| 49 `TYPE_MARKET_BUY` | C→S | sbyte kioskType, int listingId |
| 50 `TYPE_MARKET_MINE` | C→S | (rỗng) |
| 51 `TYPE_MARKET_MINE_STATE` | S→C | short n, n × Row (sellerName là của chính mình) |
| 52 `TYPE_MARKET_CANCEL` | C→S | sbyte kioskType, int listingId |
| 53 `TYPE_MARKET_SELLABLE` | C→S | (rỗng) |
| 54 `TYPE_MARKET_SELLABLE_STATE` | S→C | short n, n × SellableRow |
| 55 `TYPE_MARKET_SELL` | C→S | sbyte source, int id, int count, int price |
| 56 `TYPE_MARKET_RESULT` | S→C | sbyte action (1 buy, 2 cancel, 3 sell, 4 assign), bool ok, UTF message |
| 57 `TYPE_MARKET_ASSIGN` | C→S | sbyte kioskType, int listingId, UTF buyerName (rỗng = bỏ chỉ định) |

- **Row:** sbyte kioskType, int listingId, bool isMine, UTF name, UTF iconPath, long price, int count, UTF sellerName, int secondsLeft, UTF desc, UTF assignedName (rỗng nếu không chỉ định).
- **SellableRow:** sbyte source (0 equip, 1 normal, 4 gem, -1 pet), int id, UTF name, UTF iconPath, int count, bool tradable, UTF blockReason, UTF desc.
- Page size = **5** (hằng `MarketQuery.PageSize`). Page vượt phạm vi thì clamp.
- Sau khi buy/cancel/sell, server gửi `RESULT` rồi gửi lại state liên quan: buy → LIST_STATE (giữ filter/sort/page lần trước, lưu trong `objectPerformed` hoặc field của player); cancel/sell/assign → MINE_STATE.

## Requirements
- Functional:
  - Tab Chợ **hiện cả listing của chính mình** nhưng row có cờ `isMine=true` → client không hiện nút Mua (user chốt 2026-09-25).
  - Listing có `AssignedName` **ẩn** với mọi người trừ người bán và người được chỉ định (so tên không phân biệt hoa thường). Người được chỉ định thấy nhãn "Chỉ định cho bạn" + nút Mua.
  - Không yêu cầu đứng ở map 22.
  - Đồ khóa vẫn xuất hiện trong SELLABLE nhưng `tradable=false` và `blockReason="Vật phẩm đang khóa, bạn không thể bán được."`. Đồ pet đang mặc hoặc có ngọc khảm cũng hiện với lý do tương ứng.
- Non-functional: request có giá trị bất thường (filter/sort/page/count ngoài phạm vi) thì clamp hoặc từ chối, không throw. Mỗi file < 200 dòng.

## Architecture
```
Client ──47/49/50/52/53/55──► GameController.guider() ──► MarketService.*
                                                              │
                     MarketQuery (thuần: lọc/sắp/trang)◄──────┤
                     MarketItemCategory (template→kiosk)◄─────┤
                     Kiosk.TryBuyWhole/TryCancel/TryList ◄────┘ (phase 1)
Client ◄──48/51/54/56── MarketPacketWriter
```

## Related Code Files
- Modify: `SRCGOPETGOC\GServer\Server\GopetCMD.cs` (hằng số 47..56, kèm comment hướng)
- Modify: `SRCGOPETGOC\GServer\Server\GameController.cs` (6 `case` trong `guider()`)
- Create: `SRCGOPETGOC\GServer\Data\Market\MarketService.cs` (List/Buy/Mine/Cancel/Sellable/Sell + gửi Result)
- Create: `SRCGOPETGOC\GServer\Data\Market\MarketQuery.cs` (thuần, test được: filter/sort/paginate trên `IEnumerable<SellItem>`)
- Create: `SRCGOPETGOC\GServer\Data\Market\MarketItemCategory.cs` (template type → kiosk type, nguồn túi, lý do chặn)
- Create: `SRCGOPETGOC\GServer\Data\Market\MarketPacketWriter.cs` (ghi Row/SellableRow)
- Regenerate: `GopetUnityClient\Assets\Scripts\Net\GopetCmd.cs` qua `node GopetUnityClient/tools/gen-gopet-cmd/index.js`

## Implementation Steps
1. Thêm hằng số 47..57 vào `GopetCMD.cs`. Grep `guider()` để chắc không trùng sub.
2. `MarketItemCategory`:
   - `static sbyte? KioskTypeOf(ItemTemplate t)`.
   - `static string BlockReason(Player, Item)`: kiểm tra khóa, pet đang mặc, có ngọc, hết hạn.
   - `static string BlockReason(Pet)`: pet thử, pet đang dùng.
3. `MarketQuery.Page(IEnumerable<SellItem> all, int viewerUserId, string viewerName, sbyte filter, sbyte sort, int page)` trả về `(rows, totalPages, clampedPage)`. Sort ổn định: giá bằng nhau thì theo `expireTime` giảm dần (mới nhất trước).
4. `MarketPacketWriter.WriteRow(Message, SellItem, Player viewer)`:
   - Tên dùng `ItemSell.getEquipName`/`getName`, hoặc `pet.getNameWithStar`.
   - `iconPath` dùng cùng nguồn ảnh mà `sendMenu` MENU_KIOSK_* đang dùng.
   - `secondsLeft = max(0, (expireTime - now) / 1000)`.
   - `desc` ngắn, ≤ 120 ký tự.
5. Viết 6 handler của `MarketService`:
   - Buy/Cancel/Sell gọi các API phase 1 rồi gửi `RESULT(action, ok, msg)` + state mới.
   - SELLABLE duyệt EQUIP_PET, GEM, NORMAL và `pets`. Bỏ túi skin/wing/money và các item mà `KioskTypeOf` trả về null.
6. Nối các `case` trong `guider()`, có guard `when player.playerData != null`.
7. Regenerate `GopetCmd.cs` client và kiểm tra diff chỉ có thêm hằng số.
8. Build GServer.

## Success Criteria
- [ ] Gửi 47 với filter=1, sort=1 thì nhận 48 chỉ có vũ khí, giá tăng dần, tối đa 5 dòng, `totalPages` đúng.
- [ ] Listing của mình có trong 48 với `isMine=true` và trong 51. Gửi 49 mua đồ của mình → 56 `ok=false`.
- [ ] 55 với đồ khóa → 56 `ok=false` kèm thông báo khóa. 55 hợp lệ → 56 `ok=true` "Đăng bán vật phẩm thành công" + 51 mới.
- [ ] 49 mua thành công từ map khác 22.
- [ ] 57 chỉ định tên B → A trừ đúng phí vàng; C không thấy món trong 48; B thấy và mua được; C gửi 49 thẳng → bị từ chối. 57 với tên rỗng → món công khai lại, không trừ phí.
- [ ] Build 0 error. `GopetCmd.cs` client đã regenerate.

## Risk Assessment
- Trùng sub-id với case khác: grep kỹ bước 1.
- Pet icon path khác item: kiểm tra cách `sendMenu` MENU_KIOSK_PET gán ảnh và dùng y hệt.
- Listing bán dở từ luồng NPC: hiển thị giá còn lại, mua trọn phần còn lại qua `TryBuyWhole`.

## Security Considerations
- `listingId` là id ngẫu nhiên, nhưng server vẫn kiểm tra kiosk + owner (chặn tự mua) + AssignedName (chặn người không được chỉ định). Không trả `user_id` người bán về client.
- Price là `int` 1..2e9; count kiểm tra ≥ 1 và ≤ số đang có. Giá client gửi lên không được dùng lúc mua, chỉ dùng giá trong listing.
