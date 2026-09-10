---
title: >-
  Thành Phố Linh Thú — HUD + Character Menu + Pet + ATM + Pet Equipment (Unity
  parity)
description: >-
  Đóng khoảng cách Unity ↔ jar J2ME cho map hub `mapId 11` (Thành Phố Linh Thú).
  Thêm HUD số liệu thật, menu nhân vật, tương tác pet, Ngân hàng, trang bị pet
  5-slot; register các opcode 62/91/92/121 còn thiếu.
status: completed
priority: P1
branch: ''
tags:
  - unity
  - jar-parity
  - hud
  - character-menu
  - atm
  - pet-equipment
blockedBy:
  - 260905-1355-gopet-unity-client-rebuild
blocks: []
created: '2026-09-09T14:01:47.682Z'
createdBy: 'ck:plan'
source: skill
---

# Thành Phố Linh Thú — HUD + Character Menu + Pet + ATM + Pet Equipment (Unity parity)

## Overview

Đóng khoảng cách Unity ↔ jar cho map hub. Nguồn: [analysis-260909-2036-linh-thu-city-parity-jar-vs-unity.md](../reports/analysis-260909-2036-linh-thu-city-parity-jar-vs-unity.md).

**Phạm vi:**
- HUD hiển thị số liệu **thật** thay vì placeholder cứng.
- Nút "Menu" nhân vật mở 12 mục (Bạn bè, Hộp thư, Tủ quần áo, Chọn pet, Chat, Cài đặt, Đăng xuất, Thoát…).
- Cụm nút tương tác pet: Chơi/Hôn/Xoa + Hồi phục.
- Ngân hàng (ATM) UI 4 tab.
- Trang bị pet 5 slot + gem socket + cường hoá / tiến hoá.
- Register các opcode client jar dùng nhưng Unity còn thiếu: **62** (tủ quần áo), **91** (bang/trang bị nhiều sub), **92** (chat cộng đồng, chuyển kênh), **121** (bạn bè/thư).

**Ngoài phạm vi:**
- Portal warp + building shop-click → đã có ở `260907-2210-map-portal-warp-shop-interaction`.
- Battle core → phase-07 của `260905-1355-gopet-unity-client-rebuild`.
- Anti-cheat lớp cuối → phase-08 của `260905-1355-gopet-unity-client-rebuild`.

**Định hướng:** ưu tiên bám opcode server đã sinh sẵn (`GServer/Server/GopetCMD.cs`), không tự sinh opcode mới. Mọi menu server-driven đi qua `GenericMenuView` sẵn có; chỉ dựng view chuyên biệt cho Ngân hàng và Trang bị pet (2 UI có state phức tạp).

## Phases

| Phase | Name | Status | Priority | Notes |
|-------|------|--------|----------|-------|
| 1 | [HUD số liệu thật (stats/currency/level)](./phase-01-hud-s-li-u-th-t-stats-currency-level.md) | Completed | P0 | Star + 4 currency + Extras |
| 2 | [Nút Menu nhân vật + 12 mục](./phase-02-n-t-menu-nh-n-v-t-12-m-c.md) | Completed | P0 | 11 mục (jar 2 mục "Chat" gộp còn 1) |
| 3 | [Pet interaction UI (Cảm xúc + Hồi phục)](./phase-03-pet-interaction-ui-c-m-x-c-h-i-ph-c.md) | Completed | P0 | Radial 4 nút, cooldown 500ms |
| 4 | [Register opcode 121/62 + Bạn bè/Hộp thư/Tủ quần áo](./phase-04-register-opcode-121-62-b-n-b-h-p-th-t-qu-n-o.md) | Completed | P1 | LetterHandler + MailboxView; friend/wardrobe qua Guider generic |
| 5 | [Chat cộng đồng/bang + Đổi MK + Cài đặt (opcode 91/92)](./phase-05-chat-c-ng-ng-bang-i-mk-c-i-t-opcode-91-92.md) | Completed | P1 | Packets + ChatChannelSelector + ChangePasswordView 3-field |
| 6 | [Ngân hàng (ATM) 4-tab UI](./phase-06-ng-n-h-ng-atm-4-tab-ui.md) | Completed | P2 | Opcode 44 top-level → MENU_ATM Guider generic — không cần UI riêng |
| 7 | [Pet equipment 5-slot + Gem + Cường/Tiến hoá](./phase-07-pet-equipment-5-slot-gem-c-ng-ti-n-ho.md) | Completed | P2 | 14 packets + EQUIP_INFO handler + PetEquipView 5-slot + EnchantEvolveView + YesNoDialog + inventory picker |
| 8 | [Long-tail outline (mini-game/PVP/guild full)](./phase-08-long-tail-outline-mini-game-pvp-guild-full.md) | Completed | P3 | Packets đầy đủ (target-player/friend/guild/channel/language/mini-game); UI game engines out-of-scope |

## Thứ tự thực thi

- Phase 1–3 chạy **song song** được (đều nhắm HUD/character/pet, ít đụng file chung).
- Phase 4 chặn Phase 5 (5 dùng opcode 91/92, cùng gia đình khoảng cách nhỏ với 62/121).
- Phase 6 chặn nhau với Phase 4 ở chỗ chạm opcode 81 subcommand (không sub trùng, nhưng cùng envelope).
- Phase 7 độc lập, có thể chạy song song với Phase 6.
- Phase 8 chỉ là outline — mở khi các phase trước xong.

## Dependencies

- **blockedBy** `260905-1355-gopet-unity-client-rebuild` — cần tầng Net + UI generic + asset pipeline sẵn.
- **Bổ sung** cho `260907-2210-map-portal-warp-shop-interaction` — plan đó lo click-vào-building; plan này lo trục nhân vật (HUD + menu chính) và các flow không đi qua building (ATM, trang bị pet, bạn bè, thư).

## Ràng buộc

- Không thay đổi format byte trên dây — mọi opcode phải khớp `GServer/Server/GopetCMD.cs`.
- Không sinh test giả — luồng nào chưa có server response thật thì đánh dấu "cần LiveSmoke".
- Rule 200 dòng/file phải giữ; ngoại lệ ghi rõ ở phase liên quan.
