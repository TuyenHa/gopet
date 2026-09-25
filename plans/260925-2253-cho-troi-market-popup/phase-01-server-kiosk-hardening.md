---
phase: 1
title: Server kiosk hardening
status: completed
priority: P1
effort: 5h
dependencies: []
---

# Phase 1: Server kiosk hardening

## Overview
Sửa các lỗi kiosk hiện có và tách phần lõi "treo bán / mua / gỡ" thành API dùng chung. Luồng NPC cũ và popup mới (phase 2) cùng gọi các API này (DRY). Phase này không đổi UI.

## Key Insights (scout)
- `SellItem` (`Data\item\SellItem.cs:8-37`) không lưu tên người bán, chỉ có `user_id`.
- **Bảo mật:** `GameController.removeSellItem` (`GameController.cs:2926-2970`) không kiểm tra chủ. Client sửa đổi có thể gỡ đồ của người khác. Retail toggle (`selectMenu.cs:2800-2849`, option 2) cũng không kiểm tra chủ.
- **Race:** mỗi đường khóa một kiểu. Cancel dùng `lock(sellItem)`, buy dùng `sellItemMutex`, còn `Kiosk.update()` (expire) không khóa gì. Hậu quả là có thể nhân đôi đồ hoặc tiền.
- **Mất tiền:** ở `Kiosk.cs:357-360`, khi đồ bán dở hết hạn lúc người bán online, số tiền bị tính rồi bỏ đi, không cộng vào. Khi offline thì `Player.cs:523-526` cộng 100% `sumVal`, không trừ thuế 5%. Cancel lại cộng 95%. Cả 3 đường cần thống nhất về 95%.
- `Kiosk.update()` chạy 10 lần mỗi tick (`MarketMap` tạo 10 `MarketPlace`, `Data\map\MarketMap.cs:16-20`).
- Đồ khóa chỉ bị chặn tại `MenuController.cs:1333` (`!item.Template.canTrade || !item.canTrade`). `SellKioskItem` không kiểm tra lại `petEuipId`/`gemInfo`/đồ còn trong túi (item lấy từ cache `objectPerformed`).
- Tính năng "chỉ định người mua" (`AssignedName`, phí vàng `MenuController.cs:1341`, `INPUT_ASSIGNED_NAME_KIOSK` `inputDialog.cs:755-787`, option đổi tên `selectMenu.cs:2800-2849`) → **giữ**; phí bị đảo: pet đang trả 10.000, đồ trả 15.000, ngược với chữ trên menu → sửa đúng (pet 15.000, đồ 10.000). Popup mới chỉ định **sau khi đăng** (phase 2 sub 57).
- Chỉ lưu market mỗi 30 phút + khi shutdown (`Runtime\AutoSave.cs:58-62`). Nếu crash thì mất listing.
- Cast `(MarketPlace)player.getPlace()` không dùng kết quả: `sendMenu.cs:794`, `selectMenu.cs:1479`, `GameController.cs:2898`, `:2929`.
- `kiosk_recovery.item varchar(10000)` có thể không đủ chỗ cho JSON pet.

## Requirements
- Functional: chỉ chủ mới gỡ/sửa được listing. Buy/cancel/expire loại trừ lẫn nhau, mỗi listing chỉ được xử lý đúng 1 lần. Người bán luôn nhận 95% ở mọi đường (buy, cancel bán dở, expire online, expire offline). Listing mới lưu `SellerName`.
- Non-functional: không làm hỏng luồng NPC map 22. Market được lưu DB ngay sau khi có thay đổi (debounce).

## Architecture
- `SellItem`:
  - Thêm `public string SellerName`, đặt khi treo. Dữ liệu JSON cũ không có trường này sẽ là `null`, khi đó fallback `"???"`.
  - Thêm `[JsonIgnore] public readonly object Sync = new()`. Mọi thao tác buy/cancel/expire đều `lock (sellItem.Sync)` và kiểm tra-rồi-đặt `hasSell`/`hasRemoved`.
  - Bỏ `sellItemMutex`. Nếu lo ảnh hưởng thì giữ field nhưng không dùng.
- `Kiosk` thêm các hàm lõi:
  - `KioskResult TryList(Player p, ListRequest req)`: validate + remove khỏi inventory + `addKioskItem`. Trả về mã lỗi dạng chuỗi Language, không tự gửi dialog, để 2 luồng tự hiển thị.
  - `KioskResult TryBuyWhole(Player p, int listingId)`: rút từ `confirmBuy`, không hỏi confirm.
  - `KioskResult TryCancel(Player p, int listingId)`: có owner check, trả đồ + 95% `sumVal`.
  - `update()`: dùng lock + flag, cộng 95% `sumVal` khi online.
- Helper `KioskPayout.SellerShare(long v) => round(v * (100 - KIOSK_PER_SELL) / 100)`. Dùng ở mọi chỗ, kể cả `Player.cs` lúc đăng nhập restore recovery. Lưu ý: recovery lưu `sumVal` gốc, chỉ áp thuế lúc cộng.
- `MarketPlace.update()`: bỏ vòng `kiosk.update()` ở đây. Thay bằng một ticker tĩnh duy nhất `MarketExpiryTicker` (Timer 5s, hoặc gắn vào vòng tick server sẵn có nếu có) để expire chạy 1 lần/chu kỳ và chạy cả khi map 22 không cập nhật.
- `GopetManager.RequestMarketSave()`: đặt cờ dirty. AutoSave (hoặc timer 10s) thấy dirty thì gọi `saveMarket()`. Gọi hàm này sau mọi list/buy/cancel/expire.

## Related Code Files
- Modify: `SRCGOPETGOC\GServer\Data\item\SellItem.cs` (SellerName, Sync)
- Modify: `SRCGOPETGOC\GServer\Data\map\Kiosk.cs` (TryList/TryBuyWhole/TryCancel, sửa update). Nếu vượt 200 dòng thì tách `Kiosk.Operations.cs` (partial).
- Create: `SRCGOPETGOC\GServer\Data\map\KioskPayout.cs`, `SRCGOPETGOC\GServer\Data\map\MarketExpiryTicker.cs`
- Modify: `SRCGOPETGOC\GServer\Place\MarketPlace.cs` (bỏ update kiosk)
- Modify: `SRCGOPETGOC\GServer\Server\GameController.cs` (`removeSellItem` → `Kiosk.TryCancel`, bỏ cast `:2898`, `:2929`)
- Modify: `SRCGOPETGOC\GServer\Server\MenuController.cs` (`SellKioskItem` → gọi `Kiosk.TryList`; sửa phí đảo `:1341`)
- Modify: `SRCGOPETGOC\GServer\Server\MenuController.selectMenu.cs` (bỏ cast `:1479`; owner check retail toggle `:2800-2849`; buy dùng lõi mới)
- Modify: `SRCGOPETGOC\GServer\Server\MenuController.sendMenu.cs` (bỏ cast `:794`)
- Modify: `SRCGOPETGOC\GServer\Server\Player.cs:509-529` (recovery cộng `SellerShare(sumVal)`)
- Modify: `SRCGOPETGOC\GServer\Manager\GopetManager.cs` (`RequestMarketSave`), `SRCGOPETGOC\GServer\Runtime\AutoSave.cs`
- Modify: `SRCGOPETGOC\GServer\Language\LanguageData.cs` (thêm chuỗi lỗi: `KioskNotOwner`, `KioskItemMissing`, `KioskInvalidCount`…)
- Create: `SRCGOPETGOC\MariaDB_SQL\migration-260925-kiosk-recovery-mediumtext.sql` (`ALTER TABLE kiosk_recovery MODIFY item MEDIUMTEXT NOT NULL;`)

## Implementation Steps
1. Thêm `SellerName` + `Sync` vào `SellItem`. Trong `addKioskItem` đặt `SellerName = player.playerData.name`.
2. Viết `KioskPayout.SellerShare`, thay mọi phép tính 95% hiện có bằng hàm này.
3. Viết `TryList(Player, ListRequest{ sbyte source; int itemOrPetId; int count; int price })`. Validate:
   - Không đang bảo trì.
   - Giá nằm trong 1..2e9.
   - Đồ còn trong túi, tìm lại theo id (không dùng object cache).
   - `canTrade` (cả `Template` lẫn item).
   - `petEuipId<=0`, `gemInfo==null`.
   - Pet không phải trial (`Expire==null`) và không phải pet đang dùng.
   - Count nằm trong 1..item.count.
   - Kiosk type suy ra từ template type theo bảng ở phase 2.
   Validate xong thì remove/subCount khỏi túi rồi add listing.
4. Viết `TryBuyWhole`: lock Sync → kiểm tra `!hasSell && kioskItems.Contains` → `TrySpendCoin(price - sumVal)` → giao đồ → remove → cộng `SellerShare(price)` cho người bán (online dùng addCoin, offline dùng UPDATE, như code hiện có). Refactor `confirmBuy` thành wrapper của hàm này.
5. Viết `TryCancel`: owner check → lock → cờ `hasRemoved` → trả đồ + `SellerShare(sumVal)`. `removeSellItem` và retail toggle phải qua owner check.
6. Sửa `update()`: với từng item hết hạn, lock → bỏ qua nếu đã `hasSell`/`hasRemoved` → đặt `hasRemoved` → trả đồ (online) hoặc insert recovery (offline). Tiền `SellerShare(sumVal)` cộng khi online. Khi offline, recovery lưu `sumVal` gốc và `Player.cs` cộng `SellerShare`.
7. Chuyển expire sang `MarketExpiryTicker`, khởi động ở `App\Main.cs` sau `loadMarket()`.
8. Thêm `RequestMarketSave()` và gọi ở mọi mutation.
9. Bỏ 4 cast `MarketPlace`. Sửa phí chỉ định bị đảo (`MenuController.cs:1341`) → dùng helper `KioskPayout.AssignFee(bool isPet)`.
10. Thêm `KioskResult TrySetAssignedName(Player p, int listingId, string name)`: owner check → lock Sync → `name` rỗng = bỏ chỉ định (miễn phí); khác rỗng: không được là chính mình, người chơi phải tồn tại (tra `player.name` trong DB/online), khác tên hiện tại thì trừ phí `AssignFee` bằng vàng (`checkGold/mineGold`), thiếu vàng → lỗi. Luồng NPC option "đổi tên người mua" (`selectMenu.cs:2800-2849`) gọi chung hàm này.
11. Viết file migration SQL, chạy trên DB dev.
12. `dotnet build` GServer, 0 error.

## Success Criteria
- [ ] Gỡ listing của người khác bằng itemId giả thì bị từ chối (test).
- [ ] Buy và cancel đồng thời trên cùng listing: chỉ 1 thành công, không nhân đôi (test đa luồng).
- [ ] Đồ bán dở hết hạn: người bán nhận 95% `sumVal` ở cả online lẫn offline (test).
- [ ] Đồ khóa, đồ pet đang mặc, đồ có ngọc khảm đều bị `TryList` từ chối.
- [ ] Mua/bán qua NPC map 22 vẫn chạy như cũ.
- [ ] Build GServer 0 error.

## Risk Assessment
- Đổi lock có thể làm hỏng luồng mua lẻ `buyRetail`. Giảm rủi ro: chuyển `buyRetail` sang `lock(Sync)` cùng lúc và giữ nguyên logic.
- JSON market cũ không có `SellerName`: Newtonsoft bỏ qua được, fallback tên.
- Expire ticker chạy trên thread riêng trong khi player cũng thao tác: lock Sync bao cả việc trả đồ. `addItemToInventory` đã thread-safe chưa thì cần kiểm tra. Nếu chưa, đẩy vào hàng đợi main loop giống AutoSave.

## Security Considerations
- Mọi request client đều validate lại ở server (id, owner, count, price). Không tin dữ liệu cache.
- Không log thông tin nhạy cảm. History log giữ như cũ.
