---
phase: 1
title: Server độ bền và hao mòn sau trận
status: completed
priority: P1
effort: 4h
dependencies: []
---

# Phase 1: Server độ bền và hao mòn sau trận

## Overview
Thêm độ bền vào `Item`, trừ sau mỗi trận ở một điểm duy nhất, món hỏng không cộng chỉ số,
tên/mô tả hiện "Độ bền n/80" hoặc "(Hỏng)".

## Key Insights
- `Item` lưu JSON (Newtonsoft, `JsonAdapter`, `DefaultValueHandling.Include`) trong cột
  `player.items`; ctor rỗng `Item()` (`Data/item/Item.cs:69`). Thêm public field có giá trị
  khởi tạo ⇒ item cũ trong DB tự nhận giá trị đó khi nạp → **không cần migration DB**.
- Loại trang bị: `PET_EQUIP_WEAPON=1`, `ARMOUR=2`, `HAT=3`, `SHOE=104`, `GLOVE=105`
  (`Manager/GopetManager.cs:142-162`); `ItemTemplate.IsEquip`.
- Chỉ số trang bị cộng ở `Pet.applyInfo` (`Data/pet/Pet.cs:302`, vòng :309, cộng :333-336);
  dòng :332 ghi `ItemEquipType[...]` là nguồn bonus set (:425-430).
- Điểm chốt trận duy nhất: `PetBattle.win(Popup[], coin, exp)` (`Data/Battle/PetBattle.cs:987-1000`)
  — mọi nhánh PvE/PK/thách đấu/đấu trường và `surrender()` đều qua đây; `Close()` (rời map,
  rớt mạng) thì không. `win()` idempotent nhờ `hadFinished`.
- `sendMyPetInfo` KHÔNG gọi `applyInfo` ⇒ món vừa hỏng phải tự gọi `pet.applyInfo(player)`.

## Architecture
- `Data/item/EquipDurability.cs` (static, thuần, test được):
  - `Max = 80`, `WearWin = 1`, `WearLose = 2`, `WarnAt = 8`.
  - `Applies(Item)` — template là trang bị pet (5 loại trên).
  - `IsBroken(Item)` → `Applies && durability <= 0`.
  - `Wear(Item, bool lost)` → trả `WearResult { None, Warned, Broke }` (vượt ngưỡng cảnh báo/hỏng lần này).
  - `Repair(Item)` → `durability = Max`.
- `Item.durability = EquipDurability.Max` (public field).
- `PetBattle.ApplyEquipmentWear()` gọi trong `win(...)` cho `activePet` (+ `passivePet` nếu PvP):
  với từng id trong `pet.equip` → `selectItemEquipByItemId` → `Wear`; nếu có món hỏng thì
  `pet.applyInfo(player)` rồi để `sendMyPetInfo` sẵn có gửi đi; gom thông báo (xem phase 4).
  Pet của quái/boss không có trang bị người chơi → bỏ qua.
- `Pet.applyInfo`: giữ dòng :332 (`ItemEquipType[...]`, nguồn bonus set) rồi mới
  `if (EquipDurability.IsBroken(it)) continue;` **trước** :333 ⇒ món hỏng chỉ mất chỉ số riêng,
  **bonus set vẫn giữ** (user chốt).
- Chữ: `Item.getEquipName` (:441-512) và `getDescription` (:223) thêm dòng
  "Độ bền: n/80" (vàng khi ≤ 8) hoặc "(Hỏng) — mang tới Thợ rèn" — chuỗi qua `LanguageData`.

## Related Code Files
- Create: `GServer/Data/item/EquipDurability.cs`
- Modify: `GServer/Data/item/Item.cs`, `GServer/Data/pet/Pet.cs`, `GServer/Data/Battle/PetBattle.cs`, `GServer/Language/LanguageData.cs`

## Implementation Steps
1. `EquipDurability` + field `durability`.
2. Bỏ qua món hỏng trong `applyInfo`.
3. Hook mòn trong `win(...)`, gọi lại `applyInfo` khi có món hỏng.
4. Text độ bền/hỏng trong tên + mô tả. Kiểm tra parser client (`UiLogic/ShopItemText*.cs`,
   `GemItemText.cs`) không hiểu nhầm dòng mới.
5. `dotnet build`; chơi thử: đánh vài trận, xem độ bền giảm trong rương đồ.

## Success Criteria
- [ ] Item cũ nạp lên có độ bền 80; relog giữ đúng số đã mòn.
- [ ] Thắng −1, thua/xin thua −2 cho mọi món pet đang mặc; rời map giữa trận không trừ.
- [ ] Món về 0: chỉ số riêng của món mất ngay (xem màn pet), bonus set vẫn còn; sửa xong chỉ số về lại.
- [ ] Món không phải trang bị pet (skin, cánh, ngọc, vật phẩm) không bị ảnh hưởng.

## Risk Assessment
- JSON `items` lớn thêm 1 field/item — nhỏ, chấp nhận.
- Nếu `JsonAdapter` có setting bỏ field lạ/ghi đè default → kiểm lại bằng test round-trip.
- `win()` chạy trên thread tick map; `applyInfo` đã được gọi ở luồng khác (mặc đồ) → giữ như
  hiện trạng, không thêm khoá mới.
