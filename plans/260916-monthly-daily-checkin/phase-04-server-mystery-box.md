# Phase 04 — Server: Hộp quà bí ẩn UseItem + loot random

## Context
- Mẫu chuẩn: `GameBirthdayEvent` (Data/Event/Year2025/GameBirthdayEvent.cs):
  - `ID_RANDOM_EVENT_BOX=240023`, data `GIFT_OPEN_EVENT_BOX_DATA` (int[][]).
  - `UseItem(itemId, player)` → `UseEventItem` → confirm → `UseItemCount`.
  - Mở hộp: `subCountItem(item,1)` + `onReiceiveGift(GIFT_OPEN_EVENT_BOX_DATA)` + popup (dòng 258-268).
- Cơ chế random: `GIFT_RANDOM_ITEM` trong `onReiceiveGift` (GameController.cs:4120-4206). Bốc đều qua `Utilities.RandomArray`. Hỗ trợ mã âm để random nhóm mảnh ghép (GopetManager.cs:607-635).
- Đăng ký item "dùng được" của event: qua `ItemsOfEvent` + dispatch `UseItem` (xem cách EventBase gọi).

## Overview
- **Priority**: trung bình (sau 01/02).
- **Status**: chưa làm.
- Cho 2 item `240024`/`240025` mở ra random loot (data từ phase-01).

## Requirements
### Nơi xử lý UseItem
2 hộp này KHÔNG thuộc event Tết. Đưa `UseItem` cho chúng vào `DailyCheckinEvent` (phase-03) HOẶC 1 handler item thường trực:
- `DailyCheckinEvent.ItemsOfEvent = { ID_BOX_TUAN4, ID_BOX_CUOITHANG }`.
- `override UseItem(itemId, player)`:
  ```
  switch(itemId){
    case ID_BOX_TUAN4:     OpenBox(player, ID_BOX_TUAN4, BOX_TUAN4_DATA); break;
    case ID_BOX_CUOITHANG: OpenBox(player, ID_BOX_CUOITHANG, BOX_CUOITHANG_DATA); break;
  }
  ```
- `OpenBox(player, boxItemId, int[][] lootData)`:
  ```
  Item box = player.controller.selectItemsbytemp(boxItemId, NORMAL_INVENTORY)
  if (box == null) { redDialog("Không có hộp"); return }
  player.controller.subCountItem(box, 1, NORMAL_INVENTORY)   // mở 1 hộp/lần
  var popups = player.controller.onReiceiveGift(lootData)     // random 1 phần
  player.okDialog("Bạn mở hộp nhận được: " + join(popups))
  ```
- `lootData` dạng `int[][]` **1 phần tử**: `{ GIFT_RANDOM_ITEM, 1, itemId1,count1, ... }` (soLanBoc=1). Data cụ thể ở phase-01.

### Điều kiện
- `Condition` của event thường trực → hộp mở được bất cứ lúc nào (không giới hạn thời gian như hộp Tết). Không đặt `CheckEventStatus` chặn thời gian.
- Kiểm tra túi đầy trước khi mở (nếu jackpot ra mảnh ghép không stack) — `onReiceiveGift`/`addItemToInventory` đã tự xử; nếu cần, thêm check slot trống.

## Related Code Files
- Sửa: `GServer/Data/Event/DailyCheckin/DailyCheckinEvent.cs` (thêm `ItemsOfEvent`, `UseItem`, `OpenBox`).
- Dùng: `GameController.selectItemsbytemp`, `subCountItem`, `onReiceiveGift`.
- Data: `GopetManager.BOX_TUAN4_DATA`, `BOX_CUOITHANG_DATA` (phase-01).

## Todo
- [ ] `ItemsOfEvent` + `UseItem` + `OpenBox`.
- [ ] Bảo đảm event được register để `UseItem` từ túi đồ route đúng tới `DailyCheckinEvent` (kiểm cơ chế dispatch item event trong EventBase/GopetManager).
- [ ] Compile + test mở hộp thủ công (add item qua admin → dùng → xem loot).

## Success Criteria
- Dùng hộp `240024`/`240025` từ túi → trừ 1 hộp, nhận đúng 1 phần từ loot pool tương ứng, popup hiển thị tên item.
- Mở nhiều lần cho phân phối ~ đúng tỉ lệ (jackpot ~5%).

## Risk
- Nếu cơ chế dispatch UseItem của EventBase chỉ chạy khi `Condition` đúng thời gian → phải để `Condition=true` thường trực, hoặc tách handler item ra khỏi ràng buộc thời gian.
- Item không stack (mảnh ghép jackpot) khi túi đầy → cần thông báo lỗi thay vì mất quà.

## Unresolved
- Cho mở nhiều hộp 1 lần (nhập số lượng) hay chỉ 1 hộp/lần? (đề xuất: 1 hộp/lần cho đơn giản).
