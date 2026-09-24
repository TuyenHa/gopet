---
phase: 3
title: Server NPC Thợ rèn và menu sửa
status: completed
priority: P1
effort: 4h
dependencies:
  - 1
  - 2
---

# Phase 3: Server NPC Thợ rèn và menu sửa

## Overview
Đặt NPC Thợ rèn ở Thành phố Linh Thú (map 11) với 2 lựa chọn: "Sửa trang bị" và "Độ bền là gì?".
Sửa: chọn món trong danh sách → xác nhận → trừ 1 Đá mài → đầy độ bền → cập nhật chỉ số pet.

## Key Insights
- Map 11 (`MapManager.ID_LINH_THU_CITY`), `map.npc = [-1,-7,-15,-25]`.
- Đã có NPC -32 "THO REN" (ảnh `npcs/Tho_Ren.png`) nhưng đặt ở map 28, `optionId=[]`; toạ độ
  NPC là toàn cục theo NPC chứ không theo map ⇒ **tạo NPC mới -33** dùng lại ảnh, toạ độ hợp
  với map 11 (chọn khi làm, cạnh các NPC hiện có, tránh đường đi/nhà).
- Luồng: `GameController.selectOption` (:853) → `MenuController.selectNpcOption`
  (`MenuController.selectNpcOption.cs:15`). OP lớn nhất 97, MENU lớn nhất 1091.
- Mẫu menu liệt kê trang bị: `OP_DUNG_HỢP` → `MENU_FUSION_MENU_EQUIP`
  (dựng `sendMenu.cs:534-584`, lọc `getItemByMenuId` `MenuController.cs:1182-1191`, chọn `selectMenu.cs:847`).
- Client: NPC ngoài danh sách view riêng dùng `ChoiceDialogView` chung ⇒ không cần code client.

## Architecture
- Migration (gộp vào file phase 2 hoặc file riêng `migration-260924-blacksmith-npc.sql`):
  `INSERT INTO npc(npcId=-33, name='Thợ Rèn', chat='Đồ mòn thì mang đây, có Đá mài là ta sửa!', optionName=['Sửa trang bị','Độ bền là gì?'], optionId=[98,99], x, y, imgPath='npcs/Tho_Ren.png', ...)`
  + `UPDATE map SET npc='[-1,-7,-15,-25,-33]' WHERE mapID=11`.
- `MenuController`: `OP_REPAIR_EQUIP = 98`, `OP_DURABILITY_HELP = 99`, `MENU_REPAIR_EQUIP = 1092`.
- `OP_REPAIR_EQUIP` → gửi `MENU_REPAIR_EQUIP`: mọi trang bị pet trong EQUIP_PET_INVENTORY có
  `durability < Max`, mỗi dòng "Tên — Độ bền n/80" (món hỏng lên đầu), tiêu đề ghi số Đá mài đang có.
  Danh sách rỗng → "Trang bị của bạn đều còn tốt".
- Chọn món → Yes/No "Dùng 1 Đá mài sửa chữa để sửa [Tên]?" → `RepairService.Repair(player, itemId)`:
  khoá theo người chơi; kiểm item thuộc người chơi, là trang bị pet, chưa đầy; đủ Đá mài
  (`checkCount`) → `subCountItem(đá mài,1)` → `EquipDurability.Repair` → nếu món đang mặc
  thì `pet.applyInfo` + `sendMyPetInfo` → okDialog "Đã sửa xong [Tên]". Thiếu Đá mài →
  redDialog "Cần 1 Đá mài sửa chữa — có khi đánh quái, đánh boss, điểm danh".
- `OP_DURABILITY_HELP` → okDialog giải thích ngắn luật độ bền + nguồn Đá mài.

## Related Code Files
- Create: `GServer/Data/item/EquipRepairService.cs`, migration NPC.
- Modify: `GServer/Server/MenuController.cs` (hằng), `MenuController.selectNpcOption.cs`, `MenuController.sendMenu.cs`/`selectMenu.cs`, `answerYesNo.cs` (nếu xác nhận đi qua Yes/No của server), `LanguageData.cs`.

## Implementation Steps
1. Kiểm OP 98/99, MENU 1092, npcId -33 chưa dùng; chọn toạ độ NPC trên map 11 (xem NPC khác + ảnh map).
2. Migration NPC + map; áp DB local.
3. Service sửa + menu + xác nhận.
4. Build, restart, thử: vào map 11 thấy Thợ Rèn, sửa món hỏng, chỉ số về lại, Đá mài giảm 1.

## Success Criteria
- [ ] Thợ Rèn hiện ở Thành phố Linh Thú, bấm thấy 2 lựa chọn.
- [ ] Danh sách chỉ gồm món chưa đầy; sửa trừ đúng 1 Đá mài, đầy độ bền, pet có lại chỉ số.
- [ ] Bấm sửa liên tục/gói trùng không trừ 2 Đá mài cho 1 lần sửa; không sửa được món người khác/đồ không phải trang bị.

## Risk Assessment
- Toạ độ NPC đè nhà/ra ngoài vùng đi được → kiểm bằng ảnh chụp trong game.
- Memory: sau rebuild NPC có thể chỉ hiện tên (ImageHandler timeout) — không phải lỗi tính năng.
