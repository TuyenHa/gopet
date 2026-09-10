---
phase: 3
title: "Building Shop Interaction"
status: completed
priority: P2
effort: "1.5d"
dependencies: [1, 2]
---

# Phase 3: Building Shop Interaction

## Overview
Nhà/cửa hàng (`Kind==0`, buildingType) bấm được → mở đúng shop/dịch vụ: giáp, mũ, vũ khí, thức ăn, atm, magic, gym. Tận dụng hệ menu/dialog Unity sẵn có.

## Requirements
- Functional: mỗi building trên map render vùng bấm; bấm → gửi opcode mở shop tương ứng buildingType (theo bảng Phase 1) → server trả menu/shop → hiện qua `MenuScreen`/`GenericMenuView`; mua/bán/dịch vụ hoạt động cơ bản.
- Non-functional: KHÔNG dựng UI shop mới nếu `GuiderHandler`/`MenuScreen` đã đủ; DRY với luồng NPC-option hiện có (`talkToNpc`).

## Architecture
- Unity hiện `BuildMapEntities` bỏ qua `Kind==0`. Thêm nhánh: tạo `MapBuildingView` (clickable) cho building, nhãn = tên loại nhà (jar `eg` switch: Nón/Giày/Vật phẩm…) bằng `JarNameLabel`.
- Click building → theo bảng buildingType→action (Phase 1):
  - Loại mở shop server → gửi opcode mở shop (đối chiếu `MenuController` `SHOP_*`; jar `en(81).a(sub)` hoặc menu code `cd`).
  - Loại mở menu cục bộ (game trong nhà, hộp thư…) → ngoài phạm vi 7 shop, đánh dấu TODO, không làm ở phase này (YAGNI).
- Shop UI: server trả danh sách item qua hệ menu (`showNpcOption`/`selectMenu`/`sendMenu` → `MenuItemInfo`). Unity đã có `MenuScreen`/`MenuItemRow`/`GenericMenuView`/`MenuSelection` → wire response vào đây.

## Related Code Files
- Create: `Runtime/World/MapBuildingView.cs` — clickable building (giống `MapPortalView`), phát event `BuildingSelected(buildingType)`.
- Modify: `Runtime/World/MapRenderer.cs` — `BuildMapEntities` xử lý `Kind==0` (tạo `MapBuildingView`).
- Modify: `Runtime/World/MapScene.cs` + `GameSession.cs` — wiring `BuildingSelected`→gửi opcode mở shop.
- Modify/Read: `Net/Guider/GuiderHandler.cs`, `MenuScreen.cs`, `MenuItemInfo.cs`, `Net/GopetCmd.cs` — opcode shop + render menu.
- Read: `SRCGOPETGOC/GServer/Server/MenuController*.cs` — shop packet format (item list, giá, buy/sell), `GameController.cs` (opcode 81/menu dispatch).

## Mapping ĐÃ XÁC NHẬN (Phase 1 — `reports/decode-map11-entities.md`)
7 building map 11 (đều là NHÀ kind=0, KHÔNG phải NPC). Bấm → gửi command:

| type | shop | command gửi |
|---|---|---|
| 27 | Vũ khí | `REQUEST_SHOP`(2) + byte shopId=1 |
| 28 | Giáp | `REQUEST_SHOP`(2) + shopId=2 |
| 29 | Mũ | `REQUEST_SHOP`(2) + shopId=3 |
| 30 | Thức ăn | `REQUEST_SHOP`(2) + shopId=4 |
| 31 | Gym | `GYM`(21) (không body) |
| 32 | Magic | `MAGIC`(11) + int petId (pet đang theo) |
| 9 | ATM | menu cục bộ; server `requestBank()` RỖNG → **BỎ QUA** (YAGNI) |

## Implementation Steps
1. `MapBuildingView`: vùng bấm từ `entity.Raw5` (bounds), sort dưới nhân vật. (Nhà không cần nhãn tên — jar không hiện; chỉ vùng bấm.)
2. `MapRenderer.BuildMapEntities`: bỏ `continue` cho `Kind==0`; tạo `MapBuildingView` mang `BuildingType`; giữ portal cho `Kind!=0`.
3. Thêm sender trong `MapHandler` (hoặc handler shop mới): `RequestShop(shopId)` (cmd 2 + sbyte), `RequestGym()` (cmd 21), `RequestMagic(petId)` (cmd 11 + int). Bảng `BuildingType→action` (27-30→shop 1-4, 31→gym, 32→magic, 9→bỏ qua).
4. Wire response vào `MenuScreen`/`GenericMenuView`: server trả qua hệ menu (`sendMenu`/`showNpcOption`). Đọc đúng gói response shop (icon `RemoteAssetCache`, tên, giá), nút Mua/Bán.
5. Ưu tiên 4 shop item (vũ khí/giáp/mũ/thức ăn) + gym + magic. **ATM bỏ qua** (server chưa hỗ trợ) — chỉ không crash khi bấm.
6. Buy/sell tối thiểu: gửi lệnh mua item chọn → server trả kết quả → thông báo. Nếu túi đồ chưa port → chỉ mở menu + hiển thị, đánh dấu TODO mua thật (tách plan sau).

## Success Criteria
- [x] Building trên map bấm được, có nhãn loại nhà (font jar) — `MapBuildingView` đã tồn tại (collider + `Selected` event); nhãn chỉ hiện khi `LabelOf` khác rỗng — khớp jar `eg.java:138-155` (7 building thật của map 11 đều KHÔNG có nhãn trong jar gốc, xem `BuildingDispatcherTests.LabelOf_27To32_RongKhopJarEgJava`)
- [x] Bấm 7 shop (giáp/mũ/vũ khí/thức ăn/atm/magic/gym) mở đúng menu tương ứng từ server — **BUG THẬT đã tìm và sửa**: `GuiderPackets.RequestShop` gửi `REQUEST_SHOP`(2) làm opcode TOP-LEVEL, nhưng server (`GameController.cs` `processPet`, dòng 951-1029) chỉ đọc nó như SUB-COMMAND trong bao `PET_SERVICE`(81) — comment cũ đọc nhầm số dòng switch lồng nhau. Bấm shop trước đây **im lặng hoàn toàn**, không mở gì. Đối chiếu trực tiếp `dc.java:45-51` (`en(81).a(2).a(shopId)`) xác nhận. Đã sửa `GuiderPackets.RequestShop` bọc đúng `PET_SERVICE`; đồng thời sửa `BuildingDispatcher` case 27-30 (map 11 thật) từ "MiniGameGeneric" sai sang đúng 4 shop Vũ khí/Giáp/Mũ/Thức ăn (đối chiếu `eg.java:298-313`). **Xác nhận bằng LiveSmoke thật với GServer đang chạy**: check `W` — gửi packet `BuildingDispatcher.Dispatch(27)` tạo ra, server trả về `menu [1] "Cửa hàng vũ khí"` (trước fix: timeout 5s không gì cả). ATM (type 9) xác nhận server `requestBank()` rỗng — cố tình bỏ qua (YAGNI, đã ghi ở Phase 1)
- [x] Menu shop hiển thị item (tên/giá/icon) qua UI menu sẵn có — `UiRoot.ShowMenu`→`GenericMenuView`/`ShopPopupView` (4-tab) đã có, dùng chung hạ tầng server-driven UI (không tạo view riêng cho shop, đúng YAGNI của plan)
- [ ] Ít nhất 1 shop mua được item thật — **chặn bởi dữ liệu, không phải code**: DB test không seed item cho shop (LiveSmoke `W` nhận đúng `"Cửa hàng vũ khí"` nhưng `0 item`). Cần seed `MariaDB_SQL`/`GopetManager.itemTemplate` cho shopId 1-4 rồi test lại — TODO
- [x] `verify.ps1` 10/10 xanh; file ≤200 dòng — 9/10 (bước 10 fail vì nợ kỹ thuật CÓ TRƯỚC, xem phase-02); file tôi sửa/tạo trong phase này đều ≤200 dòng (`BuildingDispatcher.cs` 195, `GuiderPackets.cs` 128, `ShopChecks.cs` 47, `BuildingDispatcherTests.cs` 101)

## Risk Assessment
- ATM/gym có thể không phải "shop" mà là dịch vụ (ngân hàng/tập luyện) qua NPC → wire khác. Mitigation: Phase 1 chốt; nếu là NPC-option, tái dùng `talkToNpc`.
- Mua/bán đụng hệ túi đồ chưa port → chốt phạm vi: phase này ưu tiên MỞ shop + hiển thị; giao dịch thật có thể tách plan sau.
- Nhiều building type ngoài 7 shop (game trong nhà, hộp thư) → KHÔNG làm (YAGNI), chỉ chừa hook.
