---
title: 'PvP, Đấu trường & Vượt ải — hoàn thiện parity battle'
description: >-
  Đóng các lỗi chặn và khoảng trống còn lại của hệ thống chiến đấu giữa server
  GServer và GopetUnityClient: trận đấu trường không gửi gói kết thúc,
  FAST_REMOVE chỉ khớp một bên, không xem được trận người khác, thiếu UI
  buff/cooldown, và mảng Vượt ải chưa được kiểm chứng.
status: pending
priority: P0
branch: master
tags:
  - unity
  - server
  - battle
  - pvp
  - arena
  - challenge-place
  - protocol
blockedBy: []
blocks:
  - 260913-jar-unity-parity-audit-and-certification
created: '2026-09-17T11:38:20.281Z'
createdBy: 'ck:plan'
source: skill
---

# PvP, Đấu trường & Vượt ải — hoàn thiện parity battle

## Overview

Audit ngày 2026-09-17 đối chiếu `SRCGOPETGOC/GServer` với `GopetUnityClient` cho
thấy tầng mạng battle của Unity đã đúng wire format, nhưng **vòng đời trận đấu
chưa đóng được** ở nhánh đấu trường, và một số trạng thái chiến đấu mà server tính
toán không hề đến được người chơi.

Plan này sửa **cả hai đầu**: sửa gốc ở server (gói kết thúc, battleId, đẩy trạng
thái buff/cooldown) và làm lưới an toàn ở client (đóng overlay theo vòng đời place).

### Bối cảnh — 3 đường vào trận PvP đều dồn về một hàm

`GopetPlace.startFightPlayer()` (`Place/GopetPlace.cs:466`) là điểm hội tụ của:

| Đường | Opcode vào | isPkMode | coinBet |
|---|---|---|---|
| Thách đấu | `PLAYER_CHALLENGE` (12) | false | >0 (min 2000) |
| PK | `PET_SERVICE/PLAYER_PK` (96) | true | 0 |
| Đấu trường | sự kiện tự ghép cặp | **false** | **0** |

Đấu trường rơi đúng vào ô `isPkMode=false, coinBet=0` — ô duy nhất mà
`PetBattle.win()` (`Data/Battle/PetBattle.cs:875`) **không gửi gói nào**.

### Definition of Done

- Mọi trận PvP (thách đấu / PK / đấu trường) đều kết thúc bằng `PET_BATTLE_STATE`
  tới **cả hai** người chơi, đúng `battleId` của từng bên.
- Overlay battle của Unity không bao giờ treo lại sau khi trận kết thúc hoặc
  người chơi bị chuyển place — kể cả khi mất gói.
- Người chơi thấy được trận của người khác trong cùng place (đấu trường 5 cặp).
- Buff/debuff/stun/cooldown mà server đang tính đều hiển thị được trên client.
- Vượt ải (`ChallengePlace`) chạy end-to-end trên Unity, có bằng chứng.
- Jar cũ (`ApplicationVersion <= 1.4.2`) **không nhận** bất kỳ opcode mới nào.

### Nguyên tắc ràng buộc

- **Không đổi wire format của opcode cũ.** Opcode mới phải gate theo
  `Player.ApplicationVersion` (jar báo ≤1.4.2, Unity báo 1.4.3 — xem
  `Server/Player.cs:109` và `Assets/Scripts/Net/Auth/ClientInfo.cs:19`).
- **Damage luôn do server quyết định.** Client chỉ render, không tự tính lại.
- **YAGNI**: cooldown là hằng số 3 lượt (`GopetManager.MAX_SKILL_COOLDOWN`) — cân
  nhắc suy phía client trước khi thêm packet (xem phase 04).

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Server: gói kết thúc trận PvP & đấu trường](./phase-01-server-battle-end-packet.md) | Completed |
| 2 | [Client: đóng overlay an toàn theo place](./phase-02-client-battle-overlay-teardown.md) | Completed |
| 3 | [Client: xem trận người chơi khác trong world](./phase-03-client-spectator-battles.md) | Implemented; Unity visual/PlayMode validation blocked (2026-09-25) |
| 4 | [Server: đẩy buff/debuff & cooldown skill](./phase-04-server-buff-cooldown-packet.md) | Completed |
| 5 | [Client: UI buff/debuff/stun & cooldown](./phase-05-client-buff-cooldown-ui.md) | Completed |
| 6 | [Vượt ải: kiểm chứng và đóng gap](./phase-06-challenge-place-vuot-ai.md) | Pending |
| 7 | [Tests: unit + PlayMode + LiveSmoke](./phase-07-tests-unit-playmode-livesmoke.md) | Completed |
| 8 | [Docs & đồng bộ plan parity](./phase-08-docs-and-parity-sync.md) | Completed |

### Thứ tự phụ thuộc

```
01 ──► 02 ──► 07
       ▲
03 ────┘
04 ──► 05 ──► 07
06 ──────────► 07 ──► 08
```

Phase 01+02 là cặp lỗi chặn, làm trước. Phase 03 và 04/05 độc lập nhau. Phase 06
chỉ cần 02 xong (vượt ải dùng chung overlay). Phase 07 gom test, 08 chốt docs.

## Dependencies

- **blocks** `260913-jar-unity-parity-audit-and-certification` — plan đó đang ghi
  "Battle | PvE/PvP turn ... | **Hoàn thành về code**" (`plan.md:56`). Nhãn đó sai
  với các lỗi tìm được ở đây; không được cấp `certified` cho mảng battle cho tới
  khi plan này xong. Phase 08 cập nhật lại bảng trạng thái của plan kia.
- Không blockedBy plan nào.

## Môi trường

- Server: `D:\game\SRCGOPETGOC\GServer` (.NET 8, build ra temp khi exe đang chạy).
- Client: `D:\game\GopetUnityClient` (`verify.ps1`, `run-playmode-tests.ps1`).
- LiveSmoke cần server nghe ở `127.0.0.1:19180` + 2 account `gopetpvp1`/`gopetpvp2`
  (đã tồn tại, xem `tests/Gopet.Net.LiveSmoke/PvpBattleChecks.cs`).
