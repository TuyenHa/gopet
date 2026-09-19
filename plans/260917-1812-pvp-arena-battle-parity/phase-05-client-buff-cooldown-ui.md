---
phase: 5
title: 'Client: UI buff/debuff/stun & cooldown'
status: completed
priority: P2
effort: 1d
dependencies:
  - 4
---

# Phase 5: Client — UI buff/debuff/stun, cooldown skill & auto-recovery

## Overview

Tiêu thụ gói buff của phase 04, tự đếm cooldown 3 lượt, và nối `SetAutoRecovery`
(đã viết nhưng chưa có UI nào gọi) vào màn hình.

## Key Insights

- `BattleHandler.SetAutoRecovery` (`Assets/Scripts/Net/Battle/BattleHandler.cs:43`)
  gửi `PET_RECOVERY_HP` (45) — code đúng, nhưng `grep` toàn repo không có call site
  nào. Tính năng chết.
- Nút skill hiện chỉ khoá theo MP (`BattleView.cs:154`, `UnlockActions` `:180`).
  Cooldown server là 3 lượt cố định → client đếm được.
- `BattleView` đang 196 dòng — sát trần 200 dòng của `development-rules.md`.
  **Bắt buộc tách** trước khi thêm, không nhồi tiếp.
- `BattleTurn.ActorId` cho biết ai vừa đi → dùng để đếm lượt tụt cooldown.

## Requirements

**Functional**
- Icon/nhãn buff & debuff hiện trên thẻ pet của cả 2 bên, kèm số lượt còn lại.
- Pet bị stun có chỉ báo rõ ràng.
- Nút skill hiển thị "Còn N lượt" và bị khoá khi đang cooldown.
- Có công tắc bật/tắt tự hồi HP, gửi `PET_RECOVERY_HP`, nhớ trạng thái giữa các phiên.

**Non-functional**
- Mỗi file mới < 200 dòng.
- UI hoạt động ở cả overlay của mình; spectator view (phase 03) chỉ cần hiện buff,
  không cần nút.

## Architecture

Tách `BattleView` thành:

```
BattleView.cs           – dựng khung, vòng đời, route (giữ <200 dòng)
BattleView.Actions.cs   – partial: nút Đánh/Kỹ năng/Vật phẩm + cooldown + khoá
BattleBuffStrip.cs      – component hiển thị dải buff trên 1 thẻ pet
SkillCooldownTracker.cs – state thuần (không MonoBehaviour), dễ unit test
```

`SkillCooldownTracker`:
- `MarkUsed(skillId)` → `remaining[skillId] = 3`
- `OnTurnAdvanced()` → giảm mọi giá trị 1, gọi khi nhận `BattleTurn` mà
  `ActorId == LocalPet.ActorId` (lượt của mình vừa kết thúc).
- `IsReady(skillId)` / `TurnsLeft(skillId)`
- Nguồn chân lý vẫn là server: nếu server trả dialog đỏ "Chưa hồi kỹ năng xong",
  đặt lại `remaining[skillId] = 3` để đồng bộ lại.

Auto-recovery: thêm dòng vào `SettingsView` (đã có pattern toggle auto-attack ở
`Assets/Scripts/Runtime/UI/SettingsView.cs:50`), lưu bằng `PlayerPrefs` như các
toggle khác, gửi packet khi đổi và một lần sau khi login.

## Related Code Files

- Create: `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.Actions.cs`
- Create: `GopetUnityClient/Assets/Scripts/Runtime/World/BattleBuffStrip.cs`
- Create: `GopetUnityClient/Assets/Scripts/UiLogic/SkillCooldownTracker.cs`
- Create: `GopetUnityClient/Assets/Scripts/Net/Battle/BattleBuffState.cs` (model + parse)
- Modify: `Assets/Scripts/Net/Battle/BattleHandler.cs` (đăng ký opcode buff mới)
- Modify: `Assets/Scripts/Net/Battle/BattleModels.cs`
- Modify: `Assets/Scripts/Runtime/World/BattleView.cs`, `BattlePetCard.cs`
- Modify: `Assets/Scripts/Runtime/UI/SettingsView.cs`, `Assets/Scripts/Runtime/World/GameSession.cs`
- Modify: `Assets/Scripts/Net/Auth/ClientInfo.cs` (`Version = "1.5.0"`)

## Implementation Steps

1. Nâng `ClientInfo.Version` lên `"1.5.0"` — **bắt buộc**, nếu không server phase 04
   sẽ không gửi gói buff. Kiểm tra lại `Player.cs` ngưỡng chặn login vẫn là 1.4.2.
2. Thêm model `BattleBuffState { int ActorId; BuffEntry[] Entries }` +
   `BuffEntry { int TypeId, Value; int TurnsLeft }` vào `BattleBuffState.cs`.
3. `BattleHandler`: `RegisterSub(PET_SERVICE, PET_BATTLE_BUFF, OnBuffState)` với
   guard `Count()` sẵn có (actorCount ≤ 2, buffCount ≤ 32) + `ExpectFullyConsumed`.
   Bắn event `BuffStateReceived`.
4. Tách `BattleView` thành partial như Architecture. **Đọc file trước khi sửa.**
5. `BattleBuffStrip`: nhận `BattleBuffState`, render tối đa 6 icon + "+N" nếu dư.
   Nhãn lấy từ bảng ánh xạ `ItemInfo.Type` mà phase 04 ghi ra report. Type lạ →
   hiện nhãn chung, không crash.
6. `SkillCooldownTracker` + unit test (thuần C#, chạy trong `tests/Gopet.Net.Tests`).
7. Nút skill: `interactable = MP đủ && tracker.IsReady(id)`; nhãn phụ "Còn N lượt".
8. Toggle auto-recovery ở `SettingsView` + `GameSession` nối vào
   `_battleHandler.SetAutoRecovery`, lưu `PlayerPrefs`.
9. `verify.ps1` + PlayMode + net tests.

## Todo List

- [ ] `ClientInfo.Version = "1.5.0"`
- [ ] Model + parse gói buff (có guard)
- [ ] Tách `BattleView` thành partial, mọi file <200 dòng
- [ ] `BattleBuffStrip` + bảng nhãn
- [ ] `SkillCooldownTracker` + unit test
- [ ] Nút skill khoá theo cooldown, nhãn "Còn N lượt"
- [ ] Re-sync cooldown khi server trả dialog đỏ
- [ ] Toggle auto-recovery + `PlayerPrefs` + gửi sau login
- [ ] `verify.ps1` / PlayMode / net tests pass

## Success Criteria

- [ ] Bị stun → thẻ pet hiện chỉ báo, hết lượt thì mất.
- [ ] Trúng độc → thấy buff độc kèm số lượt, HP tụt đúng mỗi lượt.
- [ ] Bấm skill → nút khoá, hiện "Còn 3 lượt", mở lại sau đúng 3 lượt của mình.
- [ ] Bật auto-recovery → server nhận `PET_RECOVERY_HP`, pet tự hồi; tắt thì dừng.
- [ ] Trạng thái toggle còn nguyên sau khi thoát/vào lại.
- [ ] Không file nào trong phase này vượt 200 dòng.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Client đếm cooldown lệch server | Trung bình | Re-sync khi nhận dialog đỏ; server vẫn là chân lý, lệch chỉ gây khoá thừa/thiếu 1 lượt |
| Tách `BattleView` gây regression | Cao | Tách trước, chạy đủ test, commit riêng, rồi mới thêm tính năng |
| `ItemInfo.Type` mới xuất hiện sau này | Thấp | Nhãn fallback, không throw |
| Nâng version 1.5.0 làm server cũ từ chối login | Cao | Xác minh `Player.cs` chỉ so `>= VERSION_142`; test login lên server chưa có phase 04 |
| UI buff che mất pet trên màn nhỏ | Trung bình | Cap 6 icon + "+N"; kiểm ở 16:9 và 19.5:9 |

## Security Considerations

- Parse gói buff phải dùng `Count()` guard + `ExpectFullyConsumed` như mọi handler
  khác — chặn gói dị dạng làm treo reader.
- Cooldown phía client **không** phải cơ chế bảo mật; server vẫn phải từ chối skill
  chưa hồi. Không được bỏ check ở server.
- `PlayerPrefs` chỉ lưu bool, không lưu gì nhạy cảm.

## Next Steps

→ Phase 07 bổ sung test cho tracker và luồng buff.
