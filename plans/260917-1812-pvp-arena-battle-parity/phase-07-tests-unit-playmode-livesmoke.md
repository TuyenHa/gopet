---
phase: 7
title: 'Tests: unit + PlayMode + LiveSmoke'
status: completed
priority: P1
effort: 1d
dependencies:
  - 1
  - 2
  - 3
  - 5
  - 6
---

# Phase 7: Tests — unit, PlayMode và LiveSmoke trọn vòng đời PvP

## Overview

`PvpBattleChecks.cs` hiện dừng ngay khi cả hai bên nhận `PLAYER_BATTLE` — tức là
chỉ chứng minh **mở** được trận, không chứng minh **kết thúc** được. Đó chính là
lý do lỗi đấu trường lọt lưới. Phase này kéo dài test tới hết vòng đời và bổ sung
test cho các thành phần mới.

## Key Insights

- Hạ tầng test đã có đủ 3 tầng:
  - `tests/Gopet.Net.Tests/BattleHandlerTests.cs` — parse wire format.
  - `Assets/Tests/PlayMode/BattlePlayModeTests.cs` — view/vòng đời trong Unity.
  - `tests/Gopet.Net.LiveSmoke/{BattleChecks,PvpBattleChecks}.cs` — server thật.
- LiveSmoke cần server nghe `127.0.0.1:19180`; 2 account `gopetpvp1`/`gopetpvp2`
  đã tạo sẵn (tạo thẳng DB vì `REGISTER` bị khoá server-side).
- Plan `260913` ghi nhận PlayMode batch từng bị chặn bởi mutex
  `Unity.Licensing.Client` → phải xử lý hoặc ghi rõ là chưa chạy được. **Không**
  được ghi "pass" khi chưa có XML kết quả.

## Requirements

**Functional**
- LiveSmoke PvP đi hết: mở trận → trao đổi ít nhất 2 lượt → một bên thắng →
  **cả hai** nhận `PET_BATTLE_STATE` đúng `battleId` và `winnerId`.
- Test riêng cho nhánh đấu trường (coinBet = 0) vì đó là ca đã hỏng.
- Unit test `SkillCooldownTracker` và parse gói buff mới.
- PlayMode test: overlay đóng khi `MapUpdated`, khi timeout, khi `FAST_REMOVE` tới
  từ phía passive; spectator view không rò rỉ GameObject.

**Non-functional**
- **Không** dùng mock/fake data để ép test xanh. Nếu không chạy được, ghi rõ
  "chưa chứng nhận" thay vì bịa kết quả.

## Architecture

```
tests/Gopet.Net.Tests/          – thuần C#, chạy trong CI, nhanh
Assets/Tests/PlayMode/          – cần Unity, chạy qua run-playmode-tests.ps1
tests/Gopet.Net.LiveSmoke/      – cần server thật, chạy tay trước khi chứng nhận
```

Thêm `ArenaBattleChecks.cs` bên cạnh `PvpBattleChecks.cs` thay vì nhồi vào file cũ
(file cũ đã 157 dòng).

## Related Code Files

- Create: `GopetUnityClient/tests/Gopet.Net.LiveSmoke/ArenaBattleChecks.cs`
- Create: `GopetUnityClient/tests/Gopet.Net.Tests/SkillCooldownTrackerTests.cs`
- Modify: `GopetUnityClient/tests/Gopet.Net.LiveSmoke/PvpBattleChecks.cs` (kéo dài tới hết trận)
- Modify: `GopetUnityClient/tests/Gopet.Net.Tests/BattleHandlerTests.cs` (gói buff mới)
- Modify: `GopetUnityClient/Assets/Tests/PlayMode/BattlePlayModeTests.cs`
- Modify: `GopetUnityClient/verify.ps1` (nếu cần thêm gate)

## Implementation Steps

1. Đọc `PvpBattleChecks.cs`, `BattleChecks.cs`, `BattleHandlerTests.cs`,
   `BattlePlayModeTests.cs` trước khi sửa.
2. Kéo dài `PvpBattleChecks`: sau khi cả hai nhận `PLAYER_BATTLE`,
   - bên có lượt gửi `SendNormalAttack()`, bơm cả 2 socket, khẳng định **cả hai**
     nhận `PET_BATTLE` với `ActorId` đúng;
   - lặp đánh cho tới khi một bên hp<=0 hoặc quá 20 lượt (fail nếu vượt);
   - khẳng định cả hai nhận `PET_BATTLE_STATE`, `WinnerId` giống nhau, `BattleId`
     khác nhau và khớp userId từng bên.
3. `ArenaBattleChecks.cs`: khó ép sự kiện đấu trường chạy theo giờ. Ba lựa chọn —
   chọn cái khả thi và ghi rõ lý do vào report:
   - (a) dùng `CommandManager`/`ZoneCommand` nếu có lệnh admin ép chạy `ArenaEvent`;
   - (b) nếu không có, test ở mức thấp hơn: ép `startFightPlayer(..., false, 0)` qua
     một lệnh admin, rồi khẳng định `PET_BATTLE_STATE` vẫn tới cả 2 bên;
   - (c) nếu cả hai không làm được, ghi "chưa chứng nhận tự động" và chứng minh
     bằng packet dump thủ công của phase 01.
   **Không** giả lập kết quả trong bất kỳ lựa chọn nào.
4. `SkillCooldownTrackerTests`: đếm đủ 3 lượt, re-sync khi server từ chối, nhiều
   skill song song, skill không tồn tại.
5. `BattleHandlerTests`: thêm ca parse gói buff — đúng layout; `buffCount` vượt
   ngưỡng phải ném `ProtocolException`; byte thừa phải fail `ExpectFullyConsumed`.
6. `BattlePlayModeTests`: 4 ca mới — đóng theo `MapUpdated`, đóng theo timeout,
   `FAST_REMOVE` khớp `OpponentActorId`, spectator không rò rỉ GameObject.
7. Xử lý mutex `Unity.Licensing.Client` cho PlayMode batch; nếu vẫn bị chặn, ghi
   trạng thái thật vào report kèm cách khắc phục đã thử.
8. Chạy đủ: `verify.ps1`, `run-playmode-tests.ps1`, LiveSmoke. Ghi kết quả thật
   (kể cả fail) vào `plans/260917-1812-pvp-arena-battle-parity/reports/`.

## Todo List

- [ ] Đọc 4 file test hiện có
- [ ] Kéo dài `PvpBattleChecks` tới `PET_BATTLE_STATE` cả 2 bên
- [ ] `ArenaBattleChecks.cs` (hoặc ghi rõ lý do không tự động hoá được)
- [ ] `SkillCooldownTrackerTests`
- [ ] Ca parse gói buff trong `BattleHandlerTests`
- [ ] 4 ca PlayMode mới
- [ ] Xử lý/ghi nhận mutex Unity Licensing
- [ ] Report kết quả thật vào `reports/`

## Success Criteria

- [ ] LiveSmoke PvP xanh, chứng minh trận **kết thúc** đúng, không chỉ mở được.
- [ ] Nhánh đấu trường (coinBet=0) có bằng chứng — tự động hoặc dump thủ công có ghi chú.
- [ ] Toàn bộ net tests pass (hiện 703, phải tăng chứ không giảm).
- [ ] PlayMode pass, hoặc ghi rõ lý do không chạy được kèm cách khắc phục.
- [ ] `verify.ps1` 10/10.
- [ ] Không test nào bị skip/ignore để làm xanh build.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Không ép được `ArenaEvent` chạy theo yêu cầu | Cao | 3 phương án ở bước 3; phương án cuối là dump thủ công **có ghi chú trung thực** |
| Đánh tới chết mất nhiều lượt (25s/lượt) | Trung bình | Dùng account pet yếu, hoặc chỉnh `TimeNextTurn` trên server local (**không commit**) |
| PlayMode vẫn bị mutex chặn | Trung bình | Đã là vấn đề đã biết của plan 260913; escalate nếu chặn chứng nhận |
| Test dài làm CI chậm | Thấp | LiveSmoke vốn chạy tay, không nằm trong `verify.ps1` |

## Security Considerations

- Không commit credential mới; chỉ dùng thứ đã tồn tại trong `PvpBattleChecks.cs`.
- Không commit thay đổi tuning server (`TimeNextTurn`, `TIME_ATTACK`) dùng để rút
  ngắn thời gian test.

## Next Steps

→ Phase 08 chốt docs và sửa nhãn sai ở plan parity.
