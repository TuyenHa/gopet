# Phase 01 — Vòng lượt đúng + khoá nút theo lượt

## Context Links

- Overview: [plan.md](plan.md)
- Report chính: `plans/reports/investigation-260919-1102-battle-screen-6-yeu-cau.md`
- Report bối cảnh jar: `plans/reports/investigation-260919-1102-danh-quai-jar-vs-unity.md`
- Doc giao thức: `docs/battle-system.md` §2 Vòng lượt

## Overview

- **Priority:** P1 (chặn phase 02, giá trị cao/công sức thấp)
- **Status:** done
- **Effort:** 2.5h
- Sửa 3 lỗi nhãn "Đến lượt bạn" + thay khoá nút timer 3.5s bằng khoá theo trạng thái lượt
  (mô hình `fr.b`/`fr.c` của jar). Đóng lỗ double-send nút kỹ năng.

## Key Insights

Đã verify lại trực tiếp trên source, KHÔNG chép từ report:

1. **`ActorId` = người vừa ra đòn, không phải người sắp đánh.**
   `PetBattle.cs:307` `sendPetAttack(turnEffects, TurnEffect.createNormalAttack(..., getUserTurnId()))`
   rồi `:308 nextTurn()`. Đường kỹ năng y hệt: `:1109` gửi, `:1110` `nextTurn()`.
   `sendPetAttack` ghi `mainTurnData.petId` vào wire (`:325`).
   → `isLocalTurn = ActorId != LocalPet.ActorId`.

2. **BẪY: dùng vật phẩm KHÔNG đổi lượt.**
   `PetBattle.cs:1643` `sendPetAttack(new(), TurnEffect.createWait(p.hp-oldHp, p.mp-oldMp, getUserTurnId()))`
   — `turnDatas` **rỗng** và **không có `nextTurn()`** sau đó (`:1644-1651` chỉ trừ item + gửi pet info).
   Nếu đảo vô điều kiện: gói này cho `isLocalTurn=false` → nhãn biến mất + nút khoá chết
   cho tới lượt quái. Phải nhận diện: `Type == BattleTurn.Wait && Effects.Length == 0` → **giữ nguyên lượt**.
   Đường kỹ năng cũng dùng `Wait` (`:1109`) nhưng **luôn có ≥1 effect** (`:1102` hoặc `:1106` đều `turnEffects.add`).

3. **Gói hệ thống `petId = -1`.** Độc (`PetBattle.cs:1617`) và phản đòn (`:1547`) gửi
   `new TurnEffect(TurnEffect.NONE, -1, 0, 0, 0)`. Chúng chạy trong `nextTurn()` (`:729-730`)
   nên tới ngay sau gói hành động. → `ActorId <= 0` thì **bỏ qua** phần lượt (vẫn apply damage).

4. **PvE không gửi cờ "ai đi trước" — và QUÁI ĐI TRƯỚC.**
   ĐÍNH CHÍNH sau review: bản đầu của mục này khẳng định người chơi đi trước — **sai**.
   Constructor PvE gọi `setIsActiveTurn(false)` (`PetBattle.cs:66`) ⇒ `getUserTurnId()`
   (`:426-428`) trả `mob.getMobId()`; `MobAttackTime` khởi tạo `= DateTime.Now` (`:33`) nên
   đã quá hạn ngay ⇒ nhánh `:671` của `update()` gọi `mobAttack()` ở tick đầu tiên.
   Nhánh drain hành động người chơi (`:675`) không chạy ở lượt 1.
   → gán `start.LocalStarts = false`; gói lượt đầu (của quái) lật `IsLocalTurn` sang true.

5. **Nút kỹ năng không nằm trong `_buttons`** (`BattleActionBar.cs:12,40,48,56` chỉ có
   surrender/potion/attack) → `Lock()` không chạm tới → double-send trong 1 lượt.

6. **`Unlock()` mở lại cả nút Xin thua** (`BattleActionBar.cs:151-155` duyệt `_buttons`
   vốn chứa `_surrenderBtn` ở `:40`) → `LockSurrender()` bị huỷ ở gói lượt kế tiếp.

7. **Trường "thời gian còn lại" của gói 37 PvE là RÁC (gộp vào phase này theo yêu cầu user).**
   PvP gửi `Utilities.round(delaTimeTurn - CurrentTimeMillis)` = ms còn lại (`PetBattle.cs:328`).
   PvE gửi `(int)(DateTime.Now - MobAttackTime).TotalSeconds` (`:332`), mà
   `MobAttackTime = DateTime.Now.AddSeconds(2)` (`:1337`, `:1401`) là mốc **tương lai**
   → hiệu số luôn **âm** (~-2). Không phải sai đơn vị giây/ms — là giá trị vô nghĩa.
   Gói mở trận 36 thì đúng (`:463-466`, `:523-526` đều dùng `delaTimeTurn`).
   Unity hiện **không đọc** `RemainingMs` ở đâu cả → sửa không đổi hành vi Unity,
   nhưng sửa đúng cho jar (jar vẽ pie lượt `remaining*360/max`, `ei.java:162-171`,
   đang nhận số âm nên pie hỏng). Không đổi wire format (vẫn 1 `int`).

8. **Ngân sách dòng.** `BattleView.cs` = 185 dòng, `BattleActionBar.cs` = 167,
   `BattleSkillPanel.cs` = 195. `verify.ps1` bước 10/10 chặn > 200.
   `BattleView.cs` KHÔNG có trong allowlist `CODE_HEALTH_EXCEPTIONS.md` → phải tách.

## Requirements

**Chức năng**
- R1: Lượt người chơi → dưới badge VS hiện "Đến lượt bạn"; lượt quái → chuỗi rỗng.
- R2: Mở trận PvE để trống (quái đi trước), rồi hiện "Đến lượt bạn" ngay khi quái đánh xong.
- R2b: Bên đang tới lượt bị định thân → server vẫn gửi gói lượt, nút không bị kẹt.
- R3: Gói độc/phản đòn (`ActorId <= 0`) không xoá nhãn.
- R4: Gói dùng vật phẩm không xoá nhãn và không khoá nút.
- R5: Nút Tấn công / Thuốc / mọi nút kỹ năng bị khoá khi (a) không phải lượt mình hoặc
  (b) đã gửi hành động và chưa nhận gói 37.
- R6: Nút "Xin thua" và "‹ Quay lại" **luôn bấm được** cho tới khi đã xin thua.
- R7: Gói 37 PvE mang đúng **ms còn lại của lượt**, cùng ngữ nghĩa với PvP và với gói mở trận 36.

**Phi chức năng**
- N1: Mọi file `.cs` ≤ 200 dòng.
- N2: Logic lượt là C# thuần trong `Gopet.UiLogic` (asmdef `noEngineReferences: true`) → unit-test được.
- N3: Không đổi wire format.

## Architecture

### Luồng dữ liệu

```
server PET_BATTLE(37)
  └─ BattleHandler.OnTurn (BattleHandler.cs:104-121)
       → BattleTurn{ActorId, Type, Effects[]}
  └─ BattleCoordinator.OnTurn (BattleCoordinator.cs:67-72)
  └─ BattleView.Apply
       ├─ _turn.ApplyTurnPacket(turn.ActorId, turn.Type, turn.Effects.Length)
       ├─ _vsIndicator.SetTurn(_turn.IsLocalTurn)
       └─ RefreshLocks()  →  _actionBar.SetActionsInteractable(_turn.CanAct)
                             _skillLeft.RefreshState(mp, locked: !_turn.CanAct)

click nút (BattleView.Actions.cs)
  └─ if (!_turn.CanAct) return;
     → _handler.SendX()  →  _turn.MarkActionSent()  →  RefreshLocks()
```

### Bảng quyết định `BattleTurnState.ApplyTurnPacket`

| Điều kiện | Nguồn server | IsLocalTurn | ActionPending |
|---|---|---|---|
| `actorId <= 0` | `PetBattle.cs:1547`, `:1617` (độc/phản đòn) | giữ nguyên | giữ nguyên |
| `type == Wait && effectCount == 0` | `PetBattle.cs:1643` (dùng vật phẩm) | giữ nguyên | `false` |
| còn lại | `:307`, `:1109` (đánh thường / kỹ năng) | `actorId != localActorId` | `false` |

### Chống kẹt (failure mode có tên)

Nếu heuristic ở dòng 2 đoán sai (server thêm đường gửi WAIT rỗng mới), nút sẽ khoá vĩnh viễn.
Chốt chặn: `BattleView` giữ `_pendingSince`; nếu `ActionPending` quá **8 giây** →
`_turn.ClearPending()` + `RefreshLocks()`. (Lượt server là `GopetManager.TimeNextTurn`, client
đọc qua `TurnDurationMs`; 8s đủ ngắn để người chơi không thấy kẹt, đủ dài để không đè lên round-trip.)

## Related Code Files

**Sửa**
- `GopetUnityClient/Assets/Scripts/Net/Battle/BattleHandler.cs` (`OnMobBattle` :69-81)
- `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.cs` (:14 khai báo, :61-78 `Apply`,
  :107 `SetTurn`, :145-181 nhóm handler sẽ chuyển đi)
- `GopetUnityClient/Assets/Scripts/Runtime/World/Battle/BattleActionBar.cs` (:12-14, :145-165)
- `GopetUnityClient/Assets/Scripts/Runtime/World/Battle/BattleSkillPanel.cs` (:161-185)
- `GopetUnityClient/Assets/Scripts/Runtime/World/Battle/BattleVsIndicator.cs` (:34-37)
- `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs` (:330-333 — chỉ nhánh `else` của `sendPetAttack`)

**Tạo**
- `GopetUnityClient/Assets/Scripts/UiLogic/BattleTurnState.cs` (+ `.meta` do Unity sinh)
- `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.Actions.cs`
- `GopetUnityClient/tests/Gopet.Net.Tests/BattleTurnStateTests.cs`

**Xoá:** không.

> Tên file dùng PascalCase trùng tên class — bắt buộc với C#/Unity và là quy ước đang dùng
> toàn bộ `Assets/Scripts` (ví dụ `SkillCooldownTracker.cs`). Ghi đè quy ước kebab-case chung.

## Implementation Steps

1. **`UiLogic/BattleTurnState.cs`** — class C# thuần, KHÔNG dùng `UnityEngine`
   (asmdef `Gopet.UiLogic` đặt `noEngineReferences: true`; nó có reference `Gopet.Net`
   nên dùng được hằng `BattleTurn.Wait`).
   ```csharp
   namespace Gopet.UiLogic
   {
       /// <summary>Trạng thái lượt phía client. Server gửi ActorId = người VỪA ra đòn
       /// (PetBattle.cs:307-308) nên lượt kế tiếp thuộc về bên còn lại.</summary>
       public sealed class BattleTurnState
       {
           private readonly int _localActorId;
           public BattleTurnState(int localActorId, bool localStarts);
           public bool IsLocalTurn { get; private set; }
           public bool ActionPending { get; private set; }
           public bool CanAct => IsLocalTurn && !ActionPending;
           public void MarkActionSent();
           public void ClearPending();
           public void ApplyTurnPacket(int actorId, sbyte type, int effectCount);
       }
   }
   ```
   `ApplyTurnPacket` cài đúng bảng quyết định ở mục Architecture. Comment mỗi nhánh kèm
   `PetBattle.cs:<line>` để người sau không "tối ưu" nhầm.

2. **`BattleHandler.OnMobBattle`** — sau dòng `start.IsParticipant = ownerId == _localUserId;`
   (`:78`) thêm `start.LocalStarts = start.IsParticipant;`.
   KHÔNG đọc thêm byte nào — wire PvE không có trường này, `ExpectFullyConsumed("ATTACK_MOB")`
   ở `:79` sẽ ném nếu đọc thừa.

3. **Tách `BattleView.Actions.cs`** — đổi `public sealed class BattleView` thành
   `public sealed partial class BattleView`; chuyển nguyên `OnAttack`, `OnPotion`,
   `OnSkillUsed`, `OnSurrenderClicked`, `OnBackClicked`, `PlayHitSound` (`:145-181`) sang file mới
   cùng namespace `Gopet.Runtime.World`. Không đổi thân hàm ở bước này.

4. **`BattleView.cs`** —
   - thêm field `private BattleTurnState _turn; private float _pendingSince;`
   - trong `Build`, ngay trước `_vsIndicator.SetTurn(...)` (`:107`):
     `_turn = new BattleTurnState(_start.LocalPet.ActorId, _start.LocalStarts);`
     rồi `_vsIndicator.SetTurn(_turn.IsLocalTurn);`
   - trong `Apply` (`:61-78`) thay dòng `:64` bằng
     `_turn.ApplyTurnPacket(turn.ActorId, turn.Type, turn.Effects.Length);`
     `_vsIndicator?.SetTurn(_turn.IsLocalTurn);`
   - thay `_actionBar?.Unlock();` (`:76`) bằng `RefreshLocks();`
   - thêm `private void RefreshLocks()` gọi `_actionBar?.SetActionsInteractable(_turn.CanAct)`
     và `_skillLeft?.RefreshState(_left.Mp, !_turn.CanAct)`
   - trong `Update()` (`:183`) thêm: nếu `_turn.ActionPending` và
     `Time.unscaledTime - _pendingSince > 8f` → `_turn.ClearPending(); RefreshLocks();`
   - `_cooldowns.OnTurnAdvanced(turn.ActorId, _start.LocalPet.ActorId)` (`:65`) giữ nguyên —
     nó đếm theo actorId, không phụ thuộc hướng lượt.

5. **`BattleView.Actions.cs`** — mỗi handler gửi gói thêm gate + đánh dấu:
   ```csharp
   private void OnAttack()
   {
       if (!_turn.CanAct) return;
       _handler.SendNormalAttack(); MarkSent();
   }
   ```
   tương tự `OnPotion`, `OnSkillUsed`. `MarkSent()` = `_turn.MarkActionSent();
   _pendingSince = Time.unscaledTime; RefreshLocks();`
   `OnSurrenderClicked`/`OnBackClicked` **không** gate theo lượt (R6) — server xếp hàng
   `IsSurrender` và xử lý khi tới lượt người chơi (`PetBattle.cs:677-684`).

6. **`BattleActionBar.cs`** —
   - đổi `_buttons` thành `_actionButtons` và **bỏ `_surrenderBtn` khỏi danh sách** (xoá `:40`)
   - xoá `_unlockAt` (`:14`), `Lock()` (`:145-149`), `Unlock()` (`:151-155`), `Update()` (`:162-165`)
   - thêm `public void SetActionsInteractable(bool on)` duyệt `_actionButtons`
   - giữ `LockSurrender()` nguyên trạng — giờ nó thật sự dính vì surrender đã tách khỏi vòng lặp
   - khởi tạo: gọi `SetActionsInteractable(false)` cuối `Create`, `BattleView.Build` sẽ bật lại
     qua `RefreshLocks()`.
   Net dòng ≈ −5 → còn ~162.

7. **`BattleSkillPanel.cs`** — thêm `private bool _locked;`, đổi chữ ký
   `public void RefreshState(int currentMp, bool locked = false)` (`:161`) gán `_locked = locked;`,
   và trong `RefreshLabels` (`:182`) đổi thành
   `row.Button.interactable = !_locked && ready && row.MpCost <= _currentMp;`.
   Net +2 dòng → 197. **Nếu vượt 200**: tách phần dựng UI tĩnh trong `Create` sang partial
   `BattleSkillPanel.Layout.cs` (không đổi hành vi).

8. **`BattleVsIndicator.cs`** — nhãn đang ở `anchorY 0.54..0.59 + deltaY` (`:35-36`), badge ở
   `0.58..0.76 + deltaY` (`:29-30`) → chồng 0.01. Hạ nhãn xuống `0.495..0.555 + deltaY`
   (chừa 0.025 ≈ 13px ở độ phân giải tham chiếu 540px).

9. **Unit test `BattleTurnStateTests.cs`** (xUnit, `tests/Gopet.Net.Tests` đã include
   `Assets/Scripts/UiLogic/**` — xem `Gopet.Net.Tests.csproj`). Ca:
   - mở trận `localStarts=true` → `CanAct == true`
   - `MarkActionSent()` → `CanAct == false`, `IsLocalTurn` vẫn true
   - gói `actorId = local, type=Normal, effects=1` → `IsLocalTurn == false`
   - gói `actorId = mob, type=Normal, effects=1` → `IsLocalTurn == true`, `CanAct == true`
   - gói `actorId = -1` → không đổi `IsLocalTurn`, không reset pending
   - gói `actorId = local, type=Wait, effects=0` (vật phẩm) → `IsLocalTurn` vẫn true, `CanAct == true`
   - gói `actorId = local, type=Wait, effects=1` (kỹ năng) → `IsLocalTurn == false`
   - `ClearPending()` → `CanAct` theo `IsLocalTurn`

10. **Server — `PetBattle.sendPetAttack` (`:330-333`).** Nhánh `else` (PvE) đang gửi
    `(int)(DateTime.Now - MobAttackTime).TotalSeconds` (luôn âm). Đổi thành
    `Utilities.round(delaTimeTurn - Utilities.CurrentTimeMillis)` — **giống hệt nhánh PvP ở `:328`**.
    Sau đó 2 nhánh `if/else` giống nhau → gộp thành 1 dòng, xoá `if (!petAttackMob)`.
    KHÔNG đụng `MobAttackTime` ở `:667`, `:671`, `:1337`, `:1401` — đó là bộ đếm 2 giây
    "quái suy nghĩ", dùng đúng mục đích.
    Kiểm chứng: `dotnet build SRCGOPETGOC/GServer/Gopet.csproj`.

11. **Compile check:** `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1`
    (bước 4/6/7/8 compile, bước 5 chạy test, bước 10 chặn 200 dòng).

## Todo List

- [x] Tạo `UiLogic/BattleTurnState.cs`
- [x] `BattleHandler.OnMobBattle` gán `LocalStarts`
- [x] Tách `BattleView.Actions.cs`, đổi `BattleView` thành `partial`
- [x] `BattleView` dùng `BattleTurnState` + `RefreshLocks()` + watchdog 8s
- [x] Gate `CanAct` ở 3 handler gửi gói; giữ surrender/back không gate
- [x] `BattleActionBar`: bỏ timer, tách surrender khỏi `_actionButtons`, thêm `SetActionsInteractable`
- [x] `BattleSkillPanel.RefreshState(mp, locked)`
- [x] `BattleVsIndicator` hạ nhãn xuống dưới badge
- [x] Viết `BattleTurnStateTests.cs` (8 ca)
- [x] Server: `sendPetAttack` PvE gửi ms còn lại thay vì số âm; gộp 2 nhánh
- [x] Chạy `verify.ps1` — 10/10 xanh + `dotnet build` server

## Success Criteria

| # | Đo được bằng |
|---|---|
| S1 | Vào trận quái → nhãn "Đến lượt bạn" hiện ngay khi overlay mở |
| S2 | Bấm Tấn công → nhãn tắt ngay khi gói 37 về; 3 nút hành động + mọi nút kỹ năng xám |
| S3 | Quái đánh xong → nhãn bật lại, nút sáng lại |
| S4 | Bấm kỹ năng 2 lần thật nhanh → chỉ 1 gói `[81][37][4]` rời client (bắt bằng log gửi) |
| S5 | Trong lượt mình, dùng bình máu qua menu 1016 → nhãn KHÔNG tắt, nút vẫn sáng |
| S6 | Pet bị trúng độc/phản đòn → nhãn không nhấp nháy mất |
| S7 | Không lần nào nhận `redDialog("Chưa tới lượt của bạn")` trong 10 lượt liên tiếp |
| S8 | Nút "Xin thua"/"‹ Quay lại" bấm được cả trong lượt quái |
| S9 | `verify.ps1` 10/10 OK; `BattleTurnStateTests` 8/8 pass |
| S10 | Log gói 37 PvE: trường đầu là số **dương** ≤ 25000 và giảm dần trong lượt (không còn ~-2) |

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| Heuristic "WAIT + 0 effect = vật phẩm" đoán sai → nút khoá chết | Thấp | Cao | Watchdog 8s ở `BattleView.Update` (bước 4); ghi rõ giả định trong comment + `docs/battle-system.md` |
| PvE thực tế quái đi trước ở một map đặc biệt → `LocalStarts=true` sai | Thấp | Thấp | Gói 37 đầu tiên tự sửa lại trạng thái; nhãn sai tối đa 1 lượt |
| Tách partial làm hỏng thứ tự khởi tạo | Thấp | Trung bình | Partial chỉ chứa method, không field/ctor; `verify.ps1` bước 6 bắt lỗi compile |
| `BattleSkillPanel` vượt 200 dòng | Trung bình | Thấp | Kế hoạch dự phòng tách `BattleSkillPanel.Layout.cs` (bước 7) |
| Nhãn hạ xuống đè lên sprite pet ở màn hình tỉ lệ khác | Thấp | Thấp | CanvasScaler `matchWidthOrHeight = 1f` (khớp chiều cao) → anchor theo Y ổn định |

## Security Considerations

- Khoá nút là **tiện ích UI**, không phải rào bảo mật. Server vẫn là nguồn chân lý:
  `checkWhoseTurn` (`PetBattle.cs:314-317`, `:1622,1656`) từ chối hành động sai lượt.
- Không thêm dữ liệu nào từ client vào quyết định damage/EXP.
- Giảm spam gói 37 → giảm nguy cơ chạm luật chống auto của server
  (`GopetPlace.cs:406-414` ban nếu giết quái < 4500ms trên map 12).

## Next Steps

- Chặn: phase 02 (hàng đợi lượt dựa trên `RefreshLocks()` và `_turn.CanAct` của phase này).
- Song song: phase 03 (server) không đụng file nào của phase này.
