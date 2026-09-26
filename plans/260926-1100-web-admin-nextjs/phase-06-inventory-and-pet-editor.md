---
phase: 6
title: "Inventory and pet editor"
status: completed
priority: P3
effort: "10h"
dependencies: [4, 5]
---

> **2026-09-26**: Triển khai xong (code + unit test). Báo cáo:
> `plans/reports/fullstack-developer-260926-phase06-inventory-pet-editor.md`. Còn thiếu bước
> QA thủ công "login thật rồi dùng/trang bị/bán item" (cần GServer + client chạy).

# Phase 6: Inventory and pet editor

> Cập nhật sau Red Team 2026-09-26 (RT#3). **Vòng 3 (sau MVP)**. Việc *tặng* item/pet đã có đường an toàn (giftcode/thư, phase 5B/8). Phase này chỉ dành cho **sửa/xoá** đồ hiện có (vd thu hồi item dupe), và phải tuân thủ bất biến của server.

## Overview
Xem/sửa `player.items`, `pets`, `petSelected`, `PetDefLeague` (JSON Newtonsoft) khi người chơi offline, qua `withOfflinePlayer` (phase 4/5).

## Bất biến server (bắt buộc giữ) [RT#3]
- Mỗi danh sách item theo invType và danh sách `pets` **sắp tăng dần theo id**: server tìm bằng `BinarySearch` (`Util/Utilities.cs:345-365`) và luôn `Sort(BinaryCompare)` sau khi thêm (`Data/User/PlayerData.cs:330-345`, `Util/BinaryCompare.cs:13`). Web phải sort lại sau mọi thay đổi.
- Pet đang theo nằm ở `petSelected`, pet thủ đài ở `PetDefLeague` — **bị gỡ khỏi `pets`** (`MenuController.selectMenu.cs:472-480`). Tab pet phải hiển thị cả 3 nguồn; kiểm trùng `petId` trên `pets ∪ petSelected ∪ PetDefLeague`.
- Cấp id mới theo đúng cách `Utilities.BinaryObjectAdd` (đọc code: khoảng giá trị, ngẫu nhiên/tuần tự) — không tự chế.
- Gộp stack: khoá = `itemTemplateId + canTrade` (`Server/Player.cs:883`), không chỉ templateId.
- Item có chỉ số random lúc tạo (`Data/item/Item.cs:83-97`) → **web không tạo item mới**; thêm item = giftcode/thư. Web chỉ sửa field có sẵn: `count`, `lvl`, `expire`, `canTrade`, `durability`; xoá item (gỡ `petEuipId` liên quan trên pet `equip`).
- Giữ nguyên field không biết (round-trip); kiểm `TimeCreate`/`version` không bị đổi định dạng (`Item.cs:69,76`).

## Architecture
```
src/lib/game-json/item-json.ts     # type Item khớp Item.cs, sortInventory()
src/lib/game-json/pet-json.ts      # type Pet khớp Pet.cs, collectAllPets()
src/lib/players/inventory-actions.ts, pet-actions.ts
src/app/(admin)/players/[playerId]/inventory-tab.tsx, pet-tab.tsx
```
- Ghi: trong guard; optimistic `WHERE ID=? AND MD5(items)=?`; lưu bản cũ nguyên cột vào audit `detail.before` (khôi phục thủ công bằng SQL nếu cần — không làm nút tự động).

## Implementation Steps
1. Đọc `Utilities.cs:340-380`, `PlayerData.cs:320-360`, `Item.cs`, `Pet.cs`; ghi bất biến thành comment đầu file.
2. Test round-trip + sort + kiểm trùng id với fixtures trích từ dump thật.
3. UI + actions sửa/xoá.
4. Test thật: sửa count/xoá item/đổi chỉ số pet → login → dùng/trang bị/bán item còn lại bình thường, save lại không lỗi.

## Success Criteria
- [x] Sau khi sửa, mọi danh sách vẫn sorted, id duy nhất trên cả 3 nguồn pet (unit test: sort invariant + findDuplicatePetIds)
- [ ] Người chơi login, dùng/trang bị/bán được item sau khi web sửa (pending manual E2E with GServer + client)
- [x] Người online / server không heartbeat → không sửa được (tái dùng `withOfflinePlayer` phase 4/5, không đổi)

## Risk Assessment
- **Cao**: JSON sai làm player không load được. Giảm: zod + bất biến + test "login và dùng" bắt buộc trước khi merge; before-image trong audit.

## Security Considerations
- Chỉ admin; giới hạn count; audit mọi thay đổi.
