---
phase: 2
title: Server Đá mài sửa chữa và nguồn rơi
status: completed
priority: P1
effort: 3h
dependencies:
  - 1
---

# Phase 2: Server Đá mài sửa chữa và nguồn rơi

## Overview
Tạo vật phẩm "Đá mài sửa chữa" và 3 nguồn nhận: đánh quái, boss, điểm danh.

## Key Insights
- Bảng `item` (MAX itemId hiện 1000090). Tên/mô tả nạp vào `ItemLanguage`/`ItemDescLanguage`
  từ bảng này (`GopetManager.cs:1293-1297`). Type cao nhất `ITEM_CARD_REINCARNATION=28`.
- `Player.addItemToInventory` (`Player.cs:928`) tự gộp stack; type lạ rơi vào NORMAL (:962) = túi đồ.
- Bảng `drop_item`: mỗi trận thắng chọn **1 dòng ngẫu nhiên** của map rồi mới tung `percent`
  (`PetBattle.cs:855-885`) ⇒ chèn dòng Đá mài sẽ **pha loãng** mọi món rơi khác. Vì vậy tung
  Đá mài bằng code riêng.
- Boss: phát quà ở `PetBattle.cs:892-897` (`onReiceiveGift(boss.Template.gift)`).
- Điểm danh: `GopetManager.DAILY_CHECKIN_GIFTS` (:831-864), phần tử `{GIFT_ITEM, itemId, count, canTrade}`.
- Icon: `GServer/assets/items/<id>.png`, `iconPath = items/<id>.png`, client xin qua socket.

## Architecture
- Migration `MariaDB_SQL/migration-260924-repair-stone.sql`:
  `INSERT INTO item(itemId=1000091, name='Đá mài sửa chữa', description='Mang tới Thợ rèn ở Thành phố Linh Thú để sửa đầy độ bền 1 trang bị pet.', type=29, iconPath='items/1000091.png', isStackable=1, canTrade=1, price=...)`.
- `GopetManager`: `ITEM_REPAIR_STONE = 29`, `REPAIR_STONE_ID = 1000091`,
  `REPAIR_STONE_DROP_PERCENT = 5f` (đá rơi từ quái khoá giao dịch), `REPAIR_STONE_BOSS_COUNT = 5`.
- Rơi từ quái: trong nhánh thắng quái thường, sau drop thường:
  `if (NextFloatPer() < REPAIR_STONE_DROP_PERCENT) give 1 Đá mài` + thêm vào popup phần thưởng.
- Boss: sau `onReiceiveGift` cho người kết liễu, give `REPAIR_STONE_BOSS_COUNT`.
- Điểm danh: đổi quà ngày 3/10/17/24 → thêm `{GIFT_ITEM, REPAIR_STONE_ID, 1, 1}`, ngày 28 x2
  (giữ quà cũ, chỉ nối thêm phần tử).
- Icon Đá mài: sinh bằng `tools/image-gen` (script mới `gen-repair-stone-icon.py`, pixel art đá mài xám-xanh có tia sáng, nền trong, cắt sát, cỡ giống icon item hiện có) → `assets/items/1000091.png`.
- Dùng Đá mài trực tiếp trong túi: server đáp "Mang tới Thợ rèn ở Thành phố Linh Thú để sửa" (không tiêu).

## Related Code Files
- Create: `SRCGOPETGOC/MariaDB_SQL/migration-260924-repair-stone.sql`, `GServer/assets/items/1000091.png`
- Modify: `GServer/Manager/GopetManager.cs`, `GServer/Data/Battle/PetBattle.cs`, nơi xử lý "dùng item" theo type (MenuController) cho thông báo.
- Modify: `docs/deployment-linux-backend.md` (thêm migration vào danh sách chạy).

## Implementation Steps
1. Kiểm id 1000091 và type 29 chưa dùng; migration; áp vào DB local.
2. Hằng số + helper `GiveRepairStone(player, count)` (addItemToInventory + trả text popup).
3. Hook quái thường, boss; sửa `DAILY_CHECKIN_GIFTS`.
4. Icon; build; thử: admin cộng Đá mài / đánh quái đến khi rơi.

## Success Criteria
- [ ] Đá mài hiện trong túi đồ đúng tên, icon, mô tả, xếp chồng.
- [ ] Tỉ lệ rơi món thường khác không đổi (không chạm `drop_item`).
- [ ] Boss cho 5 Đá mài người kết liễu; lịch điểm danh hiện Đá mài đúng ngày.

## Risk Assessment
- Popup kết quả trận có thể không hiện dòng Đá mài nếu gắn sai chỗ → thêm vào cùng mảng `Popup` của phần thưởng.
- Thay quà điểm danh giữa tháng: người đã nhận ngày đó không nhận lại — chấp nhận.
