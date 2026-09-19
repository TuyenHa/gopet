---
phase: 4
title: "Client net: parse chỉ số & gửi Xin thua"
status: pending
priority: P1
effort: "2h"
dependencies: [2]
---

# Phase 4: Client net: parse chỉ số & gửi Xin thua

## Overview
Tầng mạng Unity đọc `PET_BATTLE_STATS` và gửi `PET_BATTLE_SURRENDER`.

## Requirements
- Parse đúng wire format phase 2, `ExpectFullyConsumed`, giới hạn count như các gói khác.
- Gói stats có thể đến sau `BattleStarted` → view phải cập nhật được sau khi đã dựng.

## Architecture
- `BattleModels.cs`: thêm `BattleActorStats { ActorId, Level, Atk, Def, CritPermille,
  BattleSkillCost[] Skills }`, `BattleStatsState { BattleId, Actors }`.
- `BattleHandler`: `event Action<BattleStatsState> StatsReceived`, `OnStats`, và
  `SendSurrender()`. `BattleHandler.cs` đang 226 dòng → tách parse gói phụ (buff + stats)
  sang `Net/Battle/BattleAuxPacketReader.cs` để về < 200.
- `GopetCmd.cs`: 2 hằng số khớp server.

## Related Code Files
- Modify: `Assets/Scripts/Net/GopetCmd.cs`, `Net/Battle/BattleHandler.cs`, `Net/Battle/BattleModels.cs`
- Create: `Assets/Scripts/Net/Battle/BattleAuxPacketReader.cs`

## Implementation Steps
1. Hằng số + model.
2. Tách `OnBuffState` sang reader mới, thêm `ReadStats`.
3. `RegisterSub(..., PET_BATTLE_STATS, OnStats)`; `SendSurrender`.
4. Chạy `verify.ps1` (compile + unit test).

## Success Criteria
- [ ] Unit test parse (phase 6) xanh; gói thiếu/thừa byte ném `ProtocolException`.
- [ ] `SendSurrender` tạo đúng byte `PET_SERVICE, PET_BATTLE, SURRENDER`.
- [ ] Không file nào > 200 dòng.

## Risk Assessment
- Lệch giá trị sub id client/server → test so khớp hằng số trong LiveSmoke.
