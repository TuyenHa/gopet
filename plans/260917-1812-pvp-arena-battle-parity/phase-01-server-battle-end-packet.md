---
phase: 1
title: 'Server: gói kết thúc trận PvP & đấu trường'
status: completed
priority: P1
effort: 4h
dependencies: []
---

# Phase 1: Server — gói kết thúc trận PvP & đấu trường

## Overview

Sửa `PetBattle.win()` để mọi trận PvP đều phát `PET_BATTLE_STATE`, và sửa
`sendFastRemove()` để gói huỷ trận khớp được `battleId` của **cả hai** bên.

## Key Insights

- `win()` (`Data/Battle/PetBattle.cs:731`) chia 3 nhánh. Nhánh PvE và nhánh PvP-PK
  đều gọi `win(texts, coin, exp)`. Nhánh PvP thường (`else` ở dòng ~875) **chỉ gọi
  khi `price > 0`** — trận đấu trường có `coinBet = 0` (`Place/ArenaPlace.cs:38`)
  nên rơi thẳng xuống, không gửi gì.
- Hệ quả: `hadFinished = true`, `hasWinner()` true, `GopetPlace.update()`
  (`Place/GopetPlace.cs:555`) dọn `PetBattle` — nhưng client không nhận tín hiệu
  nào. Trận "biến mất" im lặng.
- `sendFastRemove()` (`PetBattle.cs:1698`) luôn ghi `activePlayer.user.user_id`.
  Trong PvP, `sendStartFightPlayer()` (`:479`) cấp cho mỗi bên một `battleId` bằng
  userId của **chính họ** — nên bên passive không bao giờ khớp gói này.
- `win(texts, coin, exp)` (`:888`) đã gửi đúng 2 gói cho PvP. Chỉ cần gọi nó.

## Requirements

**Functional**
- Trận đấu trường kết thúc → cả 2 người chơi nhận `PET_BATTLE_STATE` với
  `battleId` riêng của mình và `winnerId` đúng.
- `sendFastRemove()` trong PvP phát 2 gói (một `battleId` mỗi bên).
- Không đổi hành vi của PvE, PK, và thách đấu có cược.

**Non-functional**
- Không thay đổi wire format của `PET_BATTLE_STATE` (opcode 16) — jar cũ vẫn đọc được.
- Không phát sinh gói trùng: `hadFinished` guard phải giữ nguyên tác dụng.

## Architecture

```
PetBattle.win()
├── petAttackMob == true        → PvE  (đã gửi, giữ nguyên)
└── petAttackMob == false
    ├── isPK == true            → PK   (đã gửi, giữ nguyên)
    └── isPK == false
        ├── price > 0           → thách đấu cược (đã gửi, giữ nguyên)
        └── price == 0  ◄── SỬA  → đấu trường: hiện KHÔNG gửi gì
```

Chỉ cần đưa `win(petBattleTexts.ToArray(), price, 0)` ra **ngoài** nhánh
`if (price > 0)`, giữ phần cộng ngọc bên trong. Không thêm field, không thêm opcode.

`sendFastRemove()` đổi từ 1 gói broadcast sang 2 gói khi `!petAttackMob`, cùng
pattern đã dùng ở `win(texts, coin, exp, battleId)` (`:903`).

## Related Code Files

- Modify: `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs`
  - `win()` — nhánh PvP không-PK (~dòng 872-886)
  - `sendFastRemove()` (~dòng 1698-1704)
- Read for context: `SRCGOPETGOC/GServer/Place/ArenaPlace.cs`,
  `SRCGOPETGOC/GServer/Place/GopetPlace.cs:466,555`

## Implementation Steps

1. Mở `PetBattle.cs`, đọc trọn `win()` từ dòng 731 đến 901 để nắm đủ 3 nhánh trước
   khi sửa.
2. Trong nhánh `else` (PvP không-PK), tái cấu trúc:
   - Giữ `if (price > 0) { ... winner.addCoin(totalPrice); ...onWinBetBattle(); }`
     cho phần thưởng cược.
   - Đưa `win(petBattleTexts.ToArray(), price, 0);` ra ngoài khối `if`, chạy cho
     mọi trận PvP không-PK.
3. Xác nhận `price` truyền vào gói khi đấu trường là `0` — client sẽ hiển thị
   "Ngọc: 0", đúng ngữ nghĩa (đấu trường thưởng bằng `AccumulatedPoint`, không
   phải ngọc). **Không** nhét điểm đấu trường vào field `coin`.
4. Nếu muốn báo điểm cho người thắng đấu trường: thêm một `Popup` vào
   `petBattleTexts` (field `Messages[]` đã có sẵn trên wire), **không** thêm field mới.
   Nguồn điểm hiện cộng ở `ArenaPlace.ArenaData.removeAllPlayer()` — cân nhắc giữ
   nguyên nơi cộng điểm, chỉ thêm text ở đây nếu làm được mà không phải truyền
   thêm state; nếu phải truyền state thì **bỏ qua bước này** (YAGNI).
5. Sửa `sendFastRemove()`:
   - PvE (`petAttackMob == true`): giữ nguyên 1 gói `activePlayer.user_id` broadcast.
   - PvP: phát 2 gói — `activePlayer.user_id` và `passivePlayer.user_id`. Gửi
     `place.sendMessage()` cho gói đầu (observer cần thấy), gói thứ hai gửi riêng
     `passivePlayer.session.sendMessage()` giống cách `sendPetAttack()` làm (`:356`).
6. Build: `dotnet build` trong `SRCGOPETGOC/GServer`. Nếu exe đang chạy khoá
   output, build ra thư mục temp (pattern đã dùng ở plan 260916).
7. Ghi lại packet dump trước/sau ở `plans/260917-1812-pvp-arena-battle-parity/reports/`.

## Todo List

- [ ] Đọc trọn `win()` 731-901
- [ ] Sửa nhánh PvP không-PK để luôn gửi `PET_BATTLE_STATE`
- [ ] Quyết định (và ghi lại) có thêm text điểm đấu trường hay không
- [ ] Sửa `sendFastRemove()` phát 2 gói cho PvP
- [ ] Build server 0 error
- [ ] Packet dump chứng minh cả 2 bên nhận gói kết thúc

## Success Criteria

- [ ] Trận đấu trường kết thúc (hết 2 phút hoặc 1 bên hp≤0) → packet dump cho thấy
      2 gói `PET_BATTLE_STATE` với `battleId` khác nhau.
- [ ] Trận thách đấu có cược vẫn cộng đúng `cược×2 − 10%` cho người thắng.
- [ ] Trận PK vẫn trừ exp/ngọc và đẩy người thua về Linh Thú thành như cũ.
- [ ] `FAST_REMOVE_MOB` trong PvP có 2 gói.
- [ ] Build server 0 error, 0 warning mới.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Gửi gói kết thúc 2 lần (đã có nhánh khác gọi) | Cao | `hadFinished` guard ở đầu `win()` đã chặn; thêm test đọc dump đếm số gói |
| Jar cũ nhận `coin=0` và hiển thị lạ | Thấp | Wire format không đổi; jar đã xử lý `coin=0` ở nhánh PK |
| Server đang chạy production bị gián đoạn | Trung bình | Build ra temp, deploy trong cửa sổ bảo trì; `AutoMaintenance.cs` có sẵn |
| Regression cho PvE | Trung bình | Không đụng nhánh `petAttackMob`; LiveSmoke `BattleChecks.cs` chạy lại |

## Security Considerations

- Không thêm input từ client → không có bề mặt tấn công mới.
- Giữ nguyên `hadFinished` để tránh một trận phát thưởng nhiều lần (duplication exploit).
- `ArenaPlace.ArenaData.removeAllPlayer()` cộng `AccumulatedPoint` — **không** được
  cộng thêm lần nữa trong `win()`.

## Next Steps

→ Phase 02 (client đóng overlay) để có lưới an toàn kể cả khi gói này rớt.
