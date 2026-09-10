---
phase: 7
title: Pet equipment 5-slot + Gem + Cường/Tiến hoá
status: completed
priority: P2
effort: 4-5d
dependencies:
  - 1
---

# Phase 7: Pet equipment 5-slot + Gem + Cường/Tiến hoá

> **BACKLOG CLOSED (2026-09-10).**
> - Slot rỗng picker → gửi `RequestNormalInventory` (NORMAL_INVENTORY sub 30 của PET_SERVICE) → server bơm menu → GenericMenuView render → user chọn → server auto-equip. Không cần picker view riêng.
> - `EnchantEvolveView` — modal chọn 2 material (matA + matB/crystal) cho cả cường/tiến hoá. Dev-mode: InputField itemId trực tiếp. Wire vào `ConfirmEnchant` (opcode 48) / `UpTierItem` (opcode 49).
> - `YesNoDialog` reusable + wire vào Destroy → gửi `RequestDestroyEquip` (PET_SERVICE / 56 / int). Server pop YN dialog xác nhận thêm.
> - `PetEquipPackets` bổ sung: `RequestPetInventory`, `RequestNormalInventory`, `RequestGemInventory`, `RequestDestroyEquip`.
> - Còn hạn chế: modal chọn material dùng InputField itemId (không phải picker từ túi có visual). Đủ để test end-to-end, cần replace bằng picker chuẩn khi có UX polish.

> **DONE — packets + handler + UI 5-slot.**
>
> **Đã có:**
> - `Assets/Scripts/Net/Pet/PetEquipPackets.cs` — 14 gói client (equip/unequip/gem/enchant/uptier + query).
> - `Assets/Scripts/Net/Pet/PetEquipEvents.cs` + `PetEquipHandler.cs` — parse EQUIP_INFO recv.
> - `Assets/Scripts/Runtime/UI/PetEquipSlot.cs` — ô 64×64 icon+level+gem-dot.
> - `Assets/Scripts/Runtime/UI/PetEquipView.cs` — panel 5 slot dọc (Nón/Vũ khí/Giáp/Giày/Bao tay).
> - `Assets/Scripts/Runtime/UI/PetSlotActionsView.cs` — menu tap slot đầy (Tháo/Gắn ngọc/Cường/Tiến/Huỷ).
> - Menu char thêm mục "Trang bị pet" → gửi `RequestEquipInfo(selfUserId)` → view mở khi handler nhận.
> - 5 test cho handler (byte-for-byte theo `GameController.cs:1825-1923`).
>
> **Chưa có (sẽ mở khi cần):**
> - **Slot rỗng → picker item từ túi:** cần handler `PET_INVENTORY` để liệt kê item mặc được.
>   Hiện tap slot rỗng chỉ log.
> - **Modal 3-mat cường / 2-mat tiến hoá:** hiện Enchant/UpTier gửi (itemId, itemId) placeholder.
>   Cần popup chọn nguyên liệu thật.
> - **Huỷ đồ:** thiếu opcode confirm rõ ràng, hiện là no-op.
> - **Preview stat tổng** khi hover slot: chỉ hiện Str/Agi/Int pet header, không breakdown mỗi item.
>
> Tổng: 5 slot render thật đủ để nhìn đồ đang mặc + tháo. Đủ cho parity Phase 1 với jar.


## Overview

`fu.java` (763 dòng) là UI trang bị pet của jar: 5 slot cố định + socket gem + cường hoá + tiến hoá + huỷ trang bị. Đây là **UI chức năng dày** — dựng đúng thì gần như xong toàn bộ vòng lặp "farm → trang bị → nâng cấp" của game.

Báo cáo cũ [`analysis-260908-2209-linh-thu-city-items.md`](../reports/analysis-260908-2209-linh-thu-city-items.md) §3 đã spec chi tiết opcode.

## Requirements

**Functional:**
- Panel Trang bị Pet với **5 slot** (Mũ / Giáp / Vũ khí / Giày / Bao tay).
- **Tháo trang bị** khỏi pet → opcode 39.
- **Huỷ trang bị** → opcode 56.
- **Gắn ngọc** → opcode 73 · **Tháo ngọc** → 75 · **Tháo nhanh** → 78.
- **Cường hoá** (3 nguyên liệu) → opcode 46/76.
- **Tiến hoá** (2 nguyên liệu) → opcode 49/79.
- Tooltip stat mỗi món (số từ server qua `dn.a[]/b[]`).

**Non-functional:**
- Panel 400×300 hoặc lớn hơn — không squeeze vào chat overlay.
- Preview sprite item load từ `RemoteAssetCache`.

## Architecture

- Reuse `MessageRouter` register opcode 91 (envelope) — sub-command trang bị pet chạy trong họ 91 (`dc.java:92-155`).
- `PetEquipView` — grid 5 slot + right panel (chi tiết + hành động).
- Item picker riêng (dropdown/list) khi tap slot rỗng — hiện danh sách item phù hợp trong túi.
- Cường/Tiến hoá là **flow modal** riêng (chọn slot → chọn nguyên liệu → confirm).

## Related Code Files

**Create:**
- `Assets/Scripts/Net/Pet/PetEquipPackets.cs` — gửi opcode 91 sub 3/14/15/16 (equip/unequip/enchant/evolve). Sub cụ thể xem jar.
- `Assets/Scripts/Net/Pet/PetEquipHandler.cs` — nhận state trang bị pet + inventory.
- `Assets/Scripts/Net/Pet/GemPackets.cs` — 73/75/78.
- `Assets/Scripts/Net/Pet/EnhancePackets.cs` — 46/76 cường; 49/79 tiến.
- `Assets/Scripts/Runtime/UI/PetEquipView.cs` — 5 slot grid.
- `Assets/Scripts/Runtime/UI/PetEquipSlot.cs` — 1 slot (image + level badge).
- `Assets/Scripts/Runtime/UI/ItemPickerView.cs` — chọn item từ túi.
- `Assets/Scripts/Runtime/UI/EnhanceView.cs` — modal cường hoá.
- `Assets/Scripts/Runtime/UI/EvolveView.cs` — modal tiến hoá.
- `Assets/Scripts/Runtime/UI/GemSocketView.cs` — quản lý gem.

**Modify:**
- `Assets/Scripts/UiLogic/CharacterMenuActions.cs` — thêm "Trang bị pet" (mở `PetEquipView`).
- `Assets/Scripts/Runtime/World/GameSession.cs` — register handler.
- Có thể cần thêm handler cho `PET_INVENTORY` (nếu chưa có).

**Read (context):**
- `client.jar_Decompiler.com/fu.java` — toàn bộ UI.
- `client.jar_Decompiler.com/r.java` — cường/tiến hoá.
- `client.jar_Decompiler.com/dn.java` — DTO stat name/value pairs.
- `client.jar_Decompiler.com/dj.java` — icon bảng.
- `plans/reports/analysis-260908-2209-linh-thu-city-items.md` §3.
- `SRCGOPETGOC/GServer/Data/Pet/PetData.cs` (nếu có) — state pet server.

## Implementation Steps

1. **Xác định sub-command pet equipment trong opcode 91** — grep `case 91` server:
   ```
   grep -n "case 91\|WEARING_PET\|EQUIP_PET" GServer/Server/GameController.cs
   ```
   Đối chiếu jar `dc.java:92-155` (sub 3/14/15/16/26/27).
2. **`PetEquipPackets`** — hàm cho từng thao tác: `Equip(slot, itemId)`, `Unequip(slot)`, `Destroy(itemId)`.
3. **`PetEquipHandler`** — parse gói server trả:
   - List item đang mặc (5 slot).
   - List item trong túi.
   - Bắn event `EquipmentUpdated(EquipmentState)`.
4. **`PetEquipView` layout** — 400×300 panel:
   - Trái: 5 ô slot 64×64 dọc (Nón/Giáp/Vũ khí/Giày/Bao tay).
   - Giữa: preview pet với item đang mặc.
   - Phải: text stat tổng.
5. **Tap slot** → hiện menu: [Chi tiết] [Tháo] [Gắn ngọc] [Cường hoá] [Tiến hoá] [Huỷ] (dựa `fu.java:232-241`).
6. **`ItemPickerView`** — cần `PET_INVENTORY` (opcode?) — hiện chưa register. Đăng ký thêm ở Phase này.
7. **`EnhanceView`** — chọn 3 nguyên liệu → confirm → gửi 46 → recv 76 (kết quả).
8. **`EvolveView`** — chọn 2 nguyên liệu → 49 → 79.
9. **`GemSocketView`** — hiển thị socket, tap socket rỗng chọn ngọc từ túi.
10. **Sprite loader** — `RemoteAssetCache` request path theo item ID (server trả path string trong gói inventory).
11. **PlayModeTest**:
    - Bơm gói state trang bị fake → verify 5 slot render đúng.
    - Bấm Cường → verify byte gửi đi.
    - Bơm gói kết quả Cường thành công → verify slot update level.

## Success Criteria

- [ ] Menu char → "Trang bị pet" mở `PetEquipView` 5 slot.
- [ ] Slot rỗng bấm mở `ItemPickerView` từ túi.
- [ ] Tháo/Huỷ/Gắn/Tháo ngọc gửi đúng byte (diff jar dump).
- [ ] Cường hoá + Tiến hoá modal chạy đủ flow — thành công update slot, thất bại hiện toast.
- [ ] Tooltip stat khớp `dn.a[]` server trả về.
- [ ] Không file nào vượt 200 dòng (chia 6-7 file như trên).

## Risk Assessment

- **R1: Sub-command 91 chồng chéo với chat bang (Phase 5)** — cùng envelope 91, khác sub. Đảm bảo `RegisterSub` không double-register.
- **R2: `PET_INVENTORY` gói format nặng (icon path + stat + level cho từng item)** — cần parse cẩn thận, dễ throw `ProtocolException`.
- **R3: Cường hoá có xác suất thất bại làm mất nguyên liệu** — cảnh báo rõ trước Submit; test 20 lần trên smoke account rồi mới release.
- **R4: Tiến hoá đổi ID item** — sau tiến hoá slot có ID khác; UI phải refresh không cache stale.

## Rollout Notes

- Đây là phase dài nhất trong plan — cân nhắc chia 2 lần merge: (a) view + equip/unequip, (b) enhance/evolve/gem.
- Sau phase này gameplay loop "chơi lâu" mới đủ nội dung — kéo user back trở lại.
